using System.Collections.Generic;

namespace SmartStock.Models.DTOs
{
    /// <summary>
    /// Résumé global du tableau de bord pour le Responsable.
    /// </summary>
    public class DashboardSummaryDto
    {
        // Counts
        public int TotalProducts { get; init; }
        public int LowStockCount { get; init; }
        public int OutOfStockCount { get; init; }

        // Sales figures
        public decimal SalesTodayAmount { get; init; }
        public int SalesTodayCount { get; init; }
        public decimal SalesMonthAmount { get; init; }
        public int SalesMonthCount { get; init; }

        // Stock movements today
        public int EntriesToday { get; init; }
        public int ExitsToday { get; init; }

        // Collections
        public IEnumerable<TopProductDto> TopProducts { get; init; } = new List<TopProductDto>();
        public IEnumerable<LowStockAlertDto> LowStockAlerts { get; init; } = new List<LowStockAlertDto>();

        // Constructor matching service call
        public DashboardSummaryDto(
            int totalProducts,
            int lowStockCount,
            int outOfStockCount,
            decimal salesTodayAmount,
            int salesTodayCount,
            decimal salesMonthAmount,
            int salesMonthCount,
            int entriesToday,
            int exitsToday,
            IEnumerable<TopProductDto> topProducts,
            IEnumerable<LowStockAlertDto> lowStockAlerts)
        {
            TotalProducts = totalProducts;
            LowStockCount = lowStockCount;
            OutOfStockCount = outOfStockCount;
            SalesTodayAmount = salesTodayAmount;
            SalesTodayCount = salesTodayCount;
            SalesMonthAmount = salesMonthAmount;
            SalesMonthCount = salesMonthCount;
            EntriesToday = entriesToday;
            ExitsToday = exitsToday;
            TopProducts = topProducts;
            LowStockAlerts = lowStockAlerts;
        }
    }

    /// <summary>
    /// Information d'un produit le plus vendu.
    /// </summary>
    public class TopProductDto
    {
        public int ProductId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? SKU { get; init; }
        public int CurrentStock { get; init; }
        public bool IsActive { get; init; }
        public int SoldQuantity { get; init; }
        public decimal TotalRevenue { get; init; }
    }

    /// <summary>
    /// Alerte de stock faible.
    /// </summary>
    public class LowStockAlertDto
    {
        public int ProductId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? SKU { get; init; }
        public int CurrentStock { get; init; }
        public int MinStockThreshold { get; init; }
        public bool IsActive { get; init; }
    }
}
