using CarBazzar.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace CarBazzar.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class CarsApiController : ControllerBase
{
    private readonly CarBazaarContext _context;

    public CarsApiController(CarBazaarContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetCars([FromQuery] string matchCondition = null, [FromQuery] string brand = null, [FromQuery] int? minPrice = null, [FromQuery] int? maxPrice = null, [FromQuery] string fuel = null, [FromQuery] string excludeSellerId = null)
    {
        var query = _context.Cars.Include(c => c.Seller).Where(c => !c.IsHidden);

        if (!string.IsNullOrEmpty(matchCondition))
        {
            query = query.Where(c => c.Condition == matchCondition);
        }
        if (!string.IsNullOrEmpty(excludeSellerId))
        {
            query = query.Where(c => c.SellerId != excludeSellerId || c.SellerId == null);
        }
        if (!string.IsNullOrEmpty(brand)) query = query.Where(c => c.Brand == brand);
        if (!string.IsNullOrEmpty(fuel)) query = query.Where(c => c.FuelType == fuel);
        if (minPrice.HasValue) query = query.Where(c => c.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(c => c.Price <= maxPrice.Value);

        var cars = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        
        return Ok(cars.Select(c => new 
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

    [HttpGet("{id}")]
    public async Task<ActionResult> GetCar(int id)
    {
        var c = await _context.Cars.Include(x => x.Seller).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        return Ok(new 
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
        });
    }

    [HttpPost]
    public async Task<ActionResult> PostCar([FromBody] ApiCarCreateDto dto)
    {
        var newCar = new Car
        {
            Title = dto.Title,
            Brand = dto.Brand,
            Model = dto.Model,
            Year = dto.Year,
            Price = dto.Price,
            Mileage = dto.Mileage,
            FuelType = dto.FuelType,
            Transmission = dto.Transmission,
            Description = dto.Description,
            Location = dto.Location,
            Condition = dto.Condition ?? "Old",
            SellerId = dto.SellerId,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.ImageData != null && dto.ImageData.Length > 0)
        {
            newCar.CarImages.Add(new CarImage
            {
                ImageData = dto.ImageData,
                ContentType = dto.ImageContentType
            });
        }

        _context.Cars.Add(newCar);
        await _context.SaveChangesAsync();
        return Ok(new { id = newCar.Id });
    }

    [HttpGet("{id}/image")]
    public async Task<IActionResult> GetImage(int id)
    {
        var carImage = await _context.CarImages.FirstOrDefaultAsync(i => i.CarId == id);
        if (carImage != null && carImage.ImageData != null)
        {
            return File(carImage.ImageData, carImage.ContentType);
        }
        return NotFound();
    }
}

public class ApiCarCreateDto
{
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
    public string SellerId { get; set; }
    public byte[] ImageData { get; set; }
    public string ImageContentType { get; set; }
}
