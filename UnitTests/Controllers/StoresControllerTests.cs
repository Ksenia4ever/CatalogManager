
using CatalogManager.Application.Interfaces;
using CatalogManager.Controllers;
using CatalogManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CatalogManager.UnitTests.Controllers;

public class StoresControllerTests
{
    private readonly Mock<IStoreService> _serviceMock;
    private readonly StoresController _controller;

    public StoresControllerTests()
    {
        _serviceMock = new Mock<IStoreService>();
        _controller = new StoresController(_serviceMock.Object);
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
    public async Task Index_ReturnsStores()
    {
        var stores = new List<Store> { CreateStore() };

        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(stores);

        var result = await _controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(stores, view.Model);
        _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Index_EmptyList_ReturnsView()
    {
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<Store>());

        var result = await _controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Empty(Assert.IsType<List<Store>>(view.Model));
    }

    [Fact]
    public async Task Details_ExistingStore_ReturnsView()
    {
        var store = CreateStore();

        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(store);

        var result = await _controller.Details(1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(store, view.Model);
    }

    [Fact]
    public async Task Details_MissingStore_ReturnsNotFound()
    {
        var result = await _controller.Details(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void CreateGet_ReturnsView()
    {
        Assert.IsType<ViewResult>(_controller.Create());
    }

    [Fact]
    public async Task CreatePost_ValidStore_Redirects()
    {
        var store = CreateStore();

        var result = await _controller.Create(store);

        var redirect =
            Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);

        _serviceMock.Verify(
            s => s.CreateAsync(store), Times.Once);
    }

    [Fact]
    public async Task CreatePost_InvalidModel_ReturnsView()
    {
        var store = CreateStore();

        _controller.ModelState.AddModelError(
            "Name", "Name is required");

        var result = await _controller.Create(store);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(store, view.Model);

        _serviceMock.Verify(
            s => s.CreateAsync(It.IsAny<Store>()),
            Times.Never);
    }

    [Fact]
    public async Task CreatePost_InvalidCoordinates_ReturnsError()
    {
        var store = CreateStore();
        store.Latitude = 100;

        _serviceMock.Setup(s => s.CreateAsync(store))
            .ThrowsAsync(new ArgumentException(
                "Invalid latitude."));

        var result = await _controller.Create(store);

        Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task CreatePost_InvalidAddress_ReturnsError()
    {
        var store = CreateStore();
        store.Address = "";

        _serviceMock.Setup(s => s.CreateAsync(store))
            .ThrowsAsync(new ArgumentException(
                "Store address is required."));

        var result = await _controller.Create(store);

        Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task EditGet_ExistingStore_ReturnsView()
    {
        var store = CreateStore();

        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(store);

        var result = await _controller.Edit(1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(store, view.Model);
    }

    [Fact]
    public async Task EditGet_MissingStore_ReturnsNotFound()
    {
        var result = await _controller.Edit(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_DifferentIds_ReturnsBadRequest()
    {
        var store = CreateStore();

        var result = await _controller.Edit(2, store);

        Assert.IsType<BadRequestResult>(result);

        _serviceMock.Verify(
            s => s.UpdateAsync(It.IsAny<Store>()),
            Times.Never);
    }

    [Fact]
    public async Task EditPost_ValidStore_Redirects()
    {
        var store = CreateStore();

        _serviceMock.Setup(s => s.UpdateAsync(store))
            .ReturnsAsync(true);

        var result = await _controller.Edit(1, store);

        var redirect =
            Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);
        _serviceMock.Verify(
            s => s.UpdateAsync(store), Times.Once);
    }

    [Fact]
    public async Task EditPost_MissingStore_ReturnsNotFound()
    {
        var store = CreateStore();

        _serviceMock.Setup(s => s.UpdateAsync(store))
            .ReturnsAsync(false);

        var result = await _controller.Edit(1, store);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_InvalidModel_ReturnsView()
    {
        var store = CreateStore();

        _controller.ModelState.AddModelError(
            "Address", "Address is required");

        var result = await _controller.Edit(1, store);

        Assert.IsType<ViewResult>(result);

        _serviceMock.Verify(
            s => s.UpdateAsync(It.IsAny<Store>()),
            Times.Never);
    }

    [Fact]
    public async Task EditPost_ServiceThrows_ReturnsError()
    {
        var store = CreateStore();

        _serviceMock.Setup(s => s.UpdateAsync(store))
            .ThrowsAsync(new ArgumentException(
                "Invalid longitude."));

        var result = await _controller.Edit(1, store);

        Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task DeleteGet_ExistingStore_ReturnsView()
    {
        var store = CreateStore();

        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(store);

        var result = await _controller.Delete(1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(store, view.Model);
    }

    [Fact]
    public async Task DeleteGet_MissingStore_ReturnsNotFound()
    {
        var result = await _controller.Delete(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_Success_Redirects()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.DeleteConfirmed(1);

        var redirect =
            Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);
        _serviceMock.Verify(
            s => s.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteConfirmed_WithProducts_ReturnsError()
    {
        var store = CreateStore();

        _serviceMock.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(false);

        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(store);

        var result = await _controller.DeleteConfirmed(1);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Equal("Delete", view.ViewName);
        Assert.Same(store, view.Model);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task DeleteConfirmed_MissingStore_ReturnsNotFound()
    {
        var result = await _controller.DeleteConfirmed(999);
        Assert.IsType<NotFoundResult>(result);
    }
}
