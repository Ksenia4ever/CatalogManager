
using CatalogManager.Domain.Entities;
using CatalogManager.Infrastructure.Data;
using CatalogManager.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.RegularExpressions;

namespace CatalogManager.IntegrationTests.Tests;

public class MvcCrudIntegrationTests
{
    private static string UniqueName(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}";

    private static async Task<string> GetTokenAsync(
        HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();

        var match = Regex.Match(
            html,
            "<input(?=[^>]*name=\"__RequestVerificationToken\")" +
            "(?=[^>]*value=\"([^\"]+)\")[^>]*>",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success,
            $"Anti-forgery token not found at {url}");

        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static async Task<HttpResponseMessage> PostFormAsync(
        HttpClient client,
        string url,
        Dictionary<string, string> fields)
    {
        fields["__RequestVerificationToken"] =
            await GetTokenAsync(client, url);

        return await client.PostAsync(
            url, new FormUrlEncodedContent(fields));
    }

    private static HttpClient CreateTestClient(
        CatalogManagerWebApplicationFactory factory)
    {
        return factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });
    }

    [Fact]
    public async Task Category_FullCrud_WorksWithPostgreSQL()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateTestClient(factory);
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var name = UniqueName("TestCategory");
        var updatedName = name + "-Updated";
        int? categoryId = null;

        try
        {
            // CREATE
            using var createResponse = await PostFormAsync(
                client,
                "/Categories/Create",
                new Dictionary<string, string>
                {
                    ["Name"] = name,
                    ["Description"] = "Initial description"
                });

            Assert.Equal(
                HttpStatusCode.Redirect,
                createResponse.StatusCode);

            var category = await db.Categories
                .AsNoTracking()
                .SingleAsync(c => c.Name == name);

            categoryId = category.Id;

            // READ
            using var readResponse =
                await client.GetAsync(
                    $"/Categories/Details/{categoryId}");

            Assert.Equal(
                HttpStatusCode.OK,
                readResponse.StatusCode);

            var html = await readResponse.Content
                .ReadAsStringAsync();

            Assert.Contains(name, html);

            // UPDATE
            using var updateResponse = await PostFormAsync(
                client,
                $"/Categories/Edit/{categoryId}",
                new Dictionary<string, string>
                {
                    ["Id"] = categoryId.Value.ToString(),
                    ["Name"] = updatedName,
                    ["Description"] = "Updated description"
                });

            Assert.Equal(
                HttpStatusCode.Redirect,
                updateResponse.StatusCode);

            var updated = await db.Categories
                .AsNoTracking()
                .SingleAsync(c => c.Id == categoryId);

            Assert.Equal(updatedName, updated.Name);
            Assert.Equal(
                "Updated description",
                updated.Description);

            // DELETE
            using var deleteResponse = await PostFormAsync(
                client,
                $"/Categories/Delete/{categoryId}",
                new Dictionary<string, string>());

            Assert.Equal(
                HttpStatusCode.Redirect,
                deleteResponse.StatusCode);

            Assert.False(await db.Categories
                .AnyAsync(c => c.Id == categoryId));
        }
        finally
        {
            if (categoryId.HasValue)
            {
                await db.Categories
                    .Where(c => c.Id == categoryId.Value)
                    .ExecuteDeleteAsync();
            }
        }
    }

    [Fact]
    public async Task Store_FullCrud_WorksWithPostgreSQL()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateTestClient(factory);
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var name = UniqueName("TestStore");
        var updatedName = name + "-Updated";
        int? storeId = null;

        try
        {
            // CREATE
            using var createResponse = await PostFormAsync(
                client,
                "/Stores/Create",
                new Dictionary<string, string>
                {
                    ["Name"] = name,
                    ["Address"] = "Kyiv",
                    ["Latitude"] = "50",
                    ["Longitude"] = "30"
                });

            Assert.Equal(
                HttpStatusCode.Redirect,
                createResponse.StatusCode);

            var store = await db.Stores
                .AsNoTracking()
                .SingleAsync(s => s.Name == name);

            storeId = store.Id;

            // READ
            using var readResponse =
                await client.GetAsync(
                    $"/Stores/Details/{storeId}");

            Assert.Equal(
                HttpStatusCode.OK,
                readResponse.StatusCode);

            var html = await readResponse.Content
                .ReadAsStringAsync();

            Assert.Contains(name, html);

            // UPDATE
            using var updateResponse = await PostFormAsync(
                client,
                $"/Stores/Edit/{storeId}",
                new Dictionary<string, string>
                {
                    ["Id"] = storeId.Value.ToString(),
                    ["Name"] = updatedName,
                    ["Address"] = "Updated Kyiv Address",
                    ["Latitude"] = "51",
                    ["Longitude"] = "31"
                });

            Assert.Equal(
                HttpStatusCode.Redirect,
                updateResponse.StatusCode);

            var updated = await db.Stores
                .AsNoTracking()
                .SingleAsync(s => s.Id == storeId);

            Assert.Equal(updatedName, updated.Name);
            Assert.Equal("Updated Kyiv Address", updated.Address);
            Assert.Equal(51, updated.Latitude);
            Assert.Equal(31, updated.Longitude);

            // DELETE
            using var deleteResponse = await PostFormAsync(
                client,
                $"/Stores/Delete/{storeId}",
                new Dictionary<string, string>());

            Assert.Equal(
                HttpStatusCode.Redirect,
                deleteResponse.StatusCode);

            Assert.False(await db.Stores
                .AnyAsync(s => s.Id == storeId));
        }
        finally
        {
            if (storeId.HasValue)
            {
                await db.Stores
                    .Where(s => s.Id == storeId.Value)
                    .ExecuteDeleteAsync();
            }
        }
    }

    [Fact]
    public async Task Product_FullCrud_WorksWithPostgreSQL()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateTestClient(factory);
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var name = UniqueName("TestProduct");
        var updatedName = name + "-Updated";

        int? categoryId = null;
        int? storeId = null;
        int? productId = null;

        try
        {
            // Prepare related entities
            var category = new Category
            {
                Name = UniqueName("ProductCategory")
            };

            var store = new Store
            {
                Name = UniqueName("ProductStore"),
                Address = "Kyiv",
                Latitude = 50,
                Longitude = 30
            };

            db.Categories.Add(category);
            db.Stores.Add(store);

            await db.SaveChangesAsync();

            categoryId = category.Id;
            storeId = store.Id;

            // CREATE
            using var createResponse = await PostFormAsync(
                client,
                "/Products/Create",
                new Dictionary<string, string>
                {
                    ["Name"] = name,
                    ["Description"] = "Initial product",
                    ["Price"] = "25000",
                    ["ImageUrl"] = "https://example.com/a.jpg",
                    ["CategoryId"] = categoryId.Value.ToString(),
                    ["StoreId"] = storeId.Value.ToString()
                });

            Assert.Equal(
                HttpStatusCode.Redirect,
                createResponse.StatusCode);

            var product = await db.Products
                .AsNoTracking()
                .SingleAsync(p => p.Name == name);

            productId = product.Id;

            Assert.Equal(categoryId.Value, product.CategoryId);
            Assert.Equal(storeId.Value, product.StoreId);

            // READ
            using var readResponse =
                await client.GetAsync(
                    $"/Products/Details/{productId}");

            Assert.Equal(
                HttpStatusCode.OK,
                readResponse.StatusCode);

            var html = await readResponse.Content
                .ReadAsStringAsync();

            Assert.Contains(name, html);

            // UPDATE
            using var updateResponse = await PostFormAsync(
                client,
                $"/Products/Edit/{productId}",
                new Dictionary<string, string>
                {
                    ["Id"] = productId.Value.ToString(),
                    ["Name"] = updatedName,
                    ["Description"] = "Updated product",
                    ["Price"] = "30000",
                    ["ImageUrl"] = "https://example.com/b.jpg",
                    ["CategoryId"] = categoryId.Value.ToString(),
                    ["StoreId"] = storeId.Value.ToString()
                });

            Assert.Equal(
                HttpStatusCode.Redirect,
                updateResponse.StatusCode);

            var updated = await db.Products
                .AsNoTracking()
                .SingleAsync(p => p.Id == productId);

            Assert.Equal(updatedName, updated.Name);
            Assert.Equal(30000m, updated.Price);
            Assert.Equal("Updated product", updated.Description);

            // DELETE
            using var deleteResponse = await PostFormAsync(
                client,
                $"/Products/Delete/{productId}",
                new Dictionary<string, string>());

            Assert.Equal(
                HttpStatusCode.Redirect,
                deleteResponse.StatusCode);

            Assert.False(await db.Products
                .AnyAsync(p => p.Id == productId));
        }
        finally
        {
            // Delete only records created by this test
            if (productId.HasValue)
            {
                await db.Products
                    .Where(p => p.Id == productId.Value)
                    .ExecuteDeleteAsync();
            }

            if (categoryId.HasValue)
            {
                await db.Categories
                    .Where(c => c.Id == categoryId.Value)
                    .ExecuteDeleteAsync();
            }

            if (storeId.HasValue)
            {
                await db.Stores
                    .Where(s => s.Id == storeId.Value)
                    .ExecuteDeleteAsync();
            }
        }
    }
}
