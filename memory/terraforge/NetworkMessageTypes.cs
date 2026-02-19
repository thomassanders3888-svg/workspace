using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TerraForge.Networking
{
    /// <summary>
    /// Message types for multiplayer communication
    /// </summary>
    public enum MessageType
    {
        PlayerMove = 1,
        BlockPlace = 2,
        BlockBreak = 3,
        Chat = 4,
        PlayerJoin = 5,
        PlayerLeave = 6,
        PlayerAuth = 7,
        WorldData = 8,
        ChunkData = 9,
        InventorySync = 10,
        PlayerAction = 11,
        ServerTime = 99
    }

    /// <summary>
    /// Base message class for all network messages
    /// </summary>
    public abstract class BaseMessage
    {
        [JsonPropertyName("type")]
        public MessageType Type { get; set; }
        
        [JsonPropertyName("playerId")]
        public string PlayerId { get; set; }
        
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
        
        protected BaseMessage(MessageType type, string playerId)
        {
            Type = type;
            PlayerId = playerId;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        
        public virtual string ToJson()
        {
            return JsonSerializer.Serialize(this, GetType());
        }
    }

    /// <summary>
    /// Player movement message
    /// </summary>
    public class PlayerMoveMessage : BaseMessage
    {
        [JsonPropertyName("position")]
        public Vec3 Position { get; set; }
        
        [JsonPropertyName("rotation")]
        public Vec3 Rotation { get; set; }
        
        [JsonPropertyName("velocity")]
        public Vec3 Velocity { get; set; }
        
        public PlayerMoveMessage(string playerId, Vec3 position, Vec3 rotation) 
            : base(MessageType.PlayerMove, playerId)
        {
            Position = position;
            Rotation = rotation;
            Velocity = new Vec3(0, 0, 0);
        }
    }

    /// <summary>
    /// Block placement message
    /// </summary>
    public class BlockPlaceMessage : BaseMessage
    {
        [JsonPropertyName("blockX")]
        public int BlockX { get; set; }
        
        [JsonPropertyName("blockY")]
        public int BlockY { get; set; }
        
        [JsonPropertyName("blockZ")]
        public int BlockZ { get; set; }
        
        [JsonPropertyName("blockType")]
        public int BlockType { get; set; }
        
        public BlockPlaceMessage(string playerId, int x, int y, int z, int blockType) 
            : base(MessageType.BlockPlace, playerId)
        {
            BlockX = x;
            BlockY = y;
            BlockZ = z;
            BlockType = blockType;
        }
    }

    /// <summary>
    /// Block break message
    /// </summary>
    public class BlockBreakMessage : BaseMessage
    {
        [JsonPropertyName("blockX")]
        public int BlockX { get; set; }
        
        [JsonPropertyName("blockY")]
        public int BlockY { get; set; }
        
        [JsonPropertyName("blockZ")]
        public int BlockZ { get; set; }
        
        [JsonPropertyName("toolTier")]
        public int ToolTier { get; set; }
        
        public BlockBreakMessage(string playerId, int x, int y, int z) 
            : base(MessageType.BlockBreak, playerId)
        {
            BlockX = x;
            BlockY = y;
            BlockZ = z;
            ToolTier = 0;
        }
    }

    /// <summary>
    /// Chat message
    /// </summary>
    public class ChatMessage : BaseMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
        
        [JsonPropertyName("isGlobal")]
        public bool IsGlobal { get; set; }
        
        [JsonPropertyName("recipient")]
        public string Recipient { get; set; }
        
        public ChatMessage(string playerId, string content) 
            : base(MessageType.Chat, playerId)
        {
            Content = content;
            IsGlobal = true;
            Recipient = null;
        }
    }

    /// <summary>
    /// Player join message
    /// </summary>
    public class PlayerJoinMessage : BaseMessage
    {
        [JsonPropertyName("playerName")]
        public string PlayerName { get; set; }
        
        [JsonPropertyName("spawnPosition")]
        public Vec3 SpawnPosition { get; set; }
        
        public PlayerJoinMessage(string playerId, string playerName) 
            : base(MessageType.PlayerJoin, playerId)
        {
            PlayerName = playerName;
            SpawnPosition = new Vec3(0, 100, 0);
        }
    }

    /// <summary>
    /// Player leave message
    /// </summary>
    public class PlayerLeaveMessage : BaseMessage
    {
        [JsonPropertyName("reason")]
        public string Reason { get; set; }
        
        public PlayerLeaveMessage(string playerId, string reason) 
            : base(MessageType.PlayerLeave, playerId)
        {
            Reason = reason ?? "Disconnected";
        }
    }

    /// <summary>
    /// Vector3 for serialization
    /// </summary>
    public struct Vec3
    {
        [JsonPropertyName("x")]
        public float X { get; set; }
        
        [JsonPropertyName("y")]
        public float Y { get; set; }
        
        [JsonPropertyName("z")]
        public float Z { get; set; }
        
        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
    }
}