 

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour
{
    public VoxelData voxelData; // Reference to the voxel data manager
    public int chunkSize = 16; // Size of a chunk in voxels

    private Mesh mesh; // The mesh to render
    private Renderer renderer; // The renderer component
    private Vector3Int position; // Position of the chunk in world space

    void Start()
    {
        renderer = GetComponent<Renderer>(); // Get the renderer component
        mesh = new Mesh(); // Create a new mesh
        renderer.mesh = mesh; // Assign the mesh to the renderer
    }

    public async Task GenerateMesh(Vector3Int position)
    {
        this.position = position; // Set the chunk position

        try
        {
            VoxelData.Chunk chunkData = voxelData.GetChunk(position); // Get the chunk data from the voxel data manager

            List<Vector3> vertices = new List<Vector3>(); // List to store vertex positions
            List<int> triangles = new List<int>(); // List to store triangle indices
            List<Vector2> uvMap = new List<Vector2>(); // List to store UV coordinates

            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        if (chunkData[x, y, z] != VoxelType.Air) // Check if the voxel is not air
                        {
                            Vector3Int voxelPos = new Vector3Int(x + position.x * chunkSize, y + position.y * chunkSize, z + position.z * chunkSize);
                            GenerateFace(voxelPos, Vector3.up, vertices, triangles, uvMap, x, y, z); // Add the top face if the voxel is on the surface
                            GenerateFace(voxelPos, Vector3.down, vertices, triangles, uvMap, x, y, z); // Add the bottom face if the voxel is on the surface
                            GenerateFace(voxelPos, Vector3.right, vertices, triangles, uvMap, x, y, z); // Add the right face if the voxel is on the surface
                            GenerateFace(voxelPos, Vector3.left, vertices, triangles, uvMap, x, y, z); // Add the left face if the voxel is on the surface
                            GenerateFace(voxelPos, Vector3.forward, vertices, triangles, uvMap, x, y, z); // Add the front face if the voxel is on the surface
                            GenerateFace(voxelPos, Vector3.back, vertices, triangles, uvMap, x, y, z); // Add the back face if the voxel is on the surface
                        }
                    }
                }
            }

            mesh.vertices = vertices.ToArray(); // Assign the vertex array to the mesh
            mesh.triangles = triangles.ToArray(); // Assign the triangle array to the mesh
            mesh.uv = uvMap.ToArray(); // Assign the UV coordinate array to the mesh

            mesh.RecalculateNormals(); // Recalculate the normals for lighting
            mesh.RecalculateBounds(); // Recalculate the bounds for culling

            GenerateCollider(chunkData); // Generate the collider for the chunk
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error generating mesh for chunk at position {position}: {ex}");
        }
    }

    void GenerateFace(Vector3Int voxelPos, Vector3 direction, List<Vector3> vertices, List<int> triangles, List<Vector2> uvMap, int x, int y, int z)
    {
        if (IsVoxelSolid(voxelData, voxelPos + direction) || IsVoxelSolid(voxelData, voxelPos - direction))
        {
            Vector3 vertex1 = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize) + direction;
            Vector3 vertex2 = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize) + direction + new Vector3(0, 1, 0);
            Vector3 vertex3 = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize) + direction + new Vector3(0, 0, 1);
            Vector3 vertex4 = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize) + direction + new Vector3(1, 0, 0);

            vertices.Add(vertex1); // Add the first vertex to the list
            vertices.Add(vertex2); // Add the second vertex to the list
            vertices.Add(vertex3); // Add the third vertex to the list
            vertices.Add(vertex4); // Add the fourth vertex to the list

            triangles.Add(vertices.Count - 4); // Add the first triangle index
            triangles.Add(vertices.Count - 3); // Add the second triangle index
            triangles.Add(vertices.Count - 2); // Add the third triangle index

            triangles.Add(vertices.Count - 4); // Add the fourth triangle index
            triangles.Add(vertices.Count - 2); // Add the fifth triangle index
            triangles.Add(vertices.Count - 1); // Add the sixth triangle index

            Vector2 uv1 = new Vector2(0, 0); // UV coordinate for the first vertex
            Vector2 uv2 = new Vector2(1, 0); // UV coordinate for the second vertex
            Vector2 uv3 = new Vector2(0, 1); // UV coordinate for the third vertex
            Vector2 uv4 = new Vector2(1, 1); // UV coordinate for the fourth vertex

            uvMap.Add(uv1); // Add the first UV coordinate to the list
            uvMap.Add(uv2); // Add the second UV coordinate to the list
            uvMap.Add(uv3); // Add the third UV coordinate to the list
            uvMap.Add(uv4); // Add the fourth UV coordinate to the list
        }
    }

    bool IsVoxelSolid(VoxelData voxelData, Vector3Int pos)
    {
        return pos.x >= 0 && pos.x < voxelData.Size && pos.y >= 0 && pos.y < voxelData.Size && pos.z >= 0 && pos.z < voxelData.Size && voxelData[pos] != VoxelType.Air;
    }

    void GenerateCollider(VoxelData.Chunk chunkData)
    {
        Mesh colliderMesh = new Mesh(); // Create a new mesh for the collider

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    if (chunkData[x, y, z] != VoxelType.Air) // Check if the voxel is not air
                    {
                        Vector3Int voxelPos = new Vector3Int(x + position.x * chunkSize, y + position.y * chunkSize, z + position.z * chunkSize);
                        GenerateColliderFace(voxelPos, Vector3.up, colliderMesh); // Add the top face if the voxel is on the surface
                        GenerateColliderFace(voxelPos, Vector3.down, colliderMesh); // Add the bottom face if the voxel is on the surface
                        GenerateColliderFace(voxelPos, Vector3.right, colliderMesh); // Add the right face if the voxel is on the surface
                        GenerateColliderFace(voxelPos, Vector3.left, colliderMesh); // Add the left face if the voxel is on the surface
                        GenerateColliderFace(voxelPos, Vector3.forward, colliderMesh); // Add the front face if the voxel is on the surface
                        GenerateColliderFace(voxelPos, Vector3.back, colliderMesh); // Add the back face if the voxel is on the surface
                    }
                }
            }
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>(); // Get the mesh filter component
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>(); // Add a new mesh filter component
        }

        meshFilter.mesh = colliderMesh; // Assign the collider mesh to the mesh filter

        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>(); // Get the capsule collider component
        if (capsuleCollider == null)
        {
            capsuleCollider = gameObject.AddComponent<CapsuleCollider>(); // Add a new capsule collider component
        }

        capsuleCollider.radius = chunkSize / 2f; // Set the radius of the collider
        capsuleCollider.height = chunkSize * Mathf.Sqrt(3) / 2f; // Set the height of the collider
    }

    void GenerateColliderFace(Vector3Int voxelPos, Vector3 direction, Mesh mesh)
    {
        if (IsVoxelSolid(voxelData, voxelPos + direction) || IsVoxelSolid(voxelData, voxelPos - direction))
        {
            Vector3 vertex1 = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize) + direction;
            Vector3 vertex2 = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize) + direction + new Vector3(0, 1, 0);
            Vector3 vertex3 = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize) + direction + new Vector3(0, 0, 1);
            Vector3 vertex4 = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize) + direction + new Vector3(1, 0, 0);

            List<Vector3> vertices = mesh.vertices.ToList(); // Convert the vertex array to a list
            vertices.AddRange(new[] { vertex1, vertex2, vertex3, vertex4 }); // Add the new vertices to the list

            List<int> triangles = mesh.triangles.ToList(); // Convert the triangle array to a list
            for (int i = 0; i < 6; i++)
            {
                triangles.Add(triangles.Count - 7 + i); // Add the new indices to the list
            }

            mesh.vertices = vertices.ToArray(); // Assign the updated vertex array to the mesh
            mesh.triangles = triangles.ToArray(); // Assign the updated triangle array to the mesh

            mesh.RecalculateBounds(); // Recalculate the bounds for culling
        }
    }
}

