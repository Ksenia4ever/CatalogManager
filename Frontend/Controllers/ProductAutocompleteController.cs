using CatalogManager.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogManager.Controllers;

[Route("Products/Autocomplete")]
public sealed class ProductAutocompleteController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProductAutocompleteController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("Categories")]
    public async Task<IActionResult> Categories(
        [FromQuery] string? term,
        CancellationToken cancellationToken)
    {
        var search = term?.Trim();
        if (string.IsNullOrEmpty(search) || search.Length < 3)
            return Ok(Array.Empty<object>());

        var pattern = "%" + EscapeLike(search) + "%";
        var results = await _db.Categories
            .AsNoTracking()
            .Where(c => EF.Functions.ILike(c.Name, pattern, "\\"))
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Select(c => new { id = c.Id, text = c.Name })
            .Take(20)
            .ToListAsync(cancellationToken);

        return Ok(results);
    }

    [HttpGet("Stores")]
    public async Task<IActionResult> Stores(
        [FromQuery] string? term,
        CancellationToken cancellationToken)
    {
        var search = term?.Trim();
        if (string.IsNullOrEmpty(search) || search.Length < 3)
            return Ok(Array.Empty<object>());

        var pattern = "%" + EscapeLike(search) + "%";
        var results = await _db.Stores
            .AsNoTracking()
            .Where(s => EF.Functions.ILike(s.Name, pattern, "\\"))
            .OrderBy(s => s.Name)
            .ThenBy(s => s.Id)
            .Select(s => new { id = s.Id, text = s.Name })
            .Take(20)
            .ToListAsync(cancellationToken);

        return Ok(results);
    }

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\")
             .Replace("%", "\\%")
             .Replace("_", "\\_");
}