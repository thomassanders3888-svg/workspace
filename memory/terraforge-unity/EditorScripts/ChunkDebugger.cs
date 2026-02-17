#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ChunkDebugger : MonoBehaviour
{
    [Header("Visualization")]
    public bool showChunkBounds = true;
    public bool showLOD = true;
    public bool showBlockCounts = true;
    public bool showActiveOnly = false;
    
    [Header("Colors")]
    public Color activeColor = new Color(0, 1, 0, 0.3f);
    public Color inactiveColor = new Color(1, 0, 0, 0.1f);
    public Color lod1Color = Color.green;
    public Color lod2Color = Color.yellow;
    public Color lod3Color = Color.red;
    
    private void OnDrawGizmos()
    {
        if (TerrainManager.Instance == null) return;
        
        var chunks = TerrainManager.Instance.GetAllChunks();
        if (chunks == null) return;
        
        foreach (var chunk in chunks)
        {
            if (chunk == null) continue;
            
            if (showActiveOnly && !chunk.IsActive) continue;
            
            DrawChunkGizmo(chunk);
        }
    }
    
    void DrawChunkGizmo(ChunkData chunk)
    {
        Vector3 center = new Vector3(
            chunk.WorldX + 8, 
            64, 
            chunk.WorldZ + 8
        );
        Vector3 size = new Vector3(16, 128, 16);
        
        if (showChunkBounds)
        {
            Gizmos.color = chunk.IsActive ? activeColor : inactiveColor;
            Gizmos.DrawWireCube(center, size);
            
            if (chunk.IsActive)
            {
                Gizmos.color = new Color(activeColor.r, activeColor.g, activeColor.b, 0.05f);
                Gizmos.DrawCube(center, size);
            }
        }
        
        if (showLOD && chunk.IsActive)
        {
            Gizmos.color = GetLODColor(chunk.LODLevel);
            Gizmos.DrawWireSphere(center, 1f + (3 - chunk.LODLevel));
        }
        
        #if UNITY_EDITOR
        if (showBlockCounts && chunk.IsActive)
        {
            Vector3 labelPos = center + Vector3.up * 70;
            Handles.Label(labelPos, $"Chunk ({chunk.X},{chunk.Z})\\nBlocks: {chunk.BlockCount}\\nLOD: {chunk.LODLevel}");
        }
        #endif
    }
    
    Color GetLODColor(int lod)
    {
        switch (lod)
        {
            case 0: return lod1Color;
            case 1: return lod2Color;
            default: return lod3Color;
        }
    }
}

[CustomEditor(typeof(ChunkDebugger))]
public class ChunkDebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Refresh Chunk Data"))
        {
            var debugger = (ChunkDebugger)target;
            EditorUtility.SetDirty(debugger);
            SceneView.RepaintAll();
        }
        
        EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
        
        if (TerrainManager.Instance != null)
        {
            EditorGUILayout.LabelField($"Total Chunks: {TerrainManager.Instance.GetTotalChunkCount()}");
            EditorGUILayout.LabelField($"Active Chunks: {TerrainManager.Instance.GetActiveChunkCount()}");
            EditorGUILayout.LabelField($"Rendered Blocks: {TerrainManager.Instance.GetTotalBlockCount()}");
        }
    }
}
#endif