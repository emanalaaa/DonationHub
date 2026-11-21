namespace DonationHub.UnitTests.Handlers;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Features.Auth.Login;
using DonationHub.Application.Features.Campaigns.CreateCampaign;
using DonationHub.Application.Features.Campaigns.UpdateCampaign;
using DonationHub.Application.Features.Categories.DeleteCategory;
using DonationHub.Domain.Common.Errors;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Entities;
using DonationHub.Domain.ValueObjects;
using DonationHub.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

public class HandlerTests
{
    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // Test 8: Category rule: Deleting a category that has linked campaigns returns CategoryErrors.HasCampaigns.
    [Fact]
    public async Task DeleteCategory_WhenCategoryHasLinkedCampaigns_ReturnsHasCampaignsError()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var categoryId = Guid.NewGuid();
        var category = new Category(categoryId, "فئة", "Category", "وصف", "Desc", DateTime.UtcNow);
        await context.Categories.AddAsync(category);

        var campaign = Campaign.Create(
            Guid.NewGuid(),
            "حملة",
            "Campaign",
            "وصف",
            "Desc",
            new Money(1000m, "USD"),
            DateTime.UtcNow.AddDays(20),
            categoryId,
            Guid.NewGuid(),
            DateTime.UtcNow).Value;
        await context.Campaigns.AddAsync(campaign);
        await context.SaveChangesAsync();

        var mockRepo = new Mock<IGenericRepository<Category>>();
        mockRepo.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var mockUow = new Mock<IUnitOfWork>();

        var handler = new DeleteCategoryCommandHandler(mockRepo.Object, mockUow.Object, context);
        var command = new DeleteCategoryCommand(categoryId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CategoryErrors.HasCampaigns.Code);
        mockRepo.Verify(r => r.Remove(It.IsAny<Category>()), Times.Never);
        mockUow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Test 11: Handler: CreateCampaignCommandHandler returns CategoryErrors.NotFound when the category does not exist and never calls SaveChangesAsync.
    [Fact]
    public async Task CreateCampaign_WhenCategoryDoesNotExist_ReturnsNotFoundAndNeverSaves()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var mockRepo = new Mock<IGenericRepository<Campaign>>();
        var mockUow = new Mock<IUnitOfWork>();
        var mockTimeProvider = new Mock<TimeProvider>();
        mockTimeProvider.Setup(t => t.GetUtcNow()).Returns(DateTimeOffset.UtcNow);

