using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models.DTOs;
using SmartStock.Services.Interfaces;

namespace SmartStock.Controllers;

/// <summary>
/// Gestion du catalogue produits et des niveaux de stock.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService) =>
        _productService = productService;

    /// <summary>
    /// Liste paginée des produits avec filtres optionnels.
    /// </summary>
    /// <param name="search">Recherche par nom ou SKU.</param>
    /// <param name="categoryId">Filtrer par catégorie.</param>
    /// <param name="lowStockOnly">N'afficher que les produits en stock faible.</param>
    /// <param name="activeOnly">N'afficher que les produits actifs (défaut: true).</param>
    /// <param name="page">Numéro de page (défaut: 1).</param>
    /// <param name="pageSize">Taille de page (défaut: 20, max: 100).</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] bool? lowStockOnly,
        [FromQuery] bool? activeOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Min(pageSize, 100);
        var result = await _productService.GetAllAsync(
            new ProductListParams(search, categoryId, lowStockOnly, activeOnly, page, pageSize));
        return Ok(result);
    }

    /// <summary>Détail d'un produit.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _productService.GetByIdAsync(id));

    /// <summary>Produits en stock faible ou rupture.</summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStock() =>
        Ok(await _productService.GetLowStockAsync());

    /// <summary>Créer un produit (Responsable et Magasinier).</summary>
    [HttpPost]
    [Authorize(Roles = "Responsable,Magasinier")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var product = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Modifier un produit (Responsable et Magasinier).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Responsable,Magasinier")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request) =>
        Ok(await _productService.UpdateAsync(id, request));

    /// <summary>Désactiver un produit (soft delete, Responsable seulement).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Responsable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}
