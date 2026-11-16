namespace DonationHub.Application.Features.Auth;

public record LoginRequest(
    string Email,
    string Password);

