namespace DonationHub.Application.Features.Auth.Login;

using DonationHub.Domain.Common.Results;
using MediatR;

public record LoginCommand(
    string Email,
    string Password) : IRequest<Result<AuthResponse>>;

