using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class NetworkDebugger : MonoBehaviour {
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI latencyText;
    
    private float pingTimer;
    private List<float> pingHistory = new();
    private int packetCount;
    
    void Update() {
        pingTimer += Time.deltaTime;
        if (pingTimer >= 1f) {
            UpdateStats();
            pingTimer = 0;
        }
    }
    
    void UpdateStats() {
        float avgPing = pingHistory.Count > 0 ? CalculateAverage(pingHistory) : 0;
        latencyText.text = $"Ping: {avgPing:F0}ms";
        statsText.text = $"Packets/s: {packetCount}\nConnected: {NetworkClient.Instance?.IsConnected ?? false}";
        packetCount = 0;
    }
    
    float CalculateAverage(List<float> values) {
        float sum = 0;
        foreach (var v in values) sum += v;
        return sum / values.Count;
    }
}
