using System;
using System.Threading.Tasks;
using System.Security.Cryptography;

public class Authentication {
    private readonly IDatabase _db;
    
    public Authentication(IDatabase db) {
        _db = db;
    }
    
    public async Task<AuthResult> Register(string username, string password, string email) {
        if (await _db.UserExists(username)) {
            return new AuthResult { Success = false, Error = "Username already exists" };
        }
        
        var (hash, salt) = HashPassword(password);
        
        var user = new User {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = hash,
            Salt = salt,
            CreatedAt = DateTime.UtcNow,
            IsEmailConfirmed = false
        };
        
        await _db.CreateUser(user);
        
        return new AuthResult {
            Success = true,
            UserId = user.Id,
            Token = GenerateToken(user.Id)
        };
    }
    
    public async Task<AuthResult> Login(string username, string password) {
        var user = await _db.GetUserByUsername(username);
        if (user == null) {
            return new AuthResult { Success = false, Error = "Invalid credentials" };
        }
        
        if (!VerifyPassword(password, user.PasswordHash, user.Salt)) {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5) {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            }
            await _db.UpdateUser(user);
            return new AuthResult { Success = false, Error = "Invalid credentials" };
        }
        
        user.LastLogin = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        await _db.UpdateUser(user);
        
        return new AuthResult {
            Success = true,
            UserId = user.Id,
            Token = GenerateToken(user.Id)
        };
    }
    
    public async Task<bool> ValidateToken(string token) {
        // Check JWT or session token
        return await _db.ValidateSessionToken(token);
    }
    
    (string hash, string salt) HashPassword(string password) {
        byte[] salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);
        
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }
    
    bool VerifyPassword(string password, string hash, string salt) {
        var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256);
        byte[] computedHash = pbkdf2.GetBytes(32);
        return Convert.ToBase64String(computedHash) == hash;
    }
    
    string GenerateToken(Guid userId) {
        // Generate JWT or similar
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}

public class AuthResult {
    public bool Success { get; set; }
    public Guid? UserId { get; set; }
    public string Token { get; set; }
    public string Error { get; set; }
}

public class User {
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Salt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
}

public interface IDatabase {
    Task<bool> UserExists(string username);
    Task<User> GetUserByUsername(string username);
    Task CreateUser(User user);
    Task UpdateUser(User user);
    Task<bool> ValidateSessionToken(string token);
}
