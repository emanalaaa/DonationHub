namespace DonationHub.Application.Features.Campaigns.CreateCampaign;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Common.Errors;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Entities;
using DonationHub.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, Result<CampaignAdminDto>>
{
    private readonly IGenericRepository<Campaign> _campaignRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreateCampaignCommandHandler(
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
        CreateCampaignCommand request,
        CancellationToken cancellationToken)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return CategoryErrors.NotFound;
        }

        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

        var moneyResult = new Money(request.GoalAmount, request.Currency);
        var campaignResult = Campaign.Create(
            Guid.NewGuid(),
            request.TitleAr,
            request.TitleEn,
            request.DescriptionAr,
            request.DescriptionEn,
            moneyResult,
            request.EndDate,
            request.CategoryId,
            request.CreatedByUserId,
            nowUtc,
            request.BannerFileName);

        if (!campaignResult.IsSuccess)
        {
            return campaignResult.Error;
        }

        var campaign = campaignResult.Value;

        await _campaignRepository.AddAsync(campaign, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

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

