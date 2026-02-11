using Npgsql;
using System.Text.Json;

namespace TerraForgeServer.Data;

/// <summary>
/// Repository for player data persistence
/// </summary>
public class PlayerRepository
{
    private readonly DatabaseManager _db;
    private readonly JsonSerializerOptions _jsonOptions;

    public PlayerRepository(DatabaseManager db)
    {
        _db = db;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public async Task<PlayerData?> GetByIdAsync(Guid id)
    {
        var row = await _db.QueryRowAsync(
            "SELECT * FROM players WHERE id = @id",
            new[] { new NpgsqlParameter("@id", id) });
        return row.Count > 0 ? MapToPlayerData(row) : null;
    }

    public async Task<PlayerData?> GetByUsernameAsync(string username)
    {
        var row = await _db.QueryRowAsync(
            "SELECT * FROM players WHERE username = @username",
            new[] { new NpgsqlParameter("@username", username) });
        return row.Count > 0 ? MapToPlayerData(row) : null;
    }

    public async Task<PlayerData> CreateAsync(string username, string passwordHash, string? displayName = null)
    {
        var id = Guid.NewGuid();
        var parameters = new NpgsqlParameter[]
        {
            new("@id", id),
            new("@username", username.ToLower()),
            new("@passwordHash", passwordHash),
            new("@displayName", displayName ?? username)
        };

        await _db.ExecuteAsync(
            "INSERT INTO players (id, username, password_hash, display_name) VALUES (@id, @username, @passwordHash, @displayName)",
            parameters);

        return new PlayerData
        {
            Id = id,
            Username = username.ToLower(),
            PasswordHash = passwordHash,
            DisplayName = displayName ?? username
        };
    }

    public async Task<bool> UpdatePositionAsync(Guid playerId, float x, float y, float z)
    {
        return await _db.ExecuteAsync(
            "UPDATE players SET last_position_x = @x, last_position_y = @y, last_position_z = @z WHERE id = @id",
            new[] { new NpgsqlParameter("@id", playerId), new NpgsqlParameter("@x", x), new NpgsqlParameter("@y", y), new NpgsqlParameter("@z", z) }) > 0;
    }

    public async Task<bool> UpdateStatsAsync(Guid playerId, int? health = null, int? hunger = null, int? experience = null)
    {
        var sets = new List<string>();
        var parameters = new List<NpgsqlParameter> { new("@id", playerId) };

        if (health.HasValue) { sets.Add("health = @health"); parameters.Add(new("@health", health.Value)); }
        if (hunger.HasValue) { sets.Add("hunger = @hunger"); parameters.Add(new("@hunger", hunger.Value)); }
        if (experience.HasValue) { sets.Add("experience = @exp"); parameters.Add(new("@exp", experience.Value)); }

        if (sets.Count == 0) return false;
        return await _db.ExecuteAsync($"UPDATE players SET {string.Join(", ", sets)} WHERE id = @id", parameters.ToArray()) > 0;
    }

    public async Task<bool> UpdateInventoryAsync(Guid playerId, List<InventoryItem> inventory)
    {
        var json = JsonSerializer.Serialize(inventory, _jsonOptions);
        return await _db.ExecuteAsync(
            "UPDATE players SET inventory_json = @inv::jsonb WHERE id = @id",
            new[] { new NpgsqlParameter("@id", playerId), new NpgsqlParameter("@inv", json) }) > 0;
    }

    public async Task<List<InventoryItem>> GetInventoryAsync(Guid playerId)
    {
        var row = await _db.QueryRowAsync(
            "SELECT inventory_json FROM players WHERE id = @id",
            new[] { new NpgsqlParameter("@id", playerId) });

        if (!row.ContainsKey("inventory_json") || row["inventory_json"] == null)
            return new List<InventoryItem>();

        var json = row["inventory_json"]?.ToString() ?? "[]";
        return JsonSerializer.Deserialize<List<InventoryItem>>(json, _jsonOptions) ?? new List<InventoryItem>();
    }

    public async Task<bool> SetOnlineStatusAsync(Guid playerId, bool isOnline)
    {
        var sql = isOnline
            ? "UPDATE players SET is_online = @online, last_login = NOW() WHERE id = @id"
            : "UPDATE players SET is_online = @online WHERE id = @id";

        return await _db.ExecuteAsync(sql, new[] { new NpgsqlParameter("@id", playerId), new NpgsqlParameter("@online", isOnline) }) > 0;
    }

    public async Task<List<PlayerData>> GetOnlinePlayersAsync()
    {
        var results = await _db.QueryAsync("SELECT * FROM players WHERE is_online = true");
        return results.Select(MapToPlayerData).ToList();
    }

    public async Task<bool> BanPlayerAsync(Guid playerId, bool banned = true)
    {
        return await _db.ExecuteAsync(
            "UPDATE players SET is_banned = @banned, is_online = false WHERE id = @id",
            new[] { new NpgsqlParameter("@id", playerId), new NpgsqlParameter("@banned", banned) }) > 0;
    }

    public async Task<bool> UpdatePasswordAsync(Guid playerId, string newPasswordHash)
    {
        return await _db.ExecuteAsync(
            "UPDATE players SET password_hash = @hash WHERE id = @id",
            new[] { new NpgsqlParameter("@id", playerId), new NpgsqlParameter("@hash", newPasswordHash) }) > 0;
    }

    public async Task<bool> DeleteAsync(Guid playerId)
    {
        return await _db.ExecuteAsync(
            "DELETE FROM players WHERE id = @id",
            new[] { new NpgsqlParameter("@id", playerId) }) > 0;
    }

    private PlayerData MapToPlayerData(Dictionary<string, object?> row)
    {
        return new PlayerData
        {
            Id = (Guid)(row["id"] ?? Guid.Empty),
            Username = row["username"]?.ToString() ?? "",
            DisplayName = row["display_name"]?.ToString() ?? "",
            PasswordHash = row["password_hash"]?.ToString() ?? "",
            CreatedAt = row["created_at"] is DateTime dt ? dt : DateTime.MinValue,
            LastLogin = row["last_login"] as DateTime?,
            Position = new PlayerPosition
            {
                X = Convert.ToSingle(row["last_position_x"] ?? 0),
                Y = Convert.ToSingle(row["last_position_y"] ?? 100),
                Z = Convert.ToSingle(row["last_position_z"] ?? 0)
            },
            Health = Convert.ToInt32(row["health"] ?? 20),
            Hunger = Convert.ToInt32(row["hunger"] ?? 20),
            Experience = Convert.ToInt32(row["experience"] ?? 0),
            IsOnline = (bool)(row["is_online"] ?? false),
            IsBanned = (bool)(row["is_banned"] ?? false)
        };
    }
}

public class PlayerData
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public string? DisplayName { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public PlayerPosition Position { get; set; } = new();
    public int Health { get; set; } = 20;
    public int Hunger { get; set; } = 20;
    public int Experience { get; set; } = 0;
    public bool IsOnline { get; set; }
    public bool IsBanned { get; set; }
}

public class PlayerPosition
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

public class InventoryItem
{
    public string ItemId { get; set; } = "";
    public int Count { get; set; }
    public int Slot { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
