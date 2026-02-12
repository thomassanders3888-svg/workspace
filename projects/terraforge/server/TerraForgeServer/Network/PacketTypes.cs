// TerraForge - Multiplayer Network Protocol Definitions
// .NET 8 + SignalR style message contracts

namespace TerraForgeServer.Network;

public enum AuthResult { Success, InvalidToken, ServerFull, VersionMismatch }

public enum PacketType : byte
{
    // Connection
    Handshake = 0x00,
    AuthRequest = 0x01,
    AuthResponse = 0x02,
    Disconnect = 0x03,
    Ping = 0x04,
    Pong = 0x05,
    
    // World
    ChunkRequest = 0x10,
    ChunkData = 0x11,
    BlockUpdate = 0x12,
    ChunkUnload = 0x13,
    
    // Entities
    PlayerJoin = 0x20,
    PlayerLeave = 0x21,
    PlayerMove = 0x22,
    PlayerAction = 0x23,
    EntitySpawn = 0x24,
    EntityDespawn = 0x25,
    EntityUpdate = 0x26,
    
    // Chat
    ChatMessage = 0x30,
    SystemMessage = 0x31,
    Whisper = 0x32,
    
    // World State
    TimeSync = 0x40,
    WeatherUpdate = 0x41,
    DifficultyUpdate = 0x42
}


public enum DisconnectReason : byte
{
    ServerShutdown = 0x00,
    Kicked = 0x01,
    Banned = 0x02,
    Timeout = 0x03,
    ProtocolError = 0x04,
    ServerError = 0x05
}

public record PacketHeader(
    PacketType Type,
    int SequenceNumber,
    long Timestamp
);

public record HandshakeRequest(
    int ProtocolVersion,
    string ClientVersion,
    string Platform
);

public record AuthRequestPacket(
    string PlayerId,
    string AuthToken,
    string Username
);

public record AuthResponsePacket(
    AuthResult Result,
    string SessionToken,
    string? ErrorMessage
);

public record DisconnectPacket(
    DisconnectReason Reason,
    string Message
);

public record ChunkRequestPacket(
    int ChunkX,
    int ChunkZ,
    int ChunkY
);

public record ChunkDataPacket(
    int ChunkX,
    int ChunkY,
    int ChunkZ,
    byte[] CompressedVoxels,
    int Checksum
);

public record BlockUpdatePacket(
    long WorldX,
    long WorldY,
    long WorldZ,
    ushort BlockId,
    byte Metadata
);

public record PlayerJoinPacket(
    string PlayerId,
    string Username,
    float X, float Y, float Z,
    float Pitch, float Yaw
);

public record PlayerMovePacket(
    float X, float Y, float Z,
    float Pitch, float Yaw,
    float VelocityX, float VelocityY, float VelocityZ,
    bool OnGround
);

public record PlayerActionPacket(
    int ActionType,
    long TargetX, long TargetY, long TargetZ,
    int Data
);

public record EntityUpdatePacket(
    int EntityId,
    float X, float Y, float Z,
    float Pitch, float Yaw,
    byte[] Metadata
);

public record ChatMessagePacket(
    string Sender,
    string Message,
    int Channel
);

public record WorldStatePacket(
    long WorldTime,
    int Weather,
    float Difficulty
);

public sealed record NetworkPacket
{
    public required PacketHeader Header { get; init; }
    public required byte[] Payload { get; init; }
    public required string SenderId { get; init; }
}
