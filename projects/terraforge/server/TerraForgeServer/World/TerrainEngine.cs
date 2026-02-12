using System;

namespace TerraForgeServer.World
{
    public class TerrainEngine
    {
        private readonly SimplexNoise _noise;
        private readonly int _seed;
        public const int SEA_LEVEL = 62;
        public const int MAX_HEIGHT = 100;
        public const int MIN_HEIGHT = 30;

        public TerrainEngine(int seed)
        {
            _seed = seed;
            _noise = new SimplexNoise(seed);
        }

        public void GenerateChunk(Chunk chunk)
        {
            int chunkX = chunk.ChunkX;
            int chunkZ = chunk.ChunkZ;
            for (int x = 0; x < Chunk.WIDTH; x++)
            {
                for (int z = 0; z < Chunk.DEPTH; z++)
                {
                    int worldX = chunkX * Chunk.WIDTH + x;
                    int worldZ = chunkZ * Chunk.DEPTH + z;
                    int height = GetTerrainHeight(worldX, worldZ);
                    GenerateColumn(chunk, x, z, height);
                }
            }
            chunk.IsDirty = true;
            chunk.NeedsSave = true;
        }

        public int GetTerrainHeight(int worldX, int worldZ)
        {
            double nx = worldX * 0.015;
            double nz = worldZ * 0.015;
            double noise = _noise.FBM2D(nx, nz, 4, 0.5, 2.0);
            double heightRange = MAX_HEIGHT - MIN_HEIGHT;
            int height = MIN_HEIGHT + (int)((noise + 1) * 0.5 * heightRange);
            return Math.Clamp(height, MIN_HEIGHT, MAX_HEIGHT);
        }

        private void GenerateColumn(Chunk chunk, int x, int z, int surfaceHeight)
        {
            for (int y = 0; y < Chunk.HEIGHT; y++)
            {
                BlockType block;
                if (y > surfaceHeight)
                {
                    if (y <= SEA_LEVEL) block = BlockType.Water;
                    else block = BlockType.Air;
                }
                else if (y == surfaceHeight)
                {
                    if (y >= SEA_LEVEL - 2) block = BlockType.Grass;
                    else block = BlockType.Sand;
                }
                else if (y > surfaceHeight - 4)
                {
                    block = BlockType.Dirt;
                }
                else if (y == 0)
                {
                    block = BlockType.Bedrock;
                }
                else
                {
                    block = BlockType.Stone;
                }
                chunk.SetBlock(x, y, z, block);
            }

            if (surfaceHeight > SEA_LEVEL)
            {
                for (int y = surfaceHeight - 1; y >= Math.Max(0, surfaceHeight - 4); y--)
                {
                    chunk.SetBlock(x, y, z, BlockType.Dirt);
                }
            }

            AddOres(chunk, x, z, surfaceHeight);
        }

        private void AddOres(Chunk chunk, int x, int z, int surfaceHeight)
        {
            for (int y = 5; y < surfaceHeight - 4; y++)
            {
                double oreNoise = _noise.Noise3D(x * 0.1, y * 0.1, z * 0.1);
                double depthFactor = 1.0 - (y / (double)Chunk.HEIGHT);
                if (oreNoise > 0.6f)
                {
                    BlockType ore = PickOre(y, depthFactor, oreNoise);
                    if (ore != BlockType.Stone) chunk.SetBlock(x, y, z, ore);
                }
            }
        }

        private BlockType PickOre(int y, double depthFactor, double noise)
        {
            if (noise > 0.85 && depthFactor > 0.6) return BlockType.OreDiamond;
            if (noise > 0.75 && depthFactor > 0.4) return BlockType.OreGold;
            if (noise > 0.65 && depthFactor > 0.25) return BlockType.OreIron;
            if (noise > 0.6) return BlockType.OreCoal;
            return BlockType.Stone;
        }
    }
}