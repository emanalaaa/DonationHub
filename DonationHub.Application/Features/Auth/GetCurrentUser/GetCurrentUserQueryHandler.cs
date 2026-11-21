namespace DonationHub.Application.Features.Auth.GetCurrentUser;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Features.Auth;
using DonationHub.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
{
    private readonly IAppDbContext _context;

    public GetCurrentUserQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CurrentUserResponse>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => new CurrentUserResponse(
                u.Id,
                u.Email,
                u.UserName,
                u.FullName,
                u.Role,
                u.PreferredLanguage,
                u.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Error.NotFound("Auth.UserNotFound", "User not found.");
        }

        return user;
    }
}

