using System.ComponentModel.DataAnnotations;

namespace CatalogManager.DTOs.Products;

public class ProductUpdateDto
{
    [Required(ErrorMessage = "Product name is required.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "999999999",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Category is required.")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Store is required.")]
    public int StoreId { get; set; }
}
