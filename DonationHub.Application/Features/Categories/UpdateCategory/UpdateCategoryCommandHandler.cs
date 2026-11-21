namespace DonationHub.Application.Features.Categories.UpdateCategory;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Common.Errors;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly IGenericRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateCategoryCommandHandler(
        IGenericRepository<Category> repository,
        IUnitOfWork unitOfWork,
        IAppDbContext context,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Result> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var nameAr = request.NameAr.Trim();
        var nameEn = request.NameEn.Trim();

        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
        {
            return CategoryErrors.NotFound;
        }

        var duplicateExists = await _context.Categories.AnyAsync(
            c => c.Id != request.Id && (c.NameAr == nameAr || c.NameEn == nameEn),
            cancellationToken);

        if (duplicateExists)
        {
            return Error.Conflict("Category.NameAlreadyExists", "A category with the same Arabic or English name already exists.");
        }

        category.Update(
            nameAr,
            nameEn,
            request.DescriptionAr?.Trim() ?? string.Empty,
            request.DescriptionEn?.Trim() ?? string.Empty,
            _timeProvider.GetUtcNow().UtcDateTime);

        _repository.Update(category);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

