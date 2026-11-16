namespace DonationHub.Application.Features.Profile.ChangePassword;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

