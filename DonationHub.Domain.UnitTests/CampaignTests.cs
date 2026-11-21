namespace DonationHub.Domain.UnitTests;

using DonationHub.Domain.Common.Errors;
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
        var result = Campaign.Create(
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
        Assert.True(result.IsSuccess);
        var campaign = result.Value;
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
    public void Create_WithEndDateInPast_ReturnsEndDateInPastError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var titleAr = "حملة إغاثة";
        var titleEn = "Relief Campaign";
        var descAr = "وصف";
        var descEn = "Description";
        var goalAmount = new Money(5000m, "USD");
        var nowUtc = DateTime.UtcNow;
        var pastEndDate = nowUtc.AddDays(-1);
        var categoryId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();

        // Act
        var result = Campaign.Create(
            id,
            titleAr,
            titleEn,
            descAr,
            descEn,
            goalAmount,
            pastEndDate,
            categoryId,
            createdByUserId,
            nowUtc);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CampaignErrors.EndDateInPast.Code, result.Error.Code);
    }

    [Fact]
    public void Create_WithNegativeOrZeroGoal_ReturnsValidationErrorOrThrows()
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

        // Act & Assert 1: Zero Goal Amount returns validation failure
        var zeroGoal = Money.Zero("USD");
        var resultZero = Campaign.Create(
            id,
            titleAr,
            titleEn,
            descAr,
            descEn,
            zeroGoal,
            endDate,
            categoryId,
            createdByUserId,
            nowUtc);

        Assert.True(resultZero.IsFailure);

        // Act & Assert 2: Negative Goal Amount (Throws in Money VO constructor)
        var exNegative = Assert.Throws<DomainException>(() => new Money(-1000m, "USD"));
        Assert.Contains("greater than or equal to zero", exNegative.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Update_WithAmountLessThanRaised_ReturnsGoalBelowRaisedError()
    {
        // Arrange
        var nowUtc = DateTime.UtcNow;
        var campaignResult = Campaign.Create(
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

        Assert.True(campaignResult.IsSuccess);
        var campaign = campaignResult.Value;

        // Simulate received donations
        var addResult = campaign.AddDonation(new Money(3000m, "USD"), nowUtc.AddDays(1));
        Assert.True(addResult.IsSuccess);
        Assert.Equal(3000m, campaign.RaisedAmount.Amount);

        // Act: Attempt to reduce goal below raised amount (e.g. 2500 < 3000)
        var invalidNewGoal = new Money(2500m, "USD");
        var updateResult = campaign.Update(
            "عنوان عربي",
            "English Title",
            "وصف",
            "Description",
            invalidNewGoal,
            nowUtc.AddDays(30),
            campaign.CategoryId,
            null,
            nowUtc.AddDays(2));

        // Assert
        Assert.True(updateResult.IsFailure);
        Assert.Equal(CampaignErrors.GoalBelowRaised.Code, updateResult.Error.Code);
    }

    [Fact]
    public void Update_WhenCampaignIsCompleted_ReturnsNotActiveError()
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
            nowUtc).Value;

        // Transition to Completed
        var completeResult = campaign.MarkAsCompleted(nowUtc.AddDays(1));
        Assert.True(completeResult.IsSuccess);
        Assert.Equal(CampaignStatus.Completed, campaign.Status);

        // Act: Attempt to update completed campaign
        var updateResult = campaign.Update(
            "New Title",
            "New Title",
            "Desc",
            "Desc",
            new Money(6000m, "USD"),
            nowUtc.AddDays(30),
            campaign.CategoryId,
            null,
            nowUtc.AddDays(2));

        // Assert
        Assert.True(updateResult.IsFailure);
        Assert.Equal(CampaignErrors.NotActive.Code, updateResult.Error.Code);
    }

    [Fact]
    public void AddDonation_WhenGoalReached_AutomaticallyMarksAsCompleted()
    {
        // Arrange
        var nowUtc = DateTime.UtcNow;
        var campaign = Campaign.Create(
            Guid.NewGuid(),
            "عنوان عربي",
            "English Title",
            "وصف",
            "Description",
            new Money(1000m, "USD"),
            nowUtc.AddDays(30),
            Guid.NewGuid(),
            Guid.NewGuid(),
            nowUtc).Value;

        // Act
        var donateResult = campaign.AddDonation(new Money(1000m, "USD"), nowUtc.AddDays(5));

        // Assert
        Assert.True(donateResult.IsSuccess);
        Assert.Equal(1000m, campaign.RaisedAmount.Amount);
        Assert.Equal(CampaignStatus.Completed, campaign.Status);
    }
}
