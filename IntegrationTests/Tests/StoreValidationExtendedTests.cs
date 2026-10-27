using System.Net;
using System.Net.Http.Json;
using CatalogManager.DTOs.Stores;
using CatalogManager.IntegrationTests.Infrastructure;

namespace CatalogManager.IntegrationTests.Tests;

[Collection("DatabaseIntegration")]
public class StoreValidationExtendedTests
{
    [Theory]
    [InlineData("", "Kyiv", 50.45, 30.52)]
    [InlineData("Test Store", "", 50.45, 30.52)]
    [InlineData("Test Store", "Kyiv", 100, 30.52)]
    [InlineData("Test Store", "Kyiv", -100, 30.52)]
    [InlineData("Test Store", "Kyiv", 50.45, 200)]
    [InlineData("Test Store", "Kyiv", 50.45, -200)]
    public async Task UpdateStore_InvalidData_Returns400(
        string name,
        string address,
        double latitude,
        double longitude)
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = factory.CreateClient();

        // Перевіряю валідацію запиту PUT.
        var dto = new StoreUpdateDto
        {
            Name = name,
            Address = address,
            Latitude = latitude,
            Longitude = longitude
        };

        using var response = await client.PutAsJsonAsync(
            "/api/stores/2147483647",
            dto);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("Address")]
    public async Task CreateStore_NullRequiredField_Returns400(
        string field)
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = factory.CreateClient();

        // Перевіряю обов'язкові поля магазину.
        var body = new Dictionary<string, object?>
        {
            ["name"] = "Test Store",
            ["address"] = "Kyiv",
            ["latitude"] = 50.45,
            ["longitude"] = 30.52
        };

        body[field.ToLowerInvariant()] = null;

        using var response = await client.PostAsJsonAsync(
            "/api/stores",
            body);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}