namespace DonationHub.Application.Common.Interfaces;

using DonationHub.Domain.Entities;

public interface IJwtTokenGenerator
{
    (string Token, int ExpiresInSeconds) GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}

