namespace DonationHub.Api.Extensions;

using System.Diagnostics;
using DonationHub.Domain.Common.Results;
using Microsoft.AspNetCore.Http;

public static class ResultExtensions
{
    public static IResult ToHttpResult(
        this Result result,
        int successStatusCode = StatusCodes.Status200OK,
        HttpContext? httpContext = null)
    {
        if (result.IsSuccess)
        {
            return successStatusCode == StatusCodes.Status204NoContent
                ? Results.NoContent()
                : Results.StatusCode(successStatusCode);
        }

        return CreateProblemDetails(result.Error, httpContext);
    }

    public static IResult ToHttpResult<T>(
        this Result<T> result,
        Func<T, IResult>? onSuccess = null,
        HttpContext? httpContext = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess is not null
                ? onSuccess(result.Value)
                : Results.Ok(result.Value);
        }

        return CreateProblemDetails(result.Error, httpContext);
    }

    public static IResult ToActionResult(
        this Result result,
        int successStatusCode = StatusCodes.Status200OK,
        HttpContext? httpContext = null) =>
        result.ToHttpResult(successStatusCode, httpContext);

    public static IResult ToActionResult<T>(
        this Result<T> result,
        Func<T, IResult>? onSuccess = null,
        HttpContext? httpContext = null) =>
        result.ToHttpResult(onSuccess, httpContext);

    private static IResult CreateProblemDetails(Error error, HttpContext? httpContext)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.PayloadTooLarge => StatusCodes.Status413PayloadTooLarge,
            ErrorType.UnprocessableEntity => StatusCodes.Status422UnprocessableEntity,
            ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        var type = $"https://httpstatuses.io/{statusCode}";
        var title = error.Message;

        var extensions = new Dictionary<string, object?>
        {
            ["code"] = error.Code
        };

        var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
        if (!string.IsNullOrEmpty(traceId))
        {
            extensions["traceId"] = traceId;
        }

        if (error.Type == ErrorType.Validation && error.ValidationErrors is not null && error.ValidationErrors.Count > 0)
        {
            extensions["errors"] = error.ValidationErrors;
        }

        return Results.Problem(
            statusCode: statusCode,
            title: title,
            type: type,
            extensions: extensions);
    }
}
