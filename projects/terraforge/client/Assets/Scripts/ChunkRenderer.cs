using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    [SerializeField] private float blockSize = 1f;
    [SerializeField] private Material terrainMaterial;
    
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Dictionary<Vector2Int, Mesh> chunkMeshes = new Dictionary<Vector2Int, Mesh>();
    
    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (terrainMaterial != null) meshRenderer.material = terrainMaterial;
        
        if (NetworkClient.Instance != null)
            NetworkClient.Instance.OnChunkReceived += ProcessChunk;
    }
    
    private void ProcessChunk(ChunkPacket chunk)
    {
        Mesh mesh = BuildMesh(chunk);
        Vector2Int key = new Vector2Int(chunk.x, chunk.z);
        
        chunkMeshes[key] = mesh;
        meshFilter.mesh = mesh; // Display latest
    }
    
    private Mesh BuildMesh(ChunkPacket chunk)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        int w = Mathf.FloorToInt(Mathf.Sqrt(chunk.data.Length));
        
        for (int x = 0; x < w; x++)
        {
            for (int z = 0; z < w; z++)
            {
                int idx = z * w + x;
                int y = chunk.data[idx];
                if (y <= 0) continue;
                
                Vector3 offset = new Vector3(
                    chunk.x * w * blockSize + x * blockSize,
                    0,
                    chunk.z * w * blockSize + z * blockSize
                );
                
                AddColumn(verts, tris, uvs, offset, y);
            }
        }
        
        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
    
    private void AddColumn(List<Vector3> v, List<int> t, List<Vector2> u, Vector3 pos, int height)
    {
        float h = height * blockSize;
        int baseIdx = v.Count;
        
        // Top face
        v.Add(pos + new Vector3(0, h, 0));
        v.Add(pos + new Vector3(blockSize, h, 0));
        v.Add(pos + new Vector3(blockSize, h, blockSize));
        v.Add(pos + new Vector3(0, h, blockSize));
        AddQuad(t, baseIdx);
        AddUVs(u, 0);
        
        // Sides (simplified - 4 sides)
        baseIdx = v.Count;
        Vector3[] sideNormals = {
            new Vector3(0, 0, -1), new Vector3(1, 0, 0),
            new Vector3(0, 0, 1), new Vector3(-1, 0, 0)
        };
        foreach (var n in sideNormals)
        {
            AddSide(v, t, u, pos, h, n);
        }
    }
    
    private void AddSide(List<Vector3> v, List<int> t, List<Vector2> u, Vector3 p, float h, Vector3 n)
    {
        int bi = v.Count;
        Vector3 up = Vector3.up * h;
        Vector3 right = Vector3.Cross(n, Vector3.up).normalized * blockSize;
        
        if (Mathf.Abs(n.x) > 0.5f)
        {
            // East/West
            float dx = n.x > 0 ? blockSize : 0;
            v.Add(p + new Vector3(dx, 0, 0));
            v.Add(p + new Vector3(dx, h, 0));
            v.Add(p + new Vector3(dx, h, blockSize));
            v.Add(p + new Vector3(dx, 0, blockSize));
        }
        else
        {
            // North/South
            float dz = n.z > 0 ? blockSize : 0;
            v.Add(p + new Vector3(0, 0, dz));
            v.Add(p + new Vector3(blockSize, 0, dz));
            v.Add(p + new Vector3(blockSize, h, dz));
            v.Add(p + new Vector3(0, h, dz));
        }
        AddQuad(t, bi);
        AddUVs(u, 1);
    }
    
    private void AddQuad(List<int> t, int baseIdx)
    {
        t.Add(baseIdx); t.Add(baseIdx + 1); t.Add(baseIdx + 2);
        t.Add(baseIdx); t.Add(baseIdx + 2); t.Add(baseIdx + 3);
    }
    
    private void AddUVs(List<Vector2> u, int texId)
    {
        float u0 = texId * 0.25f;
        float u1 = u0 + 0.25f;
        u.Add(new Vector2(u0, 0)); u.Add(new Vector2(u1, 0));
        u.Add(new Vector2(u1, 1)); u.Add(new Vector2(u0, 1));
    }
    
    private void OnDestroy()
    {
        if (NetworkClient.Instance != null)
            NetworkClient.Instance.OnChunkReceived -= ProcessChunk;
    }
}
