using CatalogManager.Application.Models;
using CatalogManager.Domain.Entities;

namespace CatalogManager.Application.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();

    Task<PagedResult<Product>> GetPagedAsync(int skip, int limit);

    Task<Product?> GetByIdAsync(int id);

    Task AddAsync(Product product);

    Task UpdateAsync(Product product);

    Task DeleteAsync(Product product);

    Task<bool> CategoryExistsAsync(int categoryId);

    Task<bool> StoreExistsAsync(int storeId);
}