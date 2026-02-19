// TerraForge - Voxel Block Type Definitions
// Generated for 32-block type system

using System;
using System.Collections.Generic;

namespace TerraForge.Voxels
{
    [Flags]
    public enum BlockFlags : byte
    {
        None = 0,
        Solid = 1 << 0,
        Transparent = 1 << 1,
        Emissive = 1 << 2,
        Flammable = 1 << 3,
        GravityAffected = 1 << 4,
        Unbreakable = 1 << 5,
        Liquid = 1 << 6,
        Foliage = 1 << 7
    }

    public enum BlockType : byte
    {
        Air = 0,
        Grass = 1,
        Dirt = 2,
        Stone = 3,
        Cobblestone = 4,
        Sand = 5,
        Gravel = 6,
        Bedrock = 7,
        Water = 8,
        Lava = 9,
        
        // Wood types
        OakWood = 10,
        BirchWood = 11,
        SpruceWood = 12,
        JungleWood = 13,
        
        // Leaves
        OakLeaves = 14,
        BirchLeaves = 15,
        
        // Ores
        CoalOre = 16,
        IronOre = 17,
        GoldOre = 18,
        DiamondOre = 19,
        RedstoneOre = 20,
        LapisOre = 21,
        EmeraldOre = 22,
        
        // Building blocks
        Planks = 23,
        Glass = 24,
        Bricks = 25,
        StoneBricks = 26,
        
        // Natural
        Clay = 27,
        Snow = 28,
        Ice = 29,
        Sandstone = 30,
        
        // Special
        Obsidian = 31
    }

    public readonly struct BlockProperties
    {
        public readonly BlockFlags Flags;
        public readonly float Hardness;
        public readonly int TextureIndex;  // Index into texture atlas
        public readonly byte LightLevel; // 0-15, for emissive blocks
        public readonly ushort Durability;
        
        public bool IsSolid => (Flags & BlockFlags.Solid) != 0;
        public bool IsTransparent => (Flags & BlockFlags.Transparent) != 0;
        public bool IsEmissive => (Flags & BlockFlags.Emissive) != 0;
        public bool IsFlammable => (Flags & BlockFlags.Flammable) != 0;
        public bool IsUnbreakable => (Flags & BlockFlags.Unbreakable) != 0;
        public bool IsLiquid => (Flags & BlockFlags.Liquid) != 0;
        public bool IsGravityAffected => (Flags & BlockFlags.GravityAffected) != 0;
        public bool IsFoliage => (Flags & BlockFlags.Foliage) != 0;

        public BlockProperties(BlockFlags flags, float hardness, int textureIndex, 
            byte lightLevel = 0, ushort durability = 100)
        {
            Flags = flags;
            Hardness = hardness;
            TextureIndex = textureIndex;
            LightLevel = lightLevel;
            Durability = durability;
        }

        public static BlockProperties Default => new(
            BlockFlags.Solid, 1.0f, 0, 0, 100);
    }

    public static class BlockRegistry
    {
        private static readonly Dictionary<BlockType, BlockProperties> _registry = new();
        
        static BlockRegistry()
        {
            InitializeDefaults();
        }

