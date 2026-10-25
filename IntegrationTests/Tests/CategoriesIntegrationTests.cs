
using CatalogManager.IntegrationTests.Infrastructure;
using System.Net;

namespace CatalogManager.IntegrationTests.Tests;

public class CategoriesIntegrationTests
{
    [Fact]
    public async Task CategoriesIndex_ReturnsSuccessStatusCode()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Categories", content);
    }
}
