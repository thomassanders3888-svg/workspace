using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;

namespace TerraForge.Editor
{
    public class BuildMenuEditor : EditorWindow
    {
        // Build configuration
        private bool buildWindows = true;
        private bool buildMac = false;
        private bool buildLinux = false;
        private bool isDevelopmentBuild = true;
        private bool buildAddressables = true;
        private bool autoRunBuild = false;
        private string buildVersion = "1.0.0";
        private string buildPath = "Builds/";
        
        // Advanced options
        private bool showAdvanced = false;
        private string scriptingDefineSymbols = "";
        private bool compressionEnabled = true;
        private BuildOptions compressionMethod = BuildOptions.CompressWithLz4HC;
        
        // Progress tracking
        private bool isBuilding = false;
        private float buildProgress = 0f;
        private string currentBuildTarget = "";
        
        [MenuItem("TerraForge/Build/Build Menu %&B")]
        public static void ShowWindow()
        {
            GetWindow<BuildMenuEditor>("TerraForge Build", true);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            // Title
            GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.2f, 0.8f, 0.4f);
            EditorGUILayout.LabelField("⚒️ TerraForge Build Manager", titleStyle);
            EditorGUILayout.Space(5);
            
            // Version
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Build Version:", GUILayout.Width(100));
            buildVersion = EditorGUILayout.TextField(buildVersion);
            if (GUILayout.Button("Auto", GUILayout.Width(50)))
            {
                // Auto-generate version from date
                buildVersion = DateTime.Now.ToString("yy.MM.dd.H");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Build targets
            EditorGUILayout.LabelField("Build Targets", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUIStyle platformStyle = new GUIStyle(EditorStyles.toggle);
            platformStyle.fontSize = 12;
            
            EditorGUILayout.BeginHorizontal();
            buildWindows = EditorGUILayout.ToggleLeft("🪟 Windows (64-bit)", buildWindows, platformStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            buildMac = EditorGUILayout.ToggleLeft("🍎 macOS (Universal)", buildMac, platformStyle);
            if (buildMac && !Application.platform.ToString().Contains("OSX"))
            {
                EditorGUILayout.LabelField("(Relay build needed)", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            buildLinux = EditorGUILayout.ToggleLeft("🐧 Linux (64-bit)", buildLinux, platformStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(15);
            
            // Build type
            EditorGUILayout.LabelField("Build Type", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUI.BeginChangeCheck();
            isDevelopmentBuild = EditorGUILayout.ToggleLeft("🔧 Development Build (Debug symbols, Profiler)", isDevelopmentBuild);
            
            if (!isDevelopmentBuild)
            {
                GUI.backgroundColor = new Color(1f, 0.9f, 0.2f);
                EditorGUILayout.HelpBox("Release build selected. Ensure all debugging code is disabled.", MessageType.Warning);
                GUI.backgroundColor = Color.white;
            }
            
            buildAddressables = EditorGUILayout.ToggleLeft("📦 Build Addressables", buildAddressables);
            autoRunBuild = EditorGUILayout.ToggleLeft("▶️ Auto-run after build (current platform only)", autoRunBuild);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(15);
            
            // Advanced options
            showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced Options", true);
            if (showAdvanced)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField("Build Path:", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                buildPath = EditorGUILayout.TextField(buildPath);
                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    string selectedPath = EditorUtility.OpenFolderPanel("Select Build Output", buildPath, "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        buildPath = selectedPath.StartsWith(Application.dataPath) 
                            ? selectedPath.Substring(Application.dataPath.Length - 6) 
                            : selectedPath;
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                compressionEnabled = EditorGUILayout.ToggleLeft("Enable Compression", compressionEnabled);
                if (compressionEnabled)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Method:", GUILayout.Width(60));
                    int compressionSelection = compressionMethod == BuildOptions.CompressWithLz4HC ? 0 : 1;
                    string[] options = { "LZ4