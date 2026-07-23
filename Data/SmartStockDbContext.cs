using Microsoft.EntityFrameworkCore;
using SmartStock.Models.Entities;

namespace SmartStock.Data;

/// <summary>
/// Contexte principal de la base de données SmartStock.
/// Supporte SQL Server et PostgreSQL via la configuration.
/// </summary>
public class SmartStockDbContext : DbContext
{
    public SmartStockDbContext(DbContextOptions<SmartStockDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasConversion<string>();
        });

        // ── Category ──────────────────────────────────────────
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
        });

        // ── Product ───────────────────────────────────────────
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.SKU).IsUnique().HasFilter("\"SKU\" IS NOT NULL");

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── StockMovement ─────────────────────────────────────
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.Property(sm => sm.Type).HasConversion<string>();

            entity.HasOne(sm => sm.Product)
                  .WithMany(p => p.StockMovements)
                  .HasForeignKey(sm => sm.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sm => sm.User)
                  .WithMany(u => u.StockMovements)
                  .HasForeignKey(sm => sm.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Sale ──────────────────────────────────────────────
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasIndex(s => s.SaleNumber).IsUnique();
            entity.Property(s => s.Status).HasConversion<string>();

            entity.HasOne(s => s.Seller)
                  .WithMany(u => u.Sales)
                  .HasForeignKey(s => s.SellerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── SaleItem ──────────────────────────────────────────
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasOne(si => si.Sale)
                  .WithMany(s => s.Items)
                  .HasForeignKey(si => si.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(si => si.Product)
                  .WithMany(p => p.SaleItems)
                  .HasForeignKey(si => si.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Seed Data ─────────────────────────────────────────
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Responsable par défaut
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            FirstName = "Admin",
            LastName = "SmartStock",
            Email = "admin@smartstock.com",
            // Pre-computed hash for "Admin@1234" – static to avoid PendingModelChangesWarning
            PasswordHash = "$2a$11$hzikGu6iVUijYkgIw5o1qO/zMEiBpRb5SVHZdJvqjLSBpjkd4VBnu",
            Role = UserRole.Responsable,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Catégories de base
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Alimentation", Description = "Produits alimentaires", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 2, Name = "Boissons", Description = "Boissons et liquides", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 3, Name = "Hygiène", Description = "Produits d'hygiène et beauté", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 4, Name = "Électronique", Description = "Appareils et accessoires électroniques", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
