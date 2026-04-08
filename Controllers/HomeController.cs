using System.Diagnostics;
using CarBazzar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CarBazzar.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace CarBazzar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
        }

        private async Task<List<int>> GetUserWishlistAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return new List<int>();
            var client = _httpClientFactory.CreateClient("CarBazaarApi");
            return await client.GetFromJsonAsync<List<int>>($"/api/WishlistApi/user/{userId}") ?? new List<int>();
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.WishlistCarIds = await GetUserWishlistAsync(userId);

            var client = _httpClientFactory.CreateClient("CarBazaarApi");
            var allCars = await client.GetFromJsonAsync<List<ApiCarDto>>("/api/CarsApi");
            
            ViewBag.LatestCars = allCars?.Take(3).ToList() ?? new List<ApiCarDto>();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
