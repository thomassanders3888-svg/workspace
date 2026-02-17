 ```csharp
using UnityEngine;
using System.Collections.Generic;

public class TerrainManager : MonoBehaviour
{
    public int chunkSize = 16;
    public float loadRadius = 50f;
    public float unloadRadius = 75f;
    public Transform ViewerTransform;

    private Dictionary<Vector2, Chunk> chunkPool = new Dictionary<Vector2, Chunk>();

    void Start()
    {
        if (ViewerTransform == null)
        {
            Debug.LogError("ViewerTransform is not assigned. Assign it in the Inspector.");
        }
    }

    void Update()
    {
        Vector3 viewerPosition = ViewerTransform.position;
        Vector2 chunkCoord = GetChunkCoordinate(viewerPosition);

        // Load chunks within load radius
        for (int x = -Mathf.FloorToInt(loadRadius / chunkSize); x <= Mathf.CeilToInt(loadRadius / chunkSize); x++)
        {
            for (int y = -Mathf.FloorToInt(loadRadius / chunkSize); y <= Mathf.CeilToInt(loadRadius / chunkSize); y++)
            {
                Vector2 targetCoord = new Vector2(chunkCoord.x + x, chunkCoord.y + y);
                LoadChunk(targetCoord);
            }
        }

        // Unload chunks outside unload radius
        foreach (var kvp in chunkPool)
        {
            if (Vector2.Distance(kvp.Key, chunkCoord) > unloadRadius)
            {
                UnloadChunk(kvp.Key);
            }
        }
    }

    Vector2 GetChunkCoordinate(Vector3 worldPosition)
    {
        return new Vector2(Mathf.Floor(worldPosition.x / chunkSize), Mathf.Floor(worldPosition.z / chunkSize));
    }

    void LoadChunk(Vector2 coord)
    {
        if (!chunkPool.ContainsKey(coord))
        {
            Chunk chunk = CreateChunk(coord);
            chunkPool[coord] = chunk;
        }
    }

    Chunk CreateChunk(Vector2 coord)
    {
        GameObject chunkObject = new GameObject($"Chunk_{coord.x}_{coord.y}");
        Chunk chunk = chunkObject.AddComponent<Chunk>();
        chunk.Initialize(chunkSize, coord * chunkSize);
        return chunk;
    }

    void UnloadChunk(Vector2 coord)
    {
        if (chunkPool.ContainsKey(coord))
        {
            Destroy(chunkPool[coord].gameObject);
            chunkPool.Remove(coord);
        }
    }

    public class Chunk : MonoBehaviour
    {
        public int size;
        public Vector2 coordinate;
        public TerrainData terrainData;

        void Initialize(int size, Vector2 coordinate)
        {
            this.size = size;
            this.coordinate = coordinate;

            terrainData = new TerrainData();
            terrainData.heightmapResolution = size + 1;
            terrainData.size = new Vector3(size, size * 10f, size);

            Terrain terrain = gameObject.AddComponent<Terrain>();
            terrain.terrainData = terrainData;

            SetHeightMap();
        }

        void SetHeightMap()
        {
            float[,] heightmap = new float[size + 1, size + 1];

            for (int x = 0; x <= size; x++)
            {
                for (int z = 0; z <= size; z++)
                {
                    float noiseValue = Mathf.PerlinNoise((x + coordinate.x) * 0.05f, (z + coordinate.y) * 0.05f);
                    heightmap[x, z] = noiseValue * terrainData.size.y / 2f;
                }
            }

            terrainData.SetHeights(0, 0, heightmap);
        }
    }
}
```

### Explanation:
1. **ChunkCoord Struct**: Represents the coordinates of a chunk in the world.
2. **Chunk Pool**: A dictionary to manage chunks efficiently by reusing them.
3. **Load and Unload Radius**: Determines when to load or unload chunks based on the viewer's position.
4. **Viewer Transform**: Refers to the transform
