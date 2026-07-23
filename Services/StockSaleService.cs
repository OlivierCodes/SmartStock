using Microsoft.EntityFrameworkCore;
using SmartStock.Data;
using SmartStock.Models.DTOs;
using SmartStock.Models.Entities;
using SmartStock.Services.Interfaces;

namespace SmartStock.Services;

public class StockService : IStockService
{
    private readonly SmartStockDbContext _context;

    public StockService(SmartStockDbContext context) => _context = context;

    public async Task<PagedResult<StockMovementDto>> GetMovementsAsync(StockMovementListParams p)
    {
        var query = _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.User)
            .AsQueryable();

        if (p.ProductId.HasValue) query = query.Where(sm => sm.ProductId == p.ProductId);
        if (p.UserId.HasValue) query = query.Where(sm => sm.UserId == p.UserId);
        if (p.Type.HasValue) query = query.Where(sm => sm.Type == p.Type);
        if (p.From.HasValue) query = query.Where(sm => sm.MovedAt >= p.From);
        if (p.To.HasValue) query = query.Where(sm => sm.MovedAt <= p.To);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(sm => sm.MovedAt)
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .Select(sm => MapToDto(sm))
            .ToListAsync();

        return new PagedResult<StockMovementDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<StockMovementDto> GetMovementByIdAsync(int id)
    {
        var sm = await _context.StockMovements
            .Include(x => x.Product)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Mouvement {id} introuvable.");
        return MapToDto(sm);
    }

    public async Task<StockMovementDto> CreateMovementAsync(int userId, CreateStockMovementRequest request)
    {
        var product = await _context.Products.FindAsync(request.ProductId)
            ?? throw new KeyNotFoundException($"Produit {request.ProductId} introuvable.");

        if (!product.IsActive)
            throw new InvalidOperationException("Ce produit est désactivé.");

        if (request.Type == MovementType.Sortie && product.CurrentStock < request.Quantity)
            throw new InvalidOperationException(
                $"Stock insuffisant. Disponible : {product.CurrentStock}, demandé : {request.Quantity}.");

        // Mettre à jour le stock
        if (request.Type == MovementType.Entree)
            product.CurrentStock += request.Quantity;
        else
            product.CurrentStock -= request.Quantity;

        product.UpdatedAt = DateTime.UtcNow;

        var movement = new StockMovement
        {
            ProductId = request.ProductId,
            UserId = userId,
            Type = request.Type,
            Quantity = request.Quantity,
            StockAfterMovement = product.CurrentStock,
            Reason = request.Reason,
            Reference = request.Reference
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        await _context.Entry(movement).Reference(m => m.Product).LoadAsync();
        await _context.Entry(movement).Reference(m => m.User).LoadAsync();
        return MapToDto(movement);
    }

    public async Task<DailyReportDto> GetDailyReportAsync(DateTime date)
    {
        var dayStart = date.Date.ToUniversalTime();
        var dayEnd = dayStart.AddDays(1);

        var movements = await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.User)
            .Where(sm => sm.MovedAt >= dayStart && sm.MovedAt < dayEnd)
            .OrderByDescending(sm => sm.MovedAt)
            .ToListAsync();

        var sales = await _context.Sales
            .Include(s => s.Seller)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .Where(s => s.SoldAt >= dayStart && s.SoldAt < dayEnd && s.Status == SaleStatus.Completed)
            .OrderByDescending(s => s.SoldAt)
            .ToListAsync();

        var entries = movements.Where(m => m.Type == MovementType.Entree).Select(MapToDto);
        var exits = movements.Where(m => m.Type == MovementType.Sortie).Select(MapToDto);

        return new DailyReportDto(
            date.Date,
            entries,
            exits,
            sales.Select(SaleService.MapToDto),
            sales.Sum(s => s.TotalAmount),
            movements.Where(m => m.Type == MovementType.Entree).Sum(m => m.Quantity),
            movements.Where(m => m.Type == MovementType.Sortie).Sum(m => m.Quantity)
        );
    }

