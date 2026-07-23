using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models.DTOs;
using SmartStock.Models.Entities;
using SmartStock.Services.Interfaces;

namespace SmartStock.Controllers;

/// <summary>
/// Gestion des mouvements de stock (entrées et sorties) par le Magasinier.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Responsable,Magasinier")]
[Produces("application/json")]
public class StockMovementsController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockMovementsController(IStockService stockService) =>
        _stockService = stockService;

    /// <summary>
    /// Historique paginé des mouvements de stock.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<StockMovementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? productId,
        [FromQuery] int? userId,
        [FromQuery] MovementType? type,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        Ok(await _stockService.GetMovementsAsync(
            new StockMovementListParams(productId, userId, type, from, to, page, pageSize)));

    /// <summary>Détail d'un mouvement de stock.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(StockMovementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _stockService.GetMovementByIdAsync(id));

    /// <summary>
    /// Enregistrer une entrée ou sortie de stock.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(StockMovementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var movement = await _stockService.CreateMovementAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement);
    }

    /// <summary>Rapport journalier des mouvements de stock.</summary>
    [HttpGet("daily-report")]
    [Authorize(Roles = "Responsable")]
    [ProducesResponseType(typeof(DailyReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DailyReport([FromQuery] DateTime? date)
    {
        var reportDate = date ?? DateTime.UtcNow.Date;
        return Ok(await _stockService.GetDailyReportAsync(reportDate));
    }
}
