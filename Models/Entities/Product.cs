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

    /// <summary>Prix détaillant (prix standard)</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal RetailPrice { get; set; }

    /// <summary>Prix moyen</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal MediumPrice { get; set; }

    /// <summary>Dernier prix (prix plancher / minimum)</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal LastPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SellingPrice
    {
        get => RetailPrice;
        set => RetailPrice = value;
    }

    /// <summary>Stock disponible en boutique (en rayon pour la vente)</summary>
    public int ShopStock { get; set; } = 0;

    /// <summary>Stock disponible en magasin (entrepôt / réserve)</summary>
    public int WarehouseStock { get; set; } = 0;

    /// <summary>Quantité totale disponible en stock (Boutique + Magasin).</summary>
    public int CurrentStock
    {
        get => ShopStock + WarehouseStock;
        set
        {
            // Pour la rétro-compatibilité / initialisation
            if (ShopStock == 0 && WarehouseStock == 0)
            {
                ShopStock = value;
            }
        }
    }

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
