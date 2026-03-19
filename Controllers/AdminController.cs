using Microsoft.AspNetCore.Mvc;

namespace CarBazzar.Controllers;

public class AdminController : Controller
{
    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult Listings()
    {
        return View();
    }

    public IActionResult Users()
    {
        return View();
    }

    [HttpGet]
    public IActionResult AddListing()
    {
        return View();
    }
}

