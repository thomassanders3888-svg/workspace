using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

public class PerformanceProfiler : MonoBehaviour {
    public static PerformanceProfiler Instance { get; private set; }
    
    private Dictionary<string, Stopwatch> _timers = new();
    private Dictionary<string, float> _avgFrameTime = new();
    private Dictionary<string, float> _maxFrameTime = new();
    
    void Awake() { Instance = this; }
    
    public void Begin(string name) {
        if (!_timers.ContainsKey(name)) _timers[name] = new Stopwatch();
        _timers[name].Restart();
    }
    
    public void End(string name) {
        if (_timers.TryGetValue(name, out var timer)) {
            float ms = timer.ElapsedMilliseconds;
            _timers[name].Stop();
            
            _avgFrameTime[name] = Mathf.Lerp(_avgFrameTime.GetValueOrDefault(name), ms, 0.1f);
            _maxFrameTime[name] = Mathf.Max(_maxFrameTime.GetValueOrDefault(name), ms);
        }
    }
    
    public float GetAverage(string name) => _avgFrameTime.GetValueOrDefault(name);
    public float GetMax(string name) => _maxFrameTime.GetValueOrDefault(name);
    
    void OnGUI() {
        int y = 10;
        foreach (var kvp in _avgFrameTime) {
            GUI.Label(new Rect(10, y, 300, 20), $"{kvp.Key}: {kvp.Value:F2}ms (max: {_maxFrameTime[kvp.Key]:F2}ms)");
            y += 20;
        }
    }
}
