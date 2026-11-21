namespace DonationHub.Application.Features.Auth.RefreshToken;

using DonationHub.Domain.Common.Results;
using MediatR;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponse>>;

