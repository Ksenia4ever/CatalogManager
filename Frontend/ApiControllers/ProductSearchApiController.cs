using CatalogManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.ApiControllers;

[ApiController]
[Route("api/search/products")]
public sealed class ProductSearchApiController(ProductSearchService searchService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductSearchResult>>> Get(
        [FromQuery] string? q,
        [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "Query parameter 'q' is required." });

        if (q.Trim().Length > 100)
            return BadRequest(new { error = "Query cannot exceed 100 characters." });

        if (limit < 1 || limit > 50)
            return BadRequest(new { error = "Limit must be between 1 and 50." });

        var results = await searchService.SearchAsync(q, limit);
        return Ok(results);
    }
}