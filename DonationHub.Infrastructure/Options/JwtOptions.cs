namespace DonationHub.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32, ErrorMessage = "SecretKey must be at least 32 characters long.")]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ExpiryMinutes must be greater than zero.")]
    public int ExpiryMinutes { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "RefreshTokenExpiryDays must be greater than zero.")]
    public int RefreshTokenExpiryDays { get; set; }
}

