
using CatalogManager.Application.Interfaces;
using CatalogManager.Application.Services;
using CatalogManager.Domain.Entities;
using Moq;

namespace CatalogManager.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _service = new ProductService(_repositoryMock.Object);
    }

    private static Product CreateValidProduct() => new()
    {
        Id = 1,
        Name = "Laptop",
        Description = "Test laptop",
        Price = 15000,
        CategoryId = 1,
        StoreId = 1
    };

    private void SetupValidRelations()
    {
        _repositoryMock
            .Setup(r => r.CategoryExistsAsync(1))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.StoreExistsAsync(1))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProducts()
    {
        var products = new List<Product>
        {
            CreateValidProduct()
        };

        _repositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        var result = await _service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Laptop", result[0].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsProduct()
    {
        var product = CreateValidProduct();

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Laptop", result.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByIdAsync_InvalidId_ReturnsNull(int id)
    {
        var result = await _service.GetByIdAsync(id);

        Assert.Null(result);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidProduct_CreatesProduct()
    {
        var product = CreateValidProduct();
        product.Name = "  Laptop  ";

        SetupValidRelations();

        var result = await _service.CreateAsync(product);

        Assert.Equal("Laptop", result.Name);

        _repositoryMock.Verify(
            r => r.AddAsync(product), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_EmptyName_ThrowsException(string name)
    {
        var product = CreateValidProduct();
        product.Name = name;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(product));

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task CreateAsync_InvalidPrice_ThrowsException(decimal price)
    {
        var product = CreateValidProduct();
        product.Price = price;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(product));
    }

    [Fact]
    public async Task CreateAsync_NonExistingCategory_ThrowsException()
    {
        var product = CreateValidProduct();

        _repositoryMock.Setup(r => r.CategoryExistsAsync(1))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(product));
    }

    [Fact]
    public async Task CreateAsync_NonExistingStore_ThrowsException()
    {
        var product = CreateValidProduct();

        _repositoryMock.Setup(r => r.CategoryExistsAsync(1))
            .ReturnsAsync(true);

        _repositoryMock.Setup(r => r.StoreExistsAsync(1))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(product));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public async Task CreateAsync_InvalidRelations_ThrowsException(
        int categoryId, int storeId)
    {
        var product = CreateValidProduct();
        product.CategoryId = categoryId;
        product.StoreId = storeId;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(product));
    }

    [Fact]
    public async Task UpdateAsync_ExistingProduct_UpdatesProduct()
    {
        var existing = CreateValidProduct();

        var updated = CreateValidProduct();
        updated.Name = "  Updated Laptop  ";
        updated.Price = 20000;

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        SetupValidRelations();

        var result = await _service.UpdateAsync(updated);

        Assert.True(result);
        Assert.Equal("Updated Laptop", existing.Name);
        Assert.Equal(20000, existing.Price);

        _repositoryMock.Verify(
            r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingProduct_ReturnsFalse()
    {
        var product = CreateValidProduct();
        product.Id = 99;

        _repositoryMock.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Product?)null);

        var result = await _service.UpdateAsync(product);

        Assert.False(result);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFalse()
    {
        var product = CreateValidProduct();
        product.Id = 0;

        var result = await _service.UpdateAsync(product);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_InvalidPrice_ThrowsException()
    {
        var product = CreateValidProduct();
        product.Price = -50;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(product));
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_DeletesProduct()
    {
        var product = CreateValidProduct();

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        var result = await _service.DeleteAsync(1);

        Assert.True(result);

        _repositoryMock.Verify(
            r => r.DeleteAsync(product), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingProduct_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Product?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteAsync_InvalidId_ReturnsFalse(int id)
    {
        var result = await _service.DeleteAsync(id);

        Assert.False(result);
    }


    [Fact]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmptyList()
    {
        _repositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Product>());

        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingProduct_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_NullProduct_ThrowsException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CreateAsync(null!));

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_TrimsTextFields()
    {
        var product = CreateValidProduct();

        product.Name = "  Laptop  ";
        product.Description = "  Gaming laptop  ";
        product.ImageUrl = "  https://example.com/image.jpg  ";

        SetupValidRelations();

        var result = await _service.CreateAsync(product);

        Assert.Equal("Laptop", result.Name);
        Assert.Equal("Gaming laptop", result.Description);
        Assert.Equal(
            "https://example.com/image.jpg",
            result.ImageUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAsync_EmptyName_ThrowsException(string name)
    {
        var product = CreateValidProduct();
        product.Name = name;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(product));

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public async Task UpdateAsync_InvalidRelationIds_ThrowsException(
        int categoryId, int storeId)
    {
        var product = CreateValidProduct();

        product.CategoryId = categoryId;
        product.StoreId = storeId;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(product));
    }

    [Fact]
    public async Task UpdateAsync_NonExistingCategory_ThrowsException()
    {
        var existing = CreateValidProduct();
        var updated = CreateValidProduct();

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        _repositoryMock.Setup(r => r.CategoryExistsAsync(1))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(updated));

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingStore_ThrowsException()
    {
        var existing = CreateValidProduct();
        var updated = CreateValidProduct();

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        _repositoryMock.Setup(r => r.CategoryExistsAsync(1))
            .ReturnsAsync(true);

        _repositoryMock.Setup(r => r.StoreExistsAsync(1))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(updated));

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAllFields()
    {
        var existing = CreateValidProduct();
        var updated = CreateValidProduct();

        updated.Name = "  New Laptop  ";
        updated.Description = "  Updated description  ";
        updated.ImageUrl = "  https://example.com/new.jpg  ";
        updated.Price = 25000;
        updated.CategoryId = 2;
        updated.StoreId = 3;

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        _repositoryMock.Setup(r => r.CategoryExistsAsync(2))
            .ReturnsAsync(true);

        _repositoryMock.Setup(r => r.StoreExistsAsync(3))
            .ReturnsAsync(true);

        var result = await _service.UpdateAsync(updated);

        Assert.True(result);
        Assert.Equal("New Laptop", existing.Name);
        Assert.Equal("Updated description", existing.Description);
        Assert.Equal(25000m, existing.Price);
        Assert.Equal("https://example.com/new.jpg", existing.ImageUrl);
        Assert.Equal(2, existing.CategoryId);
        Assert.Equal(3, existing.StoreId);

        _repositoryMock.Verify(
            r => r.UpdateAsync(existing), Times.Once);
    }

}
