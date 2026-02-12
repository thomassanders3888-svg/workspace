using System;
using System.Collections.Generic;
using System.IO;

namespace TerraForgeServer.World
{
    public class ChunkManager
    {
        private readonly Dictionary<(int, int), Chunk> _loadedChunks = new();
        private readonly TerrainEngine _terrainEngine;
        private readonly string _savePath;
        private readonly int _renderDistance;

        public int RenderDistance => _renderDistance;
        public IReadOnlyDictionary<(int, int), Chunk> LoadedChunks => _loadedChunks;

        public ChunkManager(int seed, string savePath, int renderDistance = 8)
        {
            _terrainEngine = new TerrainEngine(seed);
            _savePath = savePath;
            _renderDistance = renderDistance;
            Directory.CreateDirectory(savePath);
        }

        public Chunk GetChunk(int chunkX, int chunkZ)
        {
            var coord = (chunkX, chunkZ);
            if (_loadedChunks.TryGetValue(coord, out var chunk))
            {
                chunk.LastAccessed = DateTime.UtcNow;
                return chunk;
            }

            chunk = LoadChunkFromDisk(chunkX, chunkZ);
            if (chunk == null)
            {
                chunk = new Chunk(chunkX, chunkZ);
                _terrainEngine.GenerateChunk(chunk);
                SaveChunkToDisk(chunk);
            }
            _loadedChunks[coord] = chunk;
            return chunk;
        }

        public BlockType GetBlockAt(int worldX, int worldY, int worldZ)
        {
            if (worldY < 0 || worldY >= Chunk.HEIGHT) return BlockType.Air;
            int chunkX = (int)Math.Floor(worldX / (double)Chunk.WIDTH);
            int chunkZ = (int)Math.Floor(worldZ / (double)Chunk.DEPTH);
            int localX = worldX - chunkX * Chunk.WIDTH;
            int localZ = worldZ - chunkZ * Chunk.DEPTH;
            var chunk = GetChunk(chunkX, chunkZ);
            return chunk.GetBlock(localX, worldY, localZ);
        }

        public void SetBlockAt(int worldX, int worldY, int worldZ, BlockType type)
        {
            if (worldY < 0 || worldY >= Chunk.HEIGHT) return;
            int chunkX = (int)Math.Floor(worldX / (double)Chunk.WIDTH);
            int chunkZ = (int)Math.Floor(worldZ / (double)Chunk.DEPTH);
            int localX = worldX - chunkX * Chunk.WIDTH;
            int localZ = worldZ - chunkZ * Chunk.DEPTH;
            var chunk = GetChunk(chunkX, chunkZ);
            chunk.SetBlock(localX, worldY, localZ, type);
        }

        public void UpdatePlayerPosition(int playerChunkX, int playerChunkZ)
        {
            var toUnload = new List<(int, int)>();
            foreach (var coord in _loadedChunks.Keys)
            {
                int dist = Math.Max(Math.Abs(coord.Item1 - playerChunkX), Math.Abs(coord.Item2 - playerChunkZ));
                if (dist > _renderDistance) toUnload.Add(coord);
            }
            foreach (var coord in toUnload)
            {
                if (_loadedChunks.TryGetValue(coord, out var chunk))
                {
                    if (chunk.NeedsSave) SaveChunkToDisk(chunk);
                    chunk.Dispose();
                    _loadedChunks.Remove(coord);
                }
            }
        }

        public void PreloadAround(int chunkX, int chunkZ)
        {
            for (int dx = -_renderDistance; dx <= _renderDistance; dx++)
            {
                for (int dz = -_renderDistance; dz <= _renderDistance; dz++)
                {
                    int cx = chunkX + dx;
                    int cz = chunkZ + dz;
                    int dist = Math.Max(Math.Abs(dx), Math.Abs(dz));
                    if (dist <= _renderDistance) GetChunk(cx, cz);
                }
            }
        }

        public bool IsChunkLoaded(int chunkX, int chunkZ) => _loadedChunks.ContainsKey((chunkX, chunkZ));

        public int GetTerrainHeight(int worldX, int worldZ) => _terrainEngine.GetTerrainHeight(worldX, worldZ);

        public void SaveAll()
        {
            foreach (var chunk in _loadedChunks.Values)
                if (chunk.NeedsSave) SaveChunkToDisk(chunk);
        }

        public void UnloadAll()
        {
            SaveAll();
            foreach (var chunk in _loadedChunks.Values) chunk.Dispose();
            _loadedChunks.Clear();
        }

        private Chunk LoadChunkFromDisk(int chunkX, int chunkZ)
        {
            string path = GetChunkPath(chunkX, chunkZ);
            if (!File.Exists(path)) return null;
            try
            {
                using var fs = File.OpenRead(path);
                using var reader = new BinaryReader(fs);
                return Chunk.Deserialize(reader);
            }
            catch { return null; }
        }

        private void SaveChunkToDisk(Chunk chunk)
        {
            string path = GetChunkPath(chunk.ChunkX, chunk.ChunkZ);
            using var fs = File.Open(path, FileMode.Create);
            using var writer = new BinaryWriter(fs);
            chunk.Serialize(writer);
            chunk.NeedsSave = false;
        }

        private string GetChunkPath(int chunkX, int chunkZ) => Path.Combine(_savePath, $"c.{chunkX}.{chunkZ}.bin");
    }
}