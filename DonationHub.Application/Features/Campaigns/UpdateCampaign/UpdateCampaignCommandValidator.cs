namespace DonationHub.Application.Features.Campaigns.UpdateCampaign;

using FluentValidation;

public class UpdateCampaignCommandValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Campaign ID is required.");

        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("Arabic title is required.")
            .MaximumLength(250).WithMessage("Arabic title cannot exceed 250 characters.");

        RuleFor(x => x.TitleEn)
            .NotEmpty().WithMessage("English title is required.")
            .MaximumLength(250).WithMessage("English title cannot exceed 250 characters.");

        RuleFor(x => x.DescriptionAr)
            .MaximumLength(4000).WithMessage("Arabic description cannot exceed 4000 characters.");

        RuleFor(x => x.DescriptionEn)
            .MaximumLength(4000).WithMessage("English description cannot exceed 4000 characters.");

        RuleFor(x => x.GoalAmount)
            .GreaterThan(0).WithMessage("Goal amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required.");

        RuleFor(x => x.RowVersion)
            .NotNull().WithMessage("RowVersion is required.")
            .NotEmpty().WithMessage("RowVersion cannot be empty.");
    }
}

