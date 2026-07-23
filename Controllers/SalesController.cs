using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models.DTOs;
using SmartStock.Models.Entities;
using SmartStock.Services.Interfaces;

namespace SmartStock.Controllers;

/// <summary>
/// Gestion des ventes réalisées par les Vendeurs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService) => _saleService = saleService;

    /// <summary>
    /// Historique paginé des ventes avec filtres optionnels.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Responsable,Magasinier")]
    [ProducesResponseType(typeof(PagedResult<SaleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? sellerId,
        [FromQuery] SaleStatus? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        Ok(await _saleService.GetSalesAsync(
            new SaleListParams(sellerId, status, from, to, page, pageSize)));

    /// <summary>
    /// Historique des ventes du vendeur connecté.
    /// </summary>
    [HttpGet("my-sales")]
    [Authorize(Roles = "Vendeur")]
    [ProducesResponseType(typeof(PagedResult<SaleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMySales(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _saleService.GetSalesAsync(
            new SaleListParams(sellerId, null, from, to, page, pageSize)));
    }

    /// <summary>Détail d'une vente.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _saleService.GetSaleByIdAsync(id));

    /// <summary>
    /// Enregistrer une vente (Vendeur et Responsable).
    /// Le stock est automatiquement mis à jour.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Vendeur,Responsable")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request)
    {
        var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sale = await _saleService.CreateSaleAsync(sellerId, request);
        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
    }

    /// <summary>
    /// Annuler une vente et restaurer le stock (Responsable seulement).
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Responsable")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _saleService.CancelSaleAsync(id, userId));
    }

    /// <summary>Rapport de ventes sur une période (Responsable seulement).</summary>
    [HttpGet("report")]
    [Authorize(Roles = "Responsable")]
    [ProducesResponseType(typeof(SalesReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Report(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var dateFrom = from ?? DateTime.UtcNow.AddDays(-30);
        var dateTo = to ?? DateTime.UtcNow;
        return Ok(await _saleService.GetSalesReportAsync(dateFrom, dateTo));
    }
}
