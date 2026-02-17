using UnityEngine;
using System.Collections.Generic;

public class CaveGenerator {
    private int seed;
    private OpenSimplexNoise noise;
    
    public CaveGenerator(int worldSeed) {
        seed = worldSeed;
        noise = new OpenSimplexNoise(seed + 12345);
    }
    
    public void CarveCaves(ChunkData chunk, int chunkX, int chunkZ) {
        for (int x = 0; x < ChunkData.SIZE; x++) {
            for (int z = 0; z < ChunkData.SIZE; z++) {
                int worldX = chunkX * ChunkData.SIZE + x;
                int worldZ = chunkZ * ChunkData.SIZE + z;
                
                float caveValue = Get3DNoise(worldX * 0.08f, 64 * 0.08f, worldZ * 0.08f);
                
                for (int y = 5; y < 60; y++) {
                    caveValue = Get3DNoise(worldX * 0.08f, y * 0.08f, worldZ * 0.08f);
                    
                    if (caveValue > 0.75f) {
                        chunk.SetBlock(x, y, z, BlockType.Air);
                    }
                }
            }
        }
    }
    
    float Get3DNoise(float x, float y, float z) {
        float a = (float)noise.Evaluate(x, y, z);
        float b = (float)noise.Evaluate(x * 2, y * 2, z * 2) * 0.5f;
        float c = (float)noise.Evaluate(x * 4, y * 4, z * 4) * 0.25f;
        return (a + b + c) * 0.57f + 0.5f;
    }
}

public class OreGenerator {
    private int seed;
    private System.Random random;
    
    public OreGenerator(int worldSeed) {
        seed = worldSeed;
        random = new System.Random(seed);
    }
    
    public void GenerateOres(ChunkData chunk, int chunkX, int chunkZ) {
        GenerateCoal(chunk, chunkX, chunkZ);
        GenerateIron(chunk, chunkX, chunkZ);
        GenerateGold(chunk, chunkX, chunkZ);
        GenerateDiamond(chunk, chunkX, chunkZ);
    }
    
    void GenerateCoal(ChunkData chunk, int cx, int cz) {
        GenerateVein(chunk, cx, cz, BlockType.OreCoal, 20, 30, 17, 128, 6, 12);
    }
    
    void GenerateIron(ChunkData chunk, int cx, int cz) {
        GenerateVein(chunk, cx, cz, BlockType.OreIron, 15, 20, 0, 64, 3, 5);
    }
    
    void GenerateGold(ChunkData chunk, int cx, int cz) {
        GenerateVein(chunk, cx, cz, BlockType.OreGold, 5, 8, 0, 32, 2, 4);
    }
    
    void GenerateDiamond(ChunkData chunk, int cx, int cz) {
        GenerateVein(chunk, cx, cz, BlockType.OreDiamond, 2, 5, 0, 16, 1, 2);
    }
    
    void GenerateVein(ChunkData chunk, int cx, int cz, BlockType ore, int minVeins, 
                     int maxVeins, int minY, int maxY, int minSize, int maxSize) {
        int veins = random.Next(minVeins, maxVeins + 1);
        
        for (int i = 0; i < veins; i++) {
            int centerX = random.Next(0, ChunkData.SIZE);
            int centerZ = random.Next(0, ChunkData.SIZE);
            int centerY = random.Next(minY, maxY + 1);
            int size = random.Next(minSize, maxSize + 1);
            
            for (int j = 0; j < size; j++) {
                Vector3Int pos = new Vector3Int(centerX, centerY, centerZ);
                pos.x += random.Next(-1, 2);
                pos.y += random.Next(-1, 2);
                pos.z += random.Next(-1, 2);
                
                if (pos.x >= 0 && pos.x < ChunkData.SIZE &&
                    pos.y >= 0 && pos.y < ChunkData.HEIGHT &&
                    pos.z >= 0 && pos.z < ChunkData.SIZE) {
                    
                    if (chunk.GetBlock(pos.x, pos.y, pos.z) == BlockType.Stone) {
                        chunk.SetBlock(pos.x, pos.y, pos.z, ore);
                    }
                }
            }
        }
    }
}

// Simplex noise implementation
public class OpenSimplexNoise {
    private long seed;
    public OpenSimplexNoise(long seed) { this.seed = seed; }
    
    public double Evaluate(double x, double y) {
        // Simplified noise for demo
        return (System.Math.Sin(x * 12.9898 + y * 78.233 + seed) * 43758.5453 % 1);
    }
    
    public double Evaluate(double x, double y, double z) {
        return (System.Math.Sin(x * 12.9898 + y * 78.233 + z * 53.753 + seed) * 43758.5453 % 1);
    }
}
