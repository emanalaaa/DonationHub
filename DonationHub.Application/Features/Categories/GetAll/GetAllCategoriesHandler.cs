namespace DonationHub.Application.Features.Categories.GetAll;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

public class GetAllCategoriesHandler
{
    private readonly IApplicationDbContext _context;

    public GetAllCategoriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> HandleAsync(GetAllCategoriesQuery query, CancellationToken cancellationToken = default)
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

