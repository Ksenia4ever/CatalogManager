
using CatalogManager.Application.Interfaces;
using CatalogManager.Controllers;
using CatalogManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CatalogManager.UnitTests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _serviceMock;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _serviceMock = new Mock<ICategoryService>();
        _controller = new CategoriesController(_serviceMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewWithCategories()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics" },
            new() { Id = 2, Name = "Books" }
        };

        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(categories);

        var result = await _controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<Category>>(view.Model);

        Assert.Equal(2, model.Count);
        _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Details_ExistingCategory_ReturnsView()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Electronics"
        };

        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(category);

        var result = await _controller.Details(1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(category, view.Model);
    }

    [Fact]
    public async Task Details_NonExistingCategory_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var result = await _controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void CreateGet_ReturnsView()
    {
        var result = _controller.Create();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task CreatePost_ValidCategory_RedirectsToIndex()
    {
        var category = new Category
        {
            Name = "Electronics"
        };

        var result = await _controller.Create(category);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);

        _serviceMock.Verify(
            s => s.CreateAsync(category),
            Times.Once);
    }

    [Fact]
    public async Task CreatePost_InvalidModel_ReturnsView()
    {
        var category = new Category { Name = "" };

        _controller.ModelState.AddModelError(
            "Name", "Name is required.");

        var result = await _controller.Create(category);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Same(category, view.Model);

        _serviceMock.Verify(
            s => s.CreateAsync(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task CreatePost_ServiceThrows_ReturnsViewWithError()
    {
        var category = new Category { Name = "Test" };

        _serviceMock.Setup(s => s.CreateAsync(category))
            .ThrowsAsync(new ArgumentException(
                "Category name is required."));

        var result = await _controller.Create(category);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Same(category, view.Model);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task EditGet_ExistingCategory_ReturnsView()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Electronics"
        };

        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(category);

        var result = await _controller.Edit(1);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Same(category, view.Model);
    }

    [Fact]
    public async Task EditGet_NonExistingCategory_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var result = await _controller.Edit(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_DifferentIds_ReturnsBadRequest()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Electronics"
        };

        var result = await _controller.Edit(2, category);

        Assert.IsType<BadRequestResult>(result);

        _serviceMock.Verify(
            s => s.UpdateAsync(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task EditPost_ValidCategory_RedirectsToIndex()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Updated"
        };

        _serviceMock.Setup(s => s.UpdateAsync(category))
            .ReturnsAsync(true);

        var result = await _controller.Edit(1, category);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task EditPost_NonExistingCategory_ReturnsNotFound()
    {
        var category = new Category
        {
            Id = 999,
            Name = "Test"
        };

        _serviceMock.Setup(s => s.UpdateAsync(category))
            .ReturnsAsync(false);

        var result = await _controller.Edit(999, category);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_InvalidModel_ReturnsView()
    {
        var category = new Category
        {
            Id = 1,
            Name = ""
        };

        _controller.ModelState.AddModelError(
            "Name", "Name is required.");

        var result = await _controller.Edit(1, category);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Same(category, view.Model);

        _serviceMock.Verify(
            s => s.UpdateAsync(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task EditPost_ServiceThrows_ReturnsViewWithError()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Test"
        };

        _serviceMock.Setup(s => s.UpdateAsync(category))
            .ThrowsAsync(new ArgumentException("Invalid category."));

        var result = await _controller.Edit(1, category);

        Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task DeleteGet_ExistingCategory_ReturnsView()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Electronics"
        };

        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(category);

        var result = await _controller.Delete(1);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Same(category, view.Model);
    }

    [Fact]
    public async Task DeleteGet_NonExistingCategory_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var result = await _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_Success_RedirectsToIndex()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.DeleteConfirmed(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);

        _serviceMock.Verify(
            s => s.DeleteAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task DeleteConfirmed_CategoryWithProducts_ReturnsViewWithError()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Electronics"
        };

        _serviceMock.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(false);

        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(category);

        var result = await _controller.DeleteConfirmed(1);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Equal("Delete", view.ViewName);
        Assert.Same(category, view.Model);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task DeleteConfirmed_NonExistingCategory_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        _serviceMock.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var result = await _controller.DeleteConfirmed(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
