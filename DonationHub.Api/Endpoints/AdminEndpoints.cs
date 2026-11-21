namespace DonationHub.Api.Endpoints;

using DonationHub.Api.Extensions;
using DonationHub.Application.Features.Admin.CreateStaff;
using MediatR;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapPost("/staff", async (CreateStaffCommand command, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToHttpResult(id => Results.Created($"/api/v1/admin/staff/{id}", new { Id = id }));
        });

        return group;
    }
}
