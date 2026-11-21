namespace DonationHub.Application.Features.Campaigns;

public record CampaignAdminDto(
    Guid Id,
    string TitleAr,
    string TitleEn,
    string DescriptionAr,
    string DescriptionEn,
    decimal GoalAmount,
    decimal RaisedAmount,
    string Currency,
    DateTime EndDate,
    Guid CategoryId,
    string Status,
    string? BannerFileName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string RowVersion);

