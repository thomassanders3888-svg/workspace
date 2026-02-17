 

```csharp
using UnityEngine;

public enum Biome
{
    Plains,
    Desert,
    Mountains,
    Forest
}

public class BiomeManager : MonoBehaviour
{
    public GameObject[] plainsBlocks;
    public GameObject[] desertBlocks;
    public GameObject[] mountainBlocks;
    public GameObject[] forestBlocks;

    public void InitializeBiomes()
    {
        // Initialize or load any necessary biome data here.
    }

    public Biome GetBiomeAt(float x, float z)
    {
        // Calculate the biome based on the position (x, z).
        int biomeIndex = Mathf.FloorToInt(x / 100) + Mathf.FloorToInt(z / 100);
        switch (biomeIndex % 4)
        {
            case 0:
                return Biome.Plains;
            case 1:
                return Biome.Desert;
            case 2:
                return Biome.Mountains;
            case 3:
                return Biome.Forest;
            default:
                Debug.LogError("Invalid biome index.");
                return Biome.Plains; // Default to Plains if something goes wrong.
        }
    }

    public GameObject GetBlockForBiome(Biome biome)
    {
        switch (biome)
        {
            case Biome.Plains:
                return plainsBlocks[Random.Range(0, plainsBlocks.Length)];
            case Biome.Desert:
                return desertBlocks[Random.Range(0, desertBlocks.Length)];
            case Biome.Mountains:
                return mountainBlocks[Random.Range(0, mountainBlocks.Length)];
            case Biome.Forest:
                return forestBlocks[Random.Range(0, forestBlocks.Length)];
            default:
                Debug.LogError("Invalid biome.");
                return null; // Return null if the biome is invalid.
        }
    }

    public void GenerateBlockAt(float x, float z)
    {
        Biome biome = GetBiomeAt(x, z);
        GameObject block = GetBlockForBiome(biome);
        if (block != null)
        {
            Instantiate(block, new Vector3(x, 0f, z), Quaternion.identity);
        }
    }
}
```

This script defines a `BiomeManager` class that manages different biomes and their corresponding blocks. The `GetBiomeAt(float x, float z)` method calculates the biome based on the position (x, z). The `GetBlockForBiome(Biome biome)` method returns
