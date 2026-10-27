using System.ComponentModel.DataAnnotations;

namespace CatalogManager.DTOs.Categories;

public class CategoryCreateDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
