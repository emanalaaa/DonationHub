namespace DonationHub.Application.Features.Auth.Register;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Common.Errors;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IGenericRepository<ApplicationUser> _userRepository;
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly TimeProvider _timeProvider;

    public RegisterCommandHandler(
        IGenericRepository<ApplicationUser> userRepository,
        IGenericRepository<RefreshToken> refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IAppDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        TimeProvider timeProvider)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider;
    }

    public async Task<Result<AuthResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            return AuthErrors.EmailAlreadyExists;
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = new ApplicationUser(
            Guid.NewGuid(),
            normalizedEmail,
            request.UserName.Trim(),
            request.FullName.Trim(),
            passwordHash,
            now,
            role: "Donor",
            preferredLanguage: string.IsNullOrWhiteSpace(request.PreferredLanguage) ? "en" : request.PreferredLanguage);

        await _userRepository.AddAsync(user, cancellationToken);

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
