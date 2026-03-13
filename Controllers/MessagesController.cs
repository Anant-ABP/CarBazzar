using CarBazzar.Models.Cars;
using Microsoft.AspNetCore.Mvc;

namespace CarBazzar.Controllers;

public class MessagesController : Controller
{
    [HttpGet]
    public IActionResult Index(int? carId)
    {
        var car = CarsController.DemoCars.FirstOrDefault(c => c.Id == carId) ?? CarsController.DemoCars[0];
        return View(car);
    }
}

