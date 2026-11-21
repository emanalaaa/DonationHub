namespace DonationHub.Application.Common.Interfaces;

using DonationHub.Domain.Entities;

public interface IJwtTokenGenerator
{
    int RefreshTokenExpiryDays { get; }
    (string Token, int ExpiresInSeconds) GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
