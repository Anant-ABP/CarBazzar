using CarBazzar.Models;
using CarBazzar.Models.Entity;
using CarBazzar.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarBazzar.Controllers;

/// <summary>
/// Handles car booking requests and post-booking email notifications.
/// </summary>
[Authorize]
public class BookingController : Controller
{
    private readonly CarBazaarContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailService _emailService;
    private readonly IConfiguration _config;

    public BookingController(
        CarBazaarContext context,
        UserManager<ApplicationUser> userManager,
        EmailService emailService,
        IConfiguration config)
    {
        _context      = context;
        _userManager  = userManager;
        _emailService = emailService;
        _config       = config;
    }

    // POST: /Booking/Book
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int carId, string mobileNumber, string message)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        // 1. Load car; make sure it exists and is still available
        var car = await _context.Cars.FindAsync(carId);
        if (car == null)
        {
            TempData["Error"] = "Car not found.";
            return RedirectToAction("Index", "Cars");
        }

        if (car.IsHidden)
        {
            TempData["Error"] = "This car is no longer available for booking.";
            return RedirectToAction("Details", "Cars", new { id = carId });
        }

        // 2. Prevent duplicate booking of the same car by the SAME user
        bool alreadyBooked = await _context.Bookings
            .AnyAsync(b => b.CarId == carId && b.UserId == user.Id);

        if (alreadyBooked)
        {
            TempData["Error"] = "This car has already been booked.";
            return RedirectToAction("Details", "Cars", new { id = carId });
        }

        // 3. Save the booking
        var booking = new Booking
        {
            CarId        = carId,
            UserId       = user.Id,
            BookingDate  = DateTime.UtcNow,
            MobileNumber = mobileNumber,
            Message      = message
        };
        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();

        // 5. Send confirmation email to the user
        string carName = $"{car.Year} {car.Brand} {car.Model}";
        string formattedDate = booking.BookingDate.ToString("dd MMM yyyy, HH:mm") + " UTC";

        await _emailService.SendEmailAsync(
            toEmail: user.Email!,
            subject: "Car Booking Confirmed 🚗",
            htmlBody: $@"
                <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
                    <h2 style='color:#1a73e8;'>Booking Confirmed! 🚗</h2>
                    <p>Hi <strong>{user.FirstName}</strong>,</p>
                    <p>Your booking for <strong>{carName}</strong> has been confirmed.</p>
                    <table style='border-collapse:collapse;width:100%;'>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>Car</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{carName}</td>
                        </tr>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>Booking Date</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{formattedDate}</td>
                        </tr>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>Mobile</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{mobileNumber}</td>
                        </tr>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>Message</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{message}</td>
                        </tr>
                    </table>
                    <p style='margin-top:20px;color:#555;'>Thank you for using <strong>CarBazzar</strong>!</p>
                </div>"
        );

        // 6. Send alert email to the admin
        string adminEmail = _config["EmailSettings:AdminEmail"]!;
        await _emailService.SendEmailAsync(
            toEmail: adminEmail,
            subject: "New Booking Alert 🔔",
            htmlBody: $@"
                <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
                    <h2 style='color:#e53935;'>New Booking Alert 🔔</h2>
                    <p>A new booking has been placed on CarBazzar.</p>
                    <table style='border-collapse:collapse;width:100%;'>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>User Name</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{user.FirstName} {user.LastName}</td>
                        </tr>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>User Email</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{user.Email}</td>
                        </tr>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>Mobile Number</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{mobileNumber}</td>
                        </tr>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>Car</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{carName}</td>
                        </tr>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>Booking Date</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{formattedDate}</td>
                        </tr>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd;'><strong>Message Content</strong></td>
                            <td style='padding:8px;border:1px solid #ddd;'>{message}</td>
                        </tr>
                    </table>
                </div>"
        );

        TempData["Success"] = $"🎉 You successfully booked the {carName}! A confirmation email has been sent to {user.Email}.";
        return RedirectToAction("Details", "Cars", new { id = carId });
    }
}