        var handler = new CreateCampaignCommandHandler(mockRepo.Object, mockUow.Object, context, mockTimeProvider.Object);
        var command = new CreateCampaignCommand(
            "حملة",
            "Campaign",
            "وصف",
            "Desc",
            5000m,
            "USD",
            DateTime.UtcNow.AddDays(15),
            Guid.NewGuid(), // Non-existent category
            Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CategoryErrors.NotFound.Code);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Campaign>(), It.IsAny<CancellationToken>()), Times.Never);
        mockUow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Test 12: Handler: CreateCampaignCommandHandler adds the campaign and calls Unit of Work SaveChangesAsync once on success.
    [Fact]
    public async Task CreateCampaign_WhenValid_AddsCampaignAndCompletesUnitOfWorkOnce()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var categoryId = Guid.NewGuid();
        var category = new Category(categoryId, "فئة", "Category", "وصف", "Desc", DateTime.UtcNow);
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        var mockRepo = new Mock<IGenericRepository<Campaign>>();
        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var now = new DateTimeOffset(2026, 11, 22, 12, 0, 0, TimeSpan.Zero);
        var mockTimeProvider = new Mock<TimeProvider>();
        mockTimeProvider.Setup(t => t.GetUtcNow()).Returns(now);

        var handler = new CreateCampaignCommandHandler(mockRepo.Object, mockUow.Object, context, mockTimeProvider.Object);
        var command = new CreateCampaignCommand(
            "حملة إغاثة",
            "Relief Campaign",
            "وصف عربي",
            "English desc",
            10000m,
            "USD",
            now.UtcDateTime.AddDays(30),
            categoryId,
            Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TitleAr.Should().Be("حملة إغاثة");
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Campaign>(), It.IsAny<CancellationToken>()), Times.Once);
        mockUow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test 13: Handler: UpdateCampaignCommandHandler returns CampaignErrors.NotFound for an unknown ID.
    [Fact]
    public async Task UpdateCampaign_WhenCampaignNotFound_ReturnsNotFoundError()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var categoryId = Guid.NewGuid();
        var category = new Category(categoryId, "فئة", "Category", "وصف", "Desc", DateTime.UtcNow);
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        var mockRepo = new Mock<IGenericRepository<Campaign>>();
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campaign?)null);

        var mockUow = new Mock<IUnitOfWork>();
        var mockTimeProvider = new Mock<TimeProvider>();

        var handler = new UpdateCampaignCommandHandler(mockRepo.Object, mockUow.Object, context, mockTimeProvider.Object);
        var command = new UpdateCampaignCommand(
            Guid.NewGuid(),
            "حملة",
            "Campaign",
            "وصف",
            "Desc",
            5000m,
            "USD",
            DateTime.UtcNow.AddDays(20),
            categoryId,
            null,
            new byte[] { 1, 2, 3 });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CampaignErrors.NotFound.Code);
        mockUow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Test 14: Handler: UpdateCampaignCommandHandler maps DbUpdateConcurrencyException to CampaignErrors.ConcurrencyConflict.
    [Fact]
    public async Task UpdateCampaign_OnDbUpdateConcurrencyException_ReturnsConcurrencyConflictError()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var categoryId = Guid.NewGuid();
        var category = new Category(categoryId, "فئة", "Category", "وصف", "Desc", DateTime.UtcNow);
        await context.Categories.AddAsync(category);

        var nowUtc = new DateTime(2026, 11, 22, 12, 0, 0, DateTimeKind.Utc);
        var campaign = Campaign.Create(
            Guid.NewGuid(),
            "حملة",
            "Campaign",
            "وصف",
            "Desc",
            new Money(5000m, "USD"),
            nowUtc.AddDays(20),
            categoryId,
            Guid.NewGuid(),
            nowUtc).Value;
        await context.Campaigns.AddAsync(campaign);
        await context.SaveChangesAsync();

        var mockRepo = new Mock<IGenericRepository<Campaign>>();
        mockRepo.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency conflict occurred."));

        var mockTimeProvider = new Mock<TimeProvider>();
        mockTimeProvider.Setup(t => t.GetUtcNow()).Returns(nowUtc);

        var handler = new UpdateCampaignCommandHandler(mockRepo.Object, mockUow.Object, context, mockTimeProvider.Object);
        var command = new UpdateCampaignCommand(
            campaign.Id,
            "حملة محدثة",
            "Updated Campaign",
            "وصف",
            "Desc",
            6000m,
            "USD",
            nowUtc.AddDays(25),
            categoryId,
            null,
            new byte[] { 9, 9, 9 });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CampaignErrors.ConcurrencyConflict.Code);
    }

    // Test 16: Handler: Login returns the exact same AuthErrors.InvalidCredentials for both an unknown email and a wrong password (IDN-12).
    [Theory]
    [InlineData("unknown@example.com", "CorrectPassword123!", true)]   // Unknown user
    [InlineData("known@example.com", "WrongPassword123!", false)]        // Wrong password for existing user
    public async Task Login_WithUnknownEmailOrWrongPassword_ReturnsSameInvalidCredentialsError(
        string email,
        string password,
        bool isUnknownUser)
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var knownEmail = "known@example.com";
        var user = new ApplicationUser(
            Guid.NewGuid(),
            knownEmail,
            "knownuser",
            "Known User",
            "hashed_password",
            DateTime.UtcNow,
            "Donor",
            "en");

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var mockRepo = new Mock<IGenericRepository<RefreshToken>>();
        var mockUow = new Mock<IUnitOfWork>();
        var mockHasher = new Mock<IPasswordHasher>();
        mockHasher.Setup(h => h.VerifyPassword(It.IsAny<string>(), "hashed_password"))
            .Returns(!isUnknownUser && password == "CorrectPassword123!");

        var mockJwt = new Mock<IJwtTokenGenerator>();
        var mockTimeProvider = new Mock<TimeProvider>();

        var handler = new LoginCommandHandler(
            mockRepo.Object,
            mockUow.Object,
            context,
            mockHasher.Object,
            mockJwt.Object,
            mockTimeProvider.Object);

        var command = new LoginCommand(email, password);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AuthErrors.InvalidCredentials.Code);
        result.Error.Message.Should().Be(AuthErrors.InvalidCredentials.Message);
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }
}
