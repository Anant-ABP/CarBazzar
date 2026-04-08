using CarBazzar.Models.Cars;
using CarBazzar.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CarBazzar.Controllers;

public class CarsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    public CarsController(IHttpClientFactory httpClientFactory, UserManager<ApplicationUser> userManager)
    {
        _httpClientFactory = httpClientFactory;
        _userManager = userManager;
    }

    private async Task<List<int>> GetUserWishlistAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return new List<int>();
        var client = _httpClientFactory.CreateClient("CarBazaarApi");
        return await client.GetFromJsonAsync<List<int>>($"/api/WishlistApi/user/{userId}") ?? new List<int>();
    }

    [HttpGet]
    public async Task<IActionResult> Browse(string brand, int? minPrice, int? maxPrice, string fuel)
    {
        var userId = _userManager.GetUserId(User);
        ViewBag.WishlistCarIds = await GetUserWishlistAsync(userId);

        var client = _httpClientFactory.CreateClient("CarBazaarApi");
        var qs = $"?matchCondition=New&brand={brand}&minPrice={minPrice}&maxPrice={maxPrice}&fuel={fuel}";
        if (userId != null) qs += $"&excludeSellerId={userId}";

        var rawCars = await client.GetFromJsonAsync<List<ApiCarDto>>($"/api/CarsApi{qs}");
        
        return View(rawCars?.Select(MapToCard).ToList() ?? new List<CarCard>());
    }

    [HttpGet]
    public async Task<IActionResult> OldBrowse(string brand, int? minPrice, int? maxPrice, string fuel)
    {
        var userId = _userManager.GetUserId(User);
        ViewBag.WishlistCarIds = await GetUserWishlistAsync(userId);

        var client = _httpClientFactory.CreateClient("CarBazaarApi");
        var qs = $"?matchCondition=Old&brand={brand}&minPrice={minPrice}&maxPrice={maxPrice}&fuel={fuel}";
        if (userId != null) qs += $"&excludeSellerId={userId}";

        var rawCars = await client.GetFromJsonAsync<List<ApiCarDto>>($"/api/CarsApi{qs}");
        
        return View(rawCars?.Select(MapToCard).ToList() ?? new List<CarCard>());
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var client = _httpClientFactory.CreateClient("CarBazaarApi");
        var response = await client.GetAsync($"/api/CarsApi/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var car = await response.Content.ReadFromJsonAsync<ApiCarDto>();
        ViewBag.CurrentUserId = _userManager.GetUserId(User);

        // Record recently viewed
        if (ViewBag.CurrentUserId != null)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("userId", ViewBag.CurrentUserId),
                new KeyValuePair<string, string>("carId", id.ToString())
            });
            await client.PostAsync("/api/RecentlyViewedApi", content);
        }
        
        return View(MapToCard(car));
    }

    [HttpGet]
    public async Task<IActionResult> OldDetails(int id)
    {
        var client = _httpClientFactory.CreateClient("CarBazaarApi");
        var response = await client.GetAsync($"/api/CarsApi/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var car = await response.Content.ReadFromJsonAsync<ApiCarDto>();
        ViewBag.CurrentUserId = _userManager.GetUserId(User);
        
        // Record recently viewed
        if (ViewBag.CurrentUserId != null)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("userId", ViewBag.CurrentUserId),
                new KeyValuePair<string, string>("carId", id.ToString())
            });
            await client.PostAsync("/api/RecentlyViewedApi", content);
        }

        return View(MapToCard(car));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Sell()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null && user.IsHidden)
        {
            TempData["Toast"] = "Hidden users cannot sell cars.";
            return RedirectToAction("OldBrowse");
        }
        return View(new SellViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sell(SellViewModel sellModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null && user.IsHidden)
        {
            TempData["Toast"] = "Hidden users cannot sell cars.";
            return RedirectToAction("OldBrowse");
        }

        if (!ModelState.IsValid)
        {
            return View(sellModel);
        }

        var userId = _userManager.GetUserId(User);

        byte[] imgData = null;
        string imgContentType = null;
        if (sellModel.Image != null && sellModel.Image.Length > 0)
        {
            using var ms = new MemoryStream();
            await sellModel.Image.CopyToAsync(ms);
            imgData = ms.ToArray();
            imgContentType = sellModel.Image.ContentType;
        }

        var dto = new CarBazzar.Controllers.Api.ApiCarCreateDto
        {
            Title = sellModel.Title,
            Brand = sellModel.Brand,
            Model = sellModel.Model,
            Year = sellModel.Year,
            Price = sellModel.Price,
            Mileage = sellModel.Mileage, // Removed ?? 0 because Mileage is int
            FuelType = sellModel.FuelType,
            Transmission = sellModel.Transmission,
            Description = sellModel.Description,
            Location = sellModel.Location,
            Condition = "Old",
            SellerId = userId,
            ImageData = imgData,
            ImageContentType = imgContentType
        };

        var client = _httpClientFactory.CreateClient("CarBazaarApi");
        var res = await client.PostAsJsonAsync("/api/CarsApi", dto);
        
        if (res.IsSuccessStatusCode)
        {
            TempData["Toast"] = "Your car listing was submitted successfully.";
        }

        return RedirectToAction("OldBrowse");
    }

    [HttpGet]
    public async Task<IActionResult> GetImage(int id)
    {
        // Proxy call to API
        var client = _httpClientFactory.CreateClient("CarBazaarApi");
        var response = await client.GetAsync($"/api/CarsApi/{id}/image");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
            return File(content, contentType);
        }
        
        var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "suv.jpg");
        if (System.IO.File.Exists(defaultPath))
            return File(System.IO.File.ReadAllBytes(defaultPath), "image/jpeg");
            
        return NotFound();
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
            DealerPhone = c.SellerPhoneNumber ?? "",
            DealerEmail = c.SellerEmail ?? "",
            DealerLocation = c.Location,
            SellerId = c.SellerId ?? ""
        };
    }
}

public class ApiCarDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string FuelType { get; set; }
    public string Transmission { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string Condition { get; set; }
    public bool IsHidden { get; set; }
    public string SellerId { get; set; }
    public string SellerFirstName { get; set; }
    public string SellerLastName { get; set; }
    public string SellerPhoneNumber { get; set; }
    public string SellerEmail { get; set; }
}