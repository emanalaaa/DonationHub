namespace DonationHub.Application.Features.Campaigns.GetActiveCampaigns;

using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Common.Results;
using MediatR;

public record GetActiveCampaignsQuery(string Language = "en") : IRequest<Result<IReadOnlyList<CampaignPublicDto>>>;

