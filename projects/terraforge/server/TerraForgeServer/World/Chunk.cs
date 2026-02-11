using System; using System.Buffers; using System.Collections.Generic; using System.IO; using System.Threading;

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
        private readonly List<BlockChange> _changes = new();

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
            try
            {
                return _blocks[GetIndex(x, y, z)];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void SetBlock(int x, int y, int z, BlockType type)
        {
            if (!InBounds(x, y, z)) return;
            _lock.EnterWriteLock();
            try
            {
                int idx = GetIndex(x, y, z);
                if (_blocks[idx] != type)
                {
                    _blocks[idx] = type;
                    _changes.Add(new BlockChange(x, y, z, (ushort)type));
                }
                IsDirty = true;
                NeedsSave = true;
                LastAccessed = DateTime.UtcNow;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public static bool InBounds(int x, int y, int z) =>
            x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT && z >= 0 && z < DEPTH;

        /// <summary>
        /// Get raw block data as bytes for network transmission
        /// </summary>
        public byte[] GetRawData()
        {
            _lock.EnterReadLock();
            try
            {
                var data = new byte[TOTAL_BLOCKS * 2];
                for (int i = 0; i < TOTAL_BLOCKS; i++)
                {
                    data[i * 2] = (byte)(_blocks[i] & 0xFF);
                    data[i * 2 + 1] = (byte)((ushort)_blocks[i] >> 8);
                }
                return data;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Get and clear the list of block changes since last call
        /// </summary>
        public List<BlockChange> GetChanges()
        {
            _lock.EnterWriteLock();
            try
            {
                var result = new List<BlockChange>(_changes);
                _changes.Clear();
                return result;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

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
            finally
            {
                _lock.ExitReadLock();
            }
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

    /// <summary>
    /// Represents a single block change for network delta updates
    /// </summary>
    public record BlockChange(int X, int Y, int Z, ushort BlockId);
}
