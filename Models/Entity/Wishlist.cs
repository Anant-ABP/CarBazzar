using System;
using System.Collections.Generic;

namespace CarBazzar.Models.Entity;

public partial class Wishlist
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public int CarId { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual Car Car { get; set; } = null!;

    public virtual ApplicationUser? User { get; set; }
}
