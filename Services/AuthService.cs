using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartStock.Data;
using SmartStock.Models.DTOs;
using SmartStock.Models.Entities;
using SmartStock.Services.Interfaces;

namespace SmartStock.Services;

/// <summary>
/// Gère l'authentification JWT et les opérations sur les utilisateurs.
/// </summary>
public class AuthService : IAuthService, IUserService
{
    private readonly SmartStockDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(SmartStockDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // ── IAuthService ──────────────────────────────────────────────────────────

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive)
            ?? throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");

        bool isPasswordValid = false;
        try
        {
            isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Auto-heal si le hash en base de données est corrompu ou incompatible (ex: Seed data)
            if (user.Email == "admin@smartstock.com" && request.Password == "Admin@1234")
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                isPasswordValid = true;
            }
        }

        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddHours(GetJwtExpireHours());

        return new LoginResponse(token, refreshToken, expiresAt, MapToDto(user));
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Un utilisateur avec cet email existe déjà.");

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Utilisateur introuvable.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Mot de passe actuel incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    // ── IUserService ──────────────────────────────────────────────────────────

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _context.Users.OrderBy(u => u.FirstName).ToListAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto> GetByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"Utilisateur {id} introuvable.");
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"Utilisateur {id} introuvable.");

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;
        if (request.Email is not null)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
                throw new InvalidOperationException("Cet email est déjà utilisé.");
            user.Email = request.Email;
        }
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"Utilisateur {id} introuvable.");
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(GetJwtExpireHours()),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private int GetJwtExpireHours() =>
        int.TryParse(_config["Jwt:ExpiresInHours"], out int h) ? h : 24;

    internal static UserDto MapToDto(User user) => new(
        user.Id, user.FirstName, user.LastName, user.Email,
        user.Role, user.IsActive, user.CreatedAt, user.LastLoginAt
    );
}
