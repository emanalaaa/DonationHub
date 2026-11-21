namespace DonationHub.Infrastructure.Identity;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Features.Admin;
using DonationHub.Application.Features.Admin.CreateStaff;
using DonationHub.Application.Features.Auth;
using DonationHub.Application.Features.Auth.GetCurrentUser;
using DonationHub.Application.Features.Auth.Login;
using DonationHub.Application.Features.Auth.Logout;
using DonationHub.Application.Features.Auth.RefreshToken;
using DonationHub.Application.Features.Auth.Register;
using DonationHub.Domain.Common.Results;
using MediatR;

public class IdentityService : IIdentityService
{
    private readonly ISender _mediator;

    public IdentityService(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default) =>
        _mediator.Send(new RegisterCommand(request.Email, request.UserName, request.FullName, request.Password, request.PreferredLanguage), cancellationToken);

    public Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        _mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken);

    public Task<Result<AuthResponse>> RefreshTokenAsync(TokenRequest request, CancellationToken cancellationToken = default) =>
        _mediator.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken);

    public Task<Result> LogoutAsync(string refreshToken, Guid userId, CancellationToken cancellationToken = default) =>
        _mediator.Send(new LogoutCommand(refreshToken, userId), cancellationToken);

    public Task<Result<CurrentUserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _mediator.Send(new GetCurrentUserQuery(userId), cancellationToken);

    public Task<Result<Guid>> CreateStaffAsync(CreateStaffRequest request, CancellationToken cancellationToken = default) =>
        _mediator.Send(new CreateStaffCommand(request.Email, request.UserName, request.FullName, request.Password, request.Role, request.PreferredLanguage), cancellationToken);
}
