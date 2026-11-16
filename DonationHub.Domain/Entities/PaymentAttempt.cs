namespace DonationHub.Domain.Entities;

using DonationHub.Domain.Enums;
using DonationHub.Domain.Exceptions;
using DonationHub.Domain.ValueObjects;

public class PaymentAttempt
{
    public Guid Id { get; private set; }
    public Guid DonationId { get; private set; }
    public string StripeCheckoutSessionId { get; private set; } = string.Empty;
    public string? StripePaymentIntentId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentAttemptStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? FailureCode { get; private set; }

    private PaymentAttempt() { }

    public PaymentAttempt(
        Guid id,
        Guid donationId,
        string stripeCheckoutSessionId,
        Money amount,
        DateTime createdAtUtc)
    {
        if (donationId == Guid.Empty)
        {
            throw new DomainException("DonationId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(stripeCheckoutSessionId))
        {
            throw new DomainException("Stripe Checkout Session ID cannot be empty.");
        }

        if (amount is null || amount.Amount <= 0)
        {
            throw new DomainException("Payment attempt amount must be greater than zero.");
        }

        Id = id;
        DonationId = donationId;
        StripeCheckoutSessionId = stripeCheckoutSessionId.Trim();
        Amount = amount;
        Status = PaymentAttemptStatus.Created;
        CreatedAt = createdAtUtc;
    }

    public void MarkAsSucceeded(string? stripePaymentIntentId, DateTime updatedAtUtc)
    {
        EnsureNotFinalState();

        Status = PaymentAttemptStatus.Succeeded;
        StripePaymentIntentId = stripePaymentIntentId?.Trim();
        UpdatedAt = updatedAtUtc;
        FailureCode = null;
    }

    public void MarkAsFailed(string failureCode, DateTime updatedAtUtc)
    {
        EnsureNotFinalState();

        if (string.IsNullOrWhiteSpace(failureCode))
        {
            throw new DomainException("Failure code cannot be empty.");
        }

        Status = PaymentAttemptStatus.Failed;
        FailureCode = failureCode.Trim();
        UpdatedAt = updatedAtUtc;
    }

    public void MarkAsExpired(DateTime updatedAtUtc)
    {
        EnsureNotFinalState();

        Status = PaymentAttemptStatus.Expired;
        UpdatedAt = updatedAtUtc;
    }

    private void EnsureNotFinalState()
    {
        if (Status == PaymentAttemptStatus.Succeeded ||
            Status == PaymentAttemptStatus.Failed ||
            Status == PaymentAttemptStatus.Expired)
        {
            throw new DomainException($"Cannot change payment attempt status from final state '{Status}'.");
        }
    }
}

