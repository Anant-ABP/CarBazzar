using System.Linq;
using System.Threading.Tasks;
using CarBazzar.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarBazzar.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ProfileApiController : ControllerBase
{
    private readonly CarBazaarContext _context;

    public ProfileApiController(CarBazaarContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}/stats")]
    public async Task<ActionResult> GetUserStats(string userId)
    {
        var wishlistCount = await _context.Wishlists.CountAsync(w => w.UserId == userId);
        var totalListings = await _context.Cars.CountAsync(c => c.SellerId == userId && !c.IsHidden);

        return Ok(new 
        {
            WishlistCount = wishlistCount,
            TotalListings = totalListings
        });
    }
}
