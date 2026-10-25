using System.ComponentModel.DataAnnotations;

namespace CatalogManager.Models;

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

