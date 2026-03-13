namespace CarBazzar.Models.Cars;

public sealed class CarCard
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string Specs { get; init; } = "";
    public string Price { get; init; } = "";
    public string ImageUrl { get; init; } = "";
}

