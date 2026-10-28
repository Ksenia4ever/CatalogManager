using CatalogManager.DTOs.Pagination;
using Microsoft.AspNetCore.Http;
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

    // GET api/products?skip=0&limit=10 (B1)
    // Without pagination parameters, preserve the original C5 array response.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll()
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

        return Ok(new PagedResponseDto<ProductResponseDto>(
            page.Items.Select(MapToResponse).ToList(), skip, limit,
            page.TotalCount, nextLink));
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