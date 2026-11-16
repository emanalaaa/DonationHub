namespace DonationHub.Application.Features.Categories.GetById;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

public class GetCategoryByIdHandler
{
    private readonly IApplicationDbContext _context;

    public GetCategoryByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CategoryDto>> HandleAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == query.Id)
            .Select(c => new CategoryDto(
                c.Id,
                c.NameAr,
                c.NameEn,
                c.DescriptionAr,
                c.DescriptionEn,
                c.CreatedAt,
                c.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
        {
            return Result<CategoryDto>.Failure("Category.NotFound", $"Category with ID '{query.Id}' was not found.", 404);
        }

        return Result<CategoryDto>.Success(category);
    }
}

