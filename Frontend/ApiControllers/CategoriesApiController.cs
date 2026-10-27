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

    public CategoriesApiController(ICategoryService service)
    {
        _service = service;
    }

    // GET api/categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll()
    {
        var categories = await _service.GetAllAsync();

        var result = categories.Select(MapToResponse);

        return Ok(result);
    }

    // GET api/categories/5
    [HttpGet("{id:int}")]
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

        return NoContent();
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