using System;
using System.Collections.Generic;

namespace CarBazzar.Models.Entity;

public partial class Car
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Brand { get; set; } = null!;

    public string Model { get; set; } = null!;

    public int Year { get; set; }

    public decimal Price { get; set; }

    public int Mileage { get; set; }

    public string FuelType { get; set; } = null!;

    public string Transmission { get; set; } = null!;

    public string? Description { get; set; }

    public string Location { get; set; } = null!;

    public string Condition { get; set; } = "Old";

    public bool IsHidden { get; set; } = false;

    public string? SellerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CarImage> CarImages { get; set; } = new List<CarImage>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ApplicationUser? Seller { get; set; }

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
