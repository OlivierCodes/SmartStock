using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models.Entities;

/// <summary>
/// Représente une vente effectuée par un vendeur.
/// </summary>
public class Sale
{
    public int Id { get; set; }

    /// <summary>Numéro de vente unique (ex : VNT-20240101-0001).</summary>
    [Required, MaxLength(50)]
    public string SaleNumber { get; set; } = string.Empty;

    public int SellerId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(200)]
    public string? CustomerName { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public SaleStatus Status { get; set; } = SaleStatus.Completed;

    public DateTime SoldAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Seller { get; set; } = null!;
    public ICollection<SaleItem> Items { get; set; } = [];
}

public enum SaleStatus
{
    Pending = 0,
    Completed = 1,
    Cancelled = 2
}
