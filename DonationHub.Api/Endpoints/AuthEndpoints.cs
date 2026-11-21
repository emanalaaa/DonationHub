namespace DonationHub.Api.Endpoints;

using System.Security.Claims;
using DonationHub.Api.Extensions;
using DonationHub.Application.Features.Auth.GetCurrentUser;
using DonationHub.Application.Features.Auth.Login;
using DonationHub.Application.Features.Auth.Logout;
using DonationHub.Application.Features.Auth.RefreshToken;
using DonationHub.Application.Features.Auth.Register;
using MediatR;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (RegisterCommand command, ISender mediator, CancellationToken ct) =>
            (await mediator.Send(command, ct)).ToHttpResult())
            .AllowAnonymous();

        group.MapPost("/login", async (LoginCommand command, ISender mediator, CancellationToken ct) =>
            (await mediator.Send(command, ct)).ToHttpResult())
            .AllowAnonymous();

        group.MapPost("/refresh", async (RefreshTokenCommand command, ISender mediator, CancellationToken ct) =>
            (await mediator.Send(command, ct)).ToHttpResult())
            .AllowAnonymous();

        group.MapPost("/logout", async (TokenRequest request, ClaimsPrincipal principal, ISender mediator, CancellationToken ct) =>
        {
            var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await mediator.Send(new LogoutCommand(request.RefreshToken, userId), ct);
            return result.ToHttpResult(StatusCodes.Status204NoContent);
        })
        .RequireAuthorization();

        group.MapGet("/me", async (ClaimsPrincipal principal, ISender mediator, CancellationToken ct) =>
        {
            var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await mediator.Send(new GetCurrentUserQuery(userId), ct);
            return result.ToHttpResult();
        })
        .RequireAuthorization();

        return group;
    }
}

public record TokenRequest(string RefreshToken);
