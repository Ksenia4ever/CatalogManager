using CatalogManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.Controllers;

public sealed class ProductSearchController(ProductSearchService searchService)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Query = q ?? string.Empty;
        ViewBag.Error = null;

        if (string.IsNullOrWhiteSpace(q))
            return View(Array.Empty<ProductSearchResult>());

        if (q.Trim().Length > 100)
        {
            ViewBag.Error = "Search query cannot exceed 100 characters.";
            return View(Array.Empty<ProductSearchResult>());
        }

        var results = await searchService.SearchAsync(q);
        return View(results);
    }
}