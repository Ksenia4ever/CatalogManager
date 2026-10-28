using CatalogManager.Application.Models;
using CatalogManager.Domain.Entities;

namespace CatalogManager.Application.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync();

    Task<PagedResult<Category>> GetPagedAsync(int skip, int limit);

    Task<Category?> GetByIdAsync(int id);

    Task<Category> CreateAsync(Category category);

    Task<bool> UpdateAsync(Category category);

    Task<bool> DeleteAsync(int id);
}