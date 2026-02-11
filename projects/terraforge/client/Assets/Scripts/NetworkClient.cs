using System;
using UnityEngine;
using NativeWebSocket;
using System.Collections.Generic;

[System.Serializable]
public struct ChunkPacket
{
    public int x, z;
    // Heightmap y for each x,z
    public int[] data;
}

[System.Serializable]
public struct PlayerPos
{
    public float x, y, z;
}

public class NetworkClient : MonoBehaviour
{
    [SerializeField] private string serverUrl = "ws://localhost:8080/ws";
    [SerializeField] private bool connectOnStart = true;
    
    public static NetworkClient Instance { get; private set; }
    public event Action<ChunkPacket> OnChunkReceived;
    
    private WebSocket ws;
    private Queue<ChunkPacket> chunkQueue = new Queue<ChunkPacket>();
    private readonly object queueLock = new object();
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private async void Start()
    {
        if (connectOnStart) await Connect();
    }
    
    public async void Connect()
    {
        ws = new WebSocket(serverUrl);
        
        ws.OnOpen += () => Debug.Log("[Network] Connected");
        ws.OnError += e => Debug.LogError($"[Network] Error: {e}");
        ws.OnClose += e => Debug.Log($"[Network] Disconnected: {e}");
        ws.OnMessage += bytes => HandleMessage(bytes);
        
        await ws.Connect();
    }
    
    private void HandleMessage(byte[] bytes)
    {
        try
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            var chunk = JsonUtility.FromJson<ChunkPacket>(json);
            lock (queueLock) chunkQueue.Enqueue(chunk);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] Parse error: {e.Message}");
        }
    }
    
    public void SendPosition(Vector3 pos)
    {
        if (ws == null || ws.State != WebSocketState.Open) return;
        var pkt = new PlayerPos { x = pos.x, y = pos.y, z = pos.z };
        string json = JsonUtility.ToJson(pkt);
        ws.SendText(json);
    }
    
    private void Update()
    {
        ws?.DispatchMessageQueue();
        
        lock (queueLock)
        {
            while (chunkQueue.Count > 0)
                OnChunkReceived?.Invoke(chunkQueue.Dequeue());
        }
        
        // Send position every frame (throttle in production)
        SendPosition(Camera.main.transform.position);
    }
    
    private async void OnDestroy()
    {
        if (ws != null)
        {
            await ws.Close();
            ws = null;
        }
    }
}
