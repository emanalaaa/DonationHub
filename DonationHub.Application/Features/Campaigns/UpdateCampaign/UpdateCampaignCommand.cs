namespace DonationHub.Application.Features.Campaigns.UpdateCampaign;

using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Common.Results;
using MediatR;

public record UpdateCampaignCommand(
    Guid Id,
    string TitleAr,
    string TitleEn,
    string DescriptionAr,
    string DescriptionEn,
    decimal GoalAmount,
    string Currency,
    DateTime EndDate,
    Guid CategoryId,
    string? BannerFileName,
    byte[] RowVersion) : IRequest<Result<CampaignAdminDto>>;

