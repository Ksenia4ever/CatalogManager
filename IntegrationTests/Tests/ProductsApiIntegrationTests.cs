using System.Net;
using System.Net.Http.Json;
using CatalogManager.DTOs.Categories;
using CatalogManager.DTOs.Stores;
using CatalogManager.DTOs.Products;
using CatalogManager.IntegrationTests.Infrastructure;
using CatalogManager.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogManager.IntegrationTests.Tests;

[Collection("DatabaseIntegration")]
public class ProductsApiIntegrationTests
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

    private static async Task<int> CreateCategoryAsync(
        HttpClient client)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/categories",
            new CategoryCreateDto
            {
                Name = "Product Test Category " +
                       Guid.NewGuid().ToString("N")
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var dto = await response.Content
            .ReadFromJsonAsync<CategoryResponseDto>();

        Assert.NotNull(dto);
        Assert.True(dto.Id > 0);

        return dto.Id;
    }

    private static async Task<int> CreateStoreAsync(
        HttpClient client)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/stores",
            new StoreCreateDto
            {
                Name = "Product Test Store " +
                       Guid.NewGuid().ToString("N"),
                Address = "Kyiv, Test Street 1",
                Latitude = 50.4501,
                Longitude = 30.5234
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var dto = await response.Content
            .ReadFromJsonAsync<StoreResponseDto>();

        Assert.NotNull(dto);
        Assert.True(dto.Id > 0);

        return dto.Id;
    }

    private static async Task CleanupAsync(
        CatalogManagerWebApplicationFactory factory,
        int productId,
        int categoryId,
        int storeId)
    {
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        // Спочатку товар, потім зв'язані сутності.
        if (productId > 0)
        {
            await db.Products
                .Where(p => p.Id == productId)
                .ExecuteDeleteAsync();
        }

        if (categoryId > 0)
        {
            await db.Categories
                .Where(c => c.Id == categoryId)
                .ExecuteDeleteAsync();
        }

        if (storeId > 0)
        {
            await db.Stores
                .Where(s => s.Id == storeId)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task ProductApi_FullCrud_AndConflicts_Work()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        int categoryId = 0;
        int storeId = 0;
        int productId = 0;

        try
        {
            categoryId = await CreateCategoryAsync(client);
            storeId = await CreateStoreAsync(client);

            // CREATE PRODUCT
            using var createResponse = await client.PostAsJsonAsync(
                "/api/products",
                new ProductCreateDto
                {
                    Name = "API Integration Laptop",
                    Description = "Created by HTTP integration test",
                    Price = 25000m,
                    ImageUrl = "https://example.com/laptop.jpg",
                    CategoryId = categoryId,
                    StoreId = storeId
                });

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var created = await createResponse.Content
                .ReadFromJsonAsync<ProductResponseDto>();

            Assert.NotNull(created);
            productId = created.Id;

            Assert.True(productId > 0);
            Assert.Equal(categoryId, created.CategoryId);
            Assert.Equal(storeId, created.StoreId);
            Assert.NotNull(createResponse.Headers.Location);

            // READ PRODUCT
            using var getResponse = await client.GetAsync(
                $"/api/products/{productId}");

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            var product = await getResponse.Content
                .ReadFromJsonAsync<ProductResponseDto>();

            Assert.NotNull(product);
            Assert.Equal(productId, product.Id);
            Assert.Equal(25000m, product.Price);
            Assert.Equal(categoryId, product.CategoryId);
            Assert.Equal(storeId, product.StoreId);

            // READ ALL
            using var listResponse = await client.GetAsync(
                "/api/products");

            Assert.Equal(
                HttpStatusCode.OK,
                listResponse.StatusCode);

            var products = await listResponse.Content
                .ReadFromJsonAsync<List<ProductResponseDto>>();

            Assert.NotNull(products);
            Assert.Contains(products, p => p.Id == productId);

            // UPDATE PRODUCT
            using var updateResponse = await client.PutAsJsonAsync(
                $"/api/products/{productId}",
                new ProductUpdateDto
                {
                    Name = "Updated Integration Laptop",
                    Description = "Updated by PUT",
                    Price = 22000m,
                    ImageUrl = "https://example.com/updated.jpg",
                    CategoryId = categoryId,
                    StoreId = storeId
                });

            Assert.Equal(
                HttpStatusCode.NoContent,
                updateResponse.StatusCode);

            // VERIFY UPDATE
            var updated = await client.GetFromJsonAsync<
                ProductResponseDto>(
                $"/api/products/{productId}");

            Assert.NotNull(updated);
            Assert.Equal(
                "Updated Integration Laptop",
                updated.Name);
            Assert.Equal(22000m, updated.Price);

            // DELETE CATEGORY WITH PRODUCT -> 409
            using var categoryConflict = await client.DeleteAsync(
                $"/api/categories/{categoryId}");

            Assert.Equal(
                HttpStatusCode.Conflict,
                categoryConflict.StatusCode);

            // DELETE STORE WITH PRODUCT -> 409
            using var storeConflict = await client.DeleteAsync(
                $"/api/stores/{storeId}");

            Assert.Equal(
                HttpStatusCode.Conflict,
                storeConflict.StatusCode);

            // VERIFY CATEGORY STILL EXISTS
            using var categoryResponse = await client.GetAsync(
                $"/api/categories/{categoryId}");

            Assert.Equal(
                HttpStatusCode.OK,
                categoryResponse.StatusCode);

            // VERIFY STORE STILL EXISTS
            using var storeResponse = await client.GetAsync(
                $"/api/stores/{storeId}");

            Assert.Equal(
                HttpStatusCode.OK,
                storeResponse.StatusCode);

            // DELETE PRODUCT
            using var deleteResponse = await client.DeleteAsync(
                $"/api/products/{productId}");

            Assert.Equal(
                HttpStatusCode.NoContent,
                deleteResponse.StatusCode);

            // VERIFY PRODUCT WAS DELETED
            using var missingProduct = await client.GetAsync(
                $"/api/products/{productId}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                missingProduct.StatusCode);

            // NOW RELATED ENTITIES CAN BE DELETED
            using var deleteCategory = await client.DeleteAsync(
                $"/api/categories/{categoryId}");

            Assert.Equal(
                HttpStatusCode.NoContent,
                deleteCategory.StatusCode);

            using var deleteStore = await client.DeleteAsync(
                $"/api/stores/{storeId}");

            Assert.Equal(
                HttpStatusCode.NoContent,
                deleteStore.StatusCode);
        }
        finally
        {
            await CleanupAsync(
                factory, productId, categoryId, storeId);
        }
    }

    [Fact]
    public async Task ProductApi_InvalidData_Returns400()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        int categoryId = 0;
        int storeId = 0;

        try
        {
            categoryId = await CreateCategoryAsync(client);
            storeId = await CreateStoreAsync(client);

            // INVALID PRICE
            using var zeroPrice = await client.PostAsJsonAsync(
                "/api/products",
                new ProductCreateDto
                {
                    Name = "Zero Price Product",
                    Price = 0,
                    CategoryId = categoryId,
                    StoreId = storeId
                });

            Assert.Equal(
                HttpStatusCode.BadRequest,
                zeroPrice.StatusCode);

            // EMPTY NAME
            using var emptyName = await client.PostAsJsonAsync(
                "/api/products",
                new ProductCreateDto
                {
                    Name = "",
                    Price = 1000m,
                    CategoryId = categoryId,
                    StoreId = storeId
                });

            Assert.Equal(
                HttpStatusCode.BadRequest,
                emptyName.StatusCode);

            // NONEXISTENT CATEGORY
            using var missingCategory = await client.PostAsJsonAsync(
                "/api/products",
                new ProductCreateDto
                {
                    Name = "Wrong Category",
                    Price = 1000m,
                    CategoryId = int.MaxValue,
                    StoreId = storeId
                });

            Assert.Equal(
                HttpStatusCode.BadRequest,
                missingCategory.StatusCode);

            // NONEXISTENT STORE
            using var missingStore = await client.PostAsJsonAsync(
                "/api/products",
                new ProductCreateDto
                {
                    Name = "Wrong Store",
                    Price = 1000m,
                    CategoryId = categoryId,
                    StoreId = int.MaxValue
                });

            Assert.Equal(
                HttpStatusCode.BadRequest,
                missingStore.StatusCode);
        }
        finally
        {
            await CleanupAsync(
                factory, 0, categoryId, storeId);
        }
    }

    [Fact]
    public async Task ProductApi_MissingId_Returns404()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        const int missingId = int.MaxValue;

        // GET
        using var getResponse = await client.GetAsync(
            $"/api/products/{missingId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);

        // PUT: existing related entities are required
        // to reach the missing-product check.
        int categoryId = 0;
        int storeId = 0;

        try
        {
            categoryId = await CreateCategoryAsync(client);
            storeId = await CreateStoreAsync(client);

            using var updateResponse = await client.PutAsJsonAsync(
                $"/api/products/{missingId}",
                new ProductUpdateDto
                {
                    Name = "Missing Product",
                    Price = 1000m,
                    CategoryId = categoryId,
                    StoreId = storeId
                });

            Assert.Equal(
                HttpStatusCode.NotFound,
                updateResponse.StatusCode);

            // DELETE
            using var deleteResponse = await client.DeleteAsync(
                $"/api/products/{missingId}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                deleteResponse.StatusCode);
        }
        finally
        {
            await CleanupAsync(
                factory, 0, categoryId, storeId);
        }
    }
}
