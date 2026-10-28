using CatalogManager.Application.Models;
using CatalogManager.Application.Interfaces;
using CatalogManager.Domain.Entities;
using CatalogManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogManager.Infrastructure.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly ApplicationDbContext _context;

    public StoreRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Store>> GetPagedAsync(int skip, int limit)
    {
        var query = _context.Stores.AsNoTracking();
        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(e => e.Id)
            .Skip(skip).Take(limit).ToListAsync();
        return new PagedResult<Store>(items, totalCount);
    }

    public async Task<List<Store>> GetAllAsync()
    {
        return await _context.Stores
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Store?> GetByIdAsync(int id)
    {
        return await _context.Stores
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Store store)
    {
        await _context.Stores.AddAsync(store);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Store store)
    {
        _context.Stores.Update(store);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Store store)
    {
        _context.Stores.Remove(store);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasProductsAsync(int storeId)
    {
        return await _context.Products
            .AnyAsync(p => p.StoreId == storeId);
    }
}