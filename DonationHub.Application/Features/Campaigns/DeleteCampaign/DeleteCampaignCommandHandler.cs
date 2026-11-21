namespace DonationHub.Application.Features.Campaigns.DeleteCampaign;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Common.Errors;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Entities;
using MediatR;

public class DeleteCampaignCommandHandler : IRequestHandler<DeleteCampaignCommand, Result>
{
    private readonly IGenericRepository<Campaign> _campaignRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public DeleteCampaignCommandHandler(
        IGenericRepository<Campaign> campaignRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _campaignRepository = campaignRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result> Handle(
        DeleteCampaignCommand request,
        CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(request.Id, cancellationToken);
        if (campaign is null || campaign.IsDeleted)
        {
            return CampaignErrors.NotFound;
        }

        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        campaign.SoftDelete(nowUtc);

        _campaignRepository.Update(campaign);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

