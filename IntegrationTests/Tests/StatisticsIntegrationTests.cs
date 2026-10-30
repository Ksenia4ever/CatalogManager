using System.Net;
using System.Net.Http.Json;
using CatalogManager.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CatalogManager.IntegrationTests.Tests;

[Collection("DatabaseIntegration")]
public class StatisticsIntegrationTests
{
    [Fact]
    public async Task StatisticsPage_ReturnsHtmlWithCharts()
    {
        await using var factory = new CatalogManagerWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await client.GetAsync("/Statistics");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("id=\"categoryChart\"", html);
        Assert.Contains("id=\"storeChart\"", html);
        Assert.Contains("id=\"priceChart\"", html);
        Assert.Contains("statistics.js", html);
    }

    [Fact]
    public async Task StatisticsData_ReturnsJsonWithExpectedFields()
    {
        await using var factory = new CatalogManagerWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/Statistics/Data");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.True(data.TryGetProperty("totalProducts", out _));
        Assert.True(data.TryGetProperty("byCategory", out var categories));
        Assert.Equal(System.Text.Json.JsonValueKind.Array, categories.ValueKind);
        Assert.True(data.TryGetProperty("byStore", out _));
        Assert.True(data.TryGetProperty("byPriceRange", out _));
    }

    [Theory]
    [InlineData("/js/statistics.js", "fetch(")]
    [InlineData("/css/statistics.css", ".chart-panel")]
    public async Task StatisticsStaticFiles_AreAccessible(string path, string expected)
    {
        await using var factory = new CatalogManagerWebApplicationFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expected, await response.Content.ReadAsStringAsync());
    }
}