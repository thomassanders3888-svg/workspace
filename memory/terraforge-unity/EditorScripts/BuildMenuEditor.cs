#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public class BuildMenuEditor : EditorWindow
{
    private bool buildWindows = true;
    private bool buildMac = false;
    private bool buildLinux = true;
    private bool buildServer = true;
    private bool developmentBuild = true;
    private string versionString = "0.1.0";
    private string buildPath = "../Build/";
    
    [MenuItem("TerraForge/Build Window")]
    public static void ShowWindow()
    {
        GetWindow<BuildMenuEditor>("TerraForge Build");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("TerraForge Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // Version
        versionString = EditorGUILayout.TextField("Version", versionString);
        buildPath = EditorGUILayout.TextField("Build Path", buildPath);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("Platforms", EditorStyles.boldLabel);
        
        buildWindows = EditorGUILayout.ToggleLeft("Windows (x64)", buildWindows);
        buildMac = EditorGUILayout.ToggleLeft("macOS (Apple Silicon)", buildMac);
        buildLinux = EditorGUILayout.ToggleLeft("Linux (x64)", buildLinux);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("Options", EditorStyles.boldLabel);
        
        developmentBuild = EditorGUILayout.ToggleLeft("Development Build", developmentBuild);
        buildServer = EditorGUILayout.ToggleLeft("Include Server Build", buildServer);
        
        EditorGUILayout.Space(20);
        
        GUI.backgroundColor = new Color(0.2f, 0.7f, 0.2f);
        if (GUILayout.Button("Build All", GUILayout.Height(40)))
        {
            BuildAll();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Build Server Only", GUILayout.Height(30)))
        {
            BuildServer();
        }
        
        if (GUILayout.Button("Open Build Folder"))
        {
            EditorUtility.RevealInFinder(buildPath);
        }
    }
    
    void BuildAll()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
        
        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No scenes in build settings", "OK");
            return;
        }
        
        BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;
        
        // Windows
        if (buildWindows)
        {
            BuildPipeline.BuildPlayer(scenes, 
                $"{buildPath}/TerraForge-Windows-{versionString}/TerraForge.exe",
                BuildTarget.StandaloneWindows64, 
                developmentBuild ? BuildOptions.Development : BuildOptions.None);
        }
        
        // macOS
        if (buildMac)
        {
            BuildPipeline.BuildPlayer(scenes,
                $"{buildPath}/TerraForge-Mac-{versionString}.app",
                BuildTarget.StandaloneOSX,
                developmentBuild ? BuildOptions.Development : BuildOptions.None);
        }
        
        // Linux
        if (buildLinux)
        {
            BuildPipeline.BuildPlayer(scenes,
                $"{buildPath}/TerraForge-Linux-{versionString}/TerraForge.x86_64",
                BuildTarget.StandaloneLinux64,
                developmentBuild ? BuildOptions.Development : BuildOptions.None);
        }
        
        // Server
        if (buildServer)
        {
            BuildServer();
        }
        
        EditorUtility.DisplayDialog("Build Complete", 
            $"Built TerraForge v{versionString} to {buildPath}", "OK");
    }
    
    void BuildServer()
    {
        // Invoke server build via dotnet
        var serverPath = "../Server/TerraForgeServer";
        // System.Diagnostics.Process.Start("dotnet", $"publish {serverPath} -c Release");
        Debug.Log($"[Build] Server build at {serverPath}");
    }
}
#endif