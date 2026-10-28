using CatalogManager.Application.Models;
using CatalogManager.Application.Interfaces;
using CatalogManager.Domain.Entities;
using CatalogManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogManager.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Product>> GetPagedAsync(int skip, int limit)
    {
        var query = _context.Products.AsNoTracking().Include(p => p.Category).Include(p => p.Store);
        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(e => e.Id)
            .Skip(skip).Take(limit).ToListAsync();
        return new PagedResult<Product>(items, totalCount);
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Store)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Store)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _context.Categories
            .AnyAsync(c => c.Id == categoryId);
    }

    public async Task<bool> StoreExistsAsync(int storeId)
    {
        return await _context.Stores
            .AnyAsync(s => s.Id == storeId);
    }
}