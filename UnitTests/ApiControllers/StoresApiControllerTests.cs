using CatalogManager.ApiControllers;
using CatalogManager.Application.Interfaces;
using CatalogManager.Domain.Entities;
using CatalogManager.DTOs.Stores;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CatalogManager.UnitTests.ApiControllers;

public class StoresApiControllerTests
{
    private readonly Mock<IStoreService> _serviceMock = new();
    private readonly StoresApiController _controller;

    public StoresApiControllerTests()
    {
        _controller = new StoresApiController(_serviceMock.Object);
    }

    private static Store CreateStore() => new()
    {
        Id = 1,
        Name = "Tech Store",
        Address = "Kyiv",
        Latitude = 50.4501,
        Longitude = 30.5234
    };

    [Fact]
    public async Task GetAll_ReturnsOkWithStores()
    {
        var stores = new List<Store> { CreateStore() };

        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(stores);

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var data = Assert.IsAssignableFrom<
            IEnumerable<StoreResponseDto>>(ok.Value);

        var store = Assert.Single(data);

        Assert.Equal(1, store.Id);
        Assert.Equal("Tech Store", store.Name);
        Assert.Equal("Kyiv", store.Address);
    }

    [Fact]
    public async Task GetAll_Empty_ReturnsEmptyCollection()
    {
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<Store>());

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var data = Assert.IsAssignableFrom<
            IEnumerable<StoreResponseDto>>(ok.Value);

        Assert.Empty(data);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(CreateStore());

        var result = await _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<StoreResponseDto>(ok.Value);

        Assert.Equal("Tech Store", dto.Name);
        Assert.Equal("Kyiv", dto.Address);
        Assert.Equal(50.4501, dto.Latitude);
        Assert.Equal(30.5234, dto.Longitude);
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((Store?)null);

        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_Valid_ReturnsCreated()
    {
        var dto = new StoreCreateDto
        {
            Name = "New Store",
            Address = "Kyiv, Main Street 1",
            Latitude = 50.45,
            Longitude = 30.52
        };

        _serviceMock.Setup(s => s.CreateAsync(
                It.IsAny<Store>()))
            .ReturnsAsync((Store store) =>
            {
                store.Id = 10;
                return store;
            });

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(
            result.Result);

        Assert.Equal(nameof(StoresApiController.GetById),
            created.ActionName);
        Assert.Equal(10, created.RouteValues!["id"]);

        var response = Assert.IsType<StoreResponseDto>(
            created.Value);

        Assert.Equal(10, response.Id);
        Assert.Equal("New Store", response.Name);

        _serviceMock.Verify(s => s.CreateAsync(
            It.Is<Store>(s =>
                s.Name == dto.Name &&
                s.Address == dto.Address &&
                s.Latitude == dto.Latitude &&
                s.Longitude == dto.Longitude)),
            Times.Once);
    }

    [Fact]
    public async Task Create_ServiceThrows_ReturnsBadRequest()
    {
        _serviceMock.Setup(s => s.CreateAsync(
                It.IsAny<Store>()))
            .ThrowsAsync(
                new ArgumentException("Invalid latitude"));

        var result = await _controller.Create(
            new StoreCreateDto
            {
                Name = "Test",
                Address = "Kyiv",
                Latitude = 100,
                Longitude = 30
            });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_Existing_ReturnsNoContent()
    {
        var dto = new StoreUpdateDto
        {
            Name = "Updated Store",
            Address = "New Address",
            Latitude = 49.5,
            Longitude = 31.5
        };

        _serviceMock.Setup(s => s.UpdateAsync(
                It.IsAny<Store>()))
            .ReturnsAsync(true);

        var result = await _controller.Update(1, dto);

        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(s => s.UpdateAsync(
            It.Is<Store>(s =>
                s.Id == 1 &&
                s.Name == dto.Name &&
                s.Address == dto.Address &&
                s.Latitude == dto.Latitude &&
                s.Longitude == dto.Longitude)),
            Times.Once);
    }

    [Fact]
    public async Task Update_Missing_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.UpdateAsync(
                It.IsAny<Store>()))
            .ReturnsAsync(false);

        var result = await _controller.Update(
            999,
            new StoreUpdateDto
            {
                Name = "Missing",
                Address = "Kyiv"
            });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ServiceThrows_ReturnsBadRequest()
    {
        _serviceMock.Setup(s => s.UpdateAsync(
                It.IsAny<Store>()))
            .ThrowsAsync(
                new ArgumentException("Invalid longitude"));

        var result = await _controller.Update(
            1,
            new StoreUpdateDto
            {
                Name = "Test",
                Address = "Kyiv",
                Longitude = 200
            });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(CreateStore());

        _serviceMock.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_Missing_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((Store?)null);

        var result = await _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);

        _serviceMock.Verify(s => s.DeleteAsync(
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WithProducts_ReturnsConflict()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(CreateStore());

        _serviceMock.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(false);

        var result = await _controller.Delete(1);

        Assert.IsType<ConflictObjectResult>(result);
    }
}
