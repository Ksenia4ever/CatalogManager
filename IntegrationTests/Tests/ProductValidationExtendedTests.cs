using System.Net;
using System.Net.Http.Json;
using CatalogManager.DTOs.Products;
using CatalogManager.IntegrationTests.Infrastructure;

namespace CatalogManager.IntegrationTests.Tests;

[Collection("DatabaseIntegration")]
public class ProductValidationExtendedTests
{
    [Theory]
    [InlineData(-100, 1, 1)]
    [InlineData(0, 1, 1)]
    [InlineData(100, 0, 1)]
    [InlineData(100, 1, 0)]
    [InlineData(100, -1, 1)]
    [InlineData(100, 1, -1)]
    public async Task CreateProduct_InvalidData_Returns400(
        decimal price,
        int categoryId,
        int storeId)
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = factory.CreateClient();

        var dto = new ProductCreateDto
        {
            Name = "Invalid Product",
            Description = "Validation test",
            Price = price,
            CategoryId = categoryId,
            StoreId = storeId
        };

        using var response = await client.PostAsJsonAsync(
            "/api/products",
            dto);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    public async Task UpdateProduct_InvalidPrice_Returns400(
        decimal price)
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = factory.CreateClient();

        var dto = new ProductUpdateDto
        {
            Name = "Invalid Product",
            Price = price,
            CategoryId = 1,
            StoreId = 1
        };

        using var response = await client.PutAsJsonAsync(
            "/api/products/2147483647",
            dto);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Theory]
    [InlineData(int.MaxValue, 1)]
    [InlineData(1, int.MaxValue)]
    public async Task CreateProduct_MissingRelations_Returns400(
        int categoryId,
        int storeId)
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = factory.CreateClient();

        // Перевіряю неіснуючі зовнішні ключі.
        var dto = new ProductCreateDto
        {
            Name = "Invalid Related Product",
            Price = 1000,
            CategoryId = categoryId,
            StoreId = storeId
        };

        using var response = await client.PostAsJsonAsync(
            "/api/products",
            dto);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}