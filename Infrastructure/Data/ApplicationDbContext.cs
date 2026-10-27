using CatalogManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogManager.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Store> Stores => Set<Store>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Унікальний індекс назв категорій керується SQL-міграцією
        // CaseInsensitiveCategoryNames та використовує LOWER(TRIM("Name")).

        // Зв'язок між товарами та категоріями.
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Зв'язок між товарами та магазинами.
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Store)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}