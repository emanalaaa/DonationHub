namespace DonationHub.Api.Endpoints;

using DonationHub.Api.Extensions;
using DonationHub.Application.Features.Categories.CreateCategory;
using DonationHub.Application.Features.Categories.DeleteCategory;
using DonationHub.Application.Features.Categories.GetCategories;
using DonationHub.Application.Features.Categories.GetCategoryById;
using DonationHub.Application.Features.Categories.UpdateCategory;
using MediatR;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/categories")
            .WithTags("Categories");

        // GET /api/v1/categories (Anonymous)
        group.MapGet("/", async (ISender mediator, CancellationToken ct) =>
            (await mediator.Send(new GetCategoriesQuery(), ct)).ToHttpResult())
            .AllowAnonymous();

        // GET /api/v1/categories/{id} (Anonymous)
        group.MapGet("/{id:guid}", async (Guid id, ISender mediator, CancellationToken ct) =>
            (await mediator.Send(new GetCategoryByIdQuery(id), ct)).ToHttpResult())
            .AllowAnonymous();

        // POST /api/v1/categories (Require Role: "Admin")
        group.MapPost("/", async (CreateCategoryCommand command, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToHttpResult(id => Results.Created($"/api/v1/categories/{id}", new { Id = id }));
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // PUT /api/v1/categories/{id} (Require Role: "Admin")
        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, ISender mediator, CancellationToken ct) =>
        {
            var command = new UpdateCategoryCommand(id, request.NameAr, request.NameEn, request.DescriptionAr, request.DescriptionEn);
            var result = await mediator.Send(command, ct);
            return result.ToHttpResult(StatusCodes.Status204NoContent);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // DELETE /api/v1/categories/{id} (Require Role: "Admin")
        group.MapDelete("/{id:guid}", async (Guid id, ISender mediator, CancellationToken ct) =>
            (await mediator.Send(new DeleteCategoryCommand(id), ct)).ToHttpResult(StatusCodes.Status204NoContent))
        .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return group;
    }
}

public record UpdateCategoryRequest(
    string NameAr,
    string NameEn,
    string DescriptionAr = "",
    string DescriptionEn = "");
