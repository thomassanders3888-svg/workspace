using UnityEngine;
using UnityEditor;

namespace TerraForge.Editor
{
    [CustomEditor(typeof(TerrainGenerator))]
    public class TerrainGeneratorEditor : UnityEditor.Editor
    {
        private TerrainGenerator generator;
        private bool showPreview = true;
        private bool autoUpdate = false;
        
        // Preview texture
        private Texture2D previewTexture;
        private const int previewSize = 256;
        
        private void OnEnable()
        {
            generator = (TerrainGenerator)target;
            GeneratePreview();
        }
        
        private void OnDisable()
        {
            if (previewTexture != null)
                DestroyImmediate(previewTexture);
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space(10);
            
            // Title
            GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.2f, 0.8f, 0.4f);
            EditorGUILayout.LabelField("TerraForge Terrain Generator", titleStyle);
            EditorGUILayout.Space(5);
            
            // Seed section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Seed Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("seed"), new GUIContent("World Seed"));
            
            // Randomize seed button
            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            if (GUILayout.Button("🎲 Randomize", GUILayout.Width(100)))
            {
                Undo.RecordObject(generator, "Randomize Seed");
                generator.seed = Random.Range(int.MinValue, int.MaxValue);
                serializedObject.Update();
                if (autoUpdate) generator.GenerateTerrain();
                GeneratePreview();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Terrain settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Terrain Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("terrainSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chunkSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHeight"));
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Noise settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Noise Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("noiseScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("octaves"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("persistence"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lacunarity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("heightCurve"), new GUIContent("Height Curve"));
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Layer settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Terrain Layers", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("terrainLayers"), true);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Object placement
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Object Placement", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("placeObjects"));
            if (generator.placeObjects)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("objectDensity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabCollections"), true);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Auto update toggle
            EditorGUILayout.BeginHorizontal();
            autoUpdate = EditorGUILayout.ToggleLeft("Auto-Update Preview", autoUpdate, GUILayout.Width(150));
            if (GUI.changed && autoUpdate)
            {
                generator.GenerateTerrain();
                GeneratePreview();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Action buttons
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = new Color(0.2f, 0.9f, 0.4f);
            if (GUILayout.Button("▶ Generate Terrain", GUILayout.Height(35)))
            {
                Undo.RecordObject(generator, "Generate Terrain");
                generator.GenerateTerrain();
                GeneratePreview();
            }
            
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("🗑 Clear Terrain", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog("Clear Terrain", 
                    "Are you sure you want to clear all generated terrain chunks?", 
                    "Clear", "Cancel"))
                {
                    Undo.RecordObject(generator, "Clear Terrain");
                    generator.ClearTerrain();
                }
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(15);
            
            // Preview section
            showPreview = EditorGUILayout.Foldout(showPreview, "Heightmap Preview", true, EditorStyles.foldoutHeader);
            
            if (showPreview)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                if (previewTexture != null)
                {
                    GUILayout.FlexibleSpace();
                    
                    Rect previewRect = GUILayoutUtility.GetRect(previewSize, previewSize, 
                        GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                    previewRect.x = (EditorGUIUtility.currentViewWidth - previewSize) / 2;
                    
                    EditorGUI.DrawPreviewTexture(previewRect, previewTexture);
                    
                    GUILayout.FlexibleSpace();
                    
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Heightmap Visualization", EditorStyles.centeredGreyMiniLabel);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField("Preview not available", EditorStyles.centeredGreyMiniLabel);
                }
                
                EditorGUILayout.EndVertical();
