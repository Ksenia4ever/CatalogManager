using CatalogManager.ApiControllers;
using CatalogManager.Application.Interfaces;
using CatalogManager.Domain.Entities;
using CatalogManager.DTOs.Categories;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CatalogManager.UnitTests.ApiControllers;

public class CategoriesApiControllerTests
{
    private readonly Mock<ICategoryService> _serviceMock = new();
    private readonly CategoriesApiController _controller;

    public CategoriesApiControllerTests()
    {
        _controller = new CategoriesApiController(_serviceMock.Object);
    }

    private static Category CreateCategory() => new()
    {
        Id = 1,
        Name = "Electronics",
        Description = "Electronic devices"
    };

    [Fact]
    public async Task GetAll_ReturnsOkWithCategories()
    {
        var categories = new List<Category>
        {
            CreateCategory()
        };

        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(categories);

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var data = Assert.IsAssignableFrom<
            IEnumerable<CategoryResponseDto>>(ok.Value);

        var category = Assert.Single(data);

        Assert.Equal(1, category.Id);
        Assert.Equal("Electronics", category.Name);

        _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAll_Empty_ReturnsEmptyCollection()
    {
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<Category>());

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var data = Assert.IsAssignableFrom<
            IEnumerable<CategoryResponseDto>>(ok.Value);

        Assert.Empty(data);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(CreateCategory());

        var result = await _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CategoryResponseDto>(ok.Value);

        Assert.Equal(1, dto.Id);
        Assert.Equal("Electronics", dto.Name);
        Assert.Equal("Electronic devices", dto.Description);
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_Valid_ReturnsCreated()
    {
        var dto = new CategoryCreateDto
        {
            Name = "Books",
            Description = "Printed books"
        };

        _serviceMock.Setup(s => s.CreateAsync(
                It.IsAny<Category>()))
            .ReturnsAsync((Category category) =>
            {
                category.Id = 10;
                return category;
            });

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(
            result.Result);

        Assert.Equal(nameof(CategoriesApiController.GetById),
            created.ActionName);
        Assert.Equal(10, created.RouteValues!["id"]);

        var response = Assert.IsType<CategoryResponseDto>(
            created.Value);

        Assert.Equal(10, response.Id);
        Assert.Equal("Books", response.Name);

        _serviceMock.Verify(s => s.CreateAsync(
            It.Is<Category>(c =>
                c.Name == dto.Name &&
                c.Description == dto.Description)),
            Times.Once);
    }

    [Fact]
    public async Task Create_ServiceThrows_ReturnsBadRequest()
    {
        _serviceMock.Setup(s => s.CreateAsync(
                It.IsAny<Category>()))
            .ThrowsAsync(new ArgumentException("Invalid name"));

        var result = await _controller.Create(
            new CategoryCreateDto { Name = "Test" });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_Existing_ReturnsNoContent()
    {
        var dto = new CategoryUpdateDto
        {
            Name = "Updated Electronics",
            Description = "Updated description"
        };

        _serviceMock.Setup(s => s.UpdateAsync(
                It.IsAny<Category>()))
            .ReturnsAsync(true);

        var result = await _controller.Update(1, dto);

        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(s => s.UpdateAsync(
            It.Is<Category>(c =>
                c.Id == 1 &&
                c.Name == dto.Name &&
                c.Description == dto.Description)),
            Times.Once);
    }

    [Fact]
    public async Task Update_Missing_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.UpdateAsync(
                It.IsAny<Category>()))
            .ReturnsAsync(false);

        var result = await _controller.Update(
            999,
            new CategoryUpdateDto { Name = "Missing" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ServiceThrows_ReturnsBadRequest()
    {
        _serviceMock.Setup(s => s.UpdateAsync(
                It.IsAny<Category>()))
            .ThrowsAsync(new ArgumentException("Invalid name"));

        var result = await _controller.Update(
            1,
            new CategoryUpdateDto { Name = "Test" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(CreateCategory());

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
            .ReturnsAsync((Category?)null);

        var result = await _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);

        _serviceMock.Verify(s => s.DeleteAsync(
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WithProducts_ReturnsConflict()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(CreateCategory());

        _serviceMock.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(false);

        var result = await _controller.Delete(1);

        Assert.IsType<ConflictObjectResult>(result);
    }
}
