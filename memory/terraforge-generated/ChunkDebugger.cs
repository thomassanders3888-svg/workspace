using UnityEngine;

public class ChunkDebugger : MonoBehaviour {
    public bool showChunkBorders = true;
    public bool showChunkCoords = true;
    public Color borderColor = Color.yellow;
    
    void OnDrawGizmos() {
        if (!showChunkBorders) return;
        
        Gizmos.color = borderColor;
        Vector3 playerPos = transform.position;
        Vector3Int chunkPos = new Vector3Int(
            Mathf.FloorToInt(playerPos.x / 16) * 16,
            Mathf.FloorToInt(playerPos.y / 16) * 16,
            Mathf.FloorToInt(playerPos.z / 16) * 16
        );
        
        for (int x = -1; x <= 1; x++) {
            for (int z = -1; z <= 1; z++) {
                Vector3 center = new Vector3(
                    chunkPos.x + x * 16 + 8,
                    128,
                    chunkPos.z + z * 16 + 8
                );
                Gizmos.DrawWireCube(center, new Vector3(16, 256, 16));
            }
        }
    }
}
