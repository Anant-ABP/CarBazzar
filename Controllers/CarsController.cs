using CarBazzar.Models.Cars;
using CarBazzar.Models.Entity;
using CarBazzar.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Threading.Tasks;

namespace CarBazzar.Controllers;

public class CarsController : Controller
{
    private readonly CarBazaarContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailService _emailService;

    public CarsController(CarBazaarContext context, IHttpClientFactory httpClientFactory, UserManager<ApplicationUser> userManager, EmailService emailService)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _userManager = userManager;
        _emailService = emailService;
    }

    private async Task<List<int>> GetUserWishlistAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return new List<int>();
        return await _context.Wishlists
            .Where(w => w.UserId == userId)
            .Select(w => w.CarId)
            .ToListAsync();
    }

    [HttpGet]
    public async Task<IActionResult> Browse(string search, string brand, int? minPrice, int? maxPrice, string fuel)
    {
        var userId = _userManager.GetUserId(User);
        ViewBag.WishlistCarIds = await GetUserWishlistAsync(userId);
        ViewBag.Search = search;

        var query = _context.Cars.Include(c => c.Seller)
            .Where(c => !c.IsHidden && c.Condition == "New");

        if (!string.IsNullOrEmpty(search)) query = query.Where(c => c.Title.Contains(search) || c.Brand.Contains(search) || c.Model.Contains(search));
        if (!string.IsNullOrEmpty(brand)) query = query.Where(c => c.Brand == brand);
        if (!string.IsNullOrEmpty(fuel))  query = query.Where(c => c.FuelType == fuel);
        if (minPrice.HasValue) query = query.Where(c => c.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(c => c.Price <= maxPrice.Value);

        var cars = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(cars.Select(MapCarToCard).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> OldBrowse(string search, string brand, int? minPrice, int? maxPrice, string fuel)
    {
        var userId = _userManager.GetUserId(User);
        ViewBag.WishlistCarIds = await GetUserWishlistAsync(userId);
        ViewBag.Search = search;

        var query = _context.Cars.Include(c => c.Seller)
            .Where(c => !c.IsHidden && c.Condition == "Old");

        if (!string.IsNullOrEmpty(search)) query = query.Where(c => c.Title.Contains(search) || c.Brand.Contains(search) || c.Model.Contains(search));
        if (!string.IsNullOrEmpty(userId)) query = query.Where(c => c.SellerId != userId);
        if (!string.IsNullOrEmpty(brand)) query = query.Where(c => c.Brand == brand);
        if (!string.IsNullOrEmpty(fuel))  query = query.Where(c => c.FuelType == fuel);
        if (minPrice.HasValue) query = query.Where(c => c.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(c => c.Price <= maxPrice.Value);

        var cars = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(cars.Select(MapCarToCard).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var c = await _context.Cars.Include(x => x.Seller).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();

        string currentUserId = _userManager.GetUserId(User);
        ViewBag.CurrentUserId = currentUserId;

        // Record recently viewed
        if (currentUserId != null)
        {
            var alreadyViewed = await _context.RecentlyViewedCars
                .AnyAsync(r => r.UserId == currentUserId && r.CarId == id);
            if (!alreadyViewed)
            {
                _context.RecentlyViewedCars.Add(new RecentlyViewed
                {
                    UserId   = currentUserId,
                    CarId    = id,
                    ViewedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        return View(MapCarToCard(c));
    }

    [HttpGet]
    public async Task<IActionResult> OldDetails(int id)
    {
        var c = await _context.Cars.Include(x => x.Seller).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();

        string currentUserId = _userManager.GetUserId(User);
        ViewBag.CurrentUserId = currentUserId;

        // Record recently viewed
        if (currentUserId != null)
        {
            var alreadyViewed = await _context.RecentlyViewedCars
                .AnyAsync(r => r.UserId == currentUserId && r.CarId == id);
            if (!alreadyViewed)
            {
                _context.RecentlyViewedCars.Add(new RecentlyViewed
                {
                    UserId   = currentUserId,
                    CarId    = id,
                    ViewedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        return View(MapCarToCard(c));
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

        var newCar = new Car
        {
            Title        = sellModel.Title,
            Brand        = sellModel.Brand,
            Model        = sellModel.Model,
            Year         = sellModel.Year,
            Price        = sellModel.Price,
            Mileage      = sellModel.Mileage,
            FuelType     = sellModel.FuelType,
            Transmission = sellModel.Transmission,
            Description  = sellModel.Description,
            Location     = sellModel.Location,
            Condition    = "Old",
            SellerId     = _userManager.GetUserId(User),
            CreatedAt    = DateTime.UtcNow
        };

        if (sellModel.Image != null && sellModel.Image.Length > 0)
        {
            using var ms = new MemoryStream();
            await sellModel.Image.CopyToAsync(ms);
            newCar.CarImages.Add(new CarImage
            {
                ImageData   = ms.ToArray(),
                ContentType = sellModel.Image.ContentType
            });
        }

        _context.Cars.Add(newCar);
        await _context.SaveChangesAsync();

        TempData["Toast"] = "Your car listing was submitted successfully.";

        // Send confirmation email to the seller
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
            await _emailService.SendEmailAsync(
                toEmail: user.Email,
                subject: "Car Listing Posted Successfully",
                htmlBody: $@"
                    <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
                        <h2 style='color:#1a73e8;'>Listing Posted! 🚗</h2>
                        <p>Hello <strong>{user.FirstName}</strong>,</p>
                        <p>Your car has been successfully listed for sale on CarBazaar.</p>
                        <p>We will notify you when buyers show interest.</p>
                        <p style='margin-top:20px;color:#555;'>Thank you!</p>
                    </div>"
            );
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

    private CarCard MapCarToCard(Car c)
    {
        return new CarCard
        {
            Id           = c.Id,
            Title        = c.Title,
            Specs        = c.Model,
            Price        = "₹" + c.Price.ToString("N0"),
            ImageUrl     = Url.Action("GetImage", "Cars", new { id = c.Id }) ?? "",
            Year         = c.Year,
            FuelType     = c.FuelType,
            Mileage      = c.Mileage + " km",
            Transmission = c.Transmission,
            BodyType     = c.Brand,
            Color        = "Unknown",
            Description  = c.Description ?? "",
            Features     = new List<string>(),
            DealerName   = (c.Seller != null && !string.IsNullOrEmpty(c.Seller.FirstName))
                ? $"{c.Seller.FirstName} {c.Seller.LastName}"
                : "Unknown Seller",
            DealerPhone    = c.Seller?.PhoneNumber ?? "",
            DealerEmail    = c.Seller?.Email ?? "",
            DealerLocation = c.Location,
            SellerId       = c.SellerId ?? ""
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