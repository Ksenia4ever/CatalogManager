
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CatalogManager.Models;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }


    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    [Url(ErrorMessage = "Enter a valid image URL.")]
    public string? ImageUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select a category.")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select a store.")]
    public int StoreId { get; set; }

    public IEnumerable<SelectListItem> Categories { get; set; }
        = new List<SelectListItem>();

    public IEnumerable<SelectListItem> Stores { get; set; }
        = new List<SelectListItem>();
}
