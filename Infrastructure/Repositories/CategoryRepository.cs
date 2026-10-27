using CatalogManager.Application.Interfaces;
using CatalogManager.Application.Exceptions;
using CatalogManager.Domain.Entities;
using CatalogManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CatalogManager.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);

        await SaveChangesWithDuplicateCheckAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);

        await SaveChangesWithDuplicateCheckAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasProductsAsync(int categoryId)
    {
        return await _context.Products
            .AnyAsync(p => p.CategoryId == categoryId);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        int? excludeId = null)
    {
        var normalizedName = name.Trim().ToLower();

        return await _context.Categories.AnyAsync(c =>
            c.Name.ToLower() == normalizedName &&
            (!excludeId.HasValue || c.Id != excludeId.Value));
    }

    private async Task SaveChangesWithDuplicateCheckAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when
            (ex.InnerException is PostgresException pg &&
             pg.SqlState == PostgresErrorCodes.UniqueViolation &&
             pg.ConstraintName == "IX_Categories_Name")
        {
            throw new DuplicateCategoryNameException();
        }
    }
}