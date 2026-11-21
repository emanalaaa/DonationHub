namespace DonationHub.Application.Features.Categories.DeleteCategory;

using DonationHub.Domain.Common.Results;
using MediatR;

public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;

