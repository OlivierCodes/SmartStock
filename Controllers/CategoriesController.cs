using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models.DTOs;
using SmartStock.Services.Interfaces;

namespace SmartStock.Controllers;

/// <summary>
/// Gestion des catégories de produits.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService) =>
        _categoryService = categoryService;

    /// <summary>Liste toutes les catégories.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _categoryService.GetAllAsync());

    /// <summary>Détail d'une catégorie.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _categoryService.GetByIdAsync(id));

    /// <summary>Créer une catégorie (Responsable et Magasinier).</summary>
    [HttpPost]
    [Authorize(Roles = "Responsable,Magasinier")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var cat = await _categoryService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = cat.Id }, cat);
    }

    /// <summary>Modifier une catégorie (Responsable et Magasinier).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Responsable,Magasinier")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request) =>
        Ok(await _categoryService.UpdateAsync(id, request));

    /// <summary>Supprimer une catégorie (Responsable seulement).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Responsable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}
