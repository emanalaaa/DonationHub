namespace DonationHub.Api.Controllers;

using System.Security.Claims;
using DonationHub.Api.Extensions;
using DonationHub.Application.Features.Auth.GetCurrentUser;
using DonationHub.Application.Features.Auth.Login;
using DonationHub.Application.Features.Auth.Logout;
using DonationHub.Application.Features.Auth.RefreshToken;
using DonationHub.Application.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IResult> Register([FromBody] RegisterCommand command, CancellationToken ct) =>
        (await _mediator.Send(command, ct)).ToActionResult();

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IResult> Login([FromBody] LoginCommand command, CancellationToken ct) =>
        (await _mediator.Send(command, ct)).ToActionResult();

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken ct) =>
        (await _mediator.Send(command, ct)).ToActionResult();

    [HttpPost("logout")]
    [Authorize]
    public async Task<IResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();
        var result = await _mediator.Send(new LogoutCommand(request.RefreshToken, userId), ct);
        return result.ToActionResult(StatusCodes.Status204NoContent);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IResult> GetCurrentUser(CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();
        return (await _mediator.Send(new GetCurrentUserQuery(userId), ct)).ToActionResult();
    }
}

public record LogoutRequest(string RefreshToken);

