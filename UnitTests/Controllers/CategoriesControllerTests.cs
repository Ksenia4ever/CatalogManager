
using CatalogManager.Application.Interfaces;
using CatalogManager.Controllers;
using CatalogManager.Domain.Entities;
using CatalogManager.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CatalogManager.UnitTests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _mock = new();
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _controller = new CategoriesController(_mock.Object);
    }

    private static Category Entity() => new()
    {
        Id = 1,
        Name = "Electronics",
        Description = "Devices"
    };

    private static CategoryFormViewModel Form() => new()
    {
        Id = 1,
        Name = "Electronics",
        Description = "Devices"
    };

    [Fact]
    public async Task Index_ReturnsCategories()
    {
        var categories = new List<Category> { Entity() };

        _mock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(categories);

        var result = await _controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(categories, view.Model);
        _mock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Details_Existing_ReturnsView()
    {
        var category = Entity();
        _mock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(category);

        var result = await _controller.Details(1);

        Assert.Same(category,
            Assert.IsType<ViewResult>(result).Model);
    }

    [Fact]
    public async Task Details_Missing_ReturnsNotFound()
    {
        var result = await _controller.Details(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void CreateGet_ReturnsForm()
    {
        var result = _controller.Create();

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<CategoryFormViewModel>(view.Model);
    }

    [Fact]
    public async Task CreatePost_Valid_Redirects()
    {
        var model = Form();

        var result = await _controller.Create(model);

        var redirect =
            Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);

        _mock.Verify(x => x.CreateAsync(
            It.Is<Category>(c =>
                c.Name == model.Name &&
                c.Description == model.Description &&
                c.Id == 0)), Times.Once);
    }

    [Fact]
    public async Task CreatePost_Invalid_ReturnsForm()
    {
        var model = Form();
        model.Name = "";

        _controller.ModelState.AddModelError(
            "Name", "Required");

        var result = await _controller.Create(model);

        Assert.Same(model,
            Assert.IsType<ViewResult>(result).Model);

        _mock.Verify(x => x.CreateAsync(
            It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task CreatePost_ServiceThrows_ReturnsError()
    {
        var model = Form();

        _mock.Setup(x => x.CreateAsync(
            It.IsAny<Category>()))
            .ThrowsAsync(new ArgumentException("Invalid name"));

        var result = await _controller.Create(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task EditGet_Existing_ReturnsForm()
    {
        _mock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Entity());

        var result = await _controller.Edit(1);

        var view = Assert.IsType<ViewResult>(result);
        var model =
            Assert.IsType<CategoryFormViewModel>(view.Model);

        Assert.Equal(1, model.Id);
        Assert.Equal("Electronics", model.Name);
        Assert.Equal("Devices", model.Description);
    }

    [Fact]
    public async Task EditGet_Missing_ReturnsNotFound()
    {
        var result = await _controller.Edit(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_DifferentIds_ReturnsBadRequest()
    {
        var result = await _controller.Edit(2, Form());

        Assert.IsType<BadRequestResult>(result);

        _mock.Verify(x => x.UpdateAsync(
            It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task EditPost_Valid_Redirects()
    {
        var model = Form();

        _mock.Setup(x => x.UpdateAsync(
            It.IsAny<Category>())).ReturnsAsync(true);

        var result = await _controller.Edit(1, model);

        Assert.IsType<RedirectToActionResult>(result);

        _mock.Verify(x => x.UpdateAsync(
            It.Is<Category>(c =>
                c.Id == model.Id &&
                c.Name == model.Name &&
                c.Description == model.Description)),
            Times.Once);
    }

    [Fact]
    public async Task EditPost_Missing_ReturnsNotFound()
    {
        _mock.Setup(x => x.UpdateAsync(
            It.IsAny<Category>())).ReturnsAsync(false);

        var result = await _controller.Edit(1, Form());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_Invalid_ReturnsForm()
    {
        var model = Form();

        _controller.ModelState.AddModelError(
            "Name", "Required");

        var result = await _controller.Edit(1, model);

        Assert.Same(model,
            Assert.IsType<ViewResult>(result).Model);

        _mock.Verify(x => x.UpdateAsync(
            It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task EditPost_ServiceThrows_ReturnsError()
    {
        _mock.Setup(x => x.UpdateAsync(
            It.IsAny<Category>()))
            .ThrowsAsync(new ArgumentException("Invalid name"));

        var result = await _controller.Edit(1, Form());

        Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task DeleteGet_Existing_ReturnsView()
    {
        var category = Entity();

        _mock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(category);

        var result = await _controller.Delete(1);

        Assert.Same(category,
            Assert.IsType<ViewResult>(result).Model);
    }

    [Fact]
    public async Task DeleteGet_Missing_ReturnsNotFound()
    {
        var result = await _controller.Delete(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_Success_Redirects()
    {
        _mock.Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.DeleteConfirmed(1);

        Assert.IsType<RedirectToActionResult>(result);
        _mock.Verify(x => x.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteConfirmed_WithProducts_ReturnsError()
    {
        var category = Entity();

        _mock.Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(false);

        _mock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(category);

        var result = await _controller.DeleteConfirmed(1);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Equal("Delete", view.ViewName);
        Assert.Same(category, view.Model);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task DeleteConfirmed_Missing_ReturnsNotFound()
    {
        var result = await _controller.DeleteConfirmed(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
