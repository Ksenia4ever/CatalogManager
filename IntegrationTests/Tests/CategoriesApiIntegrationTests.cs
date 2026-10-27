using System.Net;
using System.Net.Http.Json;
using CatalogManager.DTOs.Categories;
using CatalogManager.IntegrationTests.Infrastructure;
using CatalogManager.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogManager.IntegrationTests.Tests;

[Collection("DatabaseIntegration")]
public class CategoriesApiIntegrationTests
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

    private static async Task<int> ReadIdAsync(
        HttpResponseMessage response)
    {
        var dto = await response.Content
            .ReadFromJsonAsync<CategoryResponseDto>();

        Assert.NotNull(dto);
        Assert.True(dto.Id > 0);

        return dto.Id;
    }

    [Fact]
    public async Task CategoryApi_FullCrud_Works()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        int categoryId = 0;

        try
        {
            // CREATE
            using var createResponse = await client.PostAsJsonAsync(
                "/api/categories",
                new CategoryCreateDto
                {
                    Name = "API Integration " +
                           Guid.NewGuid().ToString("N"),
                    Description = "Created by integration test"
                });

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            categoryId = await ReadIdAsync(createResponse);

            Assert.NotNull(createResponse.Headers.Location);

            // READ
            using var getResponse = await client.GetAsync(
                $"/api/categories/{categoryId}");

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            var category = await getResponse.Content
                .ReadFromJsonAsync<CategoryResponseDto>();

            Assert.NotNull(category);
            Assert.Equal(categoryId, category.Id);
            Assert.Equal(
                "Created by integration test",
                category.Description);

            // READ ALL
            using var listResponse = await client.GetAsync(
                "/api/categories");

            Assert.Equal(
                HttpStatusCode.OK,
                listResponse.StatusCode);

            var categories = await listResponse.Content
                .ReadFromJsonAsync<List<CategoryResponseDto>>();

            Assert.NotNull(categories);
            Assert.Contains(categories,
                c => c.Id == categoryId);

            // UPDATE
            using var updateResponse = await client.PutAsJsonAsync(
                $"/api/categories/{categoryId}",
                new CategoryUpdateDto
                {
                    Name = "Updated API Category",
                    Description = "Updated by integration test"
                });

            Assert.Equal(
                HttpStatusCode.NoContent,
                updateResponse.StatusCode);

            // VERIFY UPDATE
            var updated = await client.GetFromJsonAsync<
                CategoryResponseDto>(
                $"/api/categories/{categoryId}");

            Assert.NotNull(updated);
            Assert.Equal(
                "Updated API Category",
                updated.Name);

            // DELETE
            using var deleteResponse = await client.DeleteAsync(
                $"/api/categories/{categoryId}");

            Assert.Equal(
                HttpStatusCode.NoContent,
                deleteResponse.StatusCode);

            // VERIFY DELETE
            using var missingResponse = await client.GetAsync(
                $"/api/categories/{categoryId}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                missingResponse.StatusCode);
        }
        finally
        {
            // Видаляємо тільки запис цього тесту,
            // якщо він залишився через помилку.
            if (categoryId > 0)
            {
                using var scope = factory.Services.CreateScope();

                var db = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                await db.Categories
                    .Where(c => c.Id == categoryId)
                    .ExecuteDeleteAsync();
            }
        }
    }

    [Fact]
    public async Task CategoryApi_InvalidName_Returns400()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        using var response = await client.PostAsJsonAsync(
            "/api/categories",
            new CategoryCreateDto
            {
                Name = "",
                Description = "Invalid category"
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Category name is required",
            body);
    }

    [Fact]
    public async Task CategoryApi_MissingId_Returns404()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        using var getResponse = await client.GetAsync(
            "/api/categories/2147483647");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);

        using var updateResponse = await client.PutAsJsonAsync(
            "/api/categories/2147483647",
            new CategoryUpdateDto
            {
                Name = "Missing category"
            });

        Assert.Equal(
            HttpStatusCode.NotFound,
            updateResponse.StatusCode);

        using var deleteResponse = await client.DeleteAsync(
            "/api/categories/2147483647");

        Assert.Equal(
            HttpStatusCode.NotFound,
            deleteResponse.StatusCode);
    }
}
