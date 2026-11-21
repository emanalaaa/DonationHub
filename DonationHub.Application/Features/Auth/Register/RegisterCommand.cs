namespace DonationHub.Application.Features.Auth.Register;

using DonationHub.Domain.Common.Results;
using MediatR;

public record RegisterCommand(
    string Email,
    string UserName,
    string FullName,
    string Password,
    string PreferredLanguage = "en") : IRequest<Result<AuthResponse>>;

