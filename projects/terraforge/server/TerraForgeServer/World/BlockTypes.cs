using System;

namespace TerraForgeServer.World
{
    /// <summary>
    /// Block type enumeration for all terrain blocks
    /// </summary>
    public enum BlockType : byte
    {
        Air = 0,        // Empty space - not stored in chunks
        Grass = 1,
        Dirt = 2,
        Stone = 3,
        OreIron = 4,
        OreCoal = 5,
        OreGold = 6,
        OreDiamond = 7,
        Sand = 8,
        Gravel = 9,
        Bedrock = 10,
        Water = 11,
        Wood = 12,
        Leaves = 13,
        Snow = 14,
        Clay = 15
    }

    /// <summary>
    /// Block properties for gameplay mechanics
    /// </summary>
    public readonly struct BlockProperties
    {
        public readonly BlockType Type;
        public readonly float Hardness;
        public readonly bool Harvestable;
        public readonly byte RequiredToolLevel;
        public readonly bool IsSolid;
        public readonly bool IsTransparent;
        public readonly byte LightEmission;

        public BlockProperties(BlockType type, float hardness, bool harvestable, 
            byte requiredToolLevel, bool isSolid, bool isTransparent, byte lightEmission = 0)
        {
            Type = type;
            Hardness = hardness;
            Harvestable = harvestable;
            RequiredToolLevel = requiredToolLevel;
            IsSolid = isSolid;
            IsTransparent = isTransparent;
            LightEmission = lightEmission;
        }
    }

    /// <summary>
    /// Static registry for block properties
    /// </summary>
    public static class BlockRegistry
    {
        private static readonly BlockProperties[] Properties = new BlockProperties[256];
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;

            // Define properties for each block type
            Register(BlockType.Air,         new BlockProperties(BlockType.Air, 0, false, 0, false, true));
            Register(BlockType.Grass,         new BlockProperties(BlockType.Grass, 0.6f, true, 0, true, false));
            Register(BlockType.Dirt,          new BlockProperties(BlockType.Dirt, 0.5f, true, 0, true, false));
            Register(BlockType.Stone,         new BlockProperties(BlockType.Stone, 1.5f, true, 1, true, false));
            Register(BlockType.OreIron,       new BlockProperties(BlockType.OreIron, 3.0f, true, 2, true, false));
            Register(BlockType.OreCoal,       new BlockProperties(BlockType.OreCoal, 3.0f, true, 1, true, false));
            Register(BlockType.OreGold,       new BlockProperties(BlockType.OreGold, 3.0f, true, 3, true, false));
            Register(BlockType.OreDiamond,    new BlockProperties(BlockType.OreDiamond, 3.0f, true, 3, true, false));
            Register(BlockType.Sand,          new BlockProperties(BlockType.Sand, 0.5f, true, 0, true, false));
            Register(BlockType.Gravel,        new BlockProperties(BlockType.Gravel, 0.6f, true, 0, true, false));
            Register(BlockType.Bedrock,       new BlockProperties(BlockType.Bedrock, -1, false, 255, true, false));
            Register(BlockType.Water,         new BlockProperties(BlockType.Water, 0, false, 0, false, true));
            Register(BlockType.Wood,          new BlockProperties(BlockType.Wood, 2.0f, true, 0, true, false));
            Register(BlockType.Leaves,        new BlockProperties(BlockType.Leaves, 0.2f, true, 0, true, true));
            Register(BlockType.Snow,          new BlockProperties(BlockType.Snow, 0.1f, true, 0, true, true));
            Register(BlockType.Clay,          new BlockProperties(BlockType.Clay, 0.6f, true, 0, true, false));

            _initialized = true;
        }

        private static void Register(BlockType type, BlockProperties props)
        {
            Properties[(byte)type] = props;
        }

        public static ref readonly BlockProperties Get(BlockType type)
        {
            if (!_initialized) Initialize();
            return ref Properties[(byte)type];
        }

        public static bool IsAir(BlockType type) => type == BlockType.Air;
        public static bool IsSolid(BlockType type) => Get(type).IsSolid;
    }
}
