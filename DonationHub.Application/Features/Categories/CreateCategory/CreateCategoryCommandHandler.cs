namespace DonationHub.Application.Features.Categories.CreateCategory;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly IGenericRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreateCategoryCommandHandler(
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

    public async Task<Result<Guid>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var nameAr = request.NameAr.Trim();
        var nameEn = request.NameEn.Trim();

        var exists = await _context.Categories.AnyAsync(
            c => c.NameAr == nameAr || c.NameEn == nameEn,
            cancellationToken);

        if (exists)
        {
            return Error.Conflict("Category.NameAlreadyExists", "A category with the same Arabic or English name already exists.");
        }

        var category = new Category(
            Guid.NewGuid(),
            nameAr,
            nameEn,
            request.DescriptionAr?.Trim() ?? string.Empty,
            request.DescriptionEn?.Trim() ?? string.Empty,
            _timeProvider.GetUtcNow().UtcDateTime);

        await _repository.AddAsync(category, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return category.Id;
    }
}

