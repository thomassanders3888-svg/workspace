using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

public class AuthenticationService {
    private readonly TerraForgeDbContext _db;
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(24);
    
    public AuthenticationService(TerraForgeDbContext db) {
        _db = db;
    }
    
    public async Task<AuthResult> Login(string username, string password, string clientVersion) {
        var player = await _db.Players.FindAsync(username);
        
        if (player == null) {
            return AuthResult.Failed("Player not found");
        }
        
        if (!VerifyPassword(password, player.PasswordHash)) {
            player.FailedLoginAttempts++;
            await _db.SaveChangesAsync();
            return AuthResult.Failed("Invalid password");
        }
        
        if (player.IsBanned) {
            return AuthResult.Failed($"Banned until {player.BanExpiry}. Reason: {player.BanReason}");
        }
        
        if (player.ClientVersionRequired != null && player.ClientVersionRequired != clientVersion) {
            return AuthResult.VersionMismatchRequired(player.ClientVersionRequired);
        }
        
        var token = GenerateSessionToken(player.Id);
        var session = new PlayerSession {
            Token = token,
            PlayerId = player.Id,
            ExpiresAt = DateTime.UtcNow.Add(_sessionTimeout),
            IpAddress = "127.0.0.1"
        };
        
        player.LastLogin = DateTime.UtcNow;
        player.FailedLoginAttempts = 0;
        await _db.SaveChangesAsync();
        
        return AuthResult.Success(token, player);
    }
    
    public async Task<AuthResult> Register(string username, string password, string email) {
        if (await _db.Players.FindAsync(username) != null) {
            return AuthResult.Failed("Username already taken");
        }
        
        var player = new PlayerData {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = HashPassword(password),
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        
        await _db.Players.AddAsync(player);
        await _db.SaveChangesAsync();
        
        return AuthResult.Success("registered", player);
    }
    
    public async Task<bool> ValidateSession(string token) {
        var session = await _db.PlayerSessions.FindAsync(token);
        return session != null && session.ExpiresAt > DateTime.UtcNow;
    }
    
    public async Task<bool> Logout(string token) {
        var session = await _db.PlayerSessions.FindAsync(token);
        if (session != null) {
            _db.PlayerSessions.Remove(session);
            await _db.SaveChangesAsync();
        }
        return true;
    }
    
    string HashPassword(string password) {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "TerraForgeSalt"));
        return Convert.ToBase64String(bytes);
    }
    
    bool VerifyPassword(string password, string hash) {
        return HashPassword(password) == hash;
    }
    
    string GenerateSessionToken(Guid playerId) {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(playerId.ToByteArray());
    }
}

public class AuthResult {
    public bool Success { get; set; }
    public string Token { get; set; }
    public PlayerData Player { get; set; }
    public string Error { get; set; }
    public bool VersionMismatch { get; set; }
    public string RequiredVersion { get; set; }
    
    public static AuthResult Success(string token, PlayerData player) => 
        new() { Success = true, Token = token, Player = player };
    
    public static AuthResult Failed(string error) => 
        new() { Success = false, Error = error };
    
    public static AuthResult VersionMismatchRequired(string required) =>
        new() { Success = false, VersionMismatch = true, RequiredVersion = required };
}

public class PlayerSession {
    public string Token { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string IpAddress { get; set; }
    public bool IsActive { get; set; }
}

public static class PlayerExtensions {
    public static bool IsExpired(this PlayerSession session) => session.ExpiresAt < DateTime.UtcNow;
}
