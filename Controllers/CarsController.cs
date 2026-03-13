using CarBazzar.Models.Cars;
using Microsoft.AspNetCore.Mvc;

namespace CarBazzar.Controllers;

public class CarsController : Controller
{
    internal static readonly IReadOnlyList<CarCard> DemoCars =
    [
        new() { Id = 1, Title = "Toyota Camry 2022", Specs = "Petrol • Automatic • 12,000 km", Price = "₹29,50,000", ImageUrl = "/images/coupe.jpg" },
        new() { Id = 2, Title = "Honda Civic 2021", Specs = "Petrol • Manual • 18,000 km", Price = "₹24,20,000", ImageUrl = "/images/images.jpg" },
        new() { Id = 3, Title = "BMW 3 Series", Specs = "Diesel • Automatic • 9,000 km", Price = "₹42,00,000", ImageUrl = "/images/sedan.jpg" },
        new() { Id = 4, Title = "Audi A4", Specs = "Petrol • Automatic • 15,000 km", Price = "₹37,00,000", ImageUrl = "/images/suv.jpg" },
        new() { Id = 5, Title = "Mustang GT", Specs = "Petrol • Automatic • 5,000 km", Price = "₹55,00,000", ImageUrl = "/images/truc.jpg" },
        new() { Id = 6, Title = "Mercedes C-Class", Specs = "Diesel • Automatic • 10,000 km", Price = "₹48,00,000", ImageUrl = "/images/suv.jpg" },
    ];

    [HttpGet]
    public IActionResult Browse() => View(DemoCars);

    [HttpGet]
    public IActionResult OldBrowse()
    {
        ViewData["Title"] = "Browse Old Cars";
        return View(DemoCars);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var car = DemoCars.FirstOrDefault(c => c.Id == id) ?? DemoCars[0];
        return View(car);
    }

    [HttpGet]
    public IActionResult Sell()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Sell(object _)
    {
        TempData["Toast"] = "Your car listing was submitted (demo).";
        return RedirectToAction("Browse");
    }
}

