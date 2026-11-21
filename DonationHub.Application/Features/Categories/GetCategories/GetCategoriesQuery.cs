namespace DonationHub.Application.Features.Categories.GetCategories;

using DonationHub.Domain.Common.Results;
using MediatR;

public record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;

