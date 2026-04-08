using CarBazzar.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarBazzar.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class WishlistApiController : ControllerBase
{
    private readonly CarBazaarContext _context;

    public WishlistApiController(CarBazaarContext context)
    {
        _context = context;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<int>>> GetUserWishlist(string userId)
    {
        var ids = await _context.Wishlists
            .Where(w => w.UserId == userId)
            .Select(w => w.CarId)
            .ToListAsync();
        return Ok(ids);
    }

    [HttpGet("{userId}/cars")]
    public async Task<ActionResult> GetUserWishlistCars(string userId)
    {
        var wishlistCars = await _context.Wishlists
            .Include(w => w.Car)
            .ThenInclude(c => c.Seller)
            .Where(w => w.UserId == userId)
            .Select(w => w.Car)
            .ToListAsync();

        return Ok(wishlistCars.Select(c => new 
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

    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle([FromForm] int carId, [FromForm] string userId)
    {
        var existing = await _context.Wishlists.FirstOrDefaultAsync(w => w.CarId == carId && w.UserId == userId);
        if (existing != null)
        {
            _context.Wishlists.Remove(existing);
        }
        else
        {
            _context.Wishlists.Add(new Wishlist { CarId = carId, UserId = userId, AddedAt = System.DateTime.UtcNow });
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
}
