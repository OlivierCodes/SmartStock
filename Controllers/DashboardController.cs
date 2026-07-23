using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models.DTOs;
using SmartStock.Services.Interfaces;

namespace SmartStock.Controllers;

/// <summary>
/// Tableau de bord centralisé pour le Responsable.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) =>
        _dashboardService = dashboardService;

    /// <summary>
    /// Résumé global : produits, ventes du jour, du mois, alertes stock faible et top produits.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary() =>
        Ok(await _dashboardService.GetSummaryAsync());
}
