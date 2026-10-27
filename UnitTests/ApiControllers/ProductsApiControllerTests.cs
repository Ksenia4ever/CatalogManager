using CatalogManager.ApiControllers;
using CatalogManager.Application.Interfaces;
using CatalogManager.Domain.Entities;
using CatalogManager.DTOs.Products;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CatalogManager.UnitTests.ApiControllers;

public class ProductsApiControllerTests
{
    private readonly Mock<IProductService> _serviceMock = new();
    private readonly ProductsApiController _controller;

    public ProductsApiControllerTests()
    {
        _controller = new ProductsApiController(_serviceMock.Object);
    }

    private static Product CreateProduct() => new()
    {
        Id = 1,
        Name = "Lenovo IdeaPad 5",
        Description = "Laptop",
        Price = 25000m,
        ImageUrl = "https://example.com/laptop.jpg",
        CategoryId = 1,
        StoreId = 1
    };

    private static ProductCreateDto CreateDto() => new()
    {
        Name = "New Laptop",
        Description = "New product",
        Price = 20000m,
        ImageUrl = "https://example.com/new.jpg",
        CategoryId = 1,
        StoreId = 2
    };

    [Fact]
    public async Task GetAll_ReturnsOkWithProducts()
    {
        var products = new List<Product> { CreateProduct() };

        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(products);

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var data = Assert.IsAssignableFrom<
            IEnumerable<ProductResponseDto>>(ok.Value);

        var product = Assert.Single(data);

        Assert.Equal(1, product.Id);
        Assert.Equal("Lenovo IdeaPad 5", product.Name);
        Assert.Equal(25000m, product.Price);
        Assert.Equal(1, product.CategoryId);
        Assert.Equal(1, product.StoreId);
    }

    [Fact]
    public async Task GetAll_Empty_ReturnsEmptyCollection()
    {
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<Product>());

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var data = Assert.IsAssignableFrom<
            IEnumerable<ProductResponseDto>>(ok.Value);

        Assert.Empty(data);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(CreateProduct());

        var result = await _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ProductResponseDto>(ok.Value);

        Assert.Equal(1, dto.Id);
        Assert.Equal("Lenovo IdeaPad 5", dto.Name);
        Assert.Equal(25000m, dto.Price);
        Assert.Equal("https://example.com/laptop.jpg",
            dto.ImageUrl);
        Assert.Equal(1, dto.CategoryId);
        Assert.Equal(1, dto.StoreId);
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_Valid_ReturnsCreated()
    {
        var dto = CreateDto();

        _serviceMock.Setup(s => s.CreateAsync(
                It.IsAny<Product>()))
            .ReturnsAsync((Product product) =>
            {
                product.Id = 15;
                return product;
            });

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(
            result.Result);

        Assert.Equal(nameof(ProductsApiController.GetById),
            created.ActionName);
        Assert.Equal(15, created.RouteValues!["id"]);

        var response = Assert.IsType<ProductResponseDto>(
            created.Value);

        Assert.Equal(15, response.Id);
        Assert.Equal("New Laptop", response.Name);
        Assert.Equal(20000m, response.Price);
        Assert.Equal(1, response.CategoryId);
        Assert.Equal(2, response.StoreId);

        _serviceMock.Verify(s => s.CreateAsync(
            It.Is<Product>(p =>
                p.Name == dto.Name &&
                p.Description == dto.Description &&
                p.Price == dto.Price &&
                p.ImageUrl == dto.ImageUrl &&
                p.CategoryId == dto.CategoryId &&
                p.StoreId == dto.StoreId)),
            Times.Once);
    }

    [Theory]
    [InlineData("Invalid product")]
    [InlineData("Category does not exist")]
    [InlineData("Store does not exist")]
    public async Task Create_ServiceThrows_ReturnsBadRequest(
        string message)
    {
        _serviceMock.Setup(s => s.CreateAsync(
                It.IsAny<Product>()))
            .ThrowsAsync(new ArgumentException(message));

        var result = await _controller.Create(CreateDto());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_Existing_ReturnsNoContent()
    {
        var dto = new ProductUpdateDto
        {
            Name = "Updated Laptop",
            Description = "Updated description",
            Price = 22000m,
            ImageUrl = "https://example.com/updated.jpg",
            CategoryId = 2,
            StoreId = 3
        };

        _serviceMock.Setup(s => s.UpdateAsync(
                It.IsAny<Product>()))
            .ReturnsAsync(true);

        var result = await _controller.Update(1, dto);

        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(s => s.UpdateAsync(
            It.Is<Product>(p =>
                p.Id == 1 &&
                p.Name == dto.Name &&
                p.Description == dto.Description &&
                p.Price == dto.Price &&
                p.ImageUrl == dto.ImageUrl &&
                p.CategoryId == dto.CategoryId &&
                p.StoreId == dto.StoreId)),
            Times.Once);
    }

    [Fact]
    public async Task Update_Missing_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.UpdateAsync(
                It.IsAny<Product>()))
            .ReturnsAsync(false);

        var result = await _controller.Update(
            999,
            new ProductUpdateDto
            {
                Name = "Missing Product",
                Price = 1000m,
                CategoryId = 1,
                StoreId = 1
            });

        Assert.IsType<NotFoundResult>(result);
    }

    [Theory]
    [InlineData("Invalid price")]
    [InlineData("Category does not exist")]
    [InlineData("Store does not exist")]
    public async Task Update_ServiceThrows_ReturnsBadRequest(
        string message)
    {
        _serviceMock.Setup(s => s.UpdateAsync(
                It.IsAny<Product>()))
            .ThrowsAsync(new ArgumentException(message));

        var result = await _controller.Update(
            1,
            new ProductUpdateDto
            {
                Name = "Updated Product",
                Price = 2000m,
                CategoryId = 1,
                StoreId = 1
            });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(CreateProduct());

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
            .ReturnsAsync((Product?)null);

        var result = await _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);

        _serviceMock.Verify(s => s.DeleteAsync(
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ServiceReturnsFalse_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(CreateProduct());

        _serviceMock.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(false);

        var result = await _controller.Delete(1);

        Assert.IsType<NotFoundResult>(result);
    }
}
