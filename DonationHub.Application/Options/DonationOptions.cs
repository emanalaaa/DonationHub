namespace DonationHub.Application.Options;

using System.ComponentModel.DataAnnotations;

public class DonationOptions
{
    public const string SectionName = "DonationOptions";

    [Range(0.01, double.MaxValue, ErrorMessage = "MinAmount must be greater than zero.")]
    public decimal MinAmount { get; set; } = 1.00m;

    [Range(0.01, double.MaxValue, ErrorMessage = "MaxAmount must be greater than zero.")]
    public decimal MaxAmount { get; set; } = 10000.00m;

    [Range(1, int.MaxValue, ErrorMessage = "PendingTimeoutMinutes must be greater than zero.")]
    public int PendingTimeoutMinutes { get; set; } = 30;

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "DefaultCurrency must be a 3-letter ISO code.")]
    public string DefaultCurrency { get; set; } = "USD";
}

