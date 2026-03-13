using CarBazzar.Models.Profile;
using Microsoft.AspNetCore.Mvc;

namespace CarBazzar.Controllers;

public class ProfileController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = new ProfileViewModel();
        return View(model);
    }

    [HttpGet]
    public IActionResult Edit()
    {
        var model = new EditProfileViewModel
        {
            FullName = "Anant",
            Email = "anant@example.com",
            Phone = "+91 90000 00000",
            Location = "Surat, Gujarat",
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["Toast"] = "Profile updated (demo).";
        return RedirectToAction("Index");
    }
}

