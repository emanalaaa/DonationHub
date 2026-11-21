namespace DonationHub.Application.Features.Campaigns.GetSuccessStories;

using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Common.Results;
using MediatR;

public record GetSuccessStoriesQuery(string Language = "en") : IRequest<Result<IReadOnlyList<CampaignPublicDto>>>;

