using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CarBazzar.Models.Cars;

public class SellViewModel
{
    [Required]
    public string Title { get; set; } = "";
    [Required]
    public string Brand { get; set; } = "";
    [Required]
    public string Model { get; set; } = "";
    [Required]
    public int Year { get; set; }
    [Required]
    public decimal Price { get; set; }
    [Required]
    public int Mileage { get; set; }
    [Required]
    public string FuelType { get; set; } = "";
    [Required]
    public string Transmission { get; set; } = "";
    public string Description { get; set; } = "";
    public string Location { get; set; } = "";
    public string Condition { get; set; } = "";

    public IFormFile? Image { get; set; }
}
