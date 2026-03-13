using System.ComponentModel.DataAnnotations;

namespace CarBazzar.Models.Profile;

public sealed class EditProfileViewModel
{
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = "";

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Display(Name = "Location")]
    public string? Location { get; set; }
}

