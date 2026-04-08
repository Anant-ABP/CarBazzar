using CarBazzar.Models.Cars;
using CarBazzar.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace CarBazzar.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly CarBazaarContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(CarBazaarContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewBag.TotalUsers = await _userManager.Users.CountAsync();
        ViewBag.TotalCars = await _context.Cars.CountAsync();
        ViewBag.TotalMessages = await _context.Messages.CountAsync();
        var recentCars = await _context.Cars.OrderByDescending(c => c.CreatedAt).Take(5).ToListAsync();
        return View(recentCars);
    }

    public async Task<IActionResult> Listings()
    {
        var cars = await _context.Cars.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(cars);
    }

    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();
        return View(users);
    }

    [HttpGet]
    public IActionResult AddListing()
    {
        return View(new SellViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddListing(SellViewModel sellModel)
    {
        if (!ModelState.IsValid)
        {
            return View(sellModel);
        }

        var userId = _userManager.GetUserId(User);

        var newCar = new Car
        {
            Title = sellModel.Title,
            Brand = sellModel.Brand,
            Model = sellModel.Model,
            Year = sellModel.Year,
            Price = sellModel.Price,
            Mileage = sellModel.Mileage,
            FuelType = sellModel.FuelType,
            Transmission = sellModel.Transmission,
            Description = sellModel.Description,
            Location = sellModel.Location,
            Condition = "New",
            SellerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        if (sellModel.Image != null && sellModel.Image.Length > 0)
        {
            using var ms = new MemoryStream();
            await sellModel.Image.CopyToAsync(ms);
            
            var image = new CarImage
            {
                ImageData = ms.ToArray(),
                ContentType = sellModel.Image.ContentType
            };
            newCar.CarImages.Add(image);
        }

        _context.Cars.Add(newCar);
        await _context.SaveChangesAsync();

        TempData["Toast"] = "Car listing added successfully.";
        return RedirectToAction("Listings");
    }

    [HttpGet]
    public async Task<IActionResult> UserDetails(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HideUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            user.IsHidden = !user.IsHidden;
            await _userManager.UpdateAsync(user);
            TempData["Toast"] = user.IsHidden ? "User hidden." : "User visible.";
        }
        return RedirectToAction("Users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            var userCars = await _context.Cars.Where(c => c.SellerId == id).ToListAsync();
            var carIds = userCars.Select(c => c.Id).ToList();

            var carWishlists = await _context.Wishlists.Where(w => carIds.Contains(w.CarId)).ToListAsync();
            _context.Wishlists.RemoveRange(carWishlists);

            var carMessages = await _context.Messages.Where(m => carIds.Contains(m.CarId)).ToListAsync();
            _context.Messages.RemoveRange(carMessages);

            _context.Cars.RemoveRange(userCars);

            var userMessages = await _context.Messages.Where(m => m.SenderId == id || m.ReceiverId == id).ToListAsync();
            _context.Messages.RemoveRange(userMessages);

            var userWishlists = await _context.Wishlists.Where(w => w.UserId == id).ToListAsync();
            _context.Wishlists.RemoveRange(userWishlists);

            await _context.SaveChangesAsync();
            await _userManager.DeleteAsync(user);
            TempData["Toast"] = "User deleted.";
        }
        return RedirectToAction("Users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HideListing(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car != null)
        {
            car.IsHidden = !car.IsHidden;
            await _context.SaveChangesAsync();
            TempData["Toast"] = car.IsHidden ? "Listing hidden." : "Listing visible.";
        }
        return RedirectToAction("Listings");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteListing(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car != null)
        {
            var wishlists = await _context.Wishlists.Where(w => w.CarId == id).ToListAsync();
            _context.Wishlists.RemoveRange(wishlists);

            var messages = await _context.Messages.Where(m => m.CarId == id).ToListAsync();
            _context.Messages.RemoveRange(messages);

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            TempData["Toast"] = "Listing deleted.";
        }
        return RedirectToAction("Listings");
    }
}
