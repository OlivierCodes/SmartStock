using Microsoft.EntityFrameworkCore;
using SmartStock.Data;
using SmartStock.Models.DTOs;
using SmartStock.Models.Entities;
using SmartStock.Services.Interfaces;

namespace SmartStock.Services;

public class CategoryService : ICategoryService
{
    private readonly SmartStockDbContext _context;

    public CategoryService(SmartStockDbContext context) => _context = context;

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .Select(c => MapToDto(c, c.Products.Count))
            .ToListAsync();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var cat = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Catégorie {id} introuvable.");
        return MapToDto(cat, cat.Products.Count);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        if (await _context.Categories.AnyAsync(c => c.Name == request.Name))
            throw new InvalidOperationException($"La catégorie '{request.Name}' existe déjà.");

        var cat = new Category { Name = request.Name, Description = request.Description };
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();
        return MapToDto(cat, 0);
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var cat = await _context.Categories.Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Catégorie {id} introuvable.");

        if (request.Name is not null) cat.Name = request.Name;
        if (request.Description is not null) cat.Description = request.Description;

        await _context.SaveChangesAsync();
        return MapToDto(cat, cat.Products.Count);
    }

    public async Task DeleteAsync(int id)
    {
        var cat = await _context.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Catégorie {id} introuvable.");
        _context.Categories.Remove(cat);
        await _context.SaveChangesAsync();
    }

    private static CategoryDto MapToDto(Category c, int productCount) => new(
        c.Id, c.Name, c.Description, productCount, c.CreatedAt
    );
}

public class ProductService : IProductService
{
    private readonly SmartStockDbContext _context;

    public ProductService(SmartStockDbContext context) => _context = context;

    public async Task<PagedResult<ProductDto>> GetAllAsync(ProductListParams p)
    {
        var query = _context.Products
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(p.Search))
            query = query.Where(x => x.Name.Contains(p.Search) || (x.SKU != null && x.SKU.Contains(p.Search)));

        if (p.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == p.CategoryId);

        if (p.LowStockOnly == true)
            query = query.Where(x => x.CurrentStock <= x.MinStockThreshold);

        if (p.ActiveOnly != false)
            query = query.Where(x => x.IsActive);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.Name)
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .Select(x => MapToDto(x))
            .ToListAsync();

        return new PagedResult<ProductDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var p = await _context.Products.Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Produit {id} introuvable.");
        return MapToDto(p);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        if (request.SKU is not null && await _context.Products.AnyAsync(p => p.SKU == request.SKU))
            throw new InvalidOperationException($"Le SKU '{request.SKU}' est déjà utilisé.");

        if (request.CategoryId.HasValue && !await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
            throw new KeyNotFoundException($"Catégorie {request.CategoryId} introuvable.");

        var product = new Product
        {
            Name = request.Name,
            SKU = request.SKU,
            Description = request.Description,
            PurchasePrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            CurrentStock = request.InitialStock,
            MinStockThreshold = request.MinStockThreshold,
            Unit = request.Unit,
            CategoryId = request.CategoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.Products.Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Produit {id} introuvable.");

        if (request.Name is not null) product.Name = request.Name;
        if (request.SKU is not null)
        {
            if (await _context.Products.AnyAsync(p => p.SKU == request.SKU && p.Id != id))
                throw new InvalidOperationException($"Le SKU '{request.SKU}' est déjà utilisé.");
            product.SKU = request.SKU;
        }
        if (request.Description is not null) product.Description = request.Description;
        if (request.PurchasePrice.HasValue) product.PurchasePrice = request.PurchasePrice.Value;
        if (request.SellingPrice.HasValue) product.SellingPrice = request.SellingPrice.Value;
        if (request.MinStockThreshold.HasValue) product.MinStockThreshold = request.MinStockThreshold.Value;
        if (request.Unit is not null) product.Unit = request.Unit;
        if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;
        if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        return MapToDto(product);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id)
            ?? throw new KeyNotFoundException($"Produit {id} introuvable.");
        product.IsActive = false;
        if (!string.IsNullOrEmpty(product.SKU) && !product.SKU.Contains("-DEL-"))
        {
            product.SKU = product.SKU + "-DEL-" + product.Id;
        }
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.CurrentStock <= p.MinStockThreshold)
            .OrderBy(p => p.CurrentStock)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    internal static ProductDto MapToDto(Product p) => new(
        p.Id, p.Name, p.SKU, p.Description, p.PurchasePrice, p.SellingPrice,
        p.CurrentStock, p.MinStockThreshold, p.Unit, p.IsActive,
        p.CurrentStock <= p.MinStockThreshold,
        p.CategoryId, p.Category?.Name, p.CreatedAt, p.UpdatedAt
    );
}
