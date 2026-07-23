using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models.DTOs;

// ─── Category ────────────────────────────────────────────────────────────────

public record CategoryDto(
    int Id,
    string Name,
    string? Description,
    int ProductCount,
    DateTime CreatedAt
);

public record CreateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description
);

public record UpdateCategoryRequest(
    [MaxLength(100)] string? Name,
    [MaxLength(500)] string? Description
);

// ─── Product ──────────────────────────────────────────────────────────────────

public record ProductDto(
    int Id,
    string Name,
    string? SKU,
    string? Description,
    decimal PurchasePrice,
    decimal SellingPrice,
    int CurrentStock,
    int MinStockThreshold,
    string Unit,
    bool IsActive,
    bool IsLowStock,
    int? CategoryId,
    string? CategoryName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateProductRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(50)] string? SKU,
    [MaxLength(1000)] string? Description,
    [Range(0, double.MaxValue)] decimal PurchasePrice,
    [Range(0, double.MaxValue)] decimal SellingPrice,
    [Range(0, int.MaxValue)] int InitialStock,
    [Range(0, int.MaxValue)] int MinStockThreshold,
    [MaxLength(50)] string Unit,
    int? CategoryId
);

public record UpdateProductRequest(
    [MaxLength(200)] string? Name,
    [MaxLength(50)] string? SKU,
    [MaxLength(1000)] string? Description,
    decimal? PurchasePrice,
    decimal? SellingPrice,
    int? MinStockThreshold,
    [MaxLength(50)] string? Unit,
    bool? IsActive,
    int? CategoryId
);

public record ProductListParams(
    string? Search,
    int? CategoryId,
    bool? LowStockOnly,
    bool? ActiveOnly,
    int Page = 1,
    int PageSize = 20
);
