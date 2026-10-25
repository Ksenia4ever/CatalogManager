
using CatalogManager.Application.Interfaces;
using CatalogManager.Domain.Entities;

namespace CatalogManager.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _repository.GetByIdAsync(id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        ValidateProduct(product);

        await ValidateRelationsAsync(product);

        product.Name = product.Name.Trim();
        product.Description = product.Description?.Trim();
        product.ImageUrl = product.ImageUrl?.Trim();

        await _repository.AddAsync(product);

        return product;
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        if (product.Id <= 0)
            return false;

        ValidateProduct(product);

        var existing = await _repository.GetByIdAsync(product.Id);

        if (existing == null)
            return false;

        await ValidateRelationsAsync(product);

        existing.Name = product.Name.Trim();
        existing.Description = product.Description?.Trim();
        existing.Price = product.Price;
        existing.ImageUrl = product.ImageUrl?.Trim();
        existing.CategoryId = product.CategoryId;
        existing.StoreId = product.StoreId;

        await _repository.UpdateAsync(existing);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            return false;

        var product = await _repository.GetByIdAsync(id);

        if (product == null)
            return false;

        await _repository.DeleteAsync(product);

        return true;
    }

    private static void ValidateProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException(
                "Product name is required.");

        if (product.Price <= 0)
            throw new ArgumentException(
                "Product price must be greater than zero.");

        if (product.CategoryId <= 0)
            throw new ArgumentException(
                "Valid category ID is required.");

        if (product.StoreId <= 0)
            throw new ArgumentException(
                "Valid store ID is required.");
    }

    private async Task ValidateRelationsAsync(Product product)
    {
        if (!await _repository.CategoryExistsAsync(product.CategoryId))
            throw new ArgumentException(
                "Selected category does not exist.");

        if (!await _repository.StoreExistsAsync(product.StoreId))
            throw new ArgumentException(
                "Selected store does not exist.");
    }
}
