using UnityEngine;

public class GizmoDrawer : MonoBehaviour {
    public bool drawPlayerBounds = true;
    public bool drawChunkBorders = false;
    public bool drawSpawnPoints = true;
    public bool drawNetworkReach = false;
    
    public Color boundsColor = Color.green;
    public Color chunkColor = Color.yellow;
    public Color spawnColor = Color.red;
    public Color networkColor = Color.cyan;
    
    void OnDrawGizmos() {
        if (drawPlayerBounds) DrawPlayerBounds();
        if (drawChunkBorders) DrawChunkBorders();
        if (drawSpawnPoints && Application.isPlaying) DrawSpawnPoints();
        if (drawNetworkReach) DrawNetworkReach();
    }
    
    void DrawPlayerBounds() {
        Gizmos.color = boundsColor;
        Vector3 center = transform.position + Vector3.up * GameConstants.PLAYER_HEIGHT * 0.5f;
        Vector3 size = new Vector3(GameConstants.PLAYER_WIDTH, GameConstants.PLAYER_HEIGHT, GameConstants.PLAYER_WIDTH);
        Gizmos.DrawWireCube(center, size);
    }
    
    void DrawChunkBorders() {
        Gizmos.color = chunkColor;
        Vector3Int chunkPos = new Vector3Int(
            Mathf.FloorToInt(transform.position.x / GameConstants.CHUNK_SIZE) * GameConstants.CHUNK_SIZE + GameConstants.CHUNK_SIZE / 2,
            GameConstants.CHUNK_HEIGHT / 2,
            Mathf.FloorToInt(transform.position.z / GameConstants.CHUNK_SIZE) * GameConstants.CHUNK_SIZE + GameConstants.CHUNK_SIZE / 2
        );
        Gizmos.DrawWireCube(chunkPos, new Vector3(GameConstants.CHUNK_SIZE, GameConstants.CHUNK_HEIGHT, GameConstants.CHUNK_SIZE));
    }
    
    void DrawSpawnPoints() {
        Gizmos.color = spawnColor;
        var spawners = FindObjectsOfType<EnemySpawner>();
        foreach (var spawner in spawners) {
            if (spawner.spawnPoints != null) {
                foreach (var point in spawner.spawnPoints) {
                    if (point != null) {
                        Gizmos.DrawWireSphere(point.position, 1f);
                    }
                }
            }
        }
    }
    
    void DrawNetworkReach() {
        Gizmos.color = networkColor;
        Gizmos.DrawWireSphere(transform.position, 50f);
    }
}
