using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartStock.Data;

namespace SmartStock.Pages;

public class IndexModel : PageModel
{
    private readonly SmartStockDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public int TotalProducts { get; private set; }
    public int TotalCategories { get; private set; }
    public int TotalUsers { get; private set; }
    public int LowStockAlertsCount { get; private set; }
    public bool IsDatabaseConnected { get; private set; }

    public IndexModel(SmartStockDbContext context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            // Vérifier la connexion et charger les stats réelles
            TotalProducts = await _context.Products.CountAsync(p => p.IsActive);
            TotalCategories = await _context.Categories.CountAsync();
            TotalUsers = await _context.Users.CountAsync(u => u.IsActive);
            LowStockAlertsCount = await _context.Products.CountAsync(p => p.IsActive && p.CurrentStock <= p.MinStockThreshold);
            IsDatabaseConnected = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Impossible de se connecter à la base de données pour les statistiques : {Message}", ex.Message);
            // Données de secours si la base n'est pas encore créée/migrée
            TotalProducts = 120;
            TotalCategories = 4;
            TotalUsers = 3;
            LowStockAlertsCount = 5;
            IsDatabaseConnected = false;
        }
    }
}
