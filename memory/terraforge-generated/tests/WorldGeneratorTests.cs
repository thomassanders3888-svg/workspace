using Xunit;

public class WorldGeneratorTests {
    [Fact]
    public void GenerateChunk_ReturnsValidData() {
        var chunk = WorldGenerator.GenerateChunk(new Vector2Int(0, 0));
        Assert.NotNull(chunk);
        Assert.Equal(16, chunk.GetLength(0));
        Assert.Equal(256, chunk.GetLength(1));
    }
    
    [Theory]
    [InlineData(0, 0, BiomeType.Plains)]
    [InlineData(1000, 1000, BiomeType.Mountains)]
    public void GetBiomeAt_ReturnsCorrectBiome(float x, float z, BiomeType expected) {
        var biome = WorldGenerator.GetBiomeAt(x, z);
        Assert.Equal(expected, biome);
    }
}
