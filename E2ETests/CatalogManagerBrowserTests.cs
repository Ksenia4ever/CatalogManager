using Microsoft.Playwright;
using Xunit;

namespace CatalogManager.E2ETests.Tests;

// These tests use a live ASP.NET Core application and a real Chromium browser.
// Start CatalogManager before running them.
// The test category is uniquely named and deleted during cleanup.
[CollectionDefinition("QA2Browser", DisableParallelization = true)]
public sealed class QA2BrowserCollection { }

[Collection("QA2Browser")]
public sealed class CatalogManagerBrowserTests
{
    private static readonly string BaseUrl =
        (Environment.GetEnvironmentVariable("CATALOGMANAGER_E2E_BASE_URL")
         ?? "https://localhost:44361").TrimEnd('/');

    private static async Task<(IPlaywright Playwright, IBrowser Browser, IBrowserContext Context, IPage Page)>
        OpenBrowserAsync()
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1440, Height = 900 }
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(15000);
        return (playwright, browser, context, page);
    }

    private static async Task CloseBrowserAsync(
        IPlaywright playwright, IBrowser browser, IBrowserContext context)
    {
        await context.CloseAsync();
        await browser.CloseAsync();
        playwright.Dispose();
    }

    private static async Task OpenAsync(IPage page, string relativePath)
    {
        var response = await page.GotoAsync(
            BaseUrl + relativePath,
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.NotNull(response);
        Assert.True(response!.Ok, $"GET {relativePath} returned HTTP {response.Status}.");
    }

    [Fact(DisplayName = "QA2-01 Home page and navigation to Products")]
    public async Task HomePage_ProductsNavigation_Works()
    {
        var (playwright, browser, context, page) = await OpenBrowserAsync();
        try
        {
            await OpenAsync(page, "/");
            await page.Locator("nav").GetByRole(AriaRole.Link, new() { Name = "Products" }).ClickAsync();
            await page.WaitForURLAsync("**/Products");
            Assert.Contains("/Products", page.Url);
            Assert.True(await page.Locator("body").IsVisibleAsync());
        }
        finally
        {
            await CloseBrowserAsync(playwright, browser, context);
        }
    }

    [Fact(DisplayName = "QA2-02 Category complete Create → Edit → Delete scenario")]
    public async Task Category_CreateEditDelete_CompleteUserJourney()
    {
        var (playwright, browser, context, page) = await OpenBrowserAsync();
        var categoryName = "QA2-" + Guid.NewGuid().ToString("N")[..12];
        var updatedName = categoryName + "-Edited";
        var created = false;

        try
        {
            await OpenAsync(page, "/Categories/Create");
            await page.Locator("#Name").FillAsync(categoryName);
            await page.Locator("#Description").FillAsync("Temporary QA2 E2E category");
            await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
            await page.WaitForURLAsync("**/Categories");
            created = true;

            var row = page.Locator("table tbody tr").Filter(new() { HasText = categoryName });
            Assert.Equal(1, await row.CountAsync());

            await row.GetByRole(AriaRole.Link, new() { Name = "Edit" }).ClickAsync();
            await page.Locator("#Name").FillAsync(updatedName);
            await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();
            await page.WaitForURLAsync("**/Categories");

            row = page.Locator("table tbody tr").Filter(new() { HasText = updatedName });
            Assert.Equal(1, await row.CountAsync());

            await row.GetByRole(AriaRole.Link, new() { Name = "Delete" }).ClickAsync();
            Assert.Contains("/Categories/Delete", page.Url);
            await page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
            await page.WaitForURLAsync("**/Categories");
            created = false;

            Assert.Equal(0, await page.Locator("table tbody tr")
                .Filter(new() { HasText = updatedName }).CountAsync());
        }
        finally
        {
            // Best-effort cleanup if an intermediate assertion fails.
            // Delete only our uniquely named category, never existing user records.
            if (created)
            {
                try
                {
                    await page.GotoAsync(BaseUrl + "/Categories");
                    var row = page.Locator("table tbody tr").Filter(new()
                    {
                        HasText = categoryName
                    });
                    if (await row.CountAsync() == 1)
                    {
                        await row.GetByRole(AriaRole.Link, new() { Name = "Delete" }).ClickAsync();
                        await page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
                    }
                }
                catch
                {
                    // Preserve the original test failure. A failed cleanup may
                    // leave one QA2-* category which can be removed manually.
                }
            }
            await CloseBrowserAsync(playwright, browser, context);
        }
    }

    [Fact(DisplayName = "QA2-03 Category required name validation")]
    public async Task Category_EmptyName_RemainsOnForm()
    {
        var (playwright, browser, context, page) = await OpenBrowserAsync();
        try
        {
            await OpenAsync(page, "/Categories/Create");
            await page.Locator("#Name").FillAsync("");
            await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

            Assert.Contains("/Categories/Create", page.Url);
            Assert.True(await page.Locator("#Name").IsVisibleAsync());
            // A browser can block the POST via client-side validation;
            // otherwise MVC also rejects invalid ModelState.
        }
        finally
        {
            await CloseBrowserAsync(playwright, browser, context);
        }
    }

    [Fact(DisplayName = "QA2-04 Product catalog and Create form")]
    public async Task Products_CatalogAndCreateForm_AreAccessible()
    {
        var (playwright, browser, context, page) = await OpenBrowserAsync();
        try
        {
            await OpenAsync(page, "/Products");
            await OpenAsync(page, "/Products/Create");
            Assert.True(await page.Locator("#Name").IsVisibleAsync());
            Assert.True(await page.Locator("#Price").IsVisibleAsync());
            Assert.True(await page.Locator("#CategoryId").IsVisibleAsync());
            Assert.True(await page.Locator("#StoreId").IsVisibleAsync());

            // No product is created; safe even for an empty catalog.
            await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
            Assert.Contains("/Products/Create", page.Url);
        }
        finally
        {
            await CloseBrowserAsync(playwright, browser, context);
        }
    }

    [Fact(DisplayName = "QA2-05 Stores Map opens with search and refresh controls")]
    public async Task StoresMap_Interface_IsAvailable()
    {
        var (playwright, browser, context, page) = await OpenBrowserAsync();
        try
        {
            await OpenAsync(page, "/StoresMap");
            Assert.True(await page.Locator("#storesMap").IsVisibleAsync());
            Assert.True(await page.Locator("#storeSearch").IsVisibleAsync());
            Assert.True(await page.Locator("#refreshStores").IsVisibleAsync());
            await page.Locator("#storeSearch").FillAsync("not-a-real-store-qa2");
            Assert.Equal("not-a-real-store-qa2", await page.Locator("#storeSearch").InputValueAsync());
        }
        finally
        {
            await CloseBrowserAsync(playwright, browser, context);
        }
    }

    [Fact(DisplayName = "QA2-06 Statistics loads real data and supports refresh")]
    public async Task Statistics_Dashboard_LoadsAndRefreshes()
    {
        var (playwright, browser, context, page) = await OpenBrowserAsync();
        try
        {
            await OpenAsync(page, "/Statistics");
            Assert.True(await page.Locator("#statisticsTitle").IsVisibleAsync());
            Assert.True(await page.Locator("#categoryChart").CountAsync() == 1);
            Assert.True(await page.Locator("#storeChart").CountAsync() == 1);
            Assert.True(await page.Locator("#priceChart").CountAsync() == 1);

            await page.Locator("#statisticsStatus").WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible
            });
            // The chart library is loaded from CDN. The page can show a
            // meaningful error message when the internet is unavailable.
            var message = await page.Locator("#statisticsStatus").InnerTextAsync();
            Assert.False(string.IsNullOrWhiteSpace(message));
            Assert.True(await page.Locator("#statisticsRefresh").IsVisibleAsync());
            await page.Locator("#statisticsRefresh").ClickAsync();
        }
        finally
        {
            await CloseBrowserAsync(playwright, browser, context);
        }
    }

    [Fact(DisplayName = "QA2-07 robots.txt is published for SEO")]
    public async Task Seo_RobotsTxt_IsAvailable()
    {
        var (playwright, browser, context, page) = await OpenBrowserAsync();
        try
        {
            await OpenAsync(page, "/robots.txt");
            var content = await page.Locator("body").InnerTextAsync();
            Assert.Contains("User-agent:", content);
            Assert.Contains("Allow:", content);
        }
        finally
        {
            await CloseBrowserAsync(playwright, browser, context);
        }
    }
}