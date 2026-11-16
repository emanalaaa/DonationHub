namespace DonationHub.Domain.Entities;

using DonationHub.Domain.Common.Interfaces;
using DonationHub.Domain.Enums;
using DonationHub.Domain.Exceptions;
using DonationHub.Domain.ValueObjects;

public class Campaign : ISoftDeletable
{
    public Guid Id { get; private set; }
    public string TitleAr { get; private set; } = string.Empty;
    public string TitleEn { get; private set; } = string.Empty;
    public string DescriptionAr { get; private set; } = string.Empty;
    public string DescriptionEn { get; private set; } = string.Empty;
    public Money GoalAmount { get; private set; } = null!;
    public Money RaisedAmount { get; private set; } = null!;
    public DateTime EndDate { get; private set; }
    public Guid CategoryId { get; private set; }
    public string? BannerFileName { get; private set; }
    public CampaignStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public byte[] RowVersion { get; private set; } = [];

    private Campaign() { }

    public static Campaign Create(
        Guid id,
        string titleAr,
        string titleEn,
        string descriptionAr,
        string descriptionEn,
        Money goalAmount,
        DateTime endDate,
        Guid categoryId,
        Guid createdByUserId,
        DateTime nowUtc,
        string? bannerFileName = null)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Campaign ID cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(titleAr))
        {
            throw new DomainException("Arabic title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(titleEn))
        {
            throw new DomainException("English title cannot be empty.");
        }

        if (goalAmount is null || goalAmount.Amount <= 0)
        {
            throw new DomainException("Goal amount must be greater than zero.");
        }

        if (endDate <= nowUtc)
        {
            throw new DomainException("End date must be in the future relative to the current time.");
        }

        if (categoryId == Guid.Empty)
        {
            throw new DomainException("CategoryId cannot be empty.");
        }

        if (createdByUserId == Guid.Empty)
        {
            throw new DomainException("CreatedByUserId cannot be empty.");
        }

        return new Campaign
        {
            Id = id,
            TitleAr = titleAr.Trim(),
            TitleEn = titleEn.Trim(),
            DescriptionAr = descriptionAr?.Trim() ?? string.Empty,
            DescriptionEn = descriptionEn?.Trim() ?? string.Empty,
            GoalAmount = goalAmount,
            RaisedAmount = Money.Zero(goalAmount.Currency),
            EndDate = endDate,
            CategoryId = categoryId,
            BannerFileName = bannerFileName?.Trim(),
            Status = CampaignStatus.Active,
            CreatedByUserId = createdByUserId,
            CreatedAt = nowUtc,
            UpdatedAt = null,
            IsDeleted = false,
            DeletedAtUtc = null,
            RowVersion = []
        };
    }

    public void UpdateGoalAmount(Money newGoal)
    {
        if (newGoal is null || newGoal.Amount <= 0)
        {
            throw new DomainException("Goal amount must be greater than zero.");
        }

        if (!string.Equals(newGoal.Currency, GoalAmount.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException($"Currency mismatch: new goal is '{newGoal.Currency}' but campaign currency is '{GoalAmount.Currency}'.");
        }

        if (newGoal.Amount < RaisedAmount.Amount)
        {
            throw new DomainException($"New goal amount ({newGoal.Amount}) cannot be less than the current raised amount ({RaisedAmount.Amount}).");
        }

        GoalAmount = newGoal;
    }

    public void UpdateGoalAmount(Money newGoal, DateTime nowUtc)
    {
        UpdateGoalAmount(newGoal);
        UpdatedAt = nowUtc;
    }

    public void UpdateDetails(
        string titleAr,
        string titleEn,
        string descriptionAr,
        string descriptionEn,
        Guid categoryId,
        string? bannerFileName,
        DateTime nowUtc)
    {
        EnsureNotFinalState();

        if (string.IsNullOrWhiteSpace(titleAr))
        {
            throw new DomainException("Arabic title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(titleEn))
        {
            throw new DomainException("English title cannot be empty.");
        }

        if (categoryId == Guid.Empty)
        {
            throw new DomainException("CategoryId cannot be empty.");
        }

        TitleAr = titleAr.Trim();
        TitleEn = titleEn.Trim();
        DescriptionAr = descriptionAr?.Trim() ?? string.Empty;
        DescriptionEn = descriptionEn?.Trim() ?? string.Empty;
        CategoryId = categoryId;
        BannerFileName = bannerFileName?.Trim();
        UpdatedAt = nowUtc;
    }

    public void AddDonation(Money donationAmount, DateTime nowUtc)
    {
        EnsureNotFinalState();

        if (donationAmount is null || donationAmount.Amount <= 0)
        {
            throw new DomainException("Donation amount must be greater than zero.");
        }

        if (!string.Equals(donationAmount.Currency, GoalAmount.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException($"Donation currency '{donationAmount.Currency}' does not match campaign currency '{GoalAmount.Currency}'.");
        }

        RaisedAmount = RaisedAmount.Add(donationAmount);
        UpdatedAt = nowUtc;
    }

    public void MarkAsCompleted(DateTime nowUtc)
    {
        EnsureNotFinalState();

        Status = CampaignStatus.Completed;
        UpdatedAt = nowUtc;
    }

    public void MarkAsExpired(DateTime nowUtc)
    {
        EnsureNotFinalState();

        Status = CampaignStatus.Expired;
        UpdatedAt = nowUtc;
    }

    public void Delete(DateTime nowUtc)
    {
        IsDeleted = true;
        DeletedAtUtc = nowUtc;
        UpdatedAt = nowUtc;
    }

    private void EnsureNotFinalState()
    {
        if (Status == CampaignStatus.Completed || Status == CampaignStatus.Expired)
        {
            throw new DomainException($"Cannot perform state transition or modify campaign from final state '{Status}'.");
        }
    }
}
