namespace DonationHub.Application.Features.Campaigns.GetSuccessStories;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetSuccessStoriesQueryHandler : IRequestHandler<GetSuccessStoriesQuery, Result<IReadOnlyList<CampaignPublicDto>>>
{
    private readonly IAppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetSuccessStoriesQueryHandler(IAppDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Result<IReadOnlyList<CampaignPublicDto>>> Handle(
        GetSuccessStoriesQuery request,
        CancellationToken cancellationToken)
    {
        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var isArabic = request.Language.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

        var campaigns = await _context.Campaigns
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.Status == CampaignStatus.Completed)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);

        var dtos = campaigns.Select(c =>
        {
            var title = isArabic && !string.IsNullOrWhiteSpace(c.TitleAr) ? c.TitleAr : c.TitleEn;
            var description = isArabic && !string.IsNullOrWhiteSpace(c.DescriptionAr) ? c.DescriptionAr : c.DescriptionEn;

            var progressPercent = c.GoalAmount.Amount > 0
                ? Math.Round(Math.Min(100m, Math.Max(0m, (c.RaisedAmount.Amount / c.GoalAmount.Amount) * 100m)), 2)
                : 100m;

            var daysLeft = Math.Max(0, (int)Math.Ceiling((c.EndDate - nowUtc).TotalDays));

            return new CampaignPublicDto(
                c.Id,
                title,
                description,
                c.GoalAmount.Amount,
                c.RaisedAmount.Amount,
                c.GoalAmount.Currency,
                progressPercent,
                daysLeft,
                c.EndDate,
                c.CategoryId,
                c.Status.ToString(),
                c.BannerFileName,
                Convert.ToBase64String(c.RowVersion));
        }).ToList();

        return dtos;
    }
}

