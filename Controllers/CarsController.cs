using CarBazzar.Models.Cars;
using Microsoft.AspNetCore.Mvc;

namespace CarBazzar.Controllers;

public class CarsController : Controller
{
    // ── New Cars (Buy Cars) ──────────────────────────────────────────────────
    internal static readonly IReadOnlyList<CarCard> DemoCars =
    [
        new() {
            Id = 1, Title = "BMW M4 Competition", Specs = "M xDrive Coupe",
            Price = "₹78,900", ImageUrl = "/images/coupe.jpg",
            Year = 2024, FuelType = "Gasoline", Mileage = "3k km",
            Transmission = "Automatic", BodyType = "Coupe", Color = "Alpine White",
            Description = "Stunning BMW M4 Competition in Alpine White with carbon fiber package. Twin-turbo inline-6 producing 503hp. Full factory warranty remaining.",
            Features = ["Carbon Fiber Roof", "Head-Up Display", "Adaptive M Suspension", "M Carbon Bucket Seats", "Harman Kardon Audio", "M Drive Professional"],
            DealerName = "Premium Auto Gallery", DealerPhone = "(310) 555-0142", DealerLocation = "Los Angeles, CA"
        },
        new() {
            Id = 2, Title = "Tesla Model 3", Specs = "Standard Range Plus",
            Price = "₹42,500", ImageUrl = "/images/images.jpg",
            Year = 2023, FuelType = "Electric", Mileage = "13k km",
            Transmission = "Automatic", BodyType = "Sedan", Color = "Pearl White",
            Description = "Nearly new Tesla Model 3 Standard Range Plus. Autopilot enabled, over-the-air updates, and minimal charging costs.",
            Features = ["Autopilot", "Glass Roof", "15\" Touchscreen", "Premium Audio", "Wireless Charging", "Over-Air Updates"],
            DealerName = "Tesla Certified", DealerPhone = "(415) 555-0198", DealerLocation = "San Francisco, CA"
        },
        new() {
            Id = 3, Title = "Mercedes-Benz GLE 450", Specs = "4MATIC",
            Price = "₹67,800", ImageUrl = "/images/sedan.jpg",
            Year = 2024, FuelType = "Hybrid", Mileage = "2k km",
            Transmission = "Automatic", BodyType = "SUV", Color = "Graphite Grey",
            Description = "Luxurious GLE 450 4MATIC with mild-hybrid technology. Loaded with premium features and barely driven.",
            Features = ["Burmester Sound", "Panoramic Roof", "Air Suspension", "360° Camera", "Night Vision", "Augmented Nav"],
            DealerName = "Mercedes-Benz of LA", DealerPhone = "(310) 555-0234", DealerLocation = "Beverly Hills, CA"
        },
        new() {
            Id = 4, Title = "Audi Q8", Specs = "Premium Plus",
            Price = "₹72,000", ImageUrl = "/images/suv.jpg",
            Year = 2024, FuelType = "Gasoline", Mileage = "1k km",
            Transmission = "Automatic", BodyType = "SUV", Color = "Navarra Blue",
            Description = "Flagship Audi Q8 with all-wheel drive and stunning interior. Virtual cockpit and Bang & Olufsen sound.",
            Features = ["Virtual Cockpit", "B&O Sound", "Matrix LED", "Adaptive Cruise", "Massage Seats", "Head-Up Display"],
            DealerName = "Audi Beverly Hills", DealerPhone = "(310) 555-0321", DealerLocation = "Beverly Hills, CA"
        },
        new() {
            Id = 5, Title = "Porsche 911 Carrera", Specs = "S Coupe",
            Price = "₹124,000", ImageUrl = "/images/truc.jpg",
            Year = 2024, FuelType = "Gasoline", Mileage = "500 km",
            Transmission = "PDK", BodyType = "Coupe", Color = "Guards Red",
            Description = "Iconic Porsche 911 Carrera S. Sport Chrono package, PASM sport suspension, and the legendary flat-six.",
            Features = ["Sport Chrono", "PASM Suspension", "Sport Exhaust", "PCM Navigation", "Bose Sound", "Heated Seats"],
            DealerName = "Porsche Beverly Hills", DealerPhone = "(310) 555-0412", DealerLocation = "Beverly Hills, CA"
        },
        new() {
            Id = 6, Title = "Ford Mustang GT", Specs = "Premium Fastback",
            Price = "₹48,900", ImageUrl = "/images/suv.jpg",
            Year = 2024, FuelType = "Gasoline", Mileage = "2k km",
            Transmission = "Automatic", BodyType = "Coupe", Color = "Race Red",
            Description = "All-new 2024 Mustang GT with 5.0L V8 Coyote engine. Active valve exhaust and performance package included.",
            Features = ["5.0L V8 Engine", "Active Exhaust", "Performance Pkg", "12\" LCD Cluster", "B&O Sound", "MagneRide"],
            DealerName = "Ford of Hollywood", DealerPhone = "(323) 555-0567", DealerLocation = "Hollywood, CA"
        },
    ];

