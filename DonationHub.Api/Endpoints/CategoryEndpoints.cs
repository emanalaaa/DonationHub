namespace DonationHub.Api.Endpoints;

using DonationHub.Application.Features.Categories.Create;
using DonationHub.Application.Features.Categories.Delete;
using DonationHub.Application.Features.Categories.GetAll;
using DonationHub.Application.Features.Categories.GetById;
using DonationHub.Application.Features.Categories.Update;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/categories")
            .WithTags("Categories");

        // GET /api/v1/categories (Anonymous)
        group.MapGet("/", async (GetAllCategoriesHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetAllCategoriesQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        })
        .AllowAnonymous();

        // GET /api/v1/categories/{id} (Anonymous)
        group.MapGet("/{id:guid}", async (Guid id, GetCategoryByIdHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetCategoryByIdQuery(id), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        })
        .AllowAnonymous();

        // POST /api/v1/categories (Require Role: "Admin")
        group.MapPost("/", async (CreateCategoryCommand command, CreateCategoryHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/categories/{result.Value}", new { Id = result.Value })
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // PUT /api/v1/categories/{id} (Require Role: "Admin")
        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, UpdateCategoryHandler handler, CancellationToken ct) =>
        {
            var command = new UpdateCategoryCommand(id, request.NameAr, request.NameEn, request.DescriptionAr, request.DescriptionEn);
            var result = await handler.HandleAsync(command, ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // DELETE /api/v1/categories/{id} (Require Role: "Admin")
        group.MapDelete("/{id:guid}", async (Guid id, DeleteCategoryHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new DeleteCategoryCommand(id), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: result.StatusCode, title: result.ErrorCode, detail: result.ErrorMessage);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return group;
    }
}

public record UpdateCategoryRequest(
    string NameAr,
    string NameEn,
    string DescriptionAr = "",
    string DescriptionEn = "");

