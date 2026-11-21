namespace DonationHub.Application;

using DonationHub.Application.Common.Behaviors;
using DonationHub.Application.Features.Profile.ChangePassword;
using DonationHub.Application.Features.Profile.UpdateProfile;
using DonationHub.Application.Options;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddOptions<DonationOptions>()
            .BindConfiguration(DonationOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // MediatR & Automatic Validation Pipeline Behavior
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Profile Vertical Slice Handlers
        services.AddScoped<UpdateProfileHandler>();
        services.AddScoped<ChangePasswordHandler>();

        return services;
    }
}
