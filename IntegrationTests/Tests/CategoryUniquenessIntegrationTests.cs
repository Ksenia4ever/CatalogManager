
using System.Net;
using System.Net.Http.Json;
using CatalogManager.DTOs.Categories;
using CatalogManager.Infrastructure.Data;
using CatalogManager.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogManager.IntegrationTests.Tests;

[Collection("DatabaseIntegration")]
public class CategoryUniquenessIntegrationTests
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

    [Theory]
    [InlineData("same")]
    [InlineData("lowercase")]
    [InlineData("spaces")]
    public async Task CreateCategory_DuplicateName_Returns409(
        string variant)
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        // Я генерую унікальну назву, щоб ізолювати тест.
        var name = "TestCategory" +
                   Guid.NewGuid().ToString("N");

        int categoryId = 0;

        try
        {
            // Я створюю першу категорію.
            using var createResponse = await client.PostAsJsonAsync(
                "/api/categories",
                new CategoryCreateDto
                {
                    Name = name,
                    Description = "Original category"
                });

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var created = await createResponse.Content
                .ReadFromJsonAsync<CategoryResponseDto>();

            Assert.NotNull(created);

            categoryId = created.Id;

            // Я перевіряю різні варіанти дублювання назви.
            var duplicateName = variant switch
            {
                "lowercase" => name.ToLowerInvariant(),
                "spaces" => $"  {name}  ",
                _ => name
            };

            using var duplicateResponse =
                await client.PostAsJsonAsync(
                    "/api/categories",
                    new CategoryCreateDto
                    {
                        Name = duplicateName,
                        Description = "Duplicate category"
                    });

            Assert.Equal(
                HttpStatusCode.Conflict,
                duplicateResponse.StatusCode);

            // Я перевіряю, що дублікат не потрапив до БД.
            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var count = await db.Categories.CountAsync(
                c => c.Name.ToLower() ==
                     name.ToLower());

            Assert.Equal(1, count);
        }
        finally
        {
            // Я видаляю категорію, створену під час тесту.
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
    public async Task UpdateCategory_DuplicateName_Returns409()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        var uniqueSuffix = Guid.NewGuid().ToString("N");

        var firstName = "First" + uniqueSuffix;
        var secondName = "Second" + uniqueSuffix;

        int firstId = 0;
        int secondId = 0;

        try
        {
            // Я створюю дві категорії з різними назвами.
            using var firstResponse = await client.PostAsJsonAsync(
                "/api/categories",
                new CategoryCreateDto
                {
                    Name = firstName
                });

            Assert.Equal(
                HttpStatusCode.Created,
                firstResponse.StatusCode);

            var first = await firstResponse.Content
                .ReadFromJsonAsync<CategoryResponseDto>();

            Assert.NotNull(first);
            firstId = first.Id;

            using var secondResponse = await client.PostAsJsonAsync(
                "/api/categories",
                new CategoryCreateDto
                {
                    Name = secondName
                });

            Assert.Equal(
                HttpStatusCode.Created,
                secondResponse.StatusCode);

            var second = await secondResponse.Content
                .ReadFromJsonAsync<CategoryResponseDto>();

            Assert.NotNull(second);
            secondId = second.Id;

            // Я намагаюся змінити назву другої категорії
            // на назву першої.
            using var updateResponse = await client.PutAsJsonAsync(
                $"/api/categories/{secondId}",
                new CategoryUpdateDto
                {
                    Name = firstName,
                    Description = "Duplicate update"
                });

            Assert.Equal(
                HttpStatusCode.Conflict,
                updateResponse.StatusCode);

            // Я перевіряю, що назва другої категорії
            // залишилася незмінною.
            var unchanged = await client.GetFromJsonAsync<
                CategoryResponseDto>(
                $"/api/categories/{secondId}");

            Assert.NotNull(unchanged);
            Assert.Equal(secondName, unchanged.Name);
        }
        finally
        {
            // Я видаляю лише категорії, які створила під час тесту.
            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var ids = new[] { firstId, secondId }
                .Where(id => id > 0)
                .ToArray();

            if (ids.Length > 0)
            {
                await db.Categories
                    .Where(c => ids.Contains(c.Id))
                    .ExecuteDeleteAsync();
            }
        }
    }
}
