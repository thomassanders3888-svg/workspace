// NetworkMessageTypes.cs
// TerraForge Multiplayer Network Messages
// Defines all message types for client-server communication

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TerraForge.Network
{
    /// <summary>
    /// Enumeration of all network message types
    /// </summary>
    public enum MessageType
    {
        PlayerMove,
        BlockPlace,
        BlockBreak,
        Chat,
        PlayerJoin,
        PlayerLeave
    }

    /// <summary>
    /// Base class for all network messages
    /// </summary>
    public abstract class BaseMessage
    {
        /// <summary>
        /// The type of message for deserialization routing
        /// </summary>
        [JsonPropertyName("type")]
        public MessageType Type { get; set; }

        /// <summary>
        /// Timestamp when the message was created
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Unique identifier for the sender
        /// </summary>
        [JsonPropertyName("playerId")]
        public string PlayerId { get; set; }

        /// <summary>
        /// Client-generated sequence number for ordering
        /// </summary>
        [JsonPropertyName("sequenceNumber")]
        public long SequenceNumber { get; set; }

        protected BaseMessage(MessageType type, string playerId)
        {
            Type = type;
            PlayerId = playerId;
            Timestamp = DateTime.UtcNow;
            SequenceNumber = 0;
        }

        /// <summary>
        /// Serialize the message to JSON
        /// </summary>
        public virtual string ToJson()
        {
            return JsonSerializer.Serialize(this, GetType(), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Deserialize a JSON string to the appropriate message type
        /// </summary>
        public static BaseMessage FromJson(string json)
        {
            // First deserialize to get the type
            var baseMessage = JsonSerializer.Deserialize<JsonElement>(json);
            
            if (!baseMessage.TryGetProperty("type", out var typeElement))
            {
                throw new ArgumentException("Message JSON does not contain a 'type' field");
            }

            var typeString = typeElement.GetString();
            if (!Enum.TryParse<MessageType>(typeString, out var messageType))
            {
                throw new ArgumentException($"Unknown message type: {typeString}");
            }

            // Now deserialize to the specific type
            return messageType switch
            {
                MessageType.PlayerMove => JsonSerializer.Deserialize<PlayerMoveMessage>(json),
                MessageType.BlockPlace => JsonSerializer.Deserialize<BlockPlaceMessage>(json),
                MessageType.BlockBreak => JsonSerializer.Deserialize<BlockBreakMessage>(json),
                MessageType.Chat => JsonSerializer.Deserialize<ChatMessage>(json),
                MessageType.PlayerJoin => JsonSerializer.Deserialize<PlayerJoinMessage>(json),
                MessageType.PlayerLeave => JsonSerializer.Deserialize<PlayerLeaveMessage>(json),
                _ => throw new ArgumentException($"Unimplemented message type: {messageType}")
            };
        }
    }

    /// <summary>
    /// Player position/movement update
    /// </summary>
    public class PlayerMoveMessage : BaseMessage
    {
        [JsonPropertyName("x")]
        public float X { get; set; }

        [JsonPropertyName("y")]
        public float Y { get; set; }

        [JsonPropertyName("z")]
        public float Z { get; set; }

        [JsonPropertyName("yaw")]
        public float Yaw { get; set; }

        [JsonPropertyName("pitch")]
        public float Pitch { get; set; }

        [JsonPropertyName("velocityX")]
        public float VelocityX { get; set; }

        [JsonPropertyName("velocityY")]
        public float VelocityY { get; set; }

        [JsonPropertyName("velocityZ")]
        public float VelocityZ { get; set; }

        [JsonPropertyName("isOnGround")]
        public bool IsOnGround { get; set; }

        public PlayerMoveMessage(string playerId, float x, float y, float z, float yaw, float pitch) 
            : base(MessageType.PlayerMove, playerId)
        {
            X = x;
            Y = y;
            Z = z;
            Yaw = yaw;
            Pitch = pitch;
            VelocityX = 0;
            VelocityY = 0;
            VelocityZ = 0;
            IsOnGround = true;
        }

        public PlayerMoveMessage() : base(MessageType.PlayerMove, string.Empty) { }
    }

    /// <summary>
    /// Block placement event
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

        [JsonPropertyName("blockMetadata")]
        public int BlockMetadata { get; set; }

        [JsonPropertyName("toolUsed")]
        public string ToolUsed { get; set; }

        public BlockPlaceMessage(string playerId, int blockX, int blockY, int blockZ, int blockType) 
            : base(MessageType.BlockPlace, playerId)
        {
            BlockX = blockX;
            BlockY = blockY;
            BlockZ = blockZ;
            BlockType = blockType;
            BlockMetadata = 0;
            ToolUsed = null;
        }

        public BlockPlaceMessage() : base(MessageType.BlockPlace, string.Empty) { }
    }

    /// <summary>
    /// Block break/mining event
    /// </summary>
    public class BlockBreakMessage : BaseMessage
    {
        [JsonPropertyName("blockX")]
        public int BlockX { get; set; }

        [JsonPropertyName("blockY")]
        public int BlockY { get; set; }

        [JsonPropertyName("blockZ")]
        public int BlockZ { get; set; }

        [JsonPropertyName("blockType")]
        public int BlockType { get; set; }

        [JsonPropertyName("mineDuration")]
        public float MineDuration { get; set; }

        [JsonPropertyName("dropsItems")]
        public bool DropsItems { get; set; }

        [JsonPropertyName("toolUsed")]
        public string ToolUsed { get; set; }

        public BlockBreakMessage(string playerId, int blockX, int blockY, int blockZ, int blockType) 
            : base(MessageType.BlockBreak, playerId)
        {
            BlockX = blockX;
            BlockY = blockY;
            BlockZ = blockZ;
            BlockType = blockType;
            MineDuration = 0;
            DropsItems = true;
            ToolUsed = null;
        }

        public BlockBreakMessage() : base(MessageType.BlockBreak, string.Empty) { }
    }

    /// <summary>
    /// Chat message
    /// </summary>
    public class ChatMessage : BaseMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("recipientType")]
        public string RecipientType { get; set; }

        [JsonPropertyName("targetPlayerId")]
        public string TargetPlayerId { get; set; }

        [JsonPropertyName("isCommand")]
        public bool IsCommand { get; set; }

        public ChatMessage(string playerId, string content) 
            : base(MessageType.Chat, playerId)
        {
