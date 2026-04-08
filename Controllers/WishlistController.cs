using CarBazzar.Models.Cars;
using CarBazzar.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CarBazzar.Controllers;

[Authorize]
public class WishlistController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    public WishlistController(IHttpClientFactory httpClientFactory, UserManager<ApplicationUser> userManager)
    {
        _httpClientFactory = httpClientFactory;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var client = _httpClientFactory.CreateClient("CarBazaarApi");
        var wishlistCars = await client.GetFromJsonAsync<List<ApiCarDto>>($"/api/WishlistApi/{userId}/cars");

        var cards = wishlistCars?.Select(MapToCard).ToList() ?? new List<CarCard>();

        return View(cards);
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null && user.IsHidden)
        {
            TempData["Toast"] = "Hidden users cannot modify wishlist.";
            var localReferer = Request.Headers["Referer"].ToString();
            return !string.IsNullOrWhiteSpace(localReferer) ? Redirect(localReferer) : RedirectToAction("Browse", "Cars");
        }

        var userId = _userManager.GetUserId(User);

        var client = _httpClientFactory.CreateClient("CarBazaarApi");
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("carId", id.ToString()),
            new KeyValuePair<string, string>("userId", userId)
        });

        await client.PostAsync("/api/WishlistApi/toggle", content);

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrWhiteSpace(referer))
        {
            return Redirect(referer);
        }

        return RedirectToAction("Browse", "Cars");
    }

    private CarCard MapToCard(ApiCarDto c)
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
