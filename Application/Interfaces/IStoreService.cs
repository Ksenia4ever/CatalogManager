using CatalogManager.Application.Models;
using CatalogManager.Domain.Entities;

namespace CatalogManager.Application.Interfaces;

public interface IStoreService
{
    Task<List<Store>> GetAllAsync();

    Task<PagedResult<Store>> GetPagedAsync(int skip, int limit);

    Task<Store?> GetByIdAsync(int id);

    Task<Store> CreateAsync(Store store);

    Task<bool> UpdateAsync(Store store);

    Task<bool> DeleteAsync(int id);
}