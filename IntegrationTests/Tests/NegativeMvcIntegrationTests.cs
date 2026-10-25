
using CatalogManager.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.RegularExpressions;

namespace CatalogManager.IntegrationTests.Tests;

public class NegativeMvcIntegrationTests
{
    private readonly string _missingId = int.MaxValue.ToString();

    private static HttpClient CreateClient(
        CatalogManagerWebApplicationFactory factory)
    {
        return factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });
    }

    private static async Task<string> GetTokenAsync(
        HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();

        var match = Regex.Match(
            html,
            "<input(?=[^>]*name=\"__RequestVerificationToken\")" +
            "(?=[^>]*value=\"([^\"]+)\")[^>]*>",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success,
            "Anti-forgery token was not found.");

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

    [Theory]
    [InlineData("/Categories/Details/")]
    [InlineData("/Stores/Details/")]
    [InlineData("/Products/Details/")]
    public async Task Details_MissingEntity_Returns404(
        string path)
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        using var response = await client.GetAsync(
            path + _missingId);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Theory]
    [InlineData("/Categories/Edit/")]
    [InlineData("/Stores/Edit/")]
    [InlineData("/Products/Edit/")]
    public async Task Edit_MissingEntity_Returns404(
        string path)
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        using var response = await client.GetAsync(
            path + _missingId);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_EmptyName_ReturnsForm()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        using var response = await PostFormAsync(
            client,
            "/Categories/Create",
            new Dictionary<string, string>
            {
                ["Name"] = "",
                ["Description"] = "Invalid category"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Create Category", html);
    }

    [Fact]
    public async Task CreateStore_InvalidLatitude_ReturnsForm()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        using var response = await PostFormAsync(
            client,
            "/Stores/Create",
            new Dictionary<string, string>
            {
                ["Name"] = "Invalid Test Store",
                ["Address"] = "Kyiv",
                ["Latitude"] = "100",
                ["Longitude"] = "30"
            });  

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Contains("Invalid latitude", html);
    }

    [Fact]
    public async Task CreateProduct_ZeroPrice_ReturnsForm()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        using var response = await PostFormAsync(
            client,
            "/Products/Create",
            new Dictionary<string, string>
            {
                ["Name"] = "Invalid Laptop",
                ["Description"] = "Invalid price test",
                ["Price"] = "0",
                ["CategoryId"] = "0",
                ["StoreId"] = "0"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Create Product", html);
    }

    [Fact]
    public async Task CreateCategory_WithoutAntiForgeryToken_Returns400()
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        using var client = CreateClient(factory);

        using var response = await client.PostAsync(
            "/Categories/Create",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Name"] = "Unauthorized Test Category"
                }));

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}
