using CatalogManager.Application.Models;
using CatalogManager.Application.Interfaces;
using CatalogManager.Application.Exceptions;
using CatalogManager.Domain.Entities;

namespace CatalogManager.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<Category>> GetPagedAsync(int skip, int limit)
    {
        return _repository.GetPagedAsync(skip, limit);
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _repository.GetByIdAsync(id);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            throw new ArgumentException(
                "Category name is required.");
        }

        category.Name = category.Name.Trim();

        if (category.Name.Length > 150)
        {
            throw new ArgumentException(
                "Category name must not exceed 150 characters.");
        }

        if (await _repository.NameExistsAsync(category.Name))
        {
            throw new DuplicateCategoryNameException();
        }

        await _repository.AddAsync(category);

        return category;
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        if (category.Id <= 0)
            return false;

        if (string.IsNullOrWhiteSpace(category.Name))
        {
            throw new ArgumentException(
                "Category name is required.");
        }

        var existing =
            await _repository.GetByIdAsync(category.Id);

        if (existing == null)
            return false;

        var name = category.Name.Trim();

        if (name.Length > 150)
        {
            throw new ArgumentException(
                "Category name must not exceed 150 characters.");
        }

        if (await _repository.NameExistsAsync(
            name, category.Id))
        {
            throw new DuplicateCategoryNameException();
        }

        existing.Name = name;
        existing.Description = category.Description;

        await _repository.UpdateAsync(existing);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            return false;

        var category = await _repository.GetByIdAsync(id);

        if (category == null)
            return false;

        if (await _repository.HasProductsAsync(id))
            return false;

        await _repository.DeleteAsync(category);

        return true;
    }
}