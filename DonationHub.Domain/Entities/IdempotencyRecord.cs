namespace DonationHub.Domain.Entities;

using DonationHub.Domain.Exceptions;

public class IdempotencyRecord
{
    public string Key { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public string RequestHash { get; private set; } = string.Empty;
    public int ResponseStatusCode { get; private set; }
    public string ResponseBody { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private IdempotencyRecord() { }

    public IdempotencyRecord(
        string key,
        Guid userId,
        string requestHash,
        int responseStatusCode,
        string responseBody,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new DomainException("Idempotency key cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(requestHash))
        {
            throw new DomainException("Request hash cannot be empty.");
        }

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new DomainException("Expiration date must be after creation date.");
        }

        Key = key.Trim();
        UserId = userId;
        RequestHash = requestHash.Trim();
        ResponseStatusCode = responseStatusCode;
        ResponseBody = responseBody ?? string.Empty;
        CreatedAt = createdAtUtc;
        ExpiresAt = expiresAtUtc;
    }

    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAt;
}

