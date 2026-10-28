
using System.Net;
using CatalogManager.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CatalogManager.IntegrationTests.Tests;

[Collection("DatabaseIntegration")]
public class StoresMapIntegrationTests
{
    [Fact]
    public async Task StoresMap_Page_ReturnsHtmlWithMapAndSearch()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        using var response = await client.GetAsync("/StoresMap");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("id=\"storesMap\"", html);
        Assert.Contains("id=\"storeSearch\"", html);
        Assert.Contains("stores-map.js", html);
        Assert.Contains("leaflet.js", html);
    }

    [Theory]
    [InlineData("/js/stores-map.js", "fetch(")]
    [InlineData("/css/stores-map.css", "#storesMap")]
    public async Task StoresMap_StaticFiles_AreAvailable(
        string url,
        string expected)
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        using var client = factory.CreateClient();

        using var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains(expected, content);
    }
}
