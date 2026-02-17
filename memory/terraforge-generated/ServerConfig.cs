public class ServerConfig {
    public int Port { get; set; } = 7777;
    public string PostgreSQLConnection { get; set; } = "Host=localhost;Database=terraforge;Username=terra;Password=forge";
    public string RedisConnection { get; set; } = "localhost:6379";
    public bool EnableLogging { get; set; } = true;
    public int MaxPlayers { get; set; } = 100;
    public int ViewDistance { get; set; } = 10;
    public int TickRate { get; set; } = 20;
    public int WorldSeed { get; set; } = 12345;
    public bool UseSteamAuth { get; set; } = false;
    public long SteamAppId { get; set; } = 480;
}

public static class GameConstants {
    public const int CHUNK_SIZE = 16;
    public const int CHUNK_HEIGHT = 256;
    public const float BLOCK_SIZE = 1.0f;
    public const int MAX_STACK_SIZE = 64;
    public const float PLAYER_HEIGHT = 1.8f;
    public const float PLAYER_WIDTH = 0.6f;
    public const float WALK_SPEED = 4.3f;
    public const float SPRINT_SPEED = 5.6f;
    public const float JUMP_FORCE = 8.0f;
    public const float GRAVITY = -20.0f;
}
