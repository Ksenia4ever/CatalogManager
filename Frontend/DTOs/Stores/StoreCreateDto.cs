using System.ComponentModel.DataAnnotations;

namespace CatalogManager.DTOs.Stores;

public class StoreCreateDto
{
    [Required(ErrorMessage = "Store name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Store address is required.")]
    public string Address { get; set; } = string.Empty;

    [Range(-90.0, 90.0, ErrorMessage = "Invalid latitude.")]
    public double Latitude { get; set; }

    [Range(-180.0, 180.0, ErrorMessage = "Invalid longitude.")]
    public double Longitude { get; set; }
}
