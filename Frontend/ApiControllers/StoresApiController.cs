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

    // GET api/stores
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoreResponseDto>>> GetAll()
    {
        var stores = await _service.GetAllAsync();

        return Ok(stores.Select(MapToResponse));
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
