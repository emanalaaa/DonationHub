namespace DonationHub.Application.Features.Campaigns.DeleteCampaign;

using FluentValidation;

public class DeleteCampaignCommandValidator : AbstractValidator<DeleteCampaignCommand>
{
    public DeleteCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Campaign ID is required.");
    }
}

