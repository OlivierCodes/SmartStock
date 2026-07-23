using SmartStock.Models.DTOs;

namespace SmartStock.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request);
    Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request);
    Task DeleteAsync(int id);
}

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAllAsync(ProductListParams parameters);
    Task<ProductDto> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
    Task<IEnumerable<ProductDto>> GetLowStockAsync();
}
