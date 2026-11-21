namespace DonationHub.Application.Features.Admin.CreateStaff;

using DonationHub.Domain.Common.Results;
using MediatR;

public record CreateStaffCommand(
    string Email,
    string UserName,
    string FullName,
    string Password,
    string Role,
    string PreferredLanguage = "en") : IRequest<Result<Guid>>;

