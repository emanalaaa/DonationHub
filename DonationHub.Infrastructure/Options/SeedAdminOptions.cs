namespace DonationHub.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class SeedAdminOptions
{
    public const string SectionName = "SeedAdmin";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;
}

