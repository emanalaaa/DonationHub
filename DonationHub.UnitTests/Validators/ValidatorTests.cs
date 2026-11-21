namespace DonationHub.UnitTests.Validators;

using DonationHub.Application.Features.Auth.Register;
using DonationHub.Application.Features.Campaigns.CreateCampaign;
using FluentAssertions;
using Xunit;

public class ValidatorTests
{
    // Test 9: Validator: CreateCampaignCommandValidator rejects missing bilingual titles, GoalAmount <= 0, and empty CategoryId.
    [Fact]
    public void CreateCampaignCommandValidator_WithInvalidInputs_FailsValidation()
    {
        // Arrange
        var validator = new CreateCampaignCommandValidator();
        var command = new CreateCampaignCommand(
            TitleAr: "",
            TitleEn: "",
            DescriptionAr: "وصف",
            DescriptionEn: "Description",
            GoalAmount: 0m,
            Currency: "USD",
            EndDate: DateTime.UtcNow.AddDays(10),
            CategoryId: Guid.Empty,
            CreatedByUserId: Guid.NewGuid());

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.TitleAr));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.TitleEn));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.GoalAmount));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CategoryId));
    }

    // Test 10: Validator: Register command validator rejects a password that breaks the policy (8+ chars, upper, lower, digit).
    [Theory]
    [InlineData("short1A")]       // Less than 8 chars
    [InlineData("alllowercase1")] // Missing uppercase
    [InlineData("ALLUPPERCASE1")] // Missing lowercase
    [InlineData("NoDigitsInPassword")] // Missing digit
    public void RegisterCommandValidator_WithWeakPassword_FailsValidation(string weakPassword)
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand(
            Email: "user@example.com",
            UserName: "validuser",
            FullName: "Valid User",
            Password: weakPassword,
            PreferredLanguage: "en");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Password));
    }
}

