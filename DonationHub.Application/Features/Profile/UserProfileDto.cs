namespace DonationHub.Application.Features.Profile;

public record UserProfileDto(
    Guid Id,
    string Email,
    string UserName,
    string FullName,
    string Role,
    string PreferredLanguage,
    bool IsActive);

