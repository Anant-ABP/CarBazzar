using System;
using CarBazzar.Models.Entity;

namespace CarBazzar.Models;

/// <summary>
/// Represents a car booking made by a user.
/// </summary>
public class Booking
{
    public int Id { get; set; }

    /// <summary>Foreign key to the booked Car.</summary>
    public int CarId { get; set; }

    /// <summary>Foreign key to the ApplicationUser who made the booking.</summary>
    public string UserId { get; set; } = null!;

    /// <summary>Date and time the booking was created (UTC).</summary>
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    public string? MobileNumber { get; set; }

    public string? Message { get; set; }

    // ── Navigation Properties ──────────────────────────────────────────────
    public virtual Car Car { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
}
