#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldGenerator))]
public class TerrainGeneratorEditor : Editor
{
    private WorldGenerator generator;
    private bool showPreview = true;
    private bool autoUpdate = false;
    
    private void OnEnable()
    {
        generator = (WorldGenerator)target;
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);
        
        // Seed controls
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Seed"))
        {
            Undo.RecordObject(generator, "Randomize Seed");
            generator.seed = Random.Range(int.MinValue, int.MaxValue);
            EditorUtility.SetDirty(generator);
        }
        if (GUILayout.Button("Clear Seed"))
        {
            Undo.RecordObject(generator, "Clear Seed");
            generator.seed = 0;
            EditorUtility.SetDirty(generator);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Preview toggle
        showPreview = EditorGUILayout.Toggle("Show Preview", showPreview);
        autoUpdate = EditorGUILayout.Toggle("Auto Update", autoUpdate);
        
        EditorGUILayout.Space(5);
        
        // Generate/Clear buttons
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
        if (GUILayout.Button("Generate Terrain", GUILayout.Height(30)))
        {
            GenerateTerrain();
        }
        GUI.backgroundColor = Color.white;
        
        GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
        if (GUILayout.Button("Clear All", GUILayout.Height(30)))
        {
            ClearTerrain();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        // Status
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField($"Active Chunks: {TerrainManager.Instance?.GetActiveChunkCount() ?? 0}");
    }
    
    private void GenerateTerrain()
    {
        if (Application.isPlaying)
        {
            generator.GenerateWorld();
        }
        else
        {
            // Editor mode - create preview
            Debug.Log("[Editor] Generate preview terrain");
            // Generator.GeneratePreview();
        }
    }
    
    private void ClearTerrain()
    {
        if (Application.isPlaying && TerrainManager.Instance != null)
        {
            TerrainManager.Instance.ClearAllChunks();
        }
        else
        {
            // Remove preview objects
            var previewObjects = GameObject.FindGameObjectsWithTag("TerrainPreview");
            foreach (var obj in previewObjects)
            {
                DestroyImmediate(obj);
            }
        }
    }
    
    private void OnSceneGUI()
    {
        if (!showPreview) return;
        
        // Draw spawn bounds
        Handles.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        if (generator.playerSpawnPoint != null)
        {
            Handles.DrawWireCube(generator.playerSpawnPoint.position, Vector3.one * 2f);
        }
    }
}
#endif