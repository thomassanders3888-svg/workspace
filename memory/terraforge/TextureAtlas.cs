using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerraForge.Rendering
{
    /// <summary>
    /// Texture atlas for packing block textures efficiently
    /// </summary>
    public class TextureAtlas : MonoBehaviour
    {
        public static TextureAtlas Instance { get; private set; }
        
        [Header("Atlas Settings")]
        public int atlasSize = 4096;
        public int maxTextureSize = 256;
        public FilterMode filterMode = FilterMode.Point;
        
        [Header("Textures")]
        public List<BlockTexture> blockTextures;
        
        private Texture2D atlasTexture;
        private Dictionary<BlockType, Rect> uvRects = new Dictionary<BlockType, Rect>();
        private bool isGenerated = false;
        
        void Awake()
        {
            Instance = this;
            GenerateAtlas();
        }
        
        void GenerateAtlas()
        {
            if (isGenerated) return;
            
            // Create atlas texture
            atlasTexture = new Texture2D(atlasSize, atlasSize, TextureFormat.RGBA32, false);
            atlasTexture.filterMode = filterMode;
            atlasTexture.wrapMode = TextureWrapMode.Clamp;
            
            // Simple grid packing (16x16 textures per atlas)
            int texturesPerRow = atlasSize / maxTextureSize;
            int index = 0;
            
            foreach (var blockTex in blockTextures)
            {
                if (blockTex.texture != null)
                {
                    int row = index / texturesPerRow;
                    int col = index % texturesPerRow;
                    
                    float uvSize = (float)maxTextureSize / atlasSize;
                    float u = col * uvSize;
                    float v = row * uvSize;
                    
                    uvRects[blockTex.blockType] = new Rect(u, v, uvSize, uvSize);
                    
                    // Copy texture data (simplified - copy whole texture)
                    Color[] pixels = blockTex.texture.GetPixels();
                    atlasTexture.SetPixels(col * maxTextureSize, row * maxTextureSize, 
                        maxTextureSize, maxTextureSize, pixels);
                    
                    index++;
                }
            }
            
            atlasTexture.Apply();
            isGenerated = true;
        }
        
        /// <summary>
        /// Get UV coordinates for block type
        /// </summary>
        public Rect GetUVRect(BlockType blockType)
        {
            if (uvRects.TryGetValue(blockType, out Rect rect))
                return rect;
            return new Rect(0, 0, 0.0625f, 0.0625f); // Default stone
        }
        
        /// <summary>
        /// Get the atlas texture
        /// </summary>
        public Texture2D GetAtlas()
        {
            return atlasTexture;
        }
        
        /// <summary>
        /// Get UVs for all 6 faces of a cube (simplified - same texture all faces)
        /// </summary>
        public Vector2[] GetUVs(BlockType blockType)
        {
            Rect r = GetUVRect(blockType);
            return new Vector2[]
            {
                new Vector2(r.x, r.y),
                new Vector2(r.xMax, r.y),
                new Vector2(r.x, r.yMax),
                new Vector2(r.xMax, r.yMax)
            };
        }
    }
    
    [System.Serializable]
    public class BlockTexture
    {
        public BlockType blockType;
        public string textureName;
        public Texture2D texture;
    }
}