namespace DonationHub.Application.Features.Categories.Create;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Common.Models;
using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class CreateCategoryHandler
{
    private readonly IGenericRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationDbContext _context;

    public CreateCategoryHandler(
        IGenericRepository<Category> repository,
        IUnitOfWork unitOfWork,
        IApplicationDbContext context)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Result<Guid>> HandleAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var nameAr = command.NameAr?.Trim() ?? string.Empty;
        var nameEn = command.NameEn?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nameAr))
        {
            return Result<Guid>.Failure("Category.NameArRequired", "Arabic name is required.", 400);
        }

        if (nameAr.Length > 100)
        {
            return Result<Guid>.Failure("Category.NameArTooLong", "Arabic name cannot exceed 100 characters.", 400);
        }

        if (string.IsNullOrWhiteSpace(nameEn))
        {
            return Result<Guid>.Failure("Category.NameEnRequired", "English name is required.", 400);
        }

        if (nameEn.Length > 100)
        {
            return Result<Guid>.Failure("Category.NameEnTooLong", "English name cannot exceed 100 characters.", 400);
        }

        // Unique validation
        var exists = await _context.Categories.AnyAsync(
            c => c.NameAr == nameAr || c.NameEn == nameEn,
            cancellationToken);

        if (exists)
        {
            return Result<Guid>.Failure("Category.NameAlreadyExists", "A category with the same Arabic or English name already exists.", 409);
        }

        var category = new Category(
            Guid.NewGuid(),
            nameAr,
            nameEn,
            command.DescriptionAr ?? string.Empty,
            command.DescriptionEn ?? string.Empty,
            DateTime.UtcNow);

        await _repository.AddAsync(category, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result<Guid>.Success(category.Id);
    }
}

