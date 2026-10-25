
using CatalogManager.Application.Interfaces;
using CatalogManager.Controllers;
using CatalogManager.Domain.Entities;
using CatalogManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;

namespace CatalogManager.UnitTests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productMock = new();
    private readonly Mock<ICategoryService> _categoryMock = new();
    private readonly Mock<IStoreService> _storeMock = new();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _controller = new ProductsController(
            _productMock.Object,
            _categoryMock.Object,
            _storeMock.Object);

        SetupLists();
    }

    private void SetupLists()
    {
        _categoryMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<Category>
            {
                new() { Id = 1, Name = "Electronics" }
            });

        _storeMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<Store>
            {
                new() { Id = 1, Name = "Tech Store" }
            });
    }

    private static ProductFormViewModel ValidModel() => new()
    {
        Id = 1,
        Name = "Laptop",
        Description = "Test laptop",
        Price = 25000m,
        ImageUrl = "https://example.com/laptop.jpg",
        CategoryId = 1,
        StoreId = 1
    };

    private static Product ValidProduct() => new()
    {
        Id = 1,
        Name = "Laptop",
        Description = "Test laptop",
        Price = 25000m,
        ImageUrl = "https://example.com/laptop.jpg",
        CategoryId = 1,
        StoreId = 1
    };

    [Fact]
    public async Task Index_ReturnsProducts()
    {
        var products = new List<Product> { ValidProduct() };

        _productMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(products);

        var result = await _controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(products, view.Model);

        _productMock.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Index_EmptyList_ReturnsView()
    {
        _productMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<Product>());

        var result = await _controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Empty(Assert.IsType<List<Product>>(view.Model));
    }

    [Fact]
    public async Task Details_ExistingProduct_ReturnsView()
    {
        var product = ValidProduct();

        _productMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(product);

        var result = await _controller.Details(1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(product, view.Model);
    }

    [Fact]
    public async Task Details_MissingProduct_ReturnsNotFound()
    {
        var result = await _controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateGet_PopulatesDropdowns()
    {
        var result = await _controller.Create();

        var view = Assert.IsType<ViewResult>(result);
        var model =
            Assert.IsType<ProductFormViewModel>(view.Model);

        Assert.Single(model.Categories);
        Assert.Single(model.Stores);

        Assert.Equal("Electronics", model.Categories.Single().Text);
        Assert.Equal("Tech Store", model.Stores.Single().Text);

        _categoryMock.Verify(s => s.GetAllAsync(), Times.Once);
        _storeMock.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreatePost_ValidProduct_Redirects()
    {
        var model = ValidModel();

        var result = await _controller.Create(model);

        var redirect =
            Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);

        _productMock.Verify(
            s => s.CreateAsync(It.Is<Product>(p =>
                p.Name == model.Name &&
                p.Description == model.Description &&
                p.Price == model.Price &&
                p.ImageUrl == model.ImageUrl &&
                p.CategoryId == model.CategoryId &&
                p.StoreId == model.StoreId)),
            Times.Once);
    }

    [Fact]
    public async Task CreatePost_InvalidModel_ReturnsView()
    {
        var model = ValidModel();
        model.Name = "";

        _controller.ModelState.AddModelError(
            "Name", "Name is required");

        var result = await _controller.Create(model);

        var view = Assert.IsType<ViewResult>(result);
        var returned =
            Assert.IsType<ProductFormViewModel>(view.Model);

        Assert.Same(model, returned);
        Assert.Single(returned.Categories);
        Assert.Single(returned.Stores);

        _productMock.Verify(
            s => s.CreateAsync(It.IsAny<Product>()),
            Times.Never);
    }

    [Fact]
    public async Task CreatePost_InvalidPrice_ReturnsView()
    {
        var model = ValidModel();
        model.Price = 0;

        _controller.ModelState.AddModelError(
            "Price", "Invalid price");

        var result = await _controller.Create(model);

        Assert.IsType<ViewResult>(result);

        _productMock.Verify(
            s => s.CreateAsync(It.IsAny<Product>()),
            Times.Never);
    }

    [Fact]
    public async Task CreatePost_ServiceThrows_ReturnsError()
    {
        var model = ValidModel();

        _productMock
            .Setup(s => s.CreateAsync(It.IsAny<Product>()))
            .ThrowsAsync(new ArgumentException(
                "Selected category does not exist."));

        var result = await _controller.Create(model);

        var view = Assert.IsType<ViewResult>(result);
        var returned =
            Assert.IsType<ProductFormViewModel>(view.Model);

        Assert.False(_controller.ModelState.IsValid);
        Assert.Single(returned.Categories);
        Assert.Single(returned.Stores);
    }

    [Fact]
    public async Task EditGet_ExistingProduct_PopulatesModel()
    {
        var product = ValidProduct();

        _productMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(product);

        var result = await _controller.Edit(1);

        var view = Assert.IsType<ViewResult>(result);
        var model =
            Assert.IsType<ProductFormViewModel>(view.Model);

        Assert.Equal(product.Id, model.Id);
        Assert.Equal(product.Name, model.Name);
        Assert.Equal(product.Description, model.Description);
        Assert.Equal(product.Price, model.Price);
        Assert.Equal(product.ImageUrl, model.ImageUrl);
        Assert.Equal(product.CategoryId, model.CategoryId);
        Assert.Equal(product.StoreId, model.StoreId);

        Assert.Single(model.Categories);
        Assert.Single(model.Stores);
    }

    [Fact]
    public async Task EditGet_MissingProduct_ReturnsNotFound()
    {
        var result = await _controller.Edit(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_DifferentIds_ReturnsBadRequest()
    {
        var model = ValidModel();

        var result = await _controller.Edit(2, model);

        Assert.IsType<BadRequestResult>(result);

        _productMock.Verify(
            s => s.UpdateAsync(It.IsAny<Product>()),
            Times.Never);
    }

    [Fact]
    public async Task EditPost_ValidProduct_Redirects()
    {
        var model = ValidModel();

        _productMock
            .Setup(s => s.UpdateAsync(It.IsAny<Product>()))
            .ReturnsAsync(true);

        var result = await _controller.Edit(1, model);

        var redirect =
            Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);

        _productMock.Verify(
            s => s.UpdateAsync(It.Is<Product>(p =>
                p.Id == model.Id &&
                p.Name == model.Name &&
                p.Description == model.Description &&
                p.Price == model.Price &&
                p.ImageUrl == model.ImageUrl &&
                p.CategoryId == model.CategoryId &&
                p.StoreId == model.StoreId)),
            Times.Once);
    }

    [Fact]
    public async Task EditPost_MissingProduct_ReturnsNotFound()
    {
        var model = ValidModel();

        _productMock
            .Setup(s => s.UpdateAsync(It.IsAny<Product>()))
            .ReturnsAsync(false);

        var result = await _controller.Edit(1, model);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_InvalidModel_ReturnsView()
    {
        var model = ValidModel();

        _controller.ModelState.AddModelError(
            "Name", "Invalid name");

        var result = await _controller.Edit(1, model);

        var view = Assert.IsType<ViewResult>(result);
        var returned =
            Assert.IsType<ProductFormViewModel>(view.Model);

        Assert.Single(returned.Categories);
        Assert.Single(returned.Stores);

        _productMock.Verify(
            s => s.UpdateAsync(It.IsAny<Product>()),
            Times.Never);
    }

    [Fact]
    public async Task EditPost_ServiceThrows_ReturnsError()
    {
        var model = ValidModel();

        _productMock
            .Setup(s => s.UpdateAsync(It.IsAny<Product>()))
            .ThrowsAsync(new ArgumentException(
                "Selected store does not exist."));

        var result = await _controller.Edit(1, model);

        Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task DeleteGet_ExistingProduct_ReturnsView()
    {
        var product = ValidProduct();

        _productMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(product);

        var result = await _controller.Delete(1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(product, view.Model);
    }

    [Fact]
    public async Task DeleteGet_MissingProduct_ReturnsNotFound()
    {
        var result = await _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_Success_Redirects()
    {
        _productMock.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.DeleteConfirmed(1);

        var redirect =
            Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);

        _productMock.Verify(
            s => s.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteConfirmed_MissingProduct_ReturnsNotFound()
    {
        _productMock.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        var result = await _controller.DeleteConfirmed(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