        private static void InitializeDefaults()
        {
            // Air
            Register(BlockType.Air, new BlockProperties(
                BlockFlags.Transparent, 0f, 0));

            // Grass
            Register(BlockType.Grass, new BlockProperties(
                BlockFlags.Solid, 0.6f, 1));

            // Dirt
            Register(BlockType.Dirt, new BlockProperties(
                BlockFlags.Solid, 0.5f, 2));

            // Stone
            Register(BlockType.Stone, new BlockProperties(
                BlockFlags.Solid, 1.5f, 3));

            // Cobblestone
            Register(BlockType.Cobblestone, new BlockProperties(
                BlockFlags.Solid, 2.0f, 4));

            // Sand
            Register(BlockType.Sand, new BlockProperties(
                BlockFlags.Solid | BlockFlags.GravityAffected, 0.5f, 5));

            // Gravel
            Register(BlockType.Gravel, new BlockProperties(
                BlockFlags.Solid | BlockFlags.GravityAffected, 0.6f, 6));

            // Bedrock
            Register(BlockType.Bedrock, new BlockProperties(
                BlockFlags.Solid | BlockFlags.Unbreakable, -1f, 7, 0, 0));

            // Water
            Register(BlockType.Water, new BlockProperties(
                BlockFlags.Transparent | BlockFlags.Liquid, 100f, 8));

            // Lava
            Register(BlockType.Lava, new BlockProperties(
                BlockFlags.Transparent | BlockFlags.Liquid | BlockFlags.Emissive, 100f, 9, 15));

            // Oak Wood
            Register(BlockType.OakWood, new BlockProperties(
                BlockFlags.Solid | BlockFlags.Flammable, 2.0f, 10));

            // Birch Wood
            Register(BlockType.BirchWood, new BlockProperties(
                BlockFlags.Solid | BlockFlags.Flammable, 2.0f, 11));

            // Spruce Wood
            Register(BlockType.SpruceWood, new BlockProperties(
                BlockFlags.Solid | BlockFlags.Flammable, 2.0f, 12));

            // Jungle Wood
            Register(BlockType.JungleWood, new BlockProperties(
                BlockFlags.Solid | BlockFlags.Flammable, 2.0f, 13));

            // Oak Leaves
            Register(BlockType.OakLeaves, new BlockProperties(
                BlockFlags.Foliage | BlockFlags.Transparent | BlockFlags.Flammable, 0.2f, 14));

            // Birch Leaves
            Register(BlockType.BirchLeaves, new BlockProperties(
                BlockFlags.Foliage | BlockFlags.Transparent | BlockFlags.Flammable, 0.2f, 15));

            // Coal Ore
            Register(BlockType.CoalOre, new BlockProperties(
                BlockFlags.Solid, 3.0f, 16));

            // Iron Ore
            Register(BlockType.IronOre, new BlockProperties(
                BlockFlags.Solid, 3.0f, 17));

            // Gold Ore
            Register(BlockType.GoldOre, new BlockProperties(
                BlockFlags.Solid, 3.0f, 18));

            // Diamond Ore
            Register(BlockType.DiamondOre, new BlockProperties(
                BlockFlags.Solid, 3.0f, 19));

            // Redstone Ore
            Register(BlockType.RedstoneOre, new BlockProperties(
                BlockFlags.Solid | BlockFlags.Emissive, 3.0f, 20, 9));

            // Lapis Ore
            Register(BlockType.LapisOre, new BlockProperties(
                BlockFlags.Solid, 3.0f, 21));

            // Emerald Ore
            Register(BlockType.EmeraldOre, new BlockProperties(
                BlockFlags.Solid, 3.0f, 22));

            // Planks
            Register(BlockType.Planks, new BlockProperties(
                BlockFlags.Solid | BlockFlags.Flammable, 2.0f, 23));

            // Glass
            Register(BlockType.Glass, new BlockProperties(
                BlockFlags.Solid | BlockFlags.Transparent, 0.3f, 24));

            // Bricks
            Register(BlockType.Bricks, new BlockProperties(
                BlockFlags.Solid, 2.0f, 25));

            // Stone Bricks
            Register(BlockType.StoneBricks, new BlockProperties(
                BlockFlags.Solid, 1.5f, 26));

            // Clay
            Register(BlockType.Clay, new BlockProperties(
                BlockFlags.Solid, 0.6f, 27));

            // Snow
            Register(BlockType.Snow, new BlockProperties(
                BlockFlags.Transparent, 0.1f, 28));

            // Ice
            Register(BlockType.Ice, new BlockProperties(
                BlockFlags.Solid | BlockFlags.Transparent, 0.5f, 29));

            // Sandstone
            Register(BlockType.Sandstone, new BlockProperties(
                BlockFlags.Solid, 0.8f, 30));

            // Obsidian
            Register(BlockType.Obsidian, new BlockProperties(
                BlockFlags.Solid, 50.0f, 31, 0, 1000));
        }

        public static void Register(BlockType type, BlockProperties properties)
        {
            _registry[type] = properties;
        }

        public static BlockProperties GetProperties(BlockType type)
        {
            return _registry.TryGetValue(type, out var props) ? props : BlockProperties.Default;
        }

        public static bool TryGetProperties(BlockType type, out BlockProperties properties)
        {
            return _registry.TryGetValue(type, out properties);
        }

        public static bool IsSolid(BlockType type)
        {
            return GetProperties(type).IsSolid;
        }

        public static bool IsTransparent(BlockType type)
        {
            return GetProperties(type).IsTransparent;
        }

        public static float GetHardness(BlockType type)
        {
            return GetProperties(type).Hardness;
        }

        public static int GetTextureIndex(BlockType type)
        {
            return GetProperties(type).TextureIndex;
        }

        public static byte GetLightLevel(BlockType type)
        {
            return GetProperties(type).LightLevel;
        }

        public static IEnumerable<BlockType> GetAllBlockTypes()
        {
            return _registry.Keys;
        }

        public static bool IsOpaque(BlockType type)
        {
            return !IsTransparent(type);
        }
    }
}