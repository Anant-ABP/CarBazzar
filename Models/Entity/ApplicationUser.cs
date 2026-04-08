using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace CarBazzar.Models.Entity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Location { get; set; }
    public string? Address { get; set; }
    public string? ProfileImage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsHidden { get; set; } = false;

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
    public virtual ICollection<Message> MessageReceivers { get; set; } = new List<Message>();
    public virtual ICollection<Message> MessageSenders { get; set; } = new List<Message>();
    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
