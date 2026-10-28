using CatalogManager.Application.Models;
using CatalogManager.Domain.Entities;

namespace CatalogManager.Application.Interfaces;

public interface IStoreRepository
{
    Task<List<Store>> GetAllAsync();

    Task<PagedResult<Store>> GetPagedAsync(int skip, int limit);

    Task<Store?> GetByIdAsync(int id);

    Task AddAsync(Store store);

    Task UpdateAsync(Store store);

    Task DeleteAsync(Store store);

    Task<bool> HasProductsAsync(int storeId);
}