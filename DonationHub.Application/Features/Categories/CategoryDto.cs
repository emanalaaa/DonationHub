namespace DonationHub.Application.Features.Categories;

public record CategoryDto(
    Guid Id,
    string NameAr,
    string NameEn,
    string DescriptionAr,
    string DescriptionEn,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