    internal static StockMovementDto MapToDto(StockMovement sm) => new(
        sm.Id,
        sm.ProductId,
        sm.Product?.Name ?? string.Empty,
        sm.Product?.SKU,
        sm.UserId,
        $"{sm.User?.FirstName} {sm.User?.LastName}",
        sm.Type,
        sm.Type == MovementType.Entree ? "Entrée" : "Sortie",
        sm.Quantity,
        sm.StockAfterMovement,
        sm.Reason,
        sm.Reference,
        sm.MovedAt
    );
}

public class SaleService : ISaleService
{
    private readonly SmartStockDbContext _context;

    public SaleService(SmartStockDbContext context) => _context = context;

    public async Task<PagedResult<SaleDto>> GetSalesAsync(SaleListParams p)
    {
        var query = _context.Sales
            .Include(s => s.Seller)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .AsQueryable();

        if (p.SellerId.HasValue) query = query.Where(s => s.SellerId == p.SellerId);
        if (p.Status.HasValue) query = query.Where(s => s.Status == p.Status);
        if (p.From.HasValue) query = query.Where(s => s.SoldAt >= p.From);
        if (p.To.HasValue) query = query.Where(s => s.SoldAt <= p.To);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.SoldAt)
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync();

        return new PagedResult<SaleDto>(items.Select(MapToDto), total, p.Page, p.PageSize);
    }

    public async Task<SaleDto> GetSaleByIdAsync(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Seller)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Vente {id} introuvable.");
        return MapToDto(sale);
    }

    public async Task<SaleDto> CreateSaleAsync(int sellerId, CreateSaleRequest request)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        // Vérifications
        foreach (var item in request.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId)
                ?? throw new KeyNotFoundException($"Produit {item.ProductId} introuvable ou inactif.");

            if (product.CurrentStock < item.Quantity)
                throw new InvalidOperationException(
                    $"Stock insuffisant pour '{product.Name}'. Disponible : {product.CurrentStock}.");
        }

        var saleNumber = GenerateSaleNumber();
        var saleItems = new List<SaleItem>();
        decimal total = 0;

        foreach (var itemReq in request.Items)
        {
            var product = products.First(p => p.Id == itemReq.ProductId);
            var lineTotal = product.SellingPrice * itemReq.Quantity;
            total += lineTotal;

            // Déduire du stock
            product.CurrentStock -= itemReq.Quantity;
            product.UpdatedAt = DateTime.UtcNow;

            saleItems.Add(new SaleItem
            {
                ProductId = itemReq.ProductId,
                Quantity = itemReq.Quantity,
                UnitPrice = product.SellingPrice,
                TotalPrice = lineTotal
            });
        }

        var sale = new Sale
        {
            SaleNumber = saleNumber,
            SellerId = sellerId,
            TotalAmount = total,
            CustomerName = request.CustomerName,
            Notes = request.Notes,
            Status = SaleStatus.Completed,
            Items = saleItems
        };

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        await _context.Entry(sale).Reference(s => s.Seller).LoadAsync();
        foreach (var item in sale.Items)
            await _context.Entry(item).Reference(i => i.Product).LoadAsync();

        return MapToDto(sale);
    }

    public async Task<SaleDto> CancelSaleAsync(int saleId, int userId)
    {
        var sale = await _context.Sales
            .Include(s => s.Seller)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == saleId)
            ?? throw new KeyNotFoundException($"Vente {saleId} introuvable.");

        if (sale.Status == SaleStatus.Cancelled)
            throw new InvalidOperationException("Cette vente est déjà annulée.");

        // Restaurer le stock
        foreach (var item in sale.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product is not null)
            {
                product.CurrentStock += item.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }
        }

        sale.Status = SaleStatus.Cancelled;
        await _context.SaveChangesAsync();
        return MapToDto(sale);
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to)
    {
        var fromUtc = from.Date.ToUniversalTime();
        var toUtc = to.Date.AddDays(1).ToUniversalTime();

        var sales = await _context.Sales
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .Where(s => s.SoldAt >= fromUtc && s.SoldAt < toUtc && s.Status == SaleStatus.Completed)
            .ToListAsync();

        var topProducts = sales
            .SelectMany(s => s.Items)
            .GroupBy(i => new { i.ProductId, i.Product.Name })
            .Select(g => new TopProductDto {
                ProductId = g.Key.ProductId,
                Name = g.Key.Name,
                SoldQuantity = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(t => t.SoldQuantity)
            .Take(10);

        var dailySummaries = sales
            .GroupBy(s => s.SoldAt.Date)
            .Select(g => new DailySalesSummaryDto(g.Key, g.Count(), g.Sum(s => s.TotalAmount)))
            .OrderBy(d => d.Date);

        return new SalesReportDto(
            from.Date, to.Date,
            sales.Sum(s => s.TotalAmount),
            sales.Count,
            sales.SelectMany(s => s.Items).Sum(i => i.Quantity),
            topProducts,
            dailySummaries
        );
    }

    private string GenerateSaleNumber()
    {
        var date = DateTime.UtcNow;
        var sequence = _context.Sales.Count(s => s.SoldAt.Date == date.Date) + 1;
        return $"VNT-{date:yyyyMMdd}-{sequence:D4}";
    }

    internal static SaleDto MapToDto(Sale s) => new(
        s.Id, s.SaleNumber, s.SellerId,
        $"{s.Seller?.FirstName} {s.Seller?.LastName}",
        s.TotalAmount, s.CustomerName, s.Notes, s.Status, s.SoldAt,
        s.Items.Select(i => new SaleItemDto(
            i.Id, i.ProductId, i.Product?.Name ?? string.Empty,
            i.Quantity, i.UnitPrice, i.TotalPrice))
    );
}

public class DashboardService : IDashboardService
{
    private readonly SmartStockDbContext _context;

    public DashboardService(SmartStockDbContext context) => _context = context;

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
        var lowStock = await _context.Products
            .Where(p => p.IsActive && p.CurrentStock <= p.MinStockThreshold && p.CurrentStock > 0)
            .CountAsync();
        var outOfStock = await _context.Products
            .Where(p => p.IsActive && p.CurrentStock == 0)
            .CountAsync();

        var todaySales = await _context.Sales
            .Where(s => s.SoldAt >= todayStart && s.Status == SaleStatus.Completed)
            .ToListAsync();

        var monthSales = await _context.Sales
            .Where(s => s.SoldAt >= monthStart && s.Status == SaleStatus.Completed)
            .ToListAsync();

        var todayMovements = await _context.StockMovements
            .Where(sm => sm.MovedAt >= todayStart)
            .ToListAsync();

        var topProducts = await _context.SaleItems
            .Include(i => i.Product)
            .Where(i => i.Sale.Status == SaleStatus.Completed && i.Sale.SoldAt >= monthStart)
            .GroupBy(i => new { i.ProductId, i.Product.Name, i.Product.SKU, i.Product.CurrentStock, i.Product.IsActive })
            .Select(g => new TopProductDto {
                ProductId = g.Key.ProductId,
                Name = g.Key.Name,
                SKU = g.Key.SKU,
                CurrentStock = g.Key.CurrentStock,
                IsActive = g.Key.IsActive,
                SoldQuantity = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(t => t.SoldQuantity)
            .Take(5)
            .ToListAsync();

        var lowStockAlerts = await _context.Products
            .Where(p => p.IsActive && p.CurrentStock <= p.MinStockThreshold)
            .OrderBy(p => p.CurrentStock)
            .Take(10)
            .Select(p => new LowStockAlertDto {
                ProductId = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                CurrentStock = p.CurrentStock,
                MinStockThreshold = p.MinStockThreshold,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return new DashboardSummaryDto(
            totalProducts, lowStock, outOfStock,
            todaySales.Sum(s => s.TotalAmount), todaySales.Count,
            monthSales.Sum(s => s.TotalAmount), monthSales.Count,
            todayMovements.Count(m => m.Type == MovementType.Entree),
            todayMovements.Count(m => m.Type == MovementType.Sortie),
            topProducts, lowStockAlerts
        );
    }
}
