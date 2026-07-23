using SmartStock.Models.DTOs;

namespace SmartStock.Services.Interfaces;

public interface IStockService
{
    Task<PagedResult<StockMovementDto>> GetMovementsAsync(StockMovementListParams parameters);
    Task<StockMovementDto> GetMovementByIdAsync(int id);
    Task<StockMovementDto> CreateMovementAsync(int userId, CreateStockMovementRequest request);
    Task<DailyReportDto> GetDailyReportAsync(DateTime date);
}

public interface ISaleService
{
    Task<PagedResult<SaleDto>> GetSalesAsync(SaleListParams parameters);
    Task<SaleDto> GetSaleByIdAsync(int id);
    Task<SaleDto> CreateSaleAsync(int sellerId, CreateSaleRequest request);
    Task<SaleDto> CancelSaleAsync(int saleId, int userId);
    Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to);
}


