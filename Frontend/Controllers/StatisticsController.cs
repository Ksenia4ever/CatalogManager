using CatalogManager.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogManager.Controllers;

[Route("Statistics")]
public class StatisticsController : Controller
{
    private readonly ApplicationDbContext _db;

    public StatisticsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index() => View();

    // Read-only JSON endpoint used by the F1 dashboard.
    [HttpGet("Data")]
    public async Task<IActionResult> Data(CancellationToken cancellationToken)
    {
        var categories = await _db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(cancellationToken);

        var stores = await _db.Stores.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(cancellationToken);

        var products = await _db.Products.AsNoTracking()
            .Select(p => new { p.CategoryId, p.StoreId, p.Price })
            .ToListAsync(cancellationToken);

        var categoryCounts = products.GroupBy(p => p.CategoryId)
            .ToDictionary(g => g.Key, g => g.Count());
        var storeCounts = products.GroupBy(p => p.StoreId)
            .ToDictionary(g => g.Key, g => g.Count());

        var priceRanges = new[]
        {
            new { Label = "< 1,000 UAH", Count = products.Count(p => p.Price < 1000m) },
            new { Label = "1,000–4,999 UAH", Count = products.Count(p => p.Price >= 1000m && p.Price < 5000m) },
            new { Label = "5,000–19,999 UAH", Count = products.Count(p => p.Price >= 5000m && p.Price < 20000m) },
            new { Label = "20,000+ UAH", Count = products.Count(p => p.Price >= 20000m) }
        };

        return Json(new
        {
            totalProducts = products.Count,
            totalCategories = categories.Count,
            totalStores = stores.Count,
            byCategory = categories.Select(c => new
            {
                label = c.Name,
                count = categoryCounts.GetValueOrDefault(c.Id)
            }),
            byStore = stores.Select(s => new
            {
                label = s.Name,
                count = storeCounts.GetValueOrDefault(s.Id)
            }),
            byPriceRange = priceRanges.Select(r => new { label = r.Label, count = r.Count })
        });
    }
}