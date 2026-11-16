namespace DonationHub.Application.Common.Interfaces;

using DonationHub.Application.Common.Models;
using DonationHub.Application.Features.Admin;
using DonationHub.Application.Features.Auth;

public interface IIdentityService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(TokenRequest request, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(string refreshToken, Guid userId, CancellationToken cancellationToken = default);
    Task<Result<CurrentUserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateStaffAsync(CreateStaffRequest request, CancellationToken cancellationToken = default);
}

