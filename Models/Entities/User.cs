using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models.Entities;

/// <summary>
/// Représente un utilisateur du système (Magasinier, Vendeur, Responsable).
/// </summary>
public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(150), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Vendeur;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public ICollection<StockMovement> StockMovements { get; set; } = [];
    public ICollection<Sale> Sales { get; set; } = [];
}

public enum UserRole
{
    Responsable = 0,
    Magasinier = 1,
    Vendeur = 2
}
