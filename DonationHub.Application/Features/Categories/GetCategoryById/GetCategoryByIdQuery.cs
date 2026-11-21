namespace DonationHub.Application.Features.Categories.GetCategoryById;

using DonationHub.Domain.Common.Results;
using MediatR;

public record GetCategoryByIdQuery(Guid Id) : IRequest<Result<CategoryDto>>;

