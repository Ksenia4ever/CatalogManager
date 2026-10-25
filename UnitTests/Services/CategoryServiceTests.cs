
using CatalogManager.Application.Interfaces;
using CatalogManager.Application.Services;
using CatalogManager.Domain.Entities;
using Moq;

namespace CatalogManager.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _service = new CategoryService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics" },
            new() { Id = 2, Name = "Books" }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(categories);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Electronics", result[0].Name);
        Assert.Equal("Books", result[1].Name);

        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCategory_ReturnsCategory()
    {
        var category = new Category { Id = 1, Name = "Books" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Books", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(0);

        Assert.Null(result);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidCategory_CreatesCategory()
    {
        var category = new Category { Name = "  Electronics  " };

        var result = await _service.CreateAsync(category);

        Assert.Equal("Electronics", result.Name);

        _repositoryMock.Verify(
            r => r.AddAsync(category),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_EmptyName_ThrowsException(string name)
    {
        var category = new Category { Name = name };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(category));

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingCategory_UpdatesCategory()
    {
        var existing = new Category { Id = 1, Name = "Old name" };
        var updated = new Category { Id = 1, Name = "  New name  " };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        var result = await _service.UpdateAsync(updated);

        Assert.True(result);
        Assert.Equal("New name", existing.Name);

        _repositoryMock.Verify(
            r => r.UpdateAsync(existing),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingCategory_ReturnsFalse()
    {
        var category = new Category { Id = 99, Name = "Books" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Category?)null);

        var result = await _service.UpdateAsync(category);

        Assert.False(result);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingCategoryWithoutProducts_DeletesCategory()
    {
        var category = new Category { Id = 1, Name = "Books" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        _repositoryMock
            .Setup(r => r.HasProductsAsync(1))
            .ReturnsAsync(false);

        var result = await _service.DeleteAsync(1);

        Assert.True(result);

        _repositoryMock.Verify(
            r => r.DeleteAsync(category),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CategoryWithProducts_ReturnsFalse()
    {
        var category = new Category { Id = 1, Name = "Electronics" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        _repositoryMock
            .Setup(r => r.HasProductsAsync(1))
            .ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.False(result);

        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingCategory_ReturnsFalse()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Category?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result);
    }


    [Fact]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmptyList()
    {
        _repositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Category>());

        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingCategory_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateAsync_InvalidId_ReturnsFalse(int id)
    {
        var category = new Category { Id = id, Name = "Books" };

        var result = await _service.UpdateAsync(category);

        Assert.False(result);
        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Category>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAsync_EmptyName_ThrowsException(string name)
    {
        var category = new Category { Id = 1, Name = name };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(category));

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Category>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteAsync_InvalidId_ReturnsFalse(int id)
    {
        var result = await _service.DeleteAsync(id);

        Assert.False(result);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesDescription()
    {
        var existing = new Category
        {
            Id = 1,
            Name = "Books",
            Description = "Old description"
        };

        var updated = new Category
        {
            Id = 1,
            Name = "Books",
            Description = "New description"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        var result = await _service.UpdateAsync(updated);

        Assert.True(result);
        Assert.Equal("New description", existing.Description);
        _repositoryMock.Verify(
            r => r.UpdateAsync(existing), Times.Once);
    }

}
