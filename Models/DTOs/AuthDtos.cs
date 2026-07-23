using System.ComponentModel.DataAnnotations;
using SmartStock.Models.Entities;

namespace SmartStock.Models.DTOs;

// ─── Auth ────────────────────────────────────────────────────────────────────

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record LoginResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record RegisterRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] UserRole Role
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(6)] string NewPassword
);

// ─── User ─────────────────────────────────────────────────────────────────────

public record UserDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

public record UpdateUserRequest(
    [MaxLength(100)] string? FirstName,
    [MaxLength(100)] string? LastName,
    [EmailAddress] string? Email,
    bool? IsActive
);
