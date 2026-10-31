using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CatalogManager.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CatalogManager.IntegrationTests.Tests;

[Collection("DatabaseIntegration")]
public class ProductAutocompleteIntegrationTests
{
    [Theory]
    [InlineData("/Products/Autocomplete/Categories")]
    [InlineData("/Products/Autocomplete/Stores")]
    public async Task Autocomplete_ShortTerm_ReturnsEmptyArray(string url)
    {
        await using var factory = new CatalogManagerWebApplicationFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync(url + "?term=ab");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.Equal(0, json.GetArrayLength());
    }

    [Theory]
    [InlineData("/Products/Autocomplete/Categories")]
    [InlineData("/Products/Autocomplete/Stores")]
    public async Task Autocomplete_ThreeChars_ReturnsJsonArray(string url)
    {
        await using var factory = new CatalogManagerWebApplicationFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync(url + "?term=tes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.True(json.GetArrayLength() <= 20);
        foreach (var item in json.EnumerateArray())
        {
            Assert.True(item.TryGetProperty("id", out _));
            Assert.True(item.TryGetProperty("text", out _));
        }
    }

    [Theory]
    [InlineData("/Products/Create")]
    [InlineData("/Products/Edit/1")]
    public async Task ProductForm_UsesAutocompleteResources_WhenPageExists(string url)
    {
        await using var factory = new CatalogManagerWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        using var response = await client.GetAsync(url);
        // Editing ID 1 depends on test data; skip only the HTML assertion if absent.
        if (response.StatusCode == HttpStatusCode.NotFound && url.Contains("Edit")) return;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("product-autocomplete.js", html);
        Assert.Contains("select2.min.js", html);
        Assert.Contains("CategoryId", html);
        Assert.Contains("StoreId", html);
    }
}