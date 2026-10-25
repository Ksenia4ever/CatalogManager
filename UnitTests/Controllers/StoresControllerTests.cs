
using CatalogManager.Application.Interfaces;
using CatalogManager.Controllers;
using CatalogManager.Domain.Entities;
using CatalogManager.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CatalogManager.UnitTests.Controllers;

public class StoresControllerTests
{
    private readonly Mock<IStoreService> _mock = new();
    private readonly StoresController _controller;

    public StoresControllerTests()
    {
        _controller = new StoresController(_mock.Object);
    }

    private static Store Entity() => new()
    {
        Id = 1,
        Name = "Tech Store",
        Address = "Kyiv",
        Latitude = 50.4501,
        Longitude = 30.5234
    };

    private static StoreFormViewModel Form() => new()
    {
        Id = 1,
        Name = "Tech Store",
        Address = "Kyiv",
        Latitude = 50.4501,
        Longitude = 30.5234
    };

    [Fact]
    public async Task Index_ReturnsStores()
    {
        var stores = new List<Store> { Entity() };

        _mock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(stores);

        var result = await _controller.Index();

        Assert.Same(stores,
            Assert.IsType<ViewResult>(result).Model);

        _mock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Index_Empty_ReturnsEmptyList()
    {
        _mock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Store>());

        var result = await _controller.Index();

        Assert.Empty(
            Assert.IsType<List<Store>>(
                Assert.IsType<ViewResult>(result).Model));
    }

    [Fact]
    public async Task Details_Existing_ReturnsView()
    {
        var store = Entity();

        _mock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(store);

        var result = await _controller.Details(1);

        Assert.Same(store,
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

        Assert.IsType<StoreFormViewModel>(
            Assert.IsType<ViewResult>(result).Model);
    }

    [Fact]
    public async Task CreatePost_Valid_Redirects()
    {
        var model = Form();

        var result = await _controller.Create(model);

        Assert.IsType<RedirectToActionResult>(result);

        _mock.Verify(x => x.CreateAsync(
            It.Is<Store>(s =>
                s.Id == 0 &&
                s.Name == model.Name &&
                s.Address == model.Address &&
                s.Latitude == model.Latitude &&
                s.Longitude == model.Longitude)),
            Times.Once);
    }

    [Fact]
    public async Task CreatePost_Invalid_ReturnsForm()
    {
        var model = Form();

        _controller.ModelState.AddModelError(
            "Name", "Required");

        var result = await _controller.Create(model);

        Assert.Same(model,
            Assert.IsType<ViewResult>(result).Model);

        _mock.Verify(x => x.CreateAsync(
            It.IsAny<Store>()), Times.Never);
    }

    [Fact]
    public async Task CreatePost_InvalidCoordinates_ReturnsError()
    {
        var model = Form();
        model.Latitude = 100;

        _mock.Setup(x => x.CreateAsync(
            It.IsAny<Store>()))
            .ThrowsAsync(new ArgumentException(
                "Invalid latitude"));

        var result = await _controller.Create(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task CreatePost_InvalidAddress_ReturnsError()
    {
        var model = Form();
        model.Address = "";

        _mock.Setup(x => x.CreateAsync(
            It.IsAny<Store>()))
            .ThrowsAsync(new ArgumentException(
                "Address is required"));

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

        var model = Assert.IsType<StoreFormViewModel>(
            Assert.IsType<ViewResult>(result).Model);

        Assert.Equal(1, model.Id);
        Assert.Equal("Tech Store", model.Name);
        Assert.Equal("Kyiv", model.Address);
        Assert.Equal(50.4501, model.Latitude);
        Assert.Equal(30.5234, model.Longitude);
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
            It.IsAny<Store>()), Times.Never);
    }

    [Fact]
    public async Task EditPost_Valid_Redirects()
    {
        var model = Form();

        _mock.Setup(x => x.UpdateAsync(
            It.IsAny<Store>())).ReturnsAsync(true);

        var result = await _controller.Edit(1, model);

        Assert.IsType<RedirectToActionResult>(result);

        _mock.Verify(x => x.UpdateAsync(
            It.Is<Store>(s =>
                s.Id == model.Id &&
                s.Name == model.Name &&
                s.Address == model.Address &&
                s.Latitude == model.Latitude &&
                s.Longitude == model.Longitude)),
            Times.Once);
    }

    [Fact]
    public async Task EditPost_Missing_ReturnsNotFound()
    {
        _mock.Setup(x => x.UpdateAsync(
            It.IsAny<Store>())).ReturnsAsync(false);

        var result = await _controller.Edit(1, Form());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_Invalid_ReturnsForm()
    {
        var model = Form();

        _controller.ModelState.AddModelError(
            "Address", "Required");

        var result = await _controller.Edit(1, model);

        Assert.Same(model,
            Assert.IsType<ViewResult>(result).Model);

        _mock.Verify(x => x.UpdateAsync(
            It.IsAny<Store>()), Times.Never);
    }

    [Fact]
    public async Task EditPost_ServiceThrows_ReturnsError()
    {
        _mock.Setup(x => x.UpdateAsync(
            It.IsAny<Store>()))
            .ThrowsAsync(new ArgumentException(
                "Invalid longitude"));

        var result = await _controller.Edit(1, Form());

        Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task DeleteGet_Existing_ReturnsView()
    {
        var store = Entity();

        _mock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(store);

        var result = await _controller.Delete(1);

        Assert.Same(store,
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
        var store = Entity();

        _mock.Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(false);

        _mock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(store);

        var result = await _controller.DeleteConfirmed(1);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Equal("Delete", view.ViewName);
        Assert.Same(store, view.Model);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task DeleteConfirmed_Missing_ReturnsNotFound()
    {
        var result = await _controller.DeleteConfirmed(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
