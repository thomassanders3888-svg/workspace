using System;
using System.Buffers;
using System.IO;
using System.Threading;

namespace TerraForgeServer.World
{
    /// <summary>
    /// 16x16x128 voxel chunk with sparse storage
    /// </summary>
    public sealed class Chunk : IDisposable
    {
        public const int WIDTH = 16;
        public const int DEPTH = 16;
        public const int HEIGHT = 128;
        public const int TOTAL_BLOCKS = WIDTH * DEPTH * HEIGHT;
        
        public int ChunkX { get; }
        public int ChunkZ { get; }
        
        // Sparse storage: only non-air blocks
        private readonly BlockType[] _blocks;
        private readonly ReaderWriterLockSlim _lock = new();
        
        public bool IsDirty { get; set; } = true;
        public bool NeedsSave { get; set; } = false;
        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
        
        public Chunk(int chunkX, int chunkZ)
        {
            ChunkX = chunkX;
            ChunkZ = chunkZ;
            _blocks = ArrayPool<BlockType>.Shared.Rent(TOTAL_BLOCKS);
            Array.Fill(_blocks, BlockType.Air);
        }
        
        private int GetIndex(int x, int y, int z) => (y * DEPTH + z) * WIDTH + x;
        
        public BlockType GetBlock(int x, int y, int z)
        {
            if (!InBounds(x, y, z)) return BlockType.Air;
            _lock.EnterReadLock();
            try { return _blocks[GetIndex(x, y, z)]; }
            finally { _lock.ExitReadLock(); }
        }
        
        public void SetBlock(int x, int y, int z, BlockType type)
        {
            if (!InBounds(x, y, z)) return;
            _lock.EnterWriteLock();
            try 
            { 
                _blocks[GetIndex(x, y, z)] = type;
                IsDirty = true;
                NeedsSave = true;
                LastAccessed = DateTime.UtcNow;
            }
            finally { _lock.ExitWriteLock(); }
        }
        
        public static bool InBounds(int x, int y, int z) => 
            x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT && z >= 0 && z < DEPTH;
        
        public void Serialize(BinaryWriter writer)
        {
            _lock.EnterReadLock();
            try
            {
                writer.Write(ChunkX);
                writer.Write(ChunkZ);
                
                // Count non-air blocks
                int count = 0;
                for (int i = 0; i < TOTAL_BLOCKS; i++)
                    if (_blocks[i] != BlockType.Air) count++;
                
                writer.Write(count);
                
                // Write blocks
                for (int i = 0; i < TOTAL_BLOCKS; i++)
                {
                    if (_blocks[i] != BlockType.Air)
                    {
                        writer.Write((short)i);
                        writer.Write((byte)_blocks[i]);
                    }
                }
            }
            finally { _lock.ExitReadLock(); }
        }
        
        public static Chunk Deserialize(BinaryReader reader)
        {
            int cx = reader.ReadInt32();
            int cz = reader.ReadInt32();
            var chunk = new Chunk(cx, cz);
            
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                short idx = reader.ReadInt16();
                byte type = reader.ReadByte();
                chunk._blocks[idx] = (BlockType)type;
            }
            
            return chunk;
        }
        
        public void Dispose()
        {
            ArrayPool<BlockType>.Shared.Return(_blocks);
            _lock.Dispose();
        }
    }
}
