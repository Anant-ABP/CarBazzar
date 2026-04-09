using CarBazzar.Models.Cars;
using CarBazzar.Models.Entity;
using CarBazzar.Services;
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
    private readonly EmailService _emailService;

    public AdminController(CarBazaarContext context, UserManager<ApplicationUser> userManager, EmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewBag.TotalUsers = await _userManager.Users.CountAsync();
        ViewBag.TotalCars = await _context.Cars.CountAsync();
        ViewBag.TotalMessages = await _context.Messages.CountAsync();
        var recentCars = await _context.Cars.OrderByDescending(c => c.CreatedAt).Take(5).ToListAsync();
        return View(recentCars);
    }

    public async Task<IActionResult> Listings(string search, string status, string brand)
    {
        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Brand = brand;

        var query = _context.Cars.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Title.Contains(search) || c.Model.Contains(search) || c.Brand.Contains(search));
        }

        if (!string.IsNullOrEmpty(status) && status != "All Status")
        {
            if (status == "Approved") query = query.Where(c => !c.IsHidden);
            else if (status == "Pending") query = query.Where(c => c.IsHidden);
        }

        if (!string.IsNullOrEmpty(brand) && brand != "All Brands")
        {
            query = query.Where(c => c.Brand == brand);
        }

        var cars = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
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

        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;

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
            Condition = sellModel.Condition ?? "New",
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

        var conditionLabel = (newCar.Condition == "Old") ? "Used / Old Car → Buy Old Car section" : "New Car → Browse New Cars section";

        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
            await _emailService.SendEmailAsync(
                toEmail: user.Email,
                subject: $"✅ New Listing Added: {newCar.Title}",
                htmlBody: $@"
                    <div style='font-family:Inter,Arial,sans-serif;max-width:600px;margin:auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>
                        <div style='background:linear-gradient(135deg,#f97316,#e65a00);padding:32px 36px;text-align:center;'>
                            <div style='font-size:36px;margin-bottom:10px;'>🚗</div>
                            <h2 style='color:#fff;margin:0;font-size:22px;font-weight:700;letter-spacing:-0.3px;'>Listing Added Successfully!</h2>
                            <p style='color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:14px;'>CarBazaar Admin Panel</p>
                        </div>
                        <div style='padding:32px 36px;'>
                            <p style='color:#1a1008;font-size:15px;'>Hello <strong>{user.FirstName}</strong>,</p>
                            <p style='color:#555;font-size:14px;line-height:1.6;'>You have successfully posted a new vehicle listing on CarBazaar. Here are the details:</p>
                            <div style='background:#fdfaf8;border-radius:12px;padding:20px 24px;margin:20px 0;border:1px solid #f0ece7;'>
                                <table style='width:100%;border-collapse:collapse;'>
                                    <tr><td style='padding:7px 0;font-size:13px;color:#a29a8d;font-weight:600;text-transform:uppercase;letter-spacing:.06em;width:40%;'>Title</td><td style='padding:7px 0;font-size:14px;color:#1a1008;font-weight:600;'>{newCar.Title}</td></tr>
                                    <tr><td style='padding:7px 0;font-size:13px;color:#a29a8d;font-weight:600;text-transform:uppercase;letter-spacing:.06em;'>Brand</td><td style='padding:7px 0;font-size:14px;color:#1a1008;'>{newCar.Brand} {newCar.Model}</td></tr>
                                    <tr><td style='padding:7px 0;font-size:13px;color:#a29a8d;font-weight:600;text-transform:uppercase;letter-spacing:.06em;'>Year</td><td style='padding:7px 0;font-size:14px;color:#1a1008;'>{newCar.Year}</td></tr>
                                    <tr><td style='padding:7px 0;font-size:13px;color:#a29a8d;font-weight:600;text-transform:uppercase;letter-spacing:.06em;'>Price</td><td style='padding:7px 0;font-size:16px;color:#f97316;font-weight:700;'>₹{newCar.Price:N0}</td></tr>
                                    <tr><td style='padding:7px 0;font-size:13px;color:#a29a8d;font-weight:600;text-transform:uppercase;letter-spacing:.06em;'>Fuel</td><td style='padding:7px 0;font-size:14px;color:#1a1008;'>{newCar.FuelType}</td></tr>
                                    <tr><td style='padding:7px 0;font-size:13px;color:#a29a8d;font-weight:600;text-transform:uppercase;letter-spacing:.06em;'>Transmission</td><td style='padding:7px 0;font-size:14px;color:#1a1008;'>{newCar.Transmission}</td></tr>
                                    <tr><td style='padding:7px 0;font-size:13px;color:#a29a8d;font-weight:600;text-transform:uppercase;letter-spacing:.06em;'>Location</td><td style='padding:7px 0;font-size:14px;color:#1a1008;'>{newCar.Location}</td></tr>
                                    <tr><td style='padding:7px 0;font-size:13px;color:#a29a8d;font-weight:600;text-transform:uppercase;letter-spacing:.06em;'>Listed As</td><td style='padding:7px 0;'><span style='background:#e6f7ef;color:#138a4a;font-size:12px;font-weight:600;padding:3px 10px;border-radius:999px;'>{conditionLabel}</span></td></tr>
                                </table>
                            </div>
                            <p style='color:#555;font-size:13px;line-height:1.6;'>The listing is now live and visible to buyers on CarBazaar. You can manage it from the <strong>Admin Listings</strong> panel.</p>
                        </div>
                        <div style='background:#f9f6f3;padding:20px 36px;text-align:center;border-top:1px solid #f0ece7;'>
                            <p style='color:#a29a8d;font-size:12px;margin:0;'>This is an automated email from <strong>CarBazaar Admin Panel</strong>. Do not reply.</p>
                        </div>
                    </div>"
            );
        }

        TempData["Toast"] = "Car listing added successfully.";
        return RedirectToAction("Listings");
    }

    [HttpGet]
    public async Task<IActionResult> UserDetails(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        
        ViewBag.UserCars = await _context.Cars.Where(c => c.SellerId == id).ToListAsync();
        ViewBag.UserBookings = await _context.Bookings
            .Include(b => b.Car)
            .Where(b => b.UserId == id)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
            
        return View(user);
    }
    
    [HttpGet]
    public async Task<IActionResult> Bookings()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Car)
            .Include(b => b.User)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
            
        return View(bookings);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditListingAction(int id, SellViewModel model)
    {
        // Simple edit action, maps values back from modal and saves
        var car = await _context.Cars.FindAsync(id);
        if (car != null)
        {
            car.Title = model.Title;
            car.Brand = model.Brand;
            car.Model = model.Model;
            car.Price = model.Price;
            car.Condition = model.Condition;
            car.Year = model.Year;
            car.Mileage = model.Mileage;
            car.FuelType = model.FuelType;
            car.Transmission = model.Transmission;
            car.Description = model.Description;
            car.Location = model.Location;

            await _context.SaveChangesAsync();
            TempData["Toast"] = "Listing updated.";
        }
        return RedirectToAction("Listings");
    }
}
