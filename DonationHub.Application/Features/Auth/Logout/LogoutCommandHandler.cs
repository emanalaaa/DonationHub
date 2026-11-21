namespace DonationHub.Application.Features.Auth.Logout;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly TimeProvider _timeProvider;

    public LogoutCommandHandler(
        IAppDbContext context,
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator,
        TimeProvider timeProvider)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider;
    }

    public async Task<Result> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Success();
        }

        var tokenHash = _jwtTokenGenerator.HashRefreshToken(request.RefreshToken);
        var tokenRecord = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && r.UserId == request.UserId, cancellationToken);

        if (tokenRecord is not null && !tokenRecord.IsRevoked)
        {
            tokenRecord.Revoke(_timeProvider.GetUtcNow().UtcDateTime);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}

