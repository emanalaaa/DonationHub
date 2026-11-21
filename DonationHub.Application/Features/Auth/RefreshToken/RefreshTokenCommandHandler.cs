namespace DonationHub.Application.Features.Auth.RefreshToken;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Common.Errors;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly TimeProvider _timeProvider;

    public RefreshTokenCommandHandler(
        IGenericRepository<RefreshToken> refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IAppDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        TimeProvider timeProvider)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider;
    }

    public async Task<Result<AuthResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var incomingHash = _jwtTokenGenerator.HashRefreshToken(request.RefreshToken);
        var existingToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == incomingHash, cancellationToken);

        if (existingToken is null)
        {
            return AuthErrors.RefreshTokenInvalid;
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        // IDN-13: Compromise Detection
        if (existingToken.RevokedAt != null || existingToken.ReplacedByTokenId != null)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(r => r.UserId == existingToken.UserId && r.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.Revoke(now);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
            return AuthErrors.TokenCompromised;
        }

        // Check expiration
        if (existingToken.IsExpired(now))
        {
            return AuthErrors.RefreshTokenInvalid;
        }

        var user = await _context.Users.FindAsync([existingToken.UserId], cancellationToken);
        if (user is null)
        {
            return AuthErrors.RefreshTokenInvalid;
        }

        // IDN-10: Prevent deactivated users from refreshing
        if (!user.IsActive)
        {
            return AuthErrors.AccountDeactivated;
        }

        // Token Rotation: Generate new token & revoke old
        var rawNewRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var newHash = _jwtTokenGenerator.HashRefreshToken(rawNewRefreshToken);

        var newRefreshToken = new RefreshToken(
            Guid.NewGuid(),
            user.Id,
            newHash,
            now,
            now.AddDays(_jwtTokenGenerator.RefreshTokenExpiryDays));

        existingToken.Revoke(now, newRefreshToken.Id);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        var (accessToken, expiresIn) = _jwtTokenGenerator.GenerateAccessToken(user, [user.Role]);

        return new AuthResponse(accessToken, rawNewRefreshToken, expiresIn);
    }
}
