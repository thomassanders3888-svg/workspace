using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TerraForgeServer.World;

namespace TerraForgeServer.Network {
    public class WorldSync {
        private readonly NetworkManager _networkManager;
        private readonly ChunkManager _chunkManager;
        private readonly Dictionary<Guid, ChunkSubscription> _subscriptions;
        private const int ViewDistance = 8;
        private const int ChunkSize = 16;

        public WorldSync(NetworkManager networkManager, ChunkManager chunkManager) {
            _networkManager = networkManager;
            _chunkManager = chunkManager;
            _subscriptions = new Dictionary<Guid, ChunkSubscription>();
            _networkManager.OnClientConnected += OnClientConnected;
            _networkManager.OnClientDisconnected += OnClientDisconnected;
        }

        private void OnClientConnected(Guid sessionId) {
            _subscriptions[sessionId] = new ChunkSubscription(sessionId);
        }

        private void OnClientDisconnected(Guid sessionId) {
            _subscriptions.Remove(sessionId);
        }

        public void UpdatePlayerPosition(Guid sessionId, float x, float y, float z) {
            if (!_subscriptions.TryGetValue(sessionId, out var sub))
                return;

            int newChunkX = (int)(x / ChunkSize);
            int newChunkZ = (int)(z / ChunkSize);

            if (newChunkX != sub.ChunkX || newChunkZ != sub.ChunkZ) {
                sub.ChunkX = newChunkX;
                sub.ChunkZ = newChunkZ;
                _ = SendInitialChunksAsync(sessionId, newChunkX, newChunkZ);
            }
        }

        public void NotifyChunkChanged(int chunkX, int chunkZ, Chunk chunk) {
            var packet = CreateChunkDeltaPacket(chunkX, chunkZ, chunk);
            foreach (var sub in _subscriptions.Values) {
                if (IsChunkInRange(chunkX, chunkZ, sub.ChunkX, sub.ChunkZ)) {
                    _ = _networkManager.SendToClientAsync(sub.SessionId, packet);
                }
            }
        }

        private async Task SendInitialChunksAsync(Guid sessionId, int centerX, int centerZ) {
            for (int dx = -ViewDistance; dx <= ViewDistance; dx++) {
                for (int dz = -ViewDistance; dz <= ViewDistance; dz++) {
                    var chunk = _chunkManager.GetChunk(centerX + dx, centerZ + dz);
                    if (chunk != null) {
                        var packet = CreateChunkDataPacket(centerX + dx, centerZ + dz, chunk);
                        await _networkManager.SendToClientAsync(sessionId, packet);
                    }
                }
            }
        }

        private byte[] CreateChunkDataPacket(int chunkX, int chunkZ, Chunk chunk) {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)0x01);
            writer.Write(chunkX);
            writer.Write(chunkZ);
            writer.Write(chunk.GetRawData());
            return ms.ToArray();
        }

        private byte[] CreateChunkDeltaPacket(int chunkX, int chunkZ, Chunk chunk) {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)0x02);
            writer.Write(chunkX);
            writer.Write(chunkZ);
            var changes = chunk.GetChanges();
            writer.Write(changes.Count);
            foreach (var change in changes.Take(256)) {
                writer.Write((ushort)(change.X | (change.Y << 8)));
                writer.Write(change.BlockId);
            }
            return ms.ToArray();
        }

        private bool IsChunkInRange(int cx, int cz, int px, int pz) {
            int dx = Math.Abs(cx - px);
            int dz = Math.Abs(cz - pz);
            return dx <= ViewDistance && dz <= ViewDistance;
        }
    }

    public class ChunkSubscription {
        public Guid SessionId { get; }
        public int ChunkX { get; set; }
        public int ChunkZ { get; set; }

        public ChunkSubscription(Guid sessionId) {
            SessionId = sessionId;
        }
    }
}
