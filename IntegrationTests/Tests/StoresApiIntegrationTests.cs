using System.Net;
using System.Net.Http.Json;
using CatalogManager.DTOs.Stores;
using CatalogManager.IntegrationTests.Infrastructure;
using CatalogManager.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogManager.IntegrationTests.Tests;

[Collection("DatabaseIntegration")]
public class StoresApiIntegrationTests
{
    private static HttpClient CreateClient(
        CatalogManagerWebApplicationFactory factory)
    {
        return factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task StoreApi_FullCrud_Works()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        int storeId = 0;

        try
        {
            // CREATE
            using var createResponse = await client.PostAsJsonAsync(
                "/api/stores",
                new StoreCreateDto
                {
                    Name = "API Store " +
                           Guid.NewGuid().ToString("N"),
                    Address = "Kyiv, Test Street 1",
                    Latitude = 50.4501,
                    Longitude = 30.5234
                });

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var created = await createResponse.Content
                .ReadFromJsonAsync<StoreResponseDto>();

            Assert.NotNull(created);
            storeId = created.Id;

            Assert.True(storeId > 0);
            Assert.NotNull(createResponse.Headers.Location);

            // READ
            using var getResponse = await client.GetAsync(
                $"/api/stores/{storeId}");

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            var store = await getResponse.Content
                .ReadFromJsonAsync<StoreResponseDto>();

            Assert.NotNull(store);
            Assert.Equal(storeId, store.Id);
            Assert.Equal("Kyiv, Test Street 1", store.Address);
            Assert.Equal(50.4501, store.Latitude);
            Assert.Equal(30.5234, store.Longitude);

            // READ ALL
            using var listResponse = await client.GetAsync(
                "/api/stores");

            Assert.Equal(
                HttpStatusCode.OK,
                listResponse.StatusCode);

            var stores = await listResponse.Content
                .ReadFromJsonAsync<List<StoreResponseDto>>();

            Assert.NotNull(stores);
            Assert.Contains(stores, s => s.Id == storeId);

            // UPDATE
            using var updateResponse = await client.PutAsJsonAsync(
                $"/api/stores/{storeId}",
                new StoreUpdateDto
                {
                    Name = "Updated API Store",
                    Address = "Kyiv, Updated Street 2",
                    Latitude = 49.5,
                    Longitude = 31.5
                });

            Assert.Equal(
                HttpStatusCode.NoContent,
                updateResponse.StatusCode);

            // VERIFY UPDATE
            var updated = await client.GetFromJsonAsync<
                StoreResponseDto>(
                $"/api/stores/{storeId}");

            Assert.NotNull(updated);
            Assert.Equal("Updated API Store", updated.Name);
            Assert.Equal("Kyiv, Updated Street 2",
                updated.Address);
            Assert.Equal(49.5, updated.Latitude);
            Assert.Equal(31.5, updated.Longitude);

            // DELETE
            using var deleteResponse = await client.DeleteAsync(
                $"/api/stores/{storeId}");

            Assert.Equal(
                HttpStatusCode.NoContent,
                deleteResponse.StatusCode);

            // VERIFY DELETE
            using var missingResponse = await client.GetAsync(
                $"/api/stores/{storeId}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                missingResponse.StatusCode);
        }
        finally
        {
            if (storeId > 0)
            {
                using var scope = factory.Services.CreateScope();

                var db = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                await db.Stores
                    .Where(s => s.Id == storeId)
                    .ExecuteDeleteAsync();
            }
        }
    }

    [Theory]
    [InlineData(100.0, 30.5)]
    [InlineData(50.5, 200.0)]
    [InlineData(-100.0, 30.5)]
    [InlineData(50.5, -200.0)]
    public async Task StoreApi_InvalidCoordinates_Return400(
        double latitude, double longitude)
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        using var response = await client.PostAsJsonAsync(
            "/api/stores",
            new StoreCreateDto
            {
                Name = "Invalid Coordinates Store",
                Address = "Kyiv",
                Latitude = latitude,
                Longitude = longitude
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task StoreApi_EmptyName_Returns400()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        using var response = await client.PostAsJsonAsync(
            "/api/stores",
            new StoreCreateDto
            {
                Name = "",
                Address = "Kyiv",
                Latitude = 50.45,
                Longitude = 30.52
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task StoreApi_MissingId_Returns404()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        using var getResponse = await client.GetAsync(
            "/api/stores/2147483647");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);

        using var updateResponse = await client.PutAsJsonAsync(
            "/api/stores/2147483647",
            new StoreUpdateDto
            {
                Name = "Missing store",
                Address = "Kyiv",
                Latitude = 50.45,
                Longitude = 30.52
            });

        Assert.Equal(
            HttpStatusCode.NotFound,
            updateResponse.StatusCode);

        using var deleteResponse = await client.DeleteAsync(
            "/api/stores/2147483647");

        Assert.Equal(
            HttpStatusCode.NotFound,
            deleteResponse.StatusCode);
    }
}
