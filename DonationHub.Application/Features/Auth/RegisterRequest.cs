namespace DonationHub.Application.Features.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string UserName,
    string FullName,
    string PreferredLanguage = "en");

