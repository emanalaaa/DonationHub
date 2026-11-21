namespace DonationHub.Application.Features.Campaigns;

public record CampaignPublicDto(
    Guid Id,
    string Title,
    string Description,
    decimal GoalAmount,
    decimal RaisedAmount,
    string Currency,
    decimal ProgressPercent,
    int DaysLeft,
    DateTime EndDate,
    Guid CategoryId,
    string Status,
    string? BannerFileName,
    string RowVersion);

