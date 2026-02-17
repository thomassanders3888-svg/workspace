using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class WorldPersistence : MonoBehaviour {
    public static WorldPersistence Instance { get; private set; }
    public string saveDirectory = "Saves";
    public string worldName = "Default";
    private string WorldPath => Path.Combine(Application.persistentDataPath, saveDirectory, worldName);
    
    void Awake() {
        Instance = this;
        Directory.CreateDirectory(WorldPath);
    }
    
    public async Task SaveChunk(int chunkX, int chunkZ) {
        var chunk = TerrainManager.Instance.GetChunk(chunkX, chunkZ);
        if (chunk == null) return;
        
        string filePath = Path.Combine(WorldPath, $"chunk_{chunkX}_{chunkZ}.dat");
        byte[] data = chunk.Serialize();
        
        await Task.Run(() => File.WriteAllBytes(filePath, data));
    }
    
    public async Task LoadChunk(int chunkX, int chunkZ) {
        string filePath = Path.Combine(WorldPath, $"chunk_{chunkX}_{chunkZ}.dat");
        
        if (!File.Exists(filePath)) {
            TerrainManager.Instance.GenerateChunk(chunkX, chunkZ);
            return;
        }
        
        byte[] data = await Task.Run(() => File.ReadAllBytes(filePath));
        TerrainManager.Instance.LoadChunkFromData(chunkX, chunkZ, data);
    }
    
    public async Task SavePlayerData() {
        var player = FindObjectOfType<PlayerController>();
        if (player == null) return;
        
        PlayerSaveData data = new() {
            position = player.transform.position,
            rotation = player.transform.rotation,
            health = player.GetComponent<Player>().Health,
            inventory = player.GetComponent<InventorySystem>().GetSaveData(),
            timestamp = System.DateTime.UtcNow.ToString()
        };
        
        string json = JsonUtility.ToJson(data);
        await Task.Run(() => File.WriteAllText(Path.Combine(WorldPath, "player.json"), json));
    }
    
    public async Task LoadPlayerData() {
        string path = Path.Combine(WorldPath, "player.json");
        if (!File.Exists(path)) return;
        
        string json = await Task.Run(() => File.ReadAllText(path));
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
        
        var player = FindObjectOfType<PlayerController>();
        if (player != null) {
            player.transform.position = data.position;
            player.GetComponent<Player>().Health = data.health;
        }
    }
    
    public void AutoSave() {
        StartCoroutine(AutoSaveCoroutine());
    }
    
    System.Collections.IEnumerator AutoSaveCoroutine() {
        while (true) {
            yield return new WaitForSeconds(300); // 5 minutes
            SavePlayerData();
            Debug.Log("Auto-saved world.");
        }
    }
}

[System.Serializable]
public class PlayerSaveData {
    public Vector3 position;
    public Quaternion rotation;
    public float health;
    public float[] inventory;
    public string timestamp;
}
