namespace DonationHub.Application.Features.Categories.UpdateCategory;

using DonationHub.Domain.Common.Results;
using MediatR;

public record UpdateCategoryCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string DescriptionAr = "",
    string DescriptionEn = "") : IRequest<Result>;

