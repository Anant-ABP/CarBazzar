using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarBazzar.Models.Entity;

public class RecentlyViewed
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public int CarId { get; set; }

    public DateTime ViewedAt { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; }

    [ForeignKey("CarId")]
    public virtual Car Car { get; set; }
}
