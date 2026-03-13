using CarBazzar.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CarBazzar.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // UI-only scaffold: wire up Identity/auth later.
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // UI-only scaffold: wire up Identity/auth later.
        return RedirectToAction("Index", "Home");
    }
}

