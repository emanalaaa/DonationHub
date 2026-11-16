namespace DonationHub.Api.Endpoints;

using System.Security.Claims;
using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Features.Auth;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, IIdentityService identityService, CancellationToken ct) =>
        {
            var result = await identityService.RegisterAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        })
        .AllowAnonymous();

        group.MapPost("/login", async (LoginRequest request, IIdentityService identityService, CancellationToken ct) =>
        {
            var result = await identityService.LoginAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        })
        .AllowAnonymous();

        group.MapPost("/refresh", async (TokenRequest request, IIdentityService identityService, CancellationToken ct) =>
        {
            var result = await identityService.RefreshTokenAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        })
        .AllowAnonymous();

        group.MapPost("/logout", async (TokenRequest request, ClaimsPrincipal principal, IIdentityService identityService, CancellationToken ct) =>
        {
            var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await identityService.LogoutAsync(request.RefreshToken, userId, ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        })
        .RequireAuthorization();

        group.MapGet("/me", async (ClaimsPrincipal principal, IIdentityService identityService, CancellationToken ct) =>
        {
            var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await identityService.GetCurrentUserAsync(userId, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        })
        .RequireAuthorization();

        return group;
    }
}

