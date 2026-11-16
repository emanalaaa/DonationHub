namespace DonationHub.Application.Features.Categories.Update;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Common.Models;
using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class UpdateCategoryHandler
{
    private readonly IGenericRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationDbContext _context;

    public UpdateCategoryHandler(
        IGenericRepository<Category> repository,
        IUnitOfWork unitOfWork,
        IApplicationDbContext context)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Result> HandleAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var nameAr = command.NameAr?.Trim() ?? string.Empty;
        var nameEn = command.NameEn?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nameAr))
        {
            return Result.Failure("Category.NameArRequired", "Arabic name is required.", 400);
        }

        if (nameAr.Length > 100)
        {
            return Result.Failure("Category.NameArTooLong", "Arabic name cannot exceed 100 characters.", 400);
        }

        if (string.IsNullOrWhiteSpace(nameEn))
        {
            return Result.Failure("Category.NameEnRequired", "English name is required.", 400);
        }

        if (nameEn.Length > 100)
        {
            return Result.Failure("Category.NameEnTooLong", "English name cannot exceed 100 characters.", 400);
        }

        var category = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
        {
            return Result.Failure("Category.NotFound", $"Category with ID '{command.Id}' was not found.", 404);
        }

        // Uniqueness check excluding current record
        var duplicateExists = await _context.Categories.AnyAsync(
            c => c.Id != command.Id && (c.NameAr == nameAr || c.NameEn == nameEn),
            cancellationToken);

        if (duplicateExists)
        {
            return Result.Failure("Category.NameAlreadyExists", "A category with the same Arabic or English name already exists.", 409);
        }

        category.Update(
            nameAr,
            nameEn,
            command.DescriptionAr ?? string.Empty,
            command.DescriptionEn ?? string.Empty,
            DateTime.UtcNow);

        _repository.Update(category);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

