namespace DonationHub.Application.Features.Auth.GetCurrentUser;

using DonationHub.Application.Features.Auth;
using DonationHub.Domain.Common.Results;
using MediatR;

public record GetCurrentUserQuery(Guid UserId) : IRequest<Result<CurrentUserResponse>>;

