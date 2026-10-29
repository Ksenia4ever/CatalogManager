using Microsoft.AspNetCore.OutputCaching;
using CatalogManager.DTOs.Pagination;
using Microsoft.AspNetCore.Http;
using CatalogManager.Application.Interfaces;
using CatalogManager.Application.Exceptions;
using CatalogManager.Domain.Entities;
using CatalogManager.DTOs.Categories;
using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.ApiControllers;

[ApiController]
[Route("api/categories")]
public class CategoriesApiController : ControllerBase
{
    private readonly ICategoryService _service;
    private readonly IOutputCacheStore? _cache;

    public CategoriesApiController(ICategoryService service, IOutputCacheStore? cache = null)
    {
        _service = service;
        _cache = cache;
    }

    // GET api/categories?skip=0&limit=10 (B1)
    // Without pagination parameters, preserve the original C5 array response.
    [HttpGet]
    [OutputCache(PolicyName = "CategoriesCache")]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll()
    {
        var request = ControllerContext.HttpContext?.Request;
        var query = request?.Query;
        var hasSkip = query?.ContainsKey("skip") == true;
        var hasLimit = query?.ContainsKey("limit") == true;

        if (!hasSkip && !hasLimit)
        {
            var all = await _service.GetAllAsync();
            return Ok(all.Select(MapToResponse));
        }

        var skipText = hasSkip ? query!["skip"].ToString() : "0";
        var limitText = hasLimit ? query!["limit"].ToString() : "10";
        if (!int.TryParse(skipText, out var skip) || skip < 0 ||
            !int.TryParse(limitText, out var limit) || limit < 1 || limit > 100)
        {
            return BadRequest(new { error = "skip must be >= 0 and limit must be between 1 and 100." });
        }

        var page = await _service.GetPagedAsync(skip, limit);
        string? nextLink = null;
        if ((long)skip + limit < page.TotalCount)
        {
            var nextSkip = skip + limit;
            // Preserve the current host, path base and route in the continuation URL.
            // QueryString.Create encodes the parameters safely.
            var path = request!.PathBase.Add(request.Path).ToString();
            nextLink = $"{request.Scheme}://{request.Host}{path}" +
                QueryString.Create(new Dictionary<string, string?>
                {
                    ["skip"] = nextSkip.ToString(),
                    ["limit"] = limit.ToString()
                }).ToString();
        }

        return Ok(new PagedResponseDto<CategoryResponseDto>(
            page.Items.Select(MapToResponse).ToList(), skip, limit,
            page.TotalCount, nextLink));
    }

    // GET api/categories/5
    [HttpGet("{id:int}")]
    [OutputCache(PolicyName = "CategoriesCache")]
    public async Task<ActionResult<CategoryResponseDto>> GetById(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        return Ok(MapToResponse(category));
    }

    // POST api/categories
    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Create(
        CategoryCreateDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        try
        {
            await _service.CreateAsync(category);
            await EvictCacheAsync();
        }
        catch (DuplicateCategoryNameException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        var response = MapToResponse(category);

        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            response);
    }

    // PUT api/categories/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, CategoryUpdateDto dto)
    {
        var category = new Category
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description
        };

        try
        {
            var updated = await _service.UpdateAsync(category);

            if (!updated)
                return NotFound();

            await EvictCacheAsync();
        }
        catch (DuplicateCategoryNameException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return NoContent();
    }

    // DELETE api/categories/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return Conflict(new
            {
                error = "Cannot delete a category that contains products."
            });
        }

        await EvictCacheAsync();
        return NoContent();
    }

    // Invalidate cached API responses only after successful mutations.
    private async Task EvictCacheAsync()
    {
        if (_cache is not null)
        {
            await _cache.EvictByTagAsync("categories", HttpContext.RequestAborted);
        }
    }

    private static CategoryResponseDto MapToResponse(
        Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}