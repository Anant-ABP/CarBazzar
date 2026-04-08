using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarBazzar.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarBazzar.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class RecentlyViewedApiController : ControllerBase
{
    private readonly CarBazaarContext _context;

    public RecentlyViewedApiController(CarBazaarContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult> GetRecentlyViewedCars(string userId)
    {
        var viewed = await _context.RecentlyViewedCars
            .Include(r => r.Car)
            .ThenInclude(c => c.Seller)
            .Where(r => r.UserId == userId)
            .GroupBy(r => r.CarId)
            .Select(g => g.OrderByDescending(x => x.ViewedAt).First())
            .OrderByDescending(r => r.ViewedAt)
            .Take(10)
            .Select(r => r.Car)
            .ToListAsync();

        return Ok(viewed.Select(c => new 
        {
            c.Id,
            c.Title,
            c.Brand,
            c.Model,
            c.Year,
            c.Price,
            c.Mileage,
            c.FuelType,
            c.Transmission,
            c.Description,
            c.Location,
            c.Condition,
            c.IsHidden,
            c.SellerId,
            SellerFirstName = c.Seller?.FirstName,
            SellerLastName = c.Seller?.LastName,
            SellerPhoneNumber = c.Seller?.PhoneNumber,
            SellerEmail = c.Seller?.Email,
            c.CreatedAt
        }));
    }

    [HttpPost]
    public async Task<IActionResult> TrackView([FromForm] string userId, [FromForm] int carId)
    {
        if (string.IsNullOrEmpty(userId)) return BadRequest();

        var existing = await _context.RecentlyViewedCars.FirstOrDefaultAsync(r => r.UserId == userId && r.CarId == carId);
        
        if (existing != null)
        {
            existing.ViewedAt = DateTime.UtcNow;
            _context.RecentlyViewedCars.Update(existing);
        }
        else
        {
            _context.RecentlyViewedCars.Add(new RecentlyViewed
            {
                UserId = userId,
                CarId = carId,
                ViewedAt = DateTime.UtcNow
            });
        }
        
        await _context.SaveChangesAsync();
        return Ok();
    }
}
