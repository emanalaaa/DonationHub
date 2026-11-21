namespace DonationHub.Api.Controllers;

using System.Security.Claims;
using DonationHub.Api.Extensions;
using DonationHub.Application.Features.Campaigns.CreateCampaign;
using DonationHub.Application.Features.Campaigns.DeleteCampaign;
using DonationHub.Application.Features.Campaigns.GetActiveCampaigns;
using DonationHub.Application.Features.Campaigns.GetCampaignById;
using DonationHub.Application.Features.Campaigns.GetSuccessStories;
using DonationHub.Application.Features.Campaigns.UpdateCampaign;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/campaigns")]
public class CampaignsController : ControllerBase
{
    private readonly ISender _mediator;

    public CampaignsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetActive(CancellationToken ct) =>
        (await _mediator.Send(new GetActiveCampaignsQuery(GetLanguage()), ct)).ToActionResult();

    [HttpGet("success-stories")]
    [AllowAnonymous]
    public async Task<IResult> GetSuccessStories(CancellationToken ct) =>
        (await _mediator.Send(new GetSuccessStoriesQuery(GetLanguage()), ct)).ToActionResult();

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IResult> GetById(Guid id, CancellationToken ct) =>
        (await _mediator.Send(new GetCampaignByIdQuery(id, GetLanguage()), ct)).ToActionResult();

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> Create([FromBody] CreateCampaignRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var command = new CreateCampaignCommand(
            request.TitleAr,
            request.TitleEn,
            request.DescriptionAr,
            request.DescriptionEn,
            request.GoalAmount,
            request.Currency,
            request.EndDate,
            request.CategoryId,
            userId,
            request.BannerFileName);

        var result = await _mediator.Send(command, ct);
        return result.ToActionResult(dto => Results.Created($"/api/v1/campaigns/{dto.Id}", dto));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> Update(Guid id, [FromBody] UpdateCampaignRequest request, CancellationToken ct)
    {
        var rowVersionBytes = Convert.FromBase64String(request.RowVersion);
        var command = new UpdateCampaignCommand(
            id,
            request.TitleAr,
            request.TitleEn,
            request.DescriptionAr,
            request.DescriptionEn,
            request.GoalAmount,
            request.Currency,
            request.EndDate,
            request.CategoryId,
            request.BannerFileName,
            rowVersionBytes);

        var result = await _mediator.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> Delete(Guid id, CancellationToken ct) =>
        (await _mediator.Send(new DeleteCampaignCommand(id), ct)).ToActionResult(StatusCodes.Status204NoContent);

    private string GetLanguage()
    {
        var acceptLang = Request.Headers.AcceptLanguage.ToString();
        return acceptLang.StartsWith("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
    }
}

public record CreateCampaignRequest(
    string TitleAr,
    string TitleEn,
    string DescriptionAr,
    string DescriptionEn,
    decimal GoalAmount,
    string Currency,
    DateTime EndDate,
    Guid CategoryId,
    string? BannerFileName = null);

public record UpdateCampaignRequest(
    string TitleAr,
    string TitleEn,
    string DescriptionAr,
    string DescriptionEn,
    decimal GoalAmount,
    string Currency,
    DateTime EndDate,
    Guid CategoryId,
    string? BannerFileName,
    string RowVersion);

