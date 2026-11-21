namespace DonationHub.Application.Features.Categories.DeleteCategory;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Common.Errors;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly IGenericRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppDbContext _context;

    public DeleteCategoryCommandHandler(
        IGenericRepository<Category> repository,
        IUnitOfWork unitOfWork,
        IAppDbContext context)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
        {
            return CategoryErrors.NotFound;
        }

        var hasCampaigns = await _context.Campaigns
            .AnyAsync(c => c.CategoryId == request.Id && !c.IsDeleted, cancellationToken);

        if (hasCampaigns)
        {
            return CategoryErrors.HasCampaigns;
        }

        _repository.Remove(category);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

