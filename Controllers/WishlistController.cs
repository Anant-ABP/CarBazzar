using CarBazzar.Models;
using CarBazzar.Models.Cars;
using Microsoft.AspNetCore.Mvc;

namespace CarBazzar.Controllers;

public class WishlistController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var cars = CarsController.DemoCars
            .Where(c => WishlistStore.IsSaved(c.Id))
            .ToArray();

        return View(cars);
    }

    [HttpPost]
    public IActionResult Toggle(int id)
    {
        WishlistStore.Toggle(id);

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrWhiteSpace(referer))
        {
            return Redirect(referer);
        }

        return RedirectToAction("Browse", "Cars");
    }
}

