namespace DonationHub.Application.Features.Campaigns.DeleteCampaign;

using DonationHub.Domain.Common.Results;
using MediatR;

public record DeleteCampaignCommand(Guid Id) : IRequest<Result>;

