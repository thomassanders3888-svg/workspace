// TerraForge - Player Session Management
// Player data, authentication, and session state

using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace TerraForgeServer.Network;

public sealed record PlayerData
{
    public required string PlayerId { get; init; }
    public required string Username { get; init; }
    public required string ConnectionId { get; init; }
    public DateTime ConnectedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    
    // Position
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float Pitch { get; set; }
    public float Yaw { get; set; }
    
    // State
    public bool IsAuthenticated { get; set; }
    public bool IsAdmin { get; set; }
    public int ViewDistance { get; set; } = 8;
    public string? EntityId { get; set; }
    
    // Stats
    public long BytesSent { get; set; }
    public long BytesReceived { get; set; }
    public int PingMs { get; set; }
    
    // Loaded chunks
    public HashSet<(int x, int y, int z)> LoadedChunks { get; } = new();
}

public sealed class SessionManager
{
    private readonly ConcurrentDictionary<string, PlayerData> _sessions = new();
    private readonly ConcurrentDictionary<string, string> _playerIdToConnection = new();
    private readonly TimeSpan _timeoutThreshold = TimeSpan.FromMinutes(5);
    
    public IReadOnlyCollection<PlayerData> ActiveSessions => _sessions.Values.ToList();
    
    public int Count => _sessions.Count;
    
    public event EventHandler<PlayerData>? OnPlayerConnected;
    public event EventHandler<PlayerData>? OnPlayerDisconnected;
    public event EventHandler<PlayerData>? OnPlayerAuthenticated;
    
    public string CreateConnectionId() => 
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
    
    public bool TryAddSession(string connectionId, PlayerData session)
    {
        if (!_sessions.TryAdd(connectionId, session))
            return false;
            
        _playerIdToConnection[session.PlayerId] = connectionId;
        OnPlayerConnected?.Invoke(this, session);
        return true;
    }
    
    public bool TryGetSession(string connectionId, out PlayerData? session) =>
        _sessions.TryGetValue(connectionId, out session!);
    
    public bool TryGetSessionByPlayerId(string playerId, out PlayerData? session)
    {
        session = null;
        if (!_playerIdToConnection.TryGetValue(playerId, out var connId))
            return false;
        return _sessions.TryGetValue(connId, out session);
    }
    
    public bool RemoveSession(string connectionId)
    {
        if (!_sessions.TryRemove(connectionId, out var session))
            return false;
            
        _playerIdToConnection.TryRemove(session.PlayerId, out _);
        OnPlayerDisconnected?.Invoke(this, session);
        return true;
    }
    
    public void UpdateActivity(string connectionId)
    {
        if (_sessions.TryGetValue(connectionId, out var session))
            session.LastActivity = DateTime.UtcNow;
    }
    
    public void UpdatePing(string connectionId, int pingMs)
    {
        if (_sessions.TryGetValue(connectionId, out var session))
            session.PingMs = pingMs;
    }
    
    public void Authenticate(string connectionId, string username, bool isAdmin = false)
    {
        if (!_sessions.TryGetValue(connectionId, out var session))
            return;
            
        session.IsAuthenticated = true;
        session.IsAdmin = isAdmin;
        OnPlayerAuthenticated?.Invoke(this, session);
    }
    
    public IEnumerable<PlayerData> GetSessionsNearChunk(int cx, int cy, int cz, int radius)
    {
        return _sessions.Values.Where(s => 
            s.LoadedChunks.Contains((cx, cy, cz)) ||
            s.LoadedChunks.Any(lc => 
                Math.Abs(lc.x - cx) <= radius &&
                Math.Abs(lc.y - cy) <= radius &&
                Math.Abs(lc.z - cz) <= radius));
    }
    
    public IEnumerable<PlayerData> GetTimedOutSessions()
    {
        var cutoff = DateTime.UtcNow.Subtract(_timeoutThreshold);
        return _sessions.Values.Where(s => s.LastActivity < cutoff);
    }
    
    public void CleanupDeadSessions()
    {
        foreach (var session in GetTimedOutSessions())
        {
            if (_playerIdToConnection.TryGetValue(session.PlayerId, out var connId))
                RemoveSession(connId);
        }
    }
    
    public void TrackChunkLoad(string connectionId, int x, int y, int z)
    {
        if (_sessions.TryGetValue(connectionId, out var session))
            session.LoadedChunks.Add((x, y, z));
    }
    
    public void TrackChunkUnload(string connectionId, int x, int y, int z)
    {
        if (_sessions.TryGetValue(connectionId, out var session))
            session.LoadedChunks.Remove((x, y, z));
    }
    
    public bool ValidateAuth(string token) => 
        !string.IsNullOrEmpty(token) && token.Length >= 32;
}

public sealed class AuthService
{
    private readonly SessionManager _sessions;
    private static readonly int ProtocolVersion = 1;
    
    public AuthService(SessionManager sessions) => _sessions = sessions;
    
    public AuthResult Authenticate(
        string connectionId, 
        string playerId,
        string authToken,
        string username,
        int clientProtocolVersion,
        out string? error)
    {
        error = null;
        
        if (clientProtocolVersion != ProtocolVersion)
        {
            error = $"Protocol version mismatch: server {ProtocolVersion}, client {clientProtocolVersion}";
            return AuthResult.VersionMismatch;
        }
        
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 16)
        {
            error = "Invalid username length (3-16 chars required)";
            return AuthResult.InvalidToken;
        }
        
        if (!_sessions.ValidateAuth(authToken))
        {
            error = "Invalid authentication token";
            return AuthResult.InvalidToken;
        }
        
        if (_sessions.Count >= 100)
        {
            error = "Server is full";
            return AuthResult.ServerFull;
        }
        
        _sessions.Authenticate(connectionId, username);
        return AuthResult.Success;
    }
    
    public string GenerateSessionToken() => 
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
}
