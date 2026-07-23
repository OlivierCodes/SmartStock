using System.ComponentModel.DataAnnotations;
using SmartStock.Models.Entities;

namespace SmartStock.Models.DTOs;

// ─── Stock Movement ────────────────────────────────────────────────────────────

public record StockMovementDto(
    int Id,
    int ProductId,
    string ProductName,
    string? ProductSKU,
    int UserId,
    string UserFullName,
    MovementType Type,
    string TypeLabel,
    int Quantity,
    int StockAfterMovement,
    string? Reason,
    string? Reference,
    DateTime MovedAt
);

public record CreateStockMovementRequest(
    [Required] int ProductId,
    [Required] MovementType Type,
    [Required, Range(1, int.MaxValue)] int Quantity,
    [MaxLength(500)] string? Reason,
    [MaxLength(200)] string? Reference
);

public record StockMovementListParams(
    int? ProductId,
    int? UserId,
    MovementType? Type,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20
);

// ─── Sale ─────────────────────────────────────────────────────────────────────

public record SaleDto(
    int Id,
    string SaleNumber,
    int SellerId,
    string SellerFullName,
    decimal TotalAmount,
    string? CustomerName,
    string? Notes,
    SaleStatus Status,
    DateTime SoldAt,
    IEnumerable<SaleItemDto> Items
);

public record SaleItemDto(
    int Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);

public record CreateSaleRequest(
    [MaxLength(200)] string? CustomerName,
    [MaxLength(500)] string? Notes,
    [Required, MinLength(1)] IEnumerable<CreateSaleItemRequest> Items
);

public record CreateSaleItemRequest(
    [Required] int ProductId,
    [Required, Range(1, int.MaxValue)] int Quantity
);

public record SaleListParams(
    int? SellerId,
    SaleStatus? Status,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20
);

// ─── Dashboard / Reports ──────────────────────────────────────────────────────



public record DailyReportDto(
    DateTime Date,
    IEnumerable<StockMovementDto> Entries,
    IEnumerable<StockMovementDto> Exits,
    IEnumerable<SaleDto> Sales,
    decimal TotalSalesAmount,
    int TotalEntriesQuantity,
    int TotalExitsQuantity
);

public record SalesReportDto(
    DateTime From,
    DateTime To,
    decimal TotalRevenue,
    int TotalTransactions,
    int TotalItemsSold,
    IEnumerable<TopProductDto> TopProducts,
    IEnumerable<DailySalesSummaryDto> DailySummaries
);

public record DailySalesSummaryDto(
    DateTime Date,
    int SalesCount,
    decimal TotalAmount
);
