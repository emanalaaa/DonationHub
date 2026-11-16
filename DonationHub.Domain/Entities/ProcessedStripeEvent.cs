namespace DonationHub.Domain.Entities;

using DonationHub.Domain.Exceptions;

public class ProcessedStripeEvent
{
    public string EventId { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public DateTime ProcessedAt { get; private set; }

    private ProcessedStripeEvent() { }

    public ProcessedStripeEvent(string eventId, string type, DateTime processedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            throw new DomainException("Stripe Event ID cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new DomainException("Event type cannot be empty.");
        }

        EventId = eventId.Trim();
        Type = type.Trim();
        ProcessedAt = processedAtUtc;
    }
}

