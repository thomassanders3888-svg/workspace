using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace TerraForge
{
    /// <summary>
    /// Async chunk loading system with priority queue for nearby chunks
    /// </summary>
    public class ChunkLoadingSystem : MonoBehaviour
    {
        [Header("Chunk Settings")]
        public int chunkSize = 16;
        public int loadDistance = 8;
        
        [Header("Performance")]
        public int maxConcurrentLoads = 2;
        
        // Data
        private Dictionary<Vector3Int, ChunkData> loadedChunks = new Dictionary<Vector3Int, ChunkData>();
        private PriorityQueue<Vector3Int, float> loadQueue = new PriorityQueue<Vector3Int, float>();
        private HashSet<Vector3Int> loadingInProgress = new HashSet<Vector3Int>();
        
        // Events
        public event Action<Vector3Int, Chunk> OnChunkLoaded;
        public event Action<Vector3Int> OnChunkUnloaded;
        
        // Threading
        private CancellationTokenSource cts;
        
        void Awake()
        {
            cts = new CancellationTokenSource();
        }
        
        void OnDestroy()
        {
            cts?.Cancel();
        }
        
        public async Task<Chunk> LoadChunkAsync(Vector3Int position)
        {
            if (loadedChunks.ContainsKey(position))
            {
                return loadedChunks[position].Chunk;
            }
            
            if (loadingInProgress.Contains(position))
            {
                // Wait for in-progress load
                while (loadingInProgress.Contains(position))
                {
                    await Task.Delay(16, cts.Token);
                }
                return loadedChunks.ContainsKey(position) ? loadedChunks[position].Chunk : null;
            }
            
            loadingInProgress.Add(position);
            
            try
            {
                // Generate chunk on thread pool
                Chunk chunk = await Task.Run(() => GenerateChunk(position), cts.Token);
                
                // Main thread: finalize
                await UniTask.SwitchToMainThread();
                
                var data = new ChunkData
                {
                    Position = position,
                    Chunk = chunk,
                    LastAccessTime = Time.time
                };
                
                loadedChunks[position] = data;
                OnChunkLoaded?.Invoke(position, chunk);
                
                return chunk;
            }
            finally
            {
                loadingInProgress.Remove(position);
            }
        }
        
        private Chunk GenerateChunk(Vector3Int position)
        {
            // This runs on thread pool
            var blocks = new BlockType[chunkSize, chunkSize, chunkSize];
            
            // Simple noise-based generation (replace with actual WorldGen)
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++))
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        int worldY = position.y * chunkSize + y;
                        blocks[x, y, z] = worldY < 0 ? BlockType.Stone 
                            : worldY == 0 ? BlockType.Grass 
                            : BlockType.Air;
                    }
                }
            }
            
            return new Chunk(position, blocks);
        }
        
        public void UnloadChunk(Vector3Int position)
        {
            if (loadedChunks.ContainsKey(position))
            {
                var chunk = loadedChunks[position].Chunk;
                chunk?.Dispose();
                loadedChunks.Remove(position);
                OnChunkUnloaded?.Invoke(position);
            }
        }
        
        public void UpdateChunks(Vector3 playerPosition)
        {
            Vector3Int playerChunkPos = WorldToChunkCoord(playerPosition);
            
            // Queue chunks that need loading
            for (int x = -loadDistance; x <= loadDistance; x++)
            {
                for (int y = -loadDistance / 2; y <= loadDistance / 2; y++)
                {
                    for (int z = -loadDistance; z <= loadDistance; z++)
                    {
                        Vector3Int chunkPos = playerChunkPos + new Vector3Int(x, y, z);
                        
                        if (!loadedChunks.ContainsKey(chunkPos) && !loadingInProgress.Contains(chunkPos))
                        {
                            float priority = Vector3Int.Distance(chunkPos, playerChunkPos);
                            loadQueue.Enqueue(chunkPos, priority);
                        }
                    }
                }
            }
            
            // Process queue
            ProcessQueue();
            
            // Unload distant chunks
            UnloadDistantChunks(playerChunkPos);
        }
        
        private async void ProcessQueue()
        {
            while (loadingInProgress.Count < maxConcurrentLoads && loadQueue.Count > 0)
            {
                if (loadQueue.TryDequeue(out Vector3Int pos, out float _))
                {
                    await LoadChunkAsync(pos);
                }
            }
        }
        
        private void UnloadDistantChunks(Vector3Int centerPos)
        {
            var toUnload = new List<Vector3Int>();
            
            foreach (var kvp in loadedChunks)
            {
                if (Vector3Int.Distance(kvp.Key, centerPos) > loadDistance + 1)
                {
                    toUnload.Add(kvp.Key);
                }
            }
            
            foreach (var pos in toUnload)
            {
                UnloadChunk(pos);
            }
        }
        
        private Vector3Int WorldToChunkCoord(Vector3 worldPos)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x / chunkSize),
                Mathf.FloorToInt(worldPos.y / chunkSize),
                Mathf.FloorToInt(worldPos.z / chunkSize)
            );
        }
        
        // Simple priority queue implementation
        private class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
        {
            private List<(TElement Element, TPriority Priority)> elements = new List<(TElement, TPriority)>();
            
            public int Count => elements.Count;
            
            public void Enqueue(TElement element, TPriority priority)
            {
                elements.Add((element, priority));
            }
            
            public bool TryDequeue(out TElement element, out TPriority priority)
            {
                if (elements.Count == 0)
                {
                    element = default;
                    priority = default;
                    return false;
                }
                
                // Find min priority
                int minIndex = 0;
                for (int i = 1; i < elements.Count; i++)
                {
                    if (elements[i].Priority.CompareTo(elements[minIndex].Priority) < 0)
                    {
                        minIndex = i;
                    }
                }
                
                var item = elements[minIndex];
                elements.RemoveAt(minIndex);
                element = item.Element;
                priority = item.Priority;
                return true;
            }
        }
        
        private class ChunkData
        {
            public Vector3Int Position