namespace DonationHub.Api.Endpoints;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Features.Admin;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapPost("/staff", async (CreateStaffRequest request, IIdentityService identityService, CancellationToken ct) =>
        {
            var result = await identityService.CreateStaffAsync(request, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/admin/staff/{result.Value}", new { Id = result.Value })
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        });

        return group;
    }
}

