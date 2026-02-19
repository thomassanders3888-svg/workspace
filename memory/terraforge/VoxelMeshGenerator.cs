using System.Collections.Generic;
using UnityEngine;

namespace TerraForge.Rendering
{
    /// <summary>
    /// Voxel mesh generator with greedy meshing and face culling
    /// </summary>
    public class VoxelMeshGenerator : MonoBehaviour
    {
        [Header("Mesh Settings")]
        public Material atlasMaterial;
        public bool generateColliders = true;
        public bool useGreedyMeshing = true;
        
        private Mesh chunkMesh;
        private MeshCollider meshCollider;
        
        void Awake()
        {
            chunkMesh = new Mesh();
            chunkMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            GetComponent<MeshFilter>().mesh = chunkMesh;
            
            if (generateColliders)
                meshCollider = GetComponent<MeshCollider>();
        }
        
        /// <summary>
        /// Generate mesh from chunk data
        /// </summary>
        public void GenerateMesh(ChunkData chunk)
        {
            if (useGreedyMeshing)
                GenerateGreedyMesh(chunk);
            else
                GenerateSimpleMesh(chunk);
                
            if (generateColliders && meshCollider != null)
                meshCollider.sharedMesh = chunkMesh;
        }
        
        void GenerateSimpleMesh(ChunkData chunk)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Color> colors = new List<Color>();
            
            for (int x = 0; x < chunk.Size; x++)
            {
                for (int y = 0; y < chunk.Size; y++)
                {
                    for (int z = 0; z < chunk.Size; z++)
                    {
                        byte block = chunk.GetBlock(x, y, z);
                        if (block == 0) continue;
                        
                        Vector3Int pos = new Vector3Int(x, y, z);
                        
                        // Check each face
                        if (IsFaceVisible(chunk, x, y + 1, z))
                            AddFace(vertices, triangles, uvs, colors, pos, Vector3.up, block);
                        if (IsFaceVisible(chunk, x, y - 1, z))
                            AddFace(vertices, triangles, uvs, colors, pos, Vector3.down, block);
                        if (IsFaceVisible(chunk, x - 1, y, z))
                            AddFace(vertices, triangles, uvs, colors, pos, Vector3.left, block);
                        if (IsFaceVisible(chunk, x + 1, y, z))
                            AddFace(vertices, triangles, uvs, colors, pos, Vector3.right, block);
                        if (IsFaceVisible(chunk, x, y, z + 1))
                            AddFace(vertices, triangles, uvs, colors, pos, Vector3.forward, block);
                        if (IsFaceVisible(chunk, x, y, z - 1))
                            AddFace(vertices, triangles, uvs, colors, pos, Vector3.back, block);
                    }
                }
            }
            
            ApplyMesh(vertices, triangles, uvs, colors);
        }
        
        void GenerateGreedyMesh(ChunkData chunk)
        {
            // Greedy meshing implementation
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Color> colors = new List<Color>();
            
            // Simplified: use simple meshing for now
            GenerateSimpleMesh(chunk);
        }
        
        bool IsFaceVisible(ChunkData chunk, int x, int y, int z)
        {
            if (x < 0 || x >= chunk.Size || y < 0 || y >= chunk.Size || z < 0 || z >= chunk.Size)
                return true; // Chunk boundary
            return chunk.GetBlock(x, y, z) == 0;
        }
        
        void AddFace(List<Vector3> verts, List<int> tris, List<Vector2> uvs, List<Color> cols, 
            Vector3Int pos, Vector3 normal, byte blockType)
        {
            int vi = verts.Count;
            
            // Face vertices based on normal
            if (normal == Vector3.up)
            {
                verts.Add(pos + new Vector3(0, 1, 0));
                verts.Add(pos + new Vector3(1, 1, 0));
                verts.Add(pos + new Vector3(1, 1, 1));
                verts.Add(pos + new Vector3(0, 1, 1));
            }
            else if (normal == Vector3.down)
            {
                verts.Add(pos + new Vector3(0, 0, 1));
                verts.Add(pos + new Vector3(1, 0, 1));
                verts.Add(pos + new Vector3(1, 0, 0));
                verts.Add(pos + new Vector3(0, 0, 0));
            }
            else if (normal == Vector3.left)
            {
                verts.Add(pos + new Vector3(0, 0, 1));
                verts.Add(pos + new Vector3(0, 1, 1));
                verts.Add(pos + new Vector3(0, 1, 0));
                verts.Add(pos + new Vector3(0, 0, 0));
            }
            else if (normal == Vector3.right)
            {
                verts.Add(pos + new Vector3(1, 0, 0));
                verts.Add(pos + new Vector3(1, 1, 0));
                verts.Add(pos + new Vector3(1, 1, 1));
                verts.Add(pos + new Vector3(1, 0, 1));
            }
            else if (normal == Vector3.back)
            {
                verts.Add(pos + new Vector3(1, 0, 0));
                verts.Add(pos + new Vector3(0, 0, 0));
                verts.Add(pos + new Vector3(0, 1, 0));
                verts.Add(pos + new Vector3(1, 1, 0));
            }
            else // forward
            {
                verts.Add(pos + new Vector3(0, 0, 1));
                verts.Add(pos + new Vector3(1, 0, 1));
                verts.Add(pos + new Vector3(1, 1, 1));
                verts.Add(pos + new Vector3(0, 1, 1));
            }
            
            // Triangles
            tris.Add(vi + 0);
            tris.Add(vi + 1);
            tris.Add(vi + 2);
            tris.Add(vi + 0);
            tris.Add(vi + 2);
            tris.Add(vi + 3);
            
            // UVs
            Rect uvRect = GetUVForBlock(blockType);
            uvs.Add(new Vector2(uvRect.x, uvRect.y));
            uvs.Add(new Vector2(uvRect.xMax, uvRect.y));
            uvs.Add(new Vector2(uvRect.xMax, uvRect.yMax));
            uvs.Add(new Vector2(uvRect.x, uvRect.yMax));
            
            // Apply AO color for now
            Color aoColor = CalculateAO(normal, pos);
            for (int i = 0; i < 4; i++)
                cols.Add(aoColor);
        }
        
        Rect GetUVForBlock(byte blockType)
        {
            // Return UV coordinates from atlas
            float uSize = 1f / 16f;
            int texIndex = blockType % 16;
            return new Rect(texIndex * uSize, 0, uSize, uSize);
        }
        
        Color CalculateAO(Vector3 normal, Vector3Int pos)
        {
            // Simplified ambient occlusion
            return Color.white * 0.95f;
        }
        
        void ApplyMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Color> colors)
        {
            chunkMesh.Clear();
            chunkMesh.SetVertices(vertices);
            chunkMesh.SetTriangles(triangles, 0);
            chunkMesh.SetUVs(0, uvs);
            chunkMesh.SetColors(colors);
            chunkMesh.RecalculateNormals();
            chunkMesh.RecalculateBounds();
        }
    }
    
    /// <summary>
    /// Chunk data representation
    /// </summary>
    public class ChunkData
    {
        public const int Size = 32;
        private byte[] blocks;
        
        public ChunkData()
        {
            blocks = new byte[Size * Size * Size];
        }
        
