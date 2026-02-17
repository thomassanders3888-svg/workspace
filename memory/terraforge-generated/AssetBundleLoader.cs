using UnityEngine;
using System.Collections.Generic;

public class AssetBundleLoader : MonoBehaviour {
    public static AssetBundleLoader Instance { get; private set; }
    
    private Dictionary<string, AssetBundle> loadedBundles = new();
    private Dictionary<string, Object> assetCache = new();
    public string bundlePath = "AssetBundles";
    public string baseCDN = "https://terraforge-cdn.example.com/bundles";
    
    void Awake() { Instance = this; }
    
    public void LoadBundle(string bundleName, System.Action<bool> callback = null) {
        if (loadedBundles.ContainsKey(bundleName)) {
            callback?.Invoke(true);
            return;
        }
        
        StartCoroutine(LoadBundleCoroutine(bundleName, callback));
    }
    
    System.Collections.IEnumerator LoadBundleCoroutine(string bundleName, System.Action<bool> callback) {
        string bundleURL = $"{baseCDN}/{bundleName}";
        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(bundleURL);
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success) {
            AssetBundle bundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
            loadedBundles[bundleName] = bundle;
            callback?.Invoke(true);
        } else {
            Debug.LogError($"Failed to load bundle {bundleName}: {request.error}");
            callback?.Invoke(false);
        }
        
        request.Dispose();
    }
    
    public T LoadAsset<T>(string bundleName, string assetName) where T : Object {
        string cacheKey = $