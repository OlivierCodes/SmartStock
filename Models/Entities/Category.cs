using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models.Entities;

/// <summary>
/// Représente une catégorie de produits.
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Product> Products { get; set; } = [];
}
