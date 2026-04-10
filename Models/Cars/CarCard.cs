namespace CarBazzar.Models.Cars;

public sealed class CarCard
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string Specs { get; init; } = "";        
    public string Price { get; init; } = "";
    public string ImageUrl { get; init; } = "";

    
    public int Year { get; init; }
    public string FuelType { get; init; } = "";
    public string Mileage { get; init; } = "";
    public string Transmission { get; init; } = "";
    public string BodyType { get; init; } = "";
    public string Color { get; init; } = "";

    
    public string Description { get; init; } = "";
    public IReadOnlyList<string> Features { get; init; } = [];

    
    public string DealerName { get; init; } = "";
    public string DealerPhone { get; init; } = "";
    public string DealerEmail { get; init; } = "";
    public string DealerLocation { get; init; } = "";
    public string SellerId { get; init; } = "";
}