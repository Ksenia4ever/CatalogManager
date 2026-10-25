
using CatalogManager.Application.Interfaces;
using CatalogManager.Application.Services;
using CatalogManager.Domain.Entities;
using Moq;

namespace CatalogManager.UnitTests.Services;

public class StoreServiceTests
{
    private readonly Mock<IStoreRepository> _repositoryMock;
    private readonly StoreService _service;

    public StoreServiceTests()
    {
        _repositoryMock = new Mock<IStoreRepository>();
        _service = new StoreService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllStores()
    {
        var stores = new List<Store>
        {
            new() { Id = 1, Name = "Store A" },
            new() { Id = 2, Name = "Store B" }
        };

        _repositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(stores);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Store A", result[0].Name);
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingStore_ReturnsStore()
    {
        var store = new Store { Id = 1, Name = "Store A" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(store);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Store A", result.Name);
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
    public async Task CreateAsync_ValidStore_CreatesStore()
    {
        var store = new Store
        {
            Name = "  Main Store  ",
            Address = "  Kyiv  ",
            Latitude = 50.45,
            Longitude = 30.52
        };

        var result = await _service.CreateAsync(store);

        Assert.Equal("Main Store", result.Name);
        Assert.Equal("Kyiv", result.Address);

        _repositoryMock.Verify(
            r => r.AddAsync(store), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_EmptyName_ThrowsException(string name)
    {
        var store = new Store
        {
            Name = name,
            Address = "Kyiv"
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(store));

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Store>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_EmptyAddress_ThrowsException(string address)
    {
        var store = new Store
        {
            Name = "Store A",
            Address = address
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(store));
    }

    [Theory]
    [InlineData(-91, 30)]
    [InlineData(91, 30)]
    [InlineData(50, -181)]
    [InlineData(50, 181)]
    [InlineData(double.NaN, 30)]
    [InlineData(50, double.NaN)]
    [InlineData(double.PositiveInfinity, 30)]
    [InlineData(50, double.NegativeInfinity)]
    public async Task CreateAsync_InvalidCoordinates_ThrowsException(
        double latitude, double longitude)
    {
        var store = new Store
        {
            Name = "Store A",
            Address = "Kyiv",
            Latitude = latitude,
            Longitude = longitude
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(store));

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Store>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingStore_UpdatesStore()
    {
        var existing = new Store
        {
            Id = 1,
            Name = "Old Store",
            Address = "Old Address"
        };

        var updated = new Store
        {
            Id = 1,
            Name = "  New Store  ",
            Address = "  New Address  ",
            Latitude = 50.45,
            Longitude = 30.52
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        var result = await _service.UpdateAsync(updated);

        Assert.True(result);
        Assert.Equal("New Store", existing.Name);
        Assert.Equal("New Address", existing.Address);
        Assert.Equal(50.45, existing.Latitude);
        Assert.Equal(30.52, existing.Longitude);

        _repositoryMock.Verify(
            r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingStore_ReturnsFalse()
    {
        var store = new Store
        {
            Id = 99,
            Name = "Store A",
            Address = "Kyiv"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Store?)null);

        var result = await _service.UpdateAsync(store);

        Assert.False(result);
        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Store>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFalse()
    {
        var store = new Store { Id = 0 };

        var result = await _service.UpdateAsync(store);

        Assert.False(result);
    }

    [Theory]
    [InlineData("", "Kyiv")]
    [InlineData("Store A", "")]
    [InlineData("Store A", "   ")]
    public async Task UpdateAsync_InvalidData_ThrowsException(
        string name, string address)
    {
        var store = new Store
        {
            Id = 1,
            Name = name,
            Address = address
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(store));

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Store>()), Times.Never);
    }

    [Theory]
    [InlineData(-91, 30)]
    [InlineData(50, 181)]
    public async Task UpdateAsync_InvalidCoordinates_ThrowsException(
        double latitude, double longitude)
    {
        var store = new Store
        {
            Id = 1,
            Name = "Store A",
            Address = "Kyiv",
            Latitude = latitude,
            Longitude = longitude
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(store));
    }

    [Fact]
    public async Task DeleteAsync_ExistingStoreWithoutProducts_DeletesStore()
    {
        var store = new Store { Id = 1, Name = "Store A" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(store);

        _repositoryMock.Setup(r => r.HasProductsAsync(1))
            .ReturnsAsync(false);

        var result = await _service.DeleteAsync(1);

        Assert.True(result);

        _repositoryMock.Verify(
            r => r.DeleteAsync(store), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_StoreWithProducts_ReturnsFalse()
    {
        var store = new Store { Id = 1, Name = "Store A" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(store);

        _repositoryMock.Setup(r => r.HasProductsAsync(1))
            .ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.False(result);

        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Store>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingStore_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Store?)null);

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
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }


    [Fact]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmptyList()
    {
        _repositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Store>());

        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingStore_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Store?)null);

        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    [InlineData(0, 0)]
    public async Task CreateAsync_BoundaryCoordinates_Succeeds(
        double latitude, double longitude)
    {
        var store = new Store
        {
            Name = "Test Store",
            Address = "Test Address",
            Latitude = latitude,
            Longitude = longitude
        };

        var result = await _service.CreateAsync(store);

        Assert.Equal(latitude, result.Latitude);
        Assert.Equal(longitude, result.Longitude);

        _repositoryMock.Verify(
            r => r.AddAsync(store), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateAsync_NonPositiveId_ReturnsFalse(int id)
    {
        var store = new Store
        {
            Id = id,
            Name = "Test Store",
            Address = "Test Address"
        };

        var result = await _service.UpdateAsync(store);

        Assert.False(result);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_StoreWithProducts_DoesNotDelete()
    {
        var store = new Store
        {
            Id = 5,
            Name = "Store",
            Address = "Kyiv"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(store);

        _repositoryMock.Setup(r => r.HasProductsAsync(5))
            .ReturnsAsync(true);

        var result = await _service.DeleteAsync(5);

        Assert.False(result);

        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Store>()),
            Times.Never);
    }

}
