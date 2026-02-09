// Core game server logic

using System.Collections.Concurrent;
using TerraForgeServer.World;
using TerraForgeServer.Players;

namespace TerraForgeServer.Core;

public class GameServer
{
    private readonly WorldManager _world;
    private readonly ConcurrentDictionary<Guid, PlayerSession> _players = new();
    private readonly ConcurrentDictionary<int, Chunk> _loadedChunks = new();
    
    public int PlayerCount => _players.Count;
    public TimeSpan Uptime => DateTime.UtcNow - _startTime;
    
    private DateTime _startTime;
    private CancellationTokenSource _cts = new();
    
    public GameServer(WorldManager world)
    {
        _world = world;
    }
    
    public async Task StartAsync()
    {
        _startTime = DateTime.UtcNow;
        Console.WriteLine("[GameServer] Starting main loop...");
        
        // Start game tick loop
        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await GameTick();
                await Task.Delay(50, _cts.Token); // 20 ticks per second
            }
        });
        
        // Start auto-save loop
        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(5), _cts.Token);
                await SaveWorldState();
            }
        });
        
        // Start skill tick (hourly like Wurm)
        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(1), _cts.Token);
                await ProcessSkillTicks();
            }
        });
        
        await Task.CompletedTask;
    }
    
    private async Task GameTick()
    {
        // Update all loaded chunks
        foreach (var chunk in _loadedChunks.Values)
        {
            chunk.Update();
        }
        
        // Update player sessions
        foreach (var player in _players.Values)
        {
            if (player.IsConnected)
            {
                await UpdatePlayer(player);
            }
        }
        
        await Task.CompletedTask;
    }
    
    private async Task UpdatePlayer(PlayerSession player)
    {
        // Stamina regeneration
        if (player.Stamina < player.MaxStamina)
        {
            player.Stamina = Math.Min(player.MaxStamina, 
                player.Stamina + player.StaminaRegenPerTick);
        }
        
        // Hunger/thirst drains
        player.Hunger = Math.Max(0, player.Hunger - 0.001);
        player.Thirst = Math.Max(0, player.Thirst - 0.002);
        
        // Check for effects
        if (player.Hunger < 20) player.AddEffect("hungry");
        if (player.Thirst < 10) player.AddEffect("thirsty");
        
        await Task.CompletedTask;
    }
    
    private async Task SaveWorldState()
    {
        Console.WriteLine($"[Save] Saving world state ({_loadedChunks.Count} chunks)...");
        await _world.SaveChunksAsync(_loadedChunks.Values);
    }
    
    private async Task ProcessSkillTicks()
    {
        Console.WriteLine($"[Skills] Processing hourly skill ticks for {_players.Count} players...");
        foreach (var player in _players.Values)
        {
            // Passive skill gains based on actions
            await player.ProcessSkillTick();
        }
    }
    
    public async Task<PlayerSession> PlayerConnectAsync(Guid playerId, string steamId)
    {
        Console.WriteLine($"[Connection] Player {playerId} connecting (Steam: {steamId})");
        
        var player = new PlayerSession(playerId, steamId);
        _players[playerId] = player;
        
        // Load player data from database
        await player.LoadFromDatabaseAsync();
        
        // Load surrounding chunks
        await LoadChunksAroundPlayer(player);
        
        return player;
    }
    
    public async Task PlayerDisconnectAsync(Guid playerId)
    {
        if (_players.TryRemove(playerId, out var player))
        {
            Console.WriteLine($"[Connection] Player {playerId} disconnected");
            await player.SaveToDatabaseAsync();
        }
    }
    
    private async Task LoadChunksAroundPlayer(PlayerSession player)
    {
        int chunkX = (int)player.Position.X >> 4;
        int chunkZ = (int)player.Position.Z >> 4;
        
        for (int x = chunkX - 2; x <= chunkX + 2; x++)
        {
            for (int z = chunkZ - 2; z <= chunkZ + 2; z++)
            {
                var chunk = await _world.LoadChunkAsync(x, z);
                _loadedChunks[chunk.ChunkId] = chunk;
            }
        }
    }
}

public class PlayerSession
{
    public Guid PlayerId { get; }
    public string SteamId { get; }
    public Vector3 Position { get; set; } = new(0, 64, 0);
    public bool IsConnected { get; set; } = true;
    
    // Stats (Wurm-like)
    public double Health { get; set; } = 100;
    public double MaxHealth { get; set; } = 100;
    public double Stamina { get; set; } = 100;
    public double MaxStamina { get; set; } = 100;
    public double StaminaRegenPerTick => 0.5 + (Skills.GetLevel("Body Stamina") * 0.01);
    public double Hunger { get; set; } = 100;
    public double Thirst { get; set; } = 100;
    
    public SkillSystem Skills { get; } = new();
    public Inventory Inventory { get; } = new();
    private List<string> _effects = new();
    
    public PlayerSession(Guid id, string steamId)
    {
        PlayerId = id;
        SteamId = steamId;
    }
    
    public void AddEffect(string effect) => _effects.Add(effect);
    public bool HasEffect(string effect) => _effects.Contains(effect);
    
    public async Task LoadFromDatabaseAsync()
    {
        // TODO: Load from PostgreSQL
        await Task.CompletedTask;
    }
    
    public async Task SaveToDatabaseAsync()
    {
        // TODO: Save to PostgreSQL
        Console.WriteLine($"[Save] Saved player {PlayerId}");
        await Task.CompletedTask;
    }
    
    public async Task ProcessSkillTick()
    {
        await Skills.ProcessTickAsync();
    }
}

public class Vector3
{
    public float X, Y, Z;
    public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
}
