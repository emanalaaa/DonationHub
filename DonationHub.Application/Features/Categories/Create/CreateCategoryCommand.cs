namespace DonationHub.Application.Features.Categories.Create;

public record CreateCategoryCommand(
    string NameAr,
    string NameEn,
    string DescriptionAr = "",
    string DescriptionEn = "");

