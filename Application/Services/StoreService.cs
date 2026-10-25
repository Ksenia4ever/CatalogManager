
using CatalogManager.Application.Interfaces;
using CatalogManager.Domain.Entities;

namespace CatalogManager.Application.Services;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _repository;

    public StoreService(IStoreRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Store>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Store?> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _repository.GetByIdAsync(id);
    }

    public async Task<Store> CreateAsync(Store store)
    {
        ValidateStore(store);

        store.Name = store.Name.Trim();
        store.Address = store.Address.Trim();

        await _repository.AddAsync(store);

        return store;
    }

    public async Task<bool> UpdateAsync(Store store)
    {
        if (store.Id <= 0)
            return false;

        ValidateStore(store);

        var existing = await _repository.GetByIdAsync(store.Id);

        if (existing == null)
            return false;

        existing.Name = store.Name.Trim();
        existing.Address = store.Address.Trim();
        existing.Latitude = store.Latitude;
        existing.Longitude = store.Longitude;

        await _repository.UpdateAsync(existing);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            return false;

        var store = await _repository.GetByIdAsync(id);

        if (store == null)
            return false;

        if (await _repository.HasProductsAsync(id))
            return false;

        await _repository.DeleteAsync(store);

        return true;
    }

    private static void ValidateStore(Store store)
    {
        if (string.IsNullOrWhiteSpace(store.Name))
            throw new ArgumentException("Store name is required.");

        if (string.IsNullOrWhiteSpace(store.Address))
            throw new ArgumentException("Store address is required.");

        if (double.IsNaN(store.Latitude) ||
            double.IsInfinity(store.Latitude) ||
            store.Latitude < -90 || store.Latitude > 90)
            throw new ArgumentException("Invalid latitude.");

        if (double.IsNaN(store.Longitude) ||
            double.IsInfinity(store.Longitude) ||
            store.Longitude < -180 || store.Longitude > 180)
            throw new ArgumentException("Invalid longitude.");
    }
}
