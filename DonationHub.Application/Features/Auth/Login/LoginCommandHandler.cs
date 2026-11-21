namespace DonationHub.Application.Features.Auth.Login;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Common.Errors;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly TimeProvider _timeProvider;

    public LoginCommandHandler(
        IGenericRepository<RefreshToken> refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IAppDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        TimeProvider timeProvider)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider;
    }

    public async Task<Result<AuthResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // IDN-12: Generic login error for wrong email/password
            return AuthErrors.InvalidCredentials;
        }

        // IDN-10: Prevent deactivated users from logging in
        if (!user.IsActive)
        {
            return AuthErrors.AccountDeactivated;
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var (accessToken, expiresIn) = _jwtTokenGenerator.GenerateAccessToken(user, [user.Role]);
        var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var tokenHash = _jwtTokenGenerator.HashRefreshToken(rawRefreshToken);

        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            user.Id,
            tokenHash,
            now,
            now.AddDays(_jwtTokenGenerator.RefreshTokenExpiryDays));

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return new AuthResponse(accessToken, rawRefreshToken, expiresIn);
    }
}
