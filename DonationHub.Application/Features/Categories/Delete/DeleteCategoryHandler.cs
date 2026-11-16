namespace DonationHub.Application.Features.Categories.Delete;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Common.Models;
using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class DeleteCategoryHandler
{
    private readonly IGenericRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationDbContext _context;

    public DeleteCategoryHandler(
        IGenericRepository<Category> repository,
        IUnitOfWork unitOfWork,
        IApplicationDbContext context)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Result> HandleAsync(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Find category by ID
        var category = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
        {
            return Result.Failure("Category.NotFound", $"Category with ID '{command.Id}' was not found.", 404);
        }

        // 2. CAT-04 Invariant: Check if category has associated campaigns
        var hasCampaigns = await _context.Campaigns
            .AnyAsync(c => c.CategoryId == command.Id && !c.IsDeleted, cancellationToken);

        if (hasCampaigns)
        {
            return Result.Failure("Category.HasCampaigns", "Cannot delete category with associated campaigns.", 409);
        }

        // 3. Remove and persist
        _repository.Remove(category);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

