using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QM_AI.API.Data;
using QM_AI.API.DTOs.Auth;
using QM_AI.API.Helpers;
using QM_AI.API.Models;

namespace QM_AI.API.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private static readonly ConcurrentDictionary<string, (int UserId, DateTime ExpiresAt)> _refreshTokens = new();

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var expirationHours = int.Parse(jwtSettings["ExpirationHours"] ?? "24");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role?.Name ?? "User"),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrEmpty(user.DisplayName))
            claims.Add(new Claim("displayName", user.DisplayName));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(expirationHours),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = JwtHelper.GetSigningCredentials(secret)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = JwtHelper.GetTokenValidationParameters(secret, issuer, audience);

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100000,
            HashAlgorithmName.SHA256,
            32
        );

        var result = new byte[48];
        Buffer.BlockCopy(salt, 0, result, 0, 16);
        Buffer.BlockCopy(hash, 0, result, 16, 32);

        return Convert.ToBase64String(result);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var hashBytes = Convert.FromBase64String(passwordHash);

        if (hashBytes.Length != 48)
            return false;

        var salt = new byte[16];
        Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);

        var storedHash = new byte[32];
        Buffer.BlockCopy(hashBytes, 16, storedHash, 0, 32);

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100000,
            HashAlgorithmName.SHA256,
            32
        );

        return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }

    public async Task<LoginResponse?> Login(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            return null;

        var token = GenerateJwtToken(user);
        var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "24");
        var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

        var refreshToken = GenerateRefreshToken();
        _refreshTokens[refreshToken] = (user.Id, expiresAt);

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = MapToUserInfo(user)
        };
    }

    public async Task<LoginResponse?> RefreshToken(RefreshTokenRequest request)
    {
        if (!_refreshTokens.TryGetValue(request.RefreshToken, out var tokenData))
            return null;

        if (tokenData.ExpiresAt < DateTime.UtcNow)
        {
            _refreshTokens.TryRemove(request.RefreshToken, out _);
            return null;
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == tokenData.UserId && u.IsActive);

        if (user == null)
        {
            _refreshTokens.TryRemove(request.RefreshToken, out _);
            return null;
        }

        _refreshTokens.TryRemove(request.RefreshToken, out _);

        var newToken = GenerateJwtToken(user);
        var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "24");
        var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

        var newRefreshToken = GenerateRefreshToken();
        _refreshTokens[newRefreshToken] = (user.Id, expiresAt);

        return new LoginResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            User = MapToUserInfo(user)
        };
    }

    public void Logout(string refreshToken)
    {
        _refreshTokens.TryRemove(refreshToken, out _);
    }

    public async Task<UserInfoDto?> GetUserById(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user == null ? null : MapToUserInfo(user);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private static UserInfoDto MapToUserInfo(User user)
    {
        return new UserInfoDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Avatar = user.Avatar,
            Email = user.Email,
            Role = user.Role?.Name,
            CreatedAt = user.CreatedAt
        };
    }
}
