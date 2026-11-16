namespace DonationHub.Domain.Entities;

using DonationHub.Domain.Enums;
using DonationHub.Domain.Exceptions;
using DonationHub.Domain.ValueObjects;

public class Donation
{
    public Guid Id { get; private set; }
    public Guid CampaignId { get; private set; }
    public Guid DonorUserId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public DonationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string? ReceiptNumber { get; private set; }
    public string? ReceiptFileName { get; private set; }

    private Donation() { }

    public Donation(
        Guid id,
        Guid campaignId,
        Guid donorUserId,
        Money amount,
        DateTime createdAtUtc)
    {
        if (campaignId == Guid.Empty)
        {
            throw new DomainException("CampaignId cannot be empty.");
        }

        if (donorUserId == Guid.Empty)
        {
            throw new DomainException("DonorUserId cannot be empty.");
        }

        if (amount is null || amount.Amount <= 0)
        {
            throw new DomainException("Donation amount must be greater than zero.");
        }

        Id = id;
        CampaignId = campaignId;
        DonorUserId = donorUserId;
        Amount = amount;
        Status = DonationStatus.Pending;
        CreatedAt = createdAtUtc;
    }

    public void MarkAsSucceeded(DateTime completedAtUtc, string receiptNumber, string? receiptFileName = null)
    {
        EnsureNotFinalState();

        if (string.IsNullOrWhiteSpace(receiptNumber))
        {
            throw new DomainException("Receipt number is required for succeeded donation.");
        }

        Status = DonationStatus.Succeeded;
        CompletedAt = completedAtUtc;
        ReceiptNumber = receiptNumber.Trim();
        ReceiptFileName = receiptFileName?.Trim();
        FailureReason = null;
    }

    public void MarkAsFailed(string failureReason, DateTime completedAtUtc)
    {
        EnsureNotFinalState();

        if (string.IsNullOrWhiteSpace(failureReason))
        {
            throw new DomainException("Failure reason must be provided.");
        }

        Status = DonationStatus.Failed;
        CompletedAt = completedAtUtc;
        FailureReason = failureReason.Trim();
    }

    private void EnsureNotFinalState()
    {
        if (Status == DonationStatus.Succeeded || Status == DonationStatus.Failed)
        {
            throw new DomainException($"Cannot change donation status from final state '{Status}'.");
        }
    }
}

