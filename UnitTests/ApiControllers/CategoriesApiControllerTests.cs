using CatalogManager.Application.Models;
using Microsoft.AspNetCore.Http;
using CatalogManager.DTOs.Pagination;
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
    public async Task GetAll_WithPagination_ReturnsPageAndNextLink()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        var request = _controller.HttpContext.Request;
        request.Scheme = "https";
        request.Host = new HostString("localhost", 5001);
        request.Path = "/api/categories";
        request.QueryString = new QueryString("?skip=0&limit=1");

        _serviceMock.Setup(s => s.GetPagedAsync(0, 1))
            .ReturnsAsync(new PagedResult<Category>(new List<Category>(), 3));

        var result = await _controller.GetAll();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var page = Assert.IsType<PagedResponseDto<CategoryResponseDto>>(ok.Value);
        Assert.Equal(0, page.Skip);
        Assert.Equal(1, page.Limit);
        Assert.Equal(3, page.TotalCount);
        Assert.Equal("https://localhost:5001/api/categories?skip=1&limit=1", page.NextLink);
        _serviceMock.Verify(s => s.GetPagedAsync(0, 1), Times.Once);
    }

    [Theory]
    [InlineData("?skip=-1&limit=10")]
    [InlineData("?skip=0&limit=0")]
    [InlineData("?skip=abc&limit=10")]
    [InlineData("?skip=0&limit=101")]
    public async Task GetAll_InvalidPagination_ReturnsBadRequest(string query)
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.HttpContext.Request.QueryString = new QueryString(query);
        var result = await _controller.GetAll();
        Assert.IsType<BadRequestObjectResult>(result.Result);
        _serviceMock.Verify(s => s.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAll_LastPage_HasNullNextLink()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.HttpContext.Request.QueryString = new QueryString("?skip=10&limit=5");
        _serviceMock.Setup(s => s.GetPagedAsync(10, 5))
            .ReturnsAsync(new PagedResult<Category>(new List<Category>(), 12));
        var result = await _controller.GetAll();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var page = Assert.IsType<PagedResponseDto<CategoryResponseDto>>(ok.Value);
        Assert.Null(page.NextLink);
    }

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