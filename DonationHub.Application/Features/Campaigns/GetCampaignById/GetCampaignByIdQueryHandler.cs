namespace DonationHub.Application.Features.Campaigns.GetCampaignById;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Common.Errors;
using DonationHub.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetCampaignByIdQueryHandler : IRequestHandler<GetCampaignByIdQuery, Result<CampaignPublicDto>>
{
    private readonly IAppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetCampaignByIdQueryHandler(IAppDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Result<CampaignPublicDto>> Handle(
        GetCampaignByIdQuery request,
        CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (campaign is null)
        {
            return CampaignErrors.NotFound;
        }

        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var isArabic = request.Language.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

        var title = isArabic && !string.IsNullOrWhiteSpace(campaign.TitleAr) ? campaign.TitleAr : campaign.TitleEn;
        var description = isArabic && !string.IsNullOrWhiteSpace(campaign.DescriptionAr) ? campaign.DescriptionAr : campaign.DescriptionEn;

        var progressPercent = campaign.GoalAmount.Amount > 0
            ? Math.Round(Math.Min(100m, Math.Max(0m, (campaign.RaisedAmount.Amount / campaign.GoalAmount.Amount) * 100m)), 2)
            : 0m;

        var daysLeft = Math.Max(0, (int)Math.Ceiling((campaign.EndDate - nowUtc).TotalDays));

        return new CampaignPublicDto(
            campaign.Id,
            title,
            description,
            campaign.GoalAmount.Amount,
            campaign.RaisedAmount.Amount,
            campaign.GoalAmount.Currency,
            progressPercent,
            daysLeft,
            campaign.EndDate,
            campaign.CategoryId,
            campaign.Status.ToString(),
            campaign.BannerFileName,
            Convert.ToBase64String(campaign.RowVersion));
    }
}

