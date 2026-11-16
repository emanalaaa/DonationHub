namespace DonationHub.Infrastructure.Identity;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Common.Models;
using DonationHub.Application.Features.Admin;
using DonationHub.Application.Features.Auth;
using DonationHub.Domain.Entities;
using DonationHub.Infrastructure.Options;
using DonationHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class IdentityService : IIdentityService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtOptions _jwtOptions;

    public IdentityService(
        AppDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        IOptions<JwtOptions> jwtOptions)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // IDN-02: Enforce unique email (case-insensitive)
        var exists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            return Result<AuthResponse>.Failure("Auth.EmailAlreadyExists", "The email address is already registered.", 409);
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = new ApplicationUser(
            Guid.NewGuid(),
            normalizedEmail,
            request.UserName,
            request.FullName,
            passwordHash,
            DateTime.UtcNow,
            role: "Donor",
            preferredLanguage: request.PreferredLanguage);

        await _context.Users.AddAsync(user, cancellationToken);

        var (accessToken, expiresIn) = _jwtTokenGenerator.GenerateAccessToken(user, [user.Role]);
        var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var tokenHash = _jwtTokenGenerator.HashRefreshToken(rawRefreshToken);

        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            user.Id,
            tokenHash,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays));

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(accessToken, rawRefreshToken, expiresIn));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // IDN-12: Generic login error for wrong email/password
            return Result<AuthResponse>.Failure("Auth.InvalidCredentials", "Invalid email or password.", 401);
        }

        // IDN-10: Prevent deactivated users from logging in
        if (!user.IsActive)
        {
            return Result<AuthResponse>.Failure("Auth.AccountDeactivated", "This account has been deactivated.", 403);
        }

        var (accessToken, expiresIn) = _jwtTokenGenerator.GenerateAccessToken(user, [user.Role]);
        var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var tokenHash = _jwtTokenGenerator.HashRefreshToken(rawRefreshToken);

        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            user.Id,
            tokenHash,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays));

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(accessToken, rawRefreshToken, expiresIn));
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(TokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<AuthResponse>.Failure("Auth.RefreshTokenInvalid", "Refresh token is required.", 401);
        }

        var incomingHash = _jwtTokenGenerator.HashRefreshToken(request.RefreshToken);
        var existingToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == incomingHash, cancellationToken);

        if (existingToken is null)
        {
            return Result<AuthResponse>.Failure("Auth.RefreshTokenInvalid", "Invalid refresh token.", 401);
        }

        // IDN-13: Compromise Detection
        // If a refresh token exists in the database but is already marked as used or revoked,
        // this strongly indicates a replay attack using a stolen token.
        if (existingToken.RevokedAt != null || existingToken.ReplacedByTokenId != null)
        {
            // Immediately query and revoke ALL active refresh tokens for that specific UserId
            var activeTokens = await _context.RefreshTokens
                .Where(r => r.UserId == existingToken.UserId && r.RevokedAt == null)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            foreach (var token in activeTokens)
            {
                token.Revoke(now);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<AuthResponse>.Failure(
                "Auth.TokenCompromised",
                "Refresh token has already been used or revoked. All active sessions have been terminated due to potential compromise.",
                401);
        }

        // Check expiration
        if (existingToken.IsExpired(DateTime.UtcNow))
        {
            return Result<AuthResponse>.Failure("Auth.RefreshTokenExpired", "Refresh token has expired.", 401);
        }

        var user = await _context.Users.FindAsync([existingToken.UserId], cancellationToken);
        if (user is null)
        {
            return Result<AuthResponse>.Failure("Auth.RefreshTokenInvalid", "Associated user was not found.", 401);
        }

        // IDN-10: Prevent deactivated users from refreshing
        if (!user.IsActive)
        {
            return Result<AuthResponse>.Failure("Auth.AccountDeactivated", "This account has been deactivated.", 403);
        }

        // Token Rotation: Generate new token & revoke old
        var rawNewRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var newHash = _jwtTokenGenerator.HashRefreshToken(rawNewRefreshToken);

        var newRefreshToken = new RefreshToken(
            Guid.NewGuid(),
            user.Id,
            newHash,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays));

        existingToken.Revoke(DateTime.UtcNow, newRefreshToken.Id);

        await _context.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var (accessToken, expiresIn) = _jwtTokenGenerator.GenerateAccessToken(user, [user.Role]);

        return Result<AuthResponse>.Success(new AuthResponse(accessToken, rawNewRefreshToken, expiresIn));
    }

    public async Task<Result> LogoutAsync(string refreshToken, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Success();
        }

        var tokenHash = _jwtTokenGenerator.HashRefreshToken(refreshToken);
        var tokenRecord = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && r.UserId == userId, cancellationToken);

        if (tokenRecord is not null && !tokenRecord.IsRevoked)
        {
            tokenRecord.Revoke(DateTime.UtcNow);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result<CurrentUserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user is null)
        {
            return Result<CurrentUserResponse>.Failure("Auth.UserNotFound", "User not found.", 404);
        }

        return Result<CurrentUserResponse>.Success(new CurrentUserResponse(
            user.Id,
            user.Email,
            user.UserName,
            user.FullName,
            user.Role,
            user.PreferredLanguage,
            user.IsActive));
    }

    public async Task<Result<Guid>> CreateStaffAsync(CreateStaffRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Enforce valid staff roles
        var role = request.Role?.Trim();
        if (role != "Admin" && role != "CampaignManager")
        {
            return Result<Guid>.Failure("Admin.InvalidRole", "Staff role must be either 'Admin' or 'CampaignManager'.", 400);
        }

        // IDN-02: Enforce unique email
        var exists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            return Result<Guid>.Failure("Auth.EmailAlreadyExists", "The email address is already registered.", 409);
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var staffUser = new ApplicationUser(
            Guid.NewGuid(),
            normalizedEmail,
            request.UserName,
            request.FullName,
            passwordHash,
            DateTime.UtcNow,
            role: role,
            preferredLanguage: request.PreferredLanguage);

        await _context.Users.AddAsync(staffUser, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(staffUser.Id);
    }
}

