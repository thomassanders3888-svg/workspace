 Serialization uses BinaryFormatter and custom methods for handling data types.
<|im_start|>ChunkData.cs
```csharp
using System;
using System.IO;

namespace GameWorld {
    [Serializable]
    public class ChunkData {
        private const int BlockSize = 16;
        private byte[] blockTypes; // Storage for block types

        public ChunkData(int width, int height) {
            blockTypes = new byte[width * height];
        }

        public void SetBlockType(int x, int y, int type) {
            if (x >= 0 && x < BlockSize && y >= 0 && y < BlockSize) {
                blockTypes[x + y * BlockSize] = (byte)type;
            }
        }

        public int GetBlockType(int x, int y) {
            if (x >= 0 && x < BlockSize && y >= 0 && y < BlockSize) {
                return blockTypes[x + y * BlockSize];
            }
            return -1; // Return -1 for out of bounds access
        }

        public byte[] Serialize() {
            using (var memoryStream = new MemoryStream()) {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, this);
                return memoryStream.ToArray();
            }
        }

        public static ChunkData Deserialize(byte[] data) {
            using (var memoryStream = new MemoryStream(data)) {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (ChunkData)binaryFormatter.Deserialize(memoryStream);
            }
        }

        // Custom serialization methods to handle specific data types
        public void SerializeCustom(Stream stream) {
            using (var writer = new BinaryWriter(stream)) {
                writer.Write(blockTypes.Length); // Write length of blockTypes array
                foreach (byte type in blockTypes) {
                    writer.Write(type);
                }
            }
        }

        public static ChunkData DeserializeCustom(byte[]
