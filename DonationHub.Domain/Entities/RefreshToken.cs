namespace DonationHub.Domain.Entities;

using DonationHub.Domain.Exceptions;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    private RefreshToken() { }

    public RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new DomainException("Token hash cannot be empty.");
        }

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new DomainException("Expiry date must be after creation date.");
        }

        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAtUtc;
        ExpiresAt = expiresAtUtc;
    }

    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive(DateTime nowUtc) => !IsRevoked && !IsExpired(nowUtc);

    public void Revoke(DateTime revokedAtUtc, Guid? replacedByTokenId = null)
    {
        if (IsRevoked)
        {
            throw new DomainException("Refresh token is already revoked.");
        }

        RevokedAt = revokedAtUtc;
        ReplacedByTokenId = replacedByTokenId;
    }
}

