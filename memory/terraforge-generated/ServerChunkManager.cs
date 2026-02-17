using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ServerChunkManager {
    private readonly ConcurrentDictionary<(int, int), ServerChunk> _chunks = new();
    private readonly IWorldRepository _repository;
    private readonly int _seed;
    
    public ServerChunkManager(int seed, IWorldRepository repository) {
        _seed = seed;
        _repository = repository;
    }
    
    public async Task<ServerChunk> GetOrCreateChunk(int x, int z) {
        var key = (x, z);
        
        if (_chunks.TryGetValue(key, out var chunk)) {
            chunk.LastAccessed = System.DateTime.Now;
            return chunk;
        }
        
        // Try load from database
        chunk = await _repository.LoadChunk(x, z);
        
        if (chunk == null) {
            // Generate new chunk
            chunk = GenerateChunk(x, z);
            await _repository.SaveChunk(chunk);
        }
        
        _chunks[key] = chunk;
        return chunk;
    }
    
    ServerChunk GenerateChunk(int x, int z) {
        var chunk = new ServerChunk(x, z);
        
        var terrainGen = new TerrainGenerator(_seed);
        terrainGen.Generate(chunk.Blocks, x, z);
        
        var caveGen = new CaveGenerator(_seed);
        caveGen.CarveCaves(chunk, x, z);
        
        var oreGen = new OreGenerator(_seed);
        oreGen.GenerateOres(chunk, x, z);
        
        chunk.Created = System.DateTime.Now;
        chunk.LastModified = chunk.Created;
        return chunk;
    }
    
    public void SetBlock(int worldX, int worldY, int worldZ, BlockType type) {
        int chunkX = worldX >> 4;
        int chunkZ = worldZ >> 4;
        
        if (_chunks.TryGetValue((chunkX, chunkZ), out var chunk)) {
            int localX = worldX & 0xF;
            int localZ = worldZ & 0xF;
            chunk.SetBlock(localX, worldY, localZ, type);
            chunk.LastModified = System.DateTime.Now;
            chunk.IsDirty = true;
            _repository.QueueSaveChunk(chunk);
        }
    }
    
    public async Task UnloadDistantChunks(int centerX, int centerZ, int radius) {
        var toUnload = new List<(int, int)>();
        
        foreach (var kvp in _chunks) {
            int dx = kvp.Key.Item1 - centerX;
            int dz = kvp.Key.Item2 - centerZ;
            
            if (System.Math.Abs(dx) > radius || System.Math.Abs(dz) > radius) {
                if (kvp.Value.IsDirty) {
                    await _repository.SaveChunk(kvp.Value);
                }
                toUnload.Add(kvp.Key);
            }
        }
        
        foreach (var key in toUnload) {
            _chunks.TryRemove(key, out _);
        }
    }
    
    public ConcurrentDictionary<(int, int), ServerChunk> GetLoadedChunks() => _chunks;
}

public class ServerChunk {
    public int ChunkX { get; set; }
    public int ChunkZ { get; set; }
    public ChunkData Blocks { get; set; }
    public bool IsDirty { get; set; }
    public System.DateTime Created { get; set; }
    public System.DateTime LastModified { get; set; }
    public System.DateTime LastAccessed { get; set; }
    
    public ServerChunk(int x, int z) {
        ChunkX = x;
        ChunkZ = z;
        Blocks = new ChunkData(x, z);
    }
    
    public void SetBlock(int x, int y, int z, BlockType type) {
        Blocks.SetBlock(x, y, z, type);
        IsDirty = true;
    }
}

public interface IWorldRepository {
    Task<ServerChunk> LoadChunk(int x, int z);
    Task SaveChunk(ServerChunk chunk);
    void QueueSaveChunk(ServerChunk chunk);
}
