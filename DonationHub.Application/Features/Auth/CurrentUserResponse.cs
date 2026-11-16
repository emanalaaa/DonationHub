namespace DonationHub.Application.Features.Auth;

public record CurrentUserResponse(
    Guid Id,
    string Email,
    string UserName,
    string FullName,
    string Role,
    string PreferredLanguage,
    bool IsActive);

