namespace CarBazzar.Models.Profile;

public sealed class ProfileViewModel
{
    public string FullName { get; init; } = "Anant";
    public string Email { get; init; } = "anant@example.com";
    public string Location { get; init; } = "Surat, Gujarat";
    public string Phone { get; init; } = "+91 90000 00000";
    public int WishlistCount { get; init; } = 0;
    public int TotalListings { get; init; } = 0;
    public System.Collections.Generic.List<CarBazzar.Models.Cars.CarCard> RecentlyViewedCars { get; init; } = new();
}

