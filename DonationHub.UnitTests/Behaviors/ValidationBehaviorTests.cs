namespace DonationHub.UnitTests.Behaviors;

using DonationHub.Application.Common.Behaviors;
using DonationHub.Domain.Common.Results;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;

public class ValidationBehaviorTests
{
    public record TestCommand(string Name) : IRequest<Result<string>>;

    // Test 15: Pipeline: ValidationBehavior short-circuits with Validation.Failed and never invokes next() (the handler) when a validator fails.
    [Fact]
    public async Task ValidationBehavior_WhenValidationFails_ShortCircuitsAndDoesNotInvokeNext()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<TestCommand>>();
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required.")
        };

        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var behavior = new ValidationBehavior<TestCommand, Result<string>>(new[] { mockValidator.Object });
        var nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = (cancellationToken) =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success("Handled"));
        };

        var command = new TestCommand("");

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation.Failed");
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.ValidationErrors.Should().ContainKey("Name");
    }
}