    // ── Old / Used Cars (Buy Old Car) ────────────────────────────────────────
    internal static readonly IReadOnlyList<CarCard> OldCars =
    [
        new() {
            Id = 101, Title = "Tesla Model 3", Specs = "Standard Range Plus",
            Price = "₹38,990", ImageUrl = "/images/images.jpg",
            Year = 2022, FuelType = "Electric", Mileage = "15k km",
            Transmission = "Automatic", BodyType = "Sedan", Color = "White",
            Description = "Well-maintained used Tesla Model 3 with full charge history and no accidents. Ready for immediate transfer.",
            Features = ["Autopilot", "Glass Roof", "15\" Touchscreen", "Premium Audio", "Wireless Charging", "Low Mileage"],
            DealerName = "AutoTrust Gallery", DealerPhone = "(310) 555-0142", DealerLocation = "Los Angeles, CA"
        },
        new() {
            Id = 102, Title = "BMW 5 Series", Specs = "530i M Sport",
            Price = "₹52,500", ImageUrl = "/images/sedan.jpg",
            Year = 2021, FuelType = "Petrol", Mileage = "32k km",
            Transmission = "Automatic", BodyType = "Sedan", Color = "Black Sapphire",
            Description = "Pre-owned BMW 5 Series in excellent condition. Service history available. One careful owner.",
            Features = ["M Sport Package", "Harman Kardon", "Parking Sensors", "Lane Assist", "Sunroof", "Heated Seats"],
            DealerName = "Premium Motors", DealerPhone = "(310) 555-0199", DealerLocation = "Santa Monica, CA"
        },
        new() {
            Id = 103, Title = "Mercedes C-Class", Specs = "C200 Avantgarde",
            Price = "₹45,200", ImageUrl = "/images/coupe.jpg",
            Year = 2020, FuelType = "Hybrid", Mileage = "41k km",
            Transmission = "Automatic", BodyType = "Sedan", Color = "Silver",
            Description = "Elegant Mercedes C-Class hybrid with full service records. Fuel-efficient and luxuriously appointed.",
            Features = ["MBUX Infotainment", "Burmester Sound", "Ambient Lighting", "Keyless Entry", "360° Camera", "Wireless Charging"],
            DealerName = "Star Motors", DealerPhone = "(323) 555-0300", DealerLocation = "Burbank, CA"
        },
        new() {
            Id = 104, Title = "Ford Mustang GT", Specs = "5.0L V8 Fastback",
            Price = "₹56,900", ImageUrl = "/images/truc.jpg",
            Year = 2023, FuelType = "Petrol", Mileage = "5k km",
            Transmission = "Manual", BodyType = "Coupe", Color = "Race Red",
            Description = "Near-new Mustang GT with manual gearbox and performance package. Barely driven with full warranty.",
            Features = ["5.0L V8", "Performance Package", "Recaro Seats", "Active Exhaust", "Track Mode", "Launch Control"],
            DealerName = "Muscle Cars USA", DealerPhone = "(310) 555-0450", DealerLocation = "Torrance, CA"
        },
        new() {
            Id = 105, Title = "Audi A6", Specs = "45 TFSI Quattro",
            Price = "₹48,700", ImageUrl = "/images/sedan.jpg",
            Year = 2021, FuelType = "Hybrid", Mileage = "28k km",
            Transmission = "Automatic", BodyType = "Sedan", Color = "Floret Silver",
            Description = "Sophisticated Audi A6 with quattro all-wheel drive. Excellent condition with full Audi service history.",
            Features = ["Quattro AWD", "Virtual Cockpit", "Matrix LED", "Bang & Olufsen", "Air Suspension", "Massage Seats"],
            DealerName = "Audi Pre-Owned", DealerPhone = "(310) 555-0520", DealerLocation = "West Hollywood, CA"
        },
        new() {
            Id = 106, Title = "Porsche Macan", Specs = "S AWD",
            Price = "₹72,500", ImageUrl = "/images/suv.jpg",
            Year = 2022, FuelType = "Petrol", Mileage = "12k km",
            Transmission = "Automatic", BodyType = "SUV", Color = "Mahogany",
            Description = "Sporty Porsche Macan S with all-wheel drive. Sport Chrono package and panoramic roof. Immaculate condition.",
            Features = ["Sport Chrono", "Panoramic Roof", "BOSE Sound", "PASM Suspension", "Porsche Connect", "Lane Change Assist"],
            DealerName = "Porsche Approved", DealerPhone = "(310) 555-0610", DealerLocation = "Beverly Hills, CA"
        },
    ];

    // ── Actions ──────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Browse() => View(DemoCars);

    [HttpGet]
    public IActionResult OldBrowse() => View(OldCars);

    [HttpGet]
    public IActionResult Details(int id)
    {
        var car = DemoCars.FirstOrDefault(c => c.Id == id) ?? DemoCars[0];
        return View(car);
    }

    [HttpGet]
    public IActionResult OldDetails(int id)
    {
        var car = OldCars.FirstOrDefault(c => c.Id == id) ?? OldCars[0];
        return View(car);
    }

    [HttpGet]
    public IActionResult Sell() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Sell(object _)
    {
        TempData["Toast"] = "Your car listing was submitted (demo).";
        return RedirectToAction("Browse");
    }
}