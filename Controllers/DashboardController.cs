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

    /// <summary>
    /// Génère et télécharge le rapport statistique et l'inventaire de la journée au format PDF (Responsable seulement).
    /// </summary>
    [HttpGet("daily-report/pdf")]
    [Authorize(Roles = "Responsable")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyReportPdf([FromQuery] DateTime? date)
    {
        var pdfBytes = await _dashboardService.GenerateDailyReportPdfAsync(date);
        var fileName = $"SmartStock_Rapport_Journalier_{(date ?? DateTime.UtcNow):yyyyMMdd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
