namespace DonationHub.Application.Features.Auth.Logout;

using DonationHub.Domain.Common.Results;
using MediatR;

public record LogoutCommand(string RefreshToken, Guid UserId) : IRequest<Result>;

