using CatalogManager.Application.Interfaces;
using CatalogManager.Domain.Entities;
using CatalogManager.DTOs.Products;
using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.ApiControllers;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsApiController(IProductService service)
    {
        _service = service;
    }

    // GET api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll()
    {
        var products = await _service.GetAllAsync();

        return Ok(products.Select(MapToResponse));
    }

    // GET api/products/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id)
    {
        var product = await _service.GetByIdAsync(id);

        if (product == null)
            return NotFound();

        return Ok(MapToResponse(product));
    }

    // POST api/products
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create(
        ProductCreateDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            StoreId = dto.StoreId
        };

        try
        {
            await _service.CreateAsync(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            MapToResponse(product));
    }

    // PUT api/products/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, ProductUpdateDto dto)
    {
        var product = new Product
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            StoreId = dto.StoreId
        };

        try
        {
            var updated = await _service.UpdateAsync(product);

            if (!updated)
                return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return NoContent();
    }

    // DELETE api/products/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _service.GetByIdAsync(id);

        if (product == null)
            return NotFound();

        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private static ProductResponseDto MapToResponse(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            StoreId = product.StoreId
        };
    }
}
