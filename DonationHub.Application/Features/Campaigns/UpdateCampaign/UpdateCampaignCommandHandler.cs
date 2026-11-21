namespace DonationHub.Application.Features.Campaigns.UpdateCampaign;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Common.Errors;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Entities;
using DonationHub.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateCampaignCommandHandler : IRequestHandler<UpdateCampaignCommand, Result<CampaignAdminDto>>
{
    private readonly IGenericRepository<Campaign> _campaignRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateCampaignCommandHandler(
        IGenericRepository<Campaign> campaignRepository,
        IUnitOfWork unitOfWork,
        IAppDbContext context,
        TimeProvider timeProvider)
    {
        _campaignRepository = campaignRepository;
        _unitOfWork = unitOfWork;
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Result<CampaignAdminDto>> Handle(
        UpdateCampaignCommand request,
        CancellationToken cancellationToken)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return CategoryErrors.NotFound;
        }

        var campaign = await _campaignRepository.GetByIdAsync(request.Id, cancellationToken);
        if (campaign is null || campaign.IsDeleted)
        {
            return CampaignErrors.NotFound;
        }

        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var moneyResult = new Money(request.GoalAmount, request.Currency);

        var updateResult = campaign.Update(
            request.TitleAr,
            request.TitleEn,
            request.DescriptionAr,
            request.DescriptionEn,
            moneyResult,
            request.EndDate,
            request.CategoryId,
            request.BannerFileName,
            nowUtc);

        if (!updateResult.IsSuccess)
        {
            return updateResult.Error;
        }

        if (request.RowVersion is not null && request.RowVersion.Length > 0)
        {
            _context.Entry(campaign).Property(c => c.RowVersion).OriginalValue = request.RowVersion;
        }

        _campaignRepository.Update(campaign);

        try
        {
            await _unitOfWork.CompleteAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return CampaignErrors.ConcurrencyConflict;
        }

        var dto = new CampaignAdminDto(
            campaign.Id,
            campaign.TitleAr,
            campaign.TitleEn,
            campaign.DescriptionAr,
            campaign.DescriptionEn,
            campaign.GoalAmount.Amount,
            campaign.RaisedAmount.Amount,
            campaign.GoalAmount.Currency,
            campaign.EndDate,
            campaign.CategoryId,
            campaign.Status.ToString(),
            campaign.BannerFileName,
            campaign.CreatedAt,
            campaign.UpdatedAt,
            Convert.ToBase64String(campaign.RowVersion));

        return dto;
    }
}

