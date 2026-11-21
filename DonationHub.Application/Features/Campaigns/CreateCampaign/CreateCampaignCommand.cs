namespace DonationHub.Application.Features.Campaigns.CreateCampaign;

using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Common.Results;
using MediatR;

public record CreateCampaignCommand(
    string TitleAr,
    string TitleEn,
    string DescriptionAr,
    string DescriptionEn,
    decimal GoalAmount,
    string Currency,
    DateTime EndDate,
    Guid CategoryId,
    Guid CreatedByUserId,
    string? BannerFileName = null) : IRequest<Result<CampaignAdminDto>>;

