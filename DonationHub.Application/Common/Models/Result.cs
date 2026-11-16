namespace DonationHub.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public int StatusCode { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        StatusCode = 200;
    }

    private Result(string errorCode, string errorMessage, int statusCode)
    {
        IsSuccess = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string errorCode, string errorMessage, int statusCode) => new(errorCode, errorMessage, statusCode);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public int StatusCode { get; }

    private Result()
    {
        IsSuccess = true;
        StatusCode = 200;
    }

    private Result(string errorCode, string errorMessage, int statusCode)
    {
        IsSuccess = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static Result Success() => new();
    public static Result Failure(string errorCode, string errorMessage, int statusCode) => new(errorCode, errorMessage, statusCode);
}

