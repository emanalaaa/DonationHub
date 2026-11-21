namespace DonationHub.Domain.Common.Errors;

using DonationHub.Domain.Common.Results;

public static class DomainErrors
{
    public static class ValidationErrors
    {
        public static readonly Error Failed = Error.Validation(
            "Validation.Failed",
            "One or more validation errors occurred.");
    }

    public static class AuthErrors
    {
        public static readonly Error InvalidCredentials = Error.Unauthorized(
            "Auth.InvalidCredentials",
            "Invalid email or password.");

        public static readonly Error EmailAlreadyExists = Error.Conflict(
            "Auth.EmailAlreadyExists",
            "The email address is already registered.");

        public static readonly Error RefreshTokenInvalid = Error.Unauthorized(
            "Auth.RefreshTokenInvalid",
            "Invalid, expired, or revoked refresh token.");

        public static readonly Error AccountDeactivated = Error.Forbidden(
            "Auth.AccountDeactivated",
            "This account has been deactivated.");

        public static readonly Error TokenCompromised = Error.Unauthorized(
            "Auth.TokenCompromised",
            "Refresh token has already been used or revoked. All active sessions have been terminated due to potential compromise.");
    }

    public static class CategoryErrors
    {
        public static readonly Error NotFound = Error.NotFound(
            "Category.NotFound",
            "The requested category was not found.");

        public static readonly Error HasCampaigns = Error.Conflict(
            "Category.HasCampaigns",
            "Cannot delete category with associated campaigns.");
    }

    public static class CampaignErrors
    {
        public static readonly Error NotFound = Error.NotFound(
            "Campaign.NotFound",
            "The requested campaign was not found.");

        public static readonly Error NotActive = Error.Conflict(
            "Campaign.NotActive",
            "The campaign is not currently active.");

        public static readonly Error GoalBelowRaised = Error.UnprocessableEntity(
            "Campaign.GoalBelowRaised",
            "New goal amount cannot be less than the currently raised amount.");

        public static readonly Error EndDateInPast = Error.UnprocessableEntity(
            "Campaign.EndDateInPast",
            "The campaign end date must be in the future relative to the current time.");

        public static readonly Error ConcurrencyConflict = Error.Conflict(
            "Campaign.ConcurrencyConflict",
            "The campaign was modified by another operation. Please reload and try again.");
    }

    public static class DonationErrors
    {
        public static readonly Error NotFound = Error.NotFound(
            "Donation.NotFound",
            "The requested donation was not found.");

        public static readonly Error AmountOutOfRange = Error.UnprocessableEntity(
            "Donation.AmountOutOfRange",
            "The donation amount is out of the acceptable range.");

        public static readonly Error InvalidState = Error.Conflict(
            "Donation.InvalidState",
            "The donation is in an invalid state for this operation.");
    }

    public static class CommonErrors
    {
        public static readonly Error RateLimited = Error.TooManyRequests(
            "Common.RateLimited",
            "Too many requests. Please try again later.");
    }

    public static class FileErrors
    {
        public static readonly Error InvalidType = Error.Validation(
            "File.InvalidType",
            "The uploaded file type is not supported.");

        public static readonly Error TooLarge = Error.PayloadTooLarge(
            "File.TooLarge",
            "The uploaded file exceeds the maximum allowed size.");
    }

    public static class PaymentErrors
    {
        public static readonly Error InvalidSignature = Error.Validation(
            "Payment.InvalidSignature",
            "The payment webhook signature is invalid.");
    }

    public static class IdempotencyErrors
    {
        public static readonly Error KeyReusedWithDifferentRequest = Error.UnprocessableEntity(
            "Idempotency.KeyReusedWithDifferentRequest",
            "The idempotency key has already been used with a different request payload.");
    }
}

public static class ValidationErrors
{
    public static readonly Error Failed = DomainErrors.ValidationErrors.Failed;
}

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = DomainErrors.AuthErrors.InvalidCredentials;
    public static readonly Error EmailAlreadyExists = DomainErrors.AuthErrors.EmailAlreadyExists;
    public static readonly Error RefreshTokenInvalid = DomainErrors.AuthErrors.RefreshTokenInvalid;
    public static readonly Error AccountDeactivated = DomainErrors.AuthErrors.AccountDeactivated;
    public static readonly Error TokenCompromised = DomainErrors.AuthErrors.TokenCompromised;
}

public static class CategoryErrors
{
    public static readonly Error NotFound = DomainErrors.CategoryErrors.NotFound;
    public static readonly Error HasCampaigns = DomainErrors.CategoryErrors.HasCampaigns;
}

public static class CampaignErrors
{
    public static readonly Error NotFound = DomainErrors.CampaignErrors.NotFound;
    public static readonly Error NotActive = DomainErrors.CampaignErrors.NotActive;
    public static readonly Error GoalBelowRaised = DomainErrors.CampaignErrors.GoalBelowRaised;
    public static readonly Error EndDateInPast = DomainErrors.CampaignErrors.EndDateInPast;
    public static readonly Error ConcurrencyConflict = DomainErrors.CampaignErrors.ConcurrencyConflict;
}

public static class DonationErrors
{
    public static readonly Error NotFound = DomainErrors.DonationErrors.NotFound;
    public static readonly Error AmountOutOfRange = DomainErrors.DonationErrors.AmountOutOfRange;
    public static readonly Error InvalidState = DomainErrors.DonationErrors.InvalidState;
}

public static class CommonErrors
{
    public static readonly Error RateLimited = DomainErrors.CommonErrors.RateLimited;
}

public static class FileErrors
{
    public static readonly Error InvalidType = DomainErrors.FileErrors.InvalidType;
    public static readonly Error TooLarge = DomainErrors.FileErrors.TooLarge;
}

public static class PaymentErrors
{
    public static readonly Error InvalidSignature = DomainErrors.PaymentErrors.InvalidSignature;
}

public static class IdempotencyErrors
{
    public static readonly Error KeyReusedWithDifferentRequest = DomainErrors.IdempotencyErrors.KeyReusedWithDifferentRequest;
}
