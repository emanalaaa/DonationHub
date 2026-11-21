namespace DonationHub.Api.Controllers;

using DonationHub.Api.Endpoints;
using DonationHub.Api.Extensions;
using DonationHub.Application.Features.Categories.CreateCategory;
using DonationHub.Application.Features.Categories.DeleteCategory;
using DonationHub.Application.Features.Categories.GetCategories;
using DonationHub.Application.Features.Categories.GetCategoryById;
using DonationHub.Application.Features.Categories.UpdateCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ISender _mediator;

    public CategoriesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetAll(CancellationToken ct) =>
        (await _mediator.Send(new GetCategoriesQuery(), ct)).ToActionResult();

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IResult> GetById(Guid id, CancellationToken ct) =>
        (await _mediator.Send(new GetCategoryByIdQuery(id), ct)).ToActionResult();

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> Create([FromBody] CreateCategoryCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.ToActionResult(id => Results.Created($"/api/v1/categories/{id}", new { Id = id }));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var command = new UpdateCategoryCommand(id, request.NameAr, request.NameEn, request.DescriptionAr, request.DescriptionEn);
        var result = await _mediator.Send(command, ct);
        return result.ToActionResult(StatusCodes.Status204NoContent);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> Delete(Guid id, CancellationToken ct) =>
        (await _mediator.Send(new DeleteCategoryCommand(id), ct)).ToActionResult(StatusCodes.Status204NoContent);
}

