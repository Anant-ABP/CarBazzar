namespace CarBazzar.Models.Cars;

public sealed class CarCard
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string Specs { get; init; } = "";          // subtitle line e.g. "M xDrive Coupe"
    public string Price { get; init; } = "";
    public string ImageUrl { get; init; } = "";

    // Spec details
    public int Year { get; init; }
    public string FuelType { get; init; } = "";
    public string Mileage { get; init; } = "";
    public string Transmission { get; init; } = "";
    public string BodyType { get; init; } = "";
    public string Color { get; init; } = "";

    // Rich content
    public string Description { get; init; } = "";
    public IReadOnlyList<string> Features { get; init; } = [];

    // Seller info
    public string DealerName { get; init; } = "";
    public string DealerPhone { get; init; } = "";
    public string DealerLocation { get; init; } = "";
}