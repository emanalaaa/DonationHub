namespace DonationHub.Application.Features.Campaigns.CreateCampaign;

using FluentValidation;

public class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
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
    }
}

