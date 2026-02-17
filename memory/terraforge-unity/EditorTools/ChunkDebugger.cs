using UnityEngine;
using UnityEditor;

namespace TerraForge.Editor
{
    [InitializeOnLoad]
    public class ChunkDebugger : EditorWindow
    {
        private ChunkManager chunkManager;
        private bool showChunkBounds = true;
        private bool showChunkGrid = true;
        private bool showActiveChunks = true;
        private bool showInactiveChunks = false;
        private bool showChunkInfo = true;
        private bool autoRefresh = true;
        private float refreshInterval = 0.5f;
        private float lastRefreshTime;
        
        // Colors
        private Color activeChunkColor = new Color(0.2f, 1f, 0.2f, 0.3f);
        private Color inactiveChunkColor = new Color(0.7f, 0.7f, 0.7f, 0.15f);
        private Color selectedChunkColor = new Color(1f, 0.8f, 0.2f, 0.4f);
        private Color chunkBorderColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        private ChunkData selectedChunk;
        private Vector2 scrollPosition;
        
        [MenuItem("TerraForge/Tools/Chunk Debugger")]
        public static void ShowWindow()
        {
            GetWindow<ChunkDebugger>("Chunk Debugger", true, typeof(SceneView));
        }
        
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            FindChunkManager();
        }
        
        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        
        private void FindChunkManager()
        {
            if (chunkManager == null)
            {
                chunkManager = FindObjectOfType<ChunkManager>();
            }
        }
        
        private void OnGUI()
        {
            FindChunkManager();
            
            EditorGUILayout.Space(10);
            
            // Title
            GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel);
            titleStyle.fontSize = 16;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.2f, 0.8f, 0.4f);
            EditorGUILayout.LabelField("🧊 Chunk Debugger", titleStyle);
            EditorGUILayout.Space(5);
            
            // Status
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (chunkManager == null)
            {
                EditorGUILayout.HelpBox("No ChunkManager found in scene. Please ensure TerraForge is initialized.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("Status: ", $"✓ ChunkManager Active", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Active Chunks: {chunkManager.GetActiveChunkCount()}");
                EditorGUILayout.LabelField($"Total Chunks: {chunkManager.GetTotalChunkCount()}");
                EditorGUILayout.LabelField($"Chunk Size: {chunkManager.ChunkSize}m");
                EditorGUILayout.LabelField($"Render Distance: {chunkManager.RenderDistance} chunks");
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(15);
            
            // Visibility toggles
            EditorGUILayout.LabelField("Visualization Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            showChunkBounds = EditorGUILayout.ToggleLeft("Show Chunk Bounds", show