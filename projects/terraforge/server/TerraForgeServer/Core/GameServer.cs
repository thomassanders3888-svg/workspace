// Core game server logic
using System.Collections.Concurrent;
using System.Collections.Generic;
using TerraForgeServer.World;
using TerraForgeServer.Network;
using TerraForgeServer.Game;

namespace TerraForgeServer.Core;

public class GameServer {
    private readonly ChunkManager _world;
    private readonly ConcurrentDictionary<string, Player> _players = new();
    private readonly ConcurrentDictionary<(int, int), Chunk> _loadedChunks = new();
    private readonly SessionManager _sessions;

    public int PlayerCount => _players.Count;
    public TimeSpan Uptime => DateTime.UtcNow - _startTime;
    private DateTime _startTime;
    private CancellationTokenSource _cts = new();

    // Parameterless constructor for Program.cs
    public GameServer() {
        _sessions = new SessionManager();
        _world = new ChunkManager(12345, "./world", 8);
        _startTime = DateTime.UtcNow;
    }

    // Constructor with SessionManager for dependency injection
    public GameServer(SessionManager sessions) {
        _sessions = sessions;
        _world = new ChunkManager(12345, "./world", 8);
        _startTime = DateTime.UtcNow;
    }

    public async Task InitializeAsync() {
        Console.WriteLine("[GameServer] Initializing...");
        await Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default) {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _startTime = DateTime.UtcNow;
        Console.WriteLine("[GameServer] Starting main loop...");

        // Start game tick loop
        _ = Task.Run(async () => {
            while (!_cts.Token.IsCancellationRequested) {
                await GameTick();
                await Task.Delay(50, _cts.Token); // 20 ticks per second
            }
        }, _cts.Token);

        // Start auto-save loop
        _ = Task.Run(async () => {
            while (!_cts.Token.IsCancellationRequested) {
                await Task.Delay(TimeSpan.FromMinutes(5), _cts.Token);
                await SaveWorldState();
            }
        }, _cts.Token);

        // Start skill tick (hourly like Wurm)
        _ = Task.Run(async () => {
            while (!_cts.Token.IsCancellationRequested) {
                await Task.Delay(TimeSpan.FromHours(1), _cts.Token);
                await ProcessSkillTicks();
            }
        }, _cts.Token);

        await Task.CompletedTask;
    }

    private async Task GameTick() {
        // Update all loaded chunks
        foreach (var chunk in _loadedChunks.Values) {
            chunk.LastAccessed = DateTime.UtcNow;
        }

        // Update player sessions
        foreach (var player in _players.Values) {
            if (player.IsConnected) {
                await UpdatePlayer(player);
            }
        }

        await Task.CompletedTask;
    }

    private async Task UpdatePlayer(Player player) {
        // Stamina regeneration
        if (player.Stamina < player.MaxStamina) {
            player.Stamina = Math.Min(player.MaxStamina, player.Stamina + player.StaminaRegenPerTick);
        }

        // Hunger/thirst drains
        player.Hunger = Math.Max(0, player.Hunger - 0.001);
        player.Thirst = Math.Max(0, player.Thirst - 0.002);

        // Check for effects
        if (player.Hunger < 20) player.AddEffect("hungry");
        if (player.Thirst < 10) player.AddEffect("thirsty");

        await Task.CompletedTask;
    }

    private async Task SaveWorldState() {
        Console.WriteLine($"[Save] Saving world state ({_loadedChunks.Count} chunks)...");
        _world.SaveAll();
        await Task.CompletedTask;
    }

    private async Task ProcessSkillTicks() {
        Console.WriteLine($"[Skills] Processing hourly skill ticks for {_players.Count} players...");
        foreach (var player in _players.Values) {
            await player.ProcessSkillTick();
        }
    }

    public async Task<Player?> PlayerConnectAsync(string playerId) {
        Console.WriteLine($"[Connection] Player {playerId} connecting");

        // Get session from Network layer
        if (!_sessions.TryGetSessionByPlayerId(playerId, out var sessionData) || sessionData == null) {
            Console.WriteLine($"[Connection] No session found for player {playerId}");
            return null;
        }

        var player = new Player(playerId, sessionData);
        _players[playerId] = player;

        // Load player data from database
        await player.LoadFromDatabaseAsync();

        // Load surrounding chunks
        await LoadChunksAroundPlayer(player);

        return player;
    }

    public async Task PlayerDisconnectAsync(string playerId) {
        if (_players.TryRemove(playerId, out var player)) {
            Console.WriteLine($"[Connection] Player {playerId} disconnected");
            await player.SaveToDatabaseAsync();
        }
    }

    private async Task LoadChunksAroundPlayer(Player player) {
        int chunkX = (int)player.Position.X >> 4;
        int chunkZ = (int)player.Position.Z >> 4;

        for (int x = chunkX - 2; x <= chunkX + 2; x++) {
            for (int z = chunkZ - 2; z <= chunkZ + 2; z++) {
                var chunk = _world.GetChunk(x, z);
                _loadedChunks[(chunk.ChunkX, chunk.ChunkZ)] = chunk;
            }
        }

        await Task.CompletedTask;
    }
}

// Player class for game-specific state - references Network.PlayerData for session info
public class Player {
    public string PlayerId { get; }
    public PlayerData SessionData { get; }

    public Vector3 Position {
        get => new(SessionData.X, SessionData.Y, SessionData.Z);
        set {
            SessionData.X = value.X;
            SessionData.Y = value.Y;
            SessionData.Z = value.Z;
        }
    }

    public bool IsConnected => SessionData.IsAuthenticated;

    // Stats (Wurm-like)
    public double Health { get; set; } = 100;
    public double MaxHealth { get; set; } = 100;
    public double Stamina { get; set; } = 100;
    public double MaxStamina { get; set; } = 100;
    public double StaminaRegenPerTick => 0.5 + (Skills.GetSkill(SkillType.Carpentry) * 0.01);
    public double Hunger { get; set; } = 100;
    public double Thirst { get; set; } = 100;

    public SkillSystem Skills { get; } = new();
    public Inventory Inventory { get; } = new();

    private List<string> _effects = new();

    public Player(string playerId, PlayerData sessionData) {
        PlayerId = playerId;
        SessionData = sessionData;
    }

    public void AddEffect(string effect) => _effects.Add(effect);
    public bool HasEffect(string effect) => _effects.Contains(effect);

    public async Task LoadFromDatabaseAsync() {
        // TODO: Load from PostgreSQL
        await Task.CompletedTask;
    }

    public async Task SaveToDatabaseAsync() {
        // TODO: Save to PostgreSQL
        Console.WriteLine($"[Save] Saved player {PlayerId}");
        await Task.CompletedTask;
    }

    public async Task ProcessSkillTick() {
        await Skills.ProcessTickAsync();
    }
}

public class Vector3 {
    public float X, Y, Z;
    public Vector3(float x, float y, float z) {
        X = x; Y = y; Z = z;
    }
}
