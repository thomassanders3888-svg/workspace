using Xunit;

public class ChunkDataTests {
    [Fact]
    public void SetBlock_BlockIsStored() {
        var chunk = new ChunkData(0, 0);
        chunk.SetBlock(5, 10, 5, BlockType.Stone);
        Assert.Equal(BlockType.Stone, chunk.GetBlock(5, 10, 5));
    }
    
    [Fact]
    public void Serialize_Deserialize_RoundTrip() {
        var chunk = new ChunkData(0, 0);
        chunk.SetBlock(0, 0, 0, BlockType.Grass);
        var bytes = chunk.Serialize();
        var restored = ChunkData.Deserialize(bytes);
        Assert.Equal(BlockType.Grass, restored.GetBlock(0, 0, 0));
    }
}
