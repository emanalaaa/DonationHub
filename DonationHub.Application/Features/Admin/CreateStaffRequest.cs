namespace DonationHub.Application.Features.Admin;

public record CreateStaffRequest(
    string Email,
    string Password,
    string UserName,
    string FullName,
    string Role,
    string PreferredLanguage = "en");

