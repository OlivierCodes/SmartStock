using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models.Entities;

/// <summary>
/// Représente un produit géré dans le stock.
/// </summary>
public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? SKU { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PurchasePrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SellingPrice { get; set; }

    /// <summary>Quantité actuellement disponible en stock.</summary>
    public int CurrentStock { get; set; } = 0;

    /// <summary>Seuil minimum déclenchant une alerte de stock faible.</summary>
    public int MinStockThreshold { get; set; } = 5;

    /// <summary>Unité de mesure (pcs, kg, litre, etc.).</summary>
    [MaxLength(50)]
    public string Unit { get; set; } = "pcs";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Clé étrangère
    public int? CategoryId { get; set; }

    // Navigation
    public Category? Category { get; set; }
    public ICollection<StockMovement> StockMovements { get; set; } = [];
    public ICollection<SaleItem> SaleItems { get; set; } = [];
}
