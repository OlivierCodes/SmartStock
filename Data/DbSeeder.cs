using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartStock.Models.Entities;

namespace SmartStock.Data;

public static class DbSeeder
{
    public static async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        using var context = serviceProvider.GetRequiredService<SmartStockDbContext>();

        // Vérifier si la base de données est déjà alimentée en produits
        if (await context.Products.AnyAsync())
        {
            return; // Déjà alimenté
        }

        // Récupérer l'utilisateur admin par défaut
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@smartstock.com");
        if (adminUser == null) return; // Sécurité si l'utilisateur n'existe pas

        // 1. Ajouter des produits de démonstration
        var products = new List<Product>
        {
            new Product
            {
                Name = "Riz Parfumé 5kg",
                SKU = "RIZ-001",
                Description = "Sac de riz parfumé de qualité supérieure.",
                PurchasePrice = 15.00m,
                SellingPrice = 20.00m,
                CurrentStock = 45,
                MinStockThreshold = 10,
                Unit = "sac",
                IsActive = true,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Product
            {
                Name = "Huile de Palme 1L",
                SKU = "HUI-002",
                Description = "Bouteille d'huile de palme raffinée.",
                PurchasePrice = 3.50m,
                SellingPrice = 5.00m,
                CurrentStock = 8,
                MinStockThreshold = 15,
                Unit = "bouteille",
                IsActive = true,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Product
            {
                Name = "Soda local 33cl",
                SKU = "SODA-003",
                Description = "Canette de boisson gazeuse sucrée.",
                PurchasePrice = 0.40m,
                SellingPrice = 0.75m,
                CurrentStock = 120,
                MinStockThreshold = 30,
                Unit = "canette",
                IsActive = true,
                CategoryId = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Product
            {
                Name = "Eau Minérale 1.5L",
                SKU = "EAU-004",
                Description = "Bouteille d'eau minérale de source locale.",
                PurchasePrice = 0.30m,
                SellingPrice = 0.60m,
                CurrentStock = 0,
                MinStockThreshold = 20,
                Unit = "bouteille",
                IsActive = true,
                CategoryId = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Product
            {
                Name = "Savon de Marseille",
                SKU = "SAV-005",
                Description = "Pain de savon traditionnel pour la lessive et toilette.",
                PurchasePrice = 1.00m,
                SellingPrice = 1.50m,
                CurrentStock = 80,
                MinStockThreshold = 25,
                Unit = "pcs",
                IsActive = true,
                CategoryId = 3,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Product
            {
                Name = "Ampoule LED 9W",
                SKU = "LED-006",
                Description = "Ampoule économique blanche culot E27.",
                PurchasePrice = 2.00m,
                SellingPrice = 3.50m,
                CurrentStock = 6,
                MinStockThreshold = 10,
                Unit = "pcs",
                IsActive = true,
                CategoryId = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // 2. Ajouter des mouvements de stock historiques et récents
        var movements = new List<StockMovement>
        {
            new StockMovement
            {
                ProductId = products[0].Id,
                UserId = adminUser.Id,
                Type = MovementType.Entree,
                Quantity = 50,
                StockAfterMovement = 50,
                Reason = "Réception fournisseur de base",
                Reference = "REF-IN-101",
                MovedAt = DateTime.UtcNow.AddHours(-4)
            },
            new StockMovement
            {
                ProductId = products[1].Id,
                UserId = adminUser.Id,
                Type = MovementType.Entree,
                Quantity = 10,
                StockAfterMovement = 10,
                Reason = "Réception fournisseur huile",
                Reference = "REF-IN-102",
                MovedAt = DateTime.UtcNow.AddHours(-3)
            },
            new StockMovement
            {
                ProductId = products[1].Id,
                UserId = adminUser.Id,
                Type = MovementType.Sortie,
                Quantity = 2,
                StockAfterMovement = 8,
                Reason = "Perte/Avarie constatée en rayon",
                Reference = "REF-OUT-201",
                MovedAt = DateTime.UtcNow.AddHours(-2)
            }
        };

        context.StockMovements.AddRange(movements);

        // 3. Ajouter des ventes simulées pour le mois en cours et aujourd'hui
        var now = DateTime.UtcNow;
        var today = now.Date;

        var sales = new List<Sale>
        {
            // Vente 1 : il y a 10 jours
            new Sale
            {
                SaleNumber = "VNT-20260709-0001",
                SellerId = adminUser.Id,
                CustomerName = "Mme KOFFI",
                Notes = "Achat grossiste Lomé",
                Status = SaleStatus.Completed,
                SoldAt = now.AddDays(-10),
                TotalAmount = 205.00m,
                Items = new List<SaleItem>
                {
                    new SaleItem { ProductId = products[0].Id, Quantity = 10, UnitPrice = 20.00m, TotalPrice = 200.00m },
                    new SaleItem { ProductId = products[1].Id, Quantity = 1, UnitPrice = 5.00m, TotalPrice = 5.00m }
                }
            },
            // Vente 2 : il y a 5 jours
            new Sale
            {
                SaleNumber = "VNT-20260714-0001",
                SellerId = adminUser.Id,
                CustomerName = "M. SOW",
                Notes = "Règlement Cash",
                Status = SaleStatus.Completed,
                SoldAt = now.AddDays(-5),
                TotalAmount = 18.00m,
                Items = new List<SaleItem>
                {
                    new SaleItem { ProductId = products[2].Id, Quantity = 24, UnitPrice = 0.75m, TotalPrice = 18.00m }
                }
            },
            // Vente 3 : aujourd'hui, il y a 3 heures
            new Sale
            {
                SaleNumber = $"VNT-{today:yyyyMMdd}-0001",
                SellerId = adminUser.Id,
                CustomerName = "Mme AMEYO",
                Notes = "Client régulier",
                Status = SaleStatus.Completed,
                SoldAt = now.AddHours(-3),
                TotalAmount = 43.50m,
                Items = new List<SaleItem>
                {
                    new SaleItem { ProductId = products[0].Id, Quantity = 2, UnitPrice = 20.00m, TotalPrice = 40.00m },
                    new SaleItem { ProductId = products[5].Id, Quantity = 1, UnitPrice = 3.50m, TotalPrice = 3.50m }
                }
            },
            // Vente 4 : aujourd'hui, il y a 1 heure
            new Sale
            {
                SaleNumber = $"VNT-{today:yyyyMMdd}-0002",
                SellerId = adminUser.Id,
                CustomerName = "M. TCHALIM",
                Notes = "Achat rapide",
                Status = SaleStatus.Completed,
                SoldAt = now.AddHours(-1),
                TotalAmount = 45.00m,
                Items = new List<SaleItem>
                {
                    new SaleItem { ProductId = products[4].Id, Quantity = 30, UnitPrice = 1.50m, TotalPrice = 45.00m }
                }
            }
        };

        context.Sales.AddRange(sales);
        await context.SaveChangesAsync();
    }
}
