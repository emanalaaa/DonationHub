namespace DonationHub.Application.Features.Campaigns.GetCampaignById;

using FluentValidation;

public class GetCampaignByIdQueryValidator : AbstractValidator<GetCampaignByIdQuery>
{
    public GetCampaignByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Campaign ID is required.");
    }
}

