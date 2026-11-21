namespace DonationHub.Application.Features.Categories.GetCategories;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    private readonly IAppDbContext _context;

    public GetCategoriesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.NameEn)
            .Select(c => new CategoryDto(
                c.Id,
                c.NameAr,
                c.NameEn,
                c.DescriptionAr,
                c.DescriptionEn,
                c.CreatedAt,
                c.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CategoryDto>>.Success(categories);
    }
}

