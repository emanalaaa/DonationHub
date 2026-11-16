namespace DonationHub.Domain.UnitTests;

using DonationHub.Domain.Entities;
using DonationHub.Domain.Enums;
using DonationHub.Domain.Exceptions;
using DonationHub.Domain.ValueObjects;
using Xunit;

public class CampaignTests
{
    [Fact]
    public void Create_WithValidData_SetsStatusToActive()
    {
        // Arrange
        var id = Guid.NewGuid();
        var titleAr = "حملة إغاثة المحتاجين";
        var titleEn = "Relief Campaign for Families";
        var descAr = "وصف تفصيلي للحملة باللغة العربية";
        var descEn = "Detailed description of the relief campaign";
        var goalAmount = new Money(10000m, "USD");
        var nowUtc = DateTime.UtcNow;
        var endDate = nowUtc.AddDays(30);
        var categoryId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();

        // Act
        var campaign = Campaign.Create(
            id,
            titleAr,
            titleEn,
            descAr,
            descEn,
            goalAmount,
            endDate,
            categoryId,
            createdByUserId,
            nowUtc);

        // Assert
        Assert.NotNull(campaign);
        Assert.Equal(id, campaign.Id);
        Assert.Equal(CampaignStatus.Active, campaign.Status);
        Assert.Equal(titleAr, campaign.TitleAr);
        Assert.Equal(titleEn, campaign.TitleEn);
        Assert.Equal(goalAmount.Amount, campaign.GoalAmount.Amount);
        Assert.Equal(goalAmount.Currency, campaign.GoalAmount.Currency);
        Assert.Equal(0m, campaign.RaisedAmount.Amount);
        Assert.Equal(goalAmount.Currency, campaign.RaisedAmount.Currency);
        Assert.False(campaign.IsDeleted);
        Assert.Null(campaign.DeletedAtUtc);
    }

    [Fact]
    public void Create_WithNegativeOrZeroGoal_ThrowsDomainException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var titleAr = "عنوان عربي";
        var titleEn = "English Title";
        var descAr = "وصف";
        var descEn = "Description";
        var nowUtc = DateTime.UtcNow;
        var endDate = nowUtc.AddDays(15);
        var categoryId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();

        // Act & Assert 1: Zero Goal Amount
        var zeroGoal = Money.Zero("USD");
        var exZero = Assert.Throws<DomainException>(() =>
            Campaign.Create(
                id,
                titleAr,
                titleEn,
                descAr,
                descEn,
                zeroGoal,
                endDate,
                categoryId,
                createdByUserId,
                nowUtc));

        Assert.Contains("greater than zero", exZero.Message, StringComparison.OrdinalIgnoreCase);

        // Act & Assert 2: Negative Goal Amount (Throws in Money VO constructor)
        var exNegative = Assert.Throws<DomainException>(() => new Money(-1000m, "USD"));
        Assert.Contains("greater than or equal to zero", exNegative.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UpdateGoalAmount_WithAmountLessThanRaised_ThrowsDomainException()
    {
        // Arrange
        var nowUtc = DateTime.UtcNow;
        var campaign = Campaign.Create(
            Guid.NewGuid(),
            "عنوان عربي",
            "English Title",
            "وصف",
            "Description",
            new Money(5000m, "USD"),
            nowUtc.AddDays(30),
            Guid.NewGuid(),
            Guid.NewGuid(),
            nowUtc);

        // Simulate received donations
        campaign.AddDonation(new Money(3000m, "USD"), nowUtc.AddDays(1));
        Assert.Equal(3000m, campaign.RaisedAmount.Amount);

        // Act & Assert: Attempt to reduce goal below raised amount (e.g. 2500 < 3000)
        var invalidNewGoal = new Money(2500m, "USD");
        var ex = Assert.Throws<DomainException>(() => campaign.UpdateGoalAmount(invalidNewGoal));

        Assert.Contains("cannot be less than the current raised amount", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MarkAsCompleted_WhenCampaignIsAlreadyExpired_ThrowsDomainException()
    {
        // Arrange
        var nowUtc = DateTime.UtcNow;
        var campaign = Campaign.Create(
            Guid.NewGuid(),
            "عنوان عربي",
            "English Title",
            "وصف",
            "Description",
            new Money(5000m, "USD"),
            nowUtc.AddDays(30),
            Guid.NewGuid(),
            Guid.NewGuid(),
            nowUtc);

        // Transition to Expired (a final state)
        campaign.MarkAsExpired(nowUtc.AddDays(31));
        Assert.Equal(CampaignStatus.Expired, campaign.Status);

        // Act & Assert: Attempt to transition from Expired to Completed
        var ex = Assert.Throws<DomainException>(() => campaign.MarkAsCompleted(nowUtc.AddDays(32)));
        Assert.Contains("Cannot perform state transition", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}

