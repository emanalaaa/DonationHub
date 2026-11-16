namespace DonationHub.Application.Features.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn);

