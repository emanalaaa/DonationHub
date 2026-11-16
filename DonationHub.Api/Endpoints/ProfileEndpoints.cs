namespace DonationHub.Api.Endpoints;

using System.Security.Claims;
using DonationHub.Application.Features.Profile.ChangePassword;
using DonationHub.Application.Features.Profile.UpdateProfile;

public static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfileEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/profile")
            .WithTags("Profile")
            .RequireAuthorization();

        // PUT /api/v1/profile
        group.MapPut("/", async (UpdateProfileRequest request, ClaimsPrincipal principal, UpdateProfileHandler handler, CancellationToken ct) =>
        {
            var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var command = new UpdateProfileCommand(userId, request.FullName, request.PreferredLanguage);
            var result = await handler.HandleAsync(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        });

        // PUT /api/v1/profile/password
        group.MapPut("/password", async (ChangePasswordRequest request, ClaimsPrincipal principal, ChangePasswordHandler handler, CancellationToken ct) =>
        {
            var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
            var result = await handler.HandleAsync(command, ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        });

        return group;
    }
}

