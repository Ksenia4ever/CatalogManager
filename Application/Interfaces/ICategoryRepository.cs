using CatalogManager.Application.Models;
using CatalogManager.Domain.Entities;

namespace CatalogManager.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();

    Task<PagedResult<Category>> GetPagedAsync(int skip, int limit);

    Task<Category?> GetByIdAsync(int id);

    Task AddAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(Category category);

    Task<bool> HasProductsAsync(int categoryId);

    Task<bool> NameExistsAsync(
        string name,
        int? excludeId = null);
}