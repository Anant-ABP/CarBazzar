using System.Threading.Tasks;
using CarBazzar.Models.Profile;
using CarBazzar.Models.Entity;
using CarBazzar.Models.Cars;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;

namespace CarBazzar.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpClientFactory _httpClientFactory;

    public ProfileController(UserManager<ApplicationUser> userManager, IHttpClientFactory httpClientFactory)
    {
        _userManager = userManager;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var client = _httpClientFactory.CreateClient("CarBazaarApi");
        
        // Fetch stats
        int wishlistCount = 0;
        int totalListings = 0;
        try 
        {
            var statsResponse = await client.GetFromJsonAsync<ProfileStatsDto>($"/api/ProfileApi/{user.Id}/stats");
            if (statsResponse != null)
            {
                wishlistCount = statsResponse.WishlistCount;
                totalListings = statsResponse.TotalListings;
            }
        }
        catch { /* Fallback to 0 if API is down */ }

        // Fetch recently viewed cars
        var recentlyViewedRaw = new List<CarBazzar.Controllers.ApiCarDto>();
        try
        {
            var r = await client.GetFromJsonAsync<List<CarBazzar.Controllers.ApiCarDto>>($"/api/RecentlyViewedApi/{user.Id}");
            if (r != null) recentlyViewedRaw = r;
        }
        catch { /* Fallback to empty list */ }

        var recentlyViewedCards = recentlyViewedRaw.Select(MapToCard).ToList();

        var model = new ProfileViewModel
        {
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email ?? "",
            Phone = user.PhoneNumber ?? "Not provided",
            Location = string.IsNullOrEmpty(user.Location) && string.IsNullOrEmpty(user.Address) 
                       ? "Not provided" 
                       : $"{user.Address} {user.Location}".Trim(),
            WishlistCount = wishlistCount,
            TotalListings = totalListings,
            RecentlyViewedCars = recentlyViewedCards
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();
        if (user.IsHidden)
        {
            TempData["Toast"] = "Hidden users cannot edit their profile.";
            return RedirectToAction("Index");
        }

        var model = new EditProfileViewModel
        {
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email ?? "",
            Phone = user.PhoneNumber ?? "",
            Location = user.Location ?? ""
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();
        if (user.IsHidden)
        {
            TempData["Toast"] = "Hidden users cannot edit their profile.";
            return RedirectToAction("Index");
        }

        var names = model.FullName.Split(' ', 2);
        user.FirstName = names[0];
        user.LastName = names.Length > 1 ? names[1] : "";
        user.PhoneNumber = model.Phone;
        user.Location = model.Location;

        await _userManager.UpdateAsync(user);

        TempData["Toast"] = "Profile updated successfully.";
        return RedirectToAction("Index");
    }

    private CarCard MapToCard(CarBazzar.Controllers.ApiCarDto c)
    {
        return new CarCard
        {
            Id = c.Id,
            Title = c.Title,
            Specs = c.Model,
            Price = "₹" + c.Price.ToString("N0"),
            ImageUrl = Url.Action("GetImage", "Cars", new { id = c.Id }) ?? "",
            Year = c.Year,
            FuelType = c.FuelType,
            Mileage = c.Mileage + " km",
            Transmission = c.Transmission,
            BodyType = c.Brand,
            Color = "Unknown",
            Description = c.Description ?? "",
            Features = new List<string>(),
            DealerName = !string.IsNullOrEmpty(c.SellerFirstName) 
                ? $"{c.SellerFirstName} {c.SellerLastName}" 
                : "Unknown Seller",
            DealerLocation = c.Location
        };
    }
}

public class ProfileStatsDto
{
    public int WishlistCount { get; set; }
    public int TotalListings { get; set; }
}
