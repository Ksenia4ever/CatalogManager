using CatalogManager.DTOs.Pagination;
using Microsoft.AspNetCore.Http;
using CatalogManager.Application.Interfaces;
using CatalogManager.Domain.Entities;
using CatalogManager.DTOs.Stores;
using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.ApiControllers;

[ApiController]
[Route("api/stores")]
public class StoresApiController : ControllerBase
{
    private readonly IStoreService _service;

    public StoresApiController(IStoreService service)
    {
        _service = service;
    }

    // GET api/stores?skip=0&limit=10 (B1)
    // Without pagination parameters, preserve the original C5 array response.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoreResponseDto>>> GetAll()
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

        return Ok(new PagedResponseDto<StoreResponseDto>(
            page.Items.Select(MapToResponse).ToList(), skip, limit,
            page.TotalCount, nextLink));
    }

    // GET api/stores/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StoreResponseDto>> GetById(int id)
    {
        var store = await _service.GetByIdAsync(id);

        if (store == null)
            return NotFound();

        return Ok(MapToResponse(store));
    }

    // POST api/stores
    [HttpPost]
    public async Task<ActionResult<StoreResponseDto>> Create(
        StoreCreateDto dto)
    {
        var store = new Store
        {
            Name = dto.Name,
            Address = dto.Address,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };

        try
        {
            await _service.CreateAsync(store);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = store.Id },
            MapToResponse(store));
    }

    // PUT api/stores/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, StoreUpdateDto dto)
    {
        var store = new Store
        {
            Id = id,
            Name = dto.Name,
            Address = dto.Address,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };

        try
        {
            var updated = await _service.UpdateAsync(store);

            if (!updated)
                return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return NoContent();
    }

    // DELETE api/stores/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var store = await _service.GetByIdAsync(id);

        if (store == null)
            return NotFound();

        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return Conflict(new
            {
                error = "Cannot delete a store that contains products."
            });
        }

        return NoContent();
    }

    private static StoreResponseDto MapToResponse(Store store)
    {
        return new StoreResponseDto
        {
            Id = store.Id,
            Name = store.Name,
            Address = store.Address,
            Latitude = store.Latitude,
            Longitude = store.Longitude
        };
    }
}