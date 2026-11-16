namespace DonationHub.Application.Features.Categories.Update;

public record UpdateCategoryCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string DescriptionAr = "",
    string DescriptionEn = "");

