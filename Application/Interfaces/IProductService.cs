using CatalogManager.Application.Models;
using CatalogManager.Domain.Entities;

namespace CatalogManager.Application.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();

    Task<PagedResult<Product>> GetPagedAsync(int skip, int limit);

    Task<Product?> GetByIdAsync(int id);

    Task<Product> CreateAsync(Product product);

    Task<bool> UpdateAsync(Product product);

    Task<bool> DeleteAsync(int id);
}