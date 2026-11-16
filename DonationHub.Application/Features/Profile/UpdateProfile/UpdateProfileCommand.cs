namespace DonationHub.Application.Features.Profile.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    string FullName,
    string PreferredLanguage);

