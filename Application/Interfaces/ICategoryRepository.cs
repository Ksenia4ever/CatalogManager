using CatalogManager.Domain.Entities;

namespace CatalogManager.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task AddAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(Category category);

    Task<bool> HasProductsAsync(int categoryId);

    Task<bool> NameExistsAsync(
        string name,
        int? excludeId = null);
}