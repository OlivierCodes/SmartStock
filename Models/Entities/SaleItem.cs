using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models.Entities;

/// <summary>
/// Représente une ligne d'une vente (produit + quantité + prix unitaire).
/// </summary>
public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    // Navigation
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