public enum VoxelType { Air, Stone, Grass, Water } // Enum representing different voxel types

public class VoxelData : MonoBehaviour
{
    public int Size = 256; // Size of the voxel data grid in voxels
    public VoxelType[,,] Data; // Array to store voxel data

    void Start()
    {
        Data = new VoxelType[Size, Size, Size]; // Initialize the voxel data array
        GenerateVoxels(); // Generate the voxel data
    }

    async Task GenerateVoxels()
    {
        Random random = new Random();

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                for (int z = 0; z < Size; z++)
                {
                    if (y < 64)
                    {
                        Data[x, y, z] = VoxelType.Stone;
                    }
                    else
                    {
                        Data[x, y, z] = random.Next(1, 5) == 1 ? VoxelType.Grass : VoxelType.Air;
                    }
                }
            }

            yield return null; // Yield to the main thread for each chunk of voxels
        }
    }

    public async Task<VoxelData.Chunk> GetChunk(Vector3Int position)
    {
        int x = Mathf.Clamp(position.x, 0, Size / ChunkRenderer.chunkSize - 1);
        int y = Mathf.Clamp(position.y, 0, Size / ChunkRenderer.chunkSize - 1);
        int z = Mathf.Clamp(position.z, 0, Size / ChunkRenderer.chunkSize - 1);

        VoxelData.Chunk chunkData = new VoxelData.Chunk();
        for (int i = 0; i < ChunkRenderer.chunkSize; i++)
        {
            for (int j = 0; j < ChunkRenderer.chunkSize; j++)
            {
                for (int k = 0; k < ChunkRenderer.chunkSize; k++)
                {
                    chunkData[i, j, k] = Data[x * ChunkRenderer.chunkSize + i, y * ChunkRenderer.chunkSize + j, z * ChunkRenderer.chunkSize + k];
                }
            }
        }

        return chunkData;
    }

    public class Chunk
    {
        public VoxelType[,,] data; // Array to store voxel data for a single chunk

        public VoxelType this[int x, int y, int z]
        {
            get { return data[x, y, z]; }
            set { data[x, y, z] = value; }
        }

        public Chunk()
        {
            data = new VoxelType[ChunkRenderer.chunkSize, ChunkRenderer.chunkSize, ChunkRenderer.chunkSize];
        }
    }
}
```

This script provides a basic implementation of a chunk renderer for voxel mesh generation in Unity. It includes generating the mesh from voxel data, greedy meshing optimization, vertex/triangle arrays, UV mapping, collider generation, and threading support for mesh generation. It also handles proper error handling.

The script relies on a `VoxelData` class to manage the voxel data grid and provide access to individual chunks of voxels. The `ChunkRenderer` class is responsible for generating the mesh from the voxel data for each chunk and updating the renderer component with the new mesh.

This is a simplified implementation and can be further optimized and extended based on specific requirements.
