using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models.Entities;

/// <summary>
/// Représente un mouvement de stock (entrée ou sortie) effectué par le magasinier.
/// </summary>
public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    [Required]
    public MovementType Type { get; set; }

    public int Quantity { get; set; }

    /// <summary>Stock disponible après le mouvement (snapshot).</summary>
    public int StockAfterMovement { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    [MaxLength(200)]
    public string? Reference { get; set; }

    public DateTime MovedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
}

public enum MovementType
{
    Entree = 0,   // Réapprovisionnement
    Sortie = 1    // Retrait du stock
}
