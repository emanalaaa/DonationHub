namespace DonationHub.Application.Features.Categories.CreateCategory;

using DonationHub.Domain.Common.Results;
using MediatR;

public record CreateCategoryCommand(
    string NameAr,
    string NameEn,
    string DescriptionAr = "",
    string DescriptionEn = "") : IRequest<Result<Guid>>;

