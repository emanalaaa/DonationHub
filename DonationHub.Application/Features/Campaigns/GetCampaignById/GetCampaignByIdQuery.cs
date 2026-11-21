namespace DonationHub.Application.Features.Campaigns.GetCampaignById;

using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Common.Results;
using MediatR;

public record GetCampaignByIdQuery(Guid Id, string Language = "en") : IRequest<Result<CampaignPublicDto>>;

