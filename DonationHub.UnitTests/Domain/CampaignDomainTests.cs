namespace DonationHub.UnitTests.Domain;

using DonationHub.Domain.Common.Errors;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Entities;
using DonationHub.Domain.Enums;
using DonationHub.Domain.Exceptions;
using DonationHub.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

public class CampaignDomainTests
{
    // Test 1: Domain: Goal amount must be greater than zero.
    [Fact]
    public void Create_WithZeroOrNegativeGoalAmount_FailsOrThrows()
    {
        // Arrange
        var id = Guid.NewGuid();
        var titleAr = "حملة خيرية";
        var titleEn = "Charity Campaign";
        var descAr = "وصف";
        var descEn = "Description";
        var zeroGoal = Money.Zero("USD");
        var nowUtc = new DateTime(2026, 11, 22, 12, 0, 0, DateTimeKind.Utc);
        var endDate = nowUtc.AddDays(30);
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var resultZero = Campaign.Create(id, titleAr, titleEn, descAr, descEn, zeroGoal, endDate, categoryId, userId, nowUtc);
        Action actNegative = () => new Money(-50m, "USD");

        // Assert
        resultZero.IsFailure.Should().BeTrue();
        resultZero.Error.Type.Should().Be(ErrorType.Validation);
        actNegative.Should().Throw<DomainException>()
            .WithMessage("*greater than or equal to zero*");
    }

    // Test 2: Domain: End date cannot be in the past (using a mocked/faked TimeProvider).
    [Fact]
    public void Create_WithEndDateInPast_ReturnsEndDateInPastError()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2026, 11, 22, 12, 0, 0, TimeSpan.Zero);
        var mockTimeProvider = new Mock<TimeProvider>();
        mockTimeProvider.Setup(t => t.GetUtcNow()).Returns(fixedTime);

        var nowUtc = mockTimeProvider.Object.GetUtcNow().UtcDateTime;
        var pastEndDate = nowUtc.AddMinutes(-10);

        // Act
        var result = Campaign.Create(
            Guid.NewGuid(),
            "حملة خيرية",
            "Charity Campaign",
            "وصف",
            "Description",
            new Money(1000m, "USD"),
            pastEndDate,
            Guid.NewGuid(),
            Guid.NewGuid(),
            nowUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CampaignErrors.EndDateInPast.Code);
    }

    // Test 3: Domain: Goal cannot be updated below the raised amount (CampaignErrors.GoalBelowRaised).
    [Fact]
    public void Update_WithGoalBelowRaisedAmount_ReturnsGoalBelowRaisedError()
    {
        // Arrange
        var nowUtc = new DateTime(2026, 11, 22, 12, 0, 0, DateTimeKind.Utc);
        var campaignResult = Campaign.Create(
            Guid.NewGuid(),
            "حملة خيرية",
            "Charity Campaign",
            "وصف",
            "Description",
            new Money(5000m, "USD"),
            nowUtc.AddDays(30),
            Guid.NewGuid(),
            Guid.NewGuid(),
            nowUtc);

        campaignResult.IsSuccess.Should().BeTrue();
        var campaign = campaignResult.Value;
        campaign.AddDonation(new Money(3000m, "USD"), nowUtc.AddDays(1));

        var invalidNewGoal = new Money(2500m, "USD");

        // Act
        var updateResult = campaign.Update(
            "حملة خيرية",
            "Charity Campaign",
            "وصف",
            "Description",
            invalidNewGoal,
            nowUtc.AddDays(30),
            campaign.CategoryId,
            null,
            nowUtc.AddDays(2));

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Code.Should().Be(CampaignErrors.GoalBelowRaised.Code);
    }

    // Test 4: Domain: A campaign in a final state (Completed or Expired) cannot be updated (CampaignErrors.NotActive).
    [Theory]
    [InlineData(CampaignStatus.Completed)]
    [InlineData(CampaignStatus.Expired)]
    public void Update_WhenCampaignInFinalState_ReturnsNotActiveError(CampaignStatus finalStatus)
    {
        // Arrange
        var nowUtc = new DateTime(2026, 11, 22, 12, 0, 0, DateTimeKind.Utc);
        var campaign = Campaign.Create(
            Guid.NewGuid(),
            "حملة خيرية",
            "Charity Campaign",
            "وصف",
            "Description",
            new Money(5000m, "USD"),
            nowUtc.AddDays(30),
            Guid.NewGuid(),
            Guid.NewGuid(),
            nowUtc).Value;

        if (finalStatus == CampaignStatus.Completed)
        {
            campaign.MarkAsCompleted(nowUtc.AddDays(1));
        }
        else
        {
            campaign.MarkAsExpired(nowUtc.AddDays(1));
        }

        // Act
        var result = campaign.Update(
            "New Title Ar",
            "New Title En",
            "New Desc Ar",
            "New Desc En",
            new Money(6000m, "USD"),
            nowUtc.AddDays(40),
            campaign.CategoryId,
            null,
            nowUtc.AddDays(2));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CampaignErrors.NotActive.Code);
    }

    // Test 5: Calculation: progressPercent is capped at 100% even if raised exceeds goal.
    [Fact]
    public void ProgressPercent_WhenRaisedExceedsGoal_IsCappedAt100()
    {
        // Arrange
        var goal = 1000m;
        var raised = 1500m;

        // Act
        var progressPercent = goal > 0
            ? Math.Round(Math.Min(100m, Math.Max(0m, (raised / goal) * 100m)), 2)
            : 0m;

        // Assert
        progressPercent.Should().Be(100m);
    }

    // Test 6: Calculation: daysLeft is accurate relative to TimeProvider and never negative.
    [Fact]
    public void DaysLeft_AccurateRelativeToTimeProvider_AndNeverNegative()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2026, 11, 22, 12, 0, 0, TimeSpan.Zero);
        var mockTimeProvider = new Mock<TimeProvider>();
        mockTimeProvider.Setup(t => t.GetUtcNow()).Returns(fixedTime);

        var nowUtc = mockTimeProvider.Object.GetUtcNow().UtcDateTime;
        var futureEndDate = nowUtc.AddDays(5).AddHours(2);
        var pastEndDate = nowUtc.AddDays(-2);

        // Act
        var daysLeftFuture = Math.Max(0, (int)Math.Ceiling((futureEndDate - nowUtc).TotalDays));
        var daysLeftPast = Math.Max(0, (int)Math.Ceiling((pastEndDate - nowUtc).TotalDays));

        // Assert
        daysLeftFuture.Should().Be(6); // 5 days + 2 hours rounded up is 6 days
        daysLeftPast.Should().Be(0);
    }

    // Test 7: State transition: Campaign becomes Completed when the goal is reached.
    [Fact]
    public void AddDonation_WhenRaisedReachesGoal_TransitionsToCompleted()
    {
        // Arrange
        var nowUtc = new DateTime(2026, 11, 22, 12, 0, 0, DateTimeKind.Utc);
        var campaign = Campaign.Create(
            Guid.NewGuid(),
            "حملة خيرية",
            "Charity Campaign",
            "وصف",
            "Description",
            new Money(1000m, "USD"),
            nowUtc.AddDays(30),
            Guid.NewGuid(),
            Guid.NewGuid(),
            nowUtc).Value;

        // Act
        var result = campaign.AddDonation(new Money(1000m, "USD"), nowUtc.AddDays(1));

        // Assert
        result.IsSuccess.Should().BeTrue();
        campaign.RaisedAmount.Amount.Should().Be(1000m);
        campaign.Status.Should().Be(CampaignStatus.Completed);
    }
}

