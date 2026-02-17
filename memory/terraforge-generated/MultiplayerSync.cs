using UnityEngine;
using System.Collections.Generic;

public class MultiplayerSync : MonoBehaviour {
    public static MultiplayerSync Instance { get; private set; }
    public float syncInterval = 0.05f;
    public float interpolationDelay = 0.1f;
    
    private Dictionary<string, RemotePlayer> remotePlayers = new();
    private Queue<PlayerStateUpdate> pendingUpdates = new();
    
    void Awake() { Instance = this; }
    
    void Start() {
        NetworkClient.Instance.OnPlayerJoined += OnPlayerJoined;
        NetworkClient.Instance.OnPlayerLeft += OnPlayerLeft;
        NetworkClient.Instance.OnPlayerUpdate += OnPlayerUpdate;
        
        StartCoroutine(SendLocalPlayerState());
    }
    
    System.Collections.IEnumerator SendLocalPlayerState() {
        var wait = new WaitForSeconds(syncInterval);
        while (true) {
            yield return wait;
            BroadcastLocalState();
        }
    }
    
    void BroadcastLocalState() {
        var player = FindObjectOfType<PlayerController>();
        if (player == null) return;
        
        var state = new PlayerStateData {
            playerId = player.playerId,
            position = player.transform.position,
            rotation = player.transform.rotation,
            velocity = player.GetComponent<CharacterController>().velocity,
            health = player.GetComponent<Player>().Health,
            timestamp = Time.time
        };
        
        NetworkClient.Instance.SendPlayerState(state);
    }
    
    void OnPlayerJoined(string playerId, string username) {
        var remotePlayer = Instantiate(Resources.Load<GameObject>("RemotePlayer")).GetComponent<RemotePlayer>();
        remotePlayer.Initialize(playerId, username);
        remotePlayers[playerId] = remotePlayer;
    }
    
    void OnPlayerLeft(string playerId) {
        if (remotePlayers.TryGetValue(playerId, out var player)) {
            Destroy(player.gameObject);
            remotePlayers.Remove(playerId);
        }
    }
    
    void OnPlayerUpdate(PlayerStateData state) {
        if (!remotePlayers.TryGetValue(state.playerId, out var player)) return;
        
        player.AddStateToBuffer(state);
    }
}

public class RemotePlayer : MonoBehaviour {
    public string playerId;
    public string username;
    public TextMesh nameTag;
    
    private Queue<PlayerStateData> stateBuffer = new();
    private float interpolationTimer;
    
    public void Initialize(string id, string name) {
        playerId = id;
        username = name;
        nameTag.text = name;
    }
    
    public void AddStateToBuffer(PlayerStateData state) {
        stateBuffer.Enqueue(state);
    }
    
    void Update() {
        if (stateBuffer.Count < 2) return;
        
        var from = stateBuffer.Peek();
        var to = stateBuffer.ToArray()[1];
        
        float t = (Time.time - from.timestamp) / (to.timestamp - from.timestamp);
        
        if (t >= 1) {
            stateBuffer.Dequeue();
            return;
        }
        
        transform.position = Vector3.Lerp(from.position, to.position, t);
        transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, t);
    }
}

[System.Serializable]
public struct PlayerStateData {
    public string playerId;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public float health;
    public float timestamp;
}
