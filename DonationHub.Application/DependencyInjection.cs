namespace DonationHub.Application;

using DonationHub.Application.Features.Categories.Create;
using DonationHub.Application.Features.Categories.Delete;
using DonationHub.Application.Features.Categories.GetAll;
using DonationHub.Application.Features.Categories.GetById;
using DonationHub.Application.Features.Categories.Update;
using DonationHub.Application.Features.Profile.ChangePassword;
using DonationHub.Application.Features.Profile.UpdateProfile;
using DonationHub.Application.Options;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddOptions<DonationOptions>()
            .BindConfiguration(DonationOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Category Vertical Slice Handlers
        services.AddScoped<CreateCategoryHandler>();
        services.AddScoped<UpdateCategoryHandler>();
        services.AddScoped<DeleteCategoryHandler>();
        services.AddScoped<GetCategoryByIdHandler>();
        services.AddScoped<GetAllCategoriesHandler>();

        // Profile Vertical Slice Handlers
        services.AddScoped<UpdateProfileHandler>();
        services.AddScoped<ChangePasswordHandler>();

        return services;
    }
}
