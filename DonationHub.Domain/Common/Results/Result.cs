namespace DonationHub.Domain.Common.Results;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public string? ErrorCode => Error?.Code;
    public string? ErrorMessage => Error?.Message;
    public int StatusCode => Error.Type switch
    {
        ErrorType.Validation => 400,
        ErrorType.Unauthorized => 401,
        ErrorType.Forbidden => 403,
        ErrorType.NotFound => 404,
        ErrorType.Conflict => 409,
        ErrorType.PayloadTooLarge => 413,
        ErrorType.UnprocessableEntity => 422,
        ErrorType.TooManyRequests => 429,
        _ => IsSuccess ? 200 : 500
    };

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("A success result cannot have an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("A failure result must have an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);

    public static Result Failure(string errorCode, string errorMessage, int statusCode = 400)
    {
        var errorType = statusCode switch
        {
            400 => ErrorType.Validation,
            401 => ErrorType.Unauthorized,
            403 => ErrorType.Forbidden,
            404 => ErrorType.NotFound,
            409 => ErrorType.Conflict,
            413 => ErrorType.PayloadTooLarge,
            422 => ErrorType.UnprocessableEntity,
            429 => ErrorType.TooManyRequests,
            _ => ErrorType.Failure
        };

        return new Result(false, new Error(errorCode, errorMessage, errorType));
    }

    public static implicit operator Result(Error error) => Failure(error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    protected internal Result(T value) : base(true, Error.None)
    {
        _value = value;
    }

    protected internal Result(Error error) : base(false, error)
    {
        _value = default;
    }

    public static Result<T> Success(T value) => new(value);
    public static new Result<T> Failure(Error error) => new(error);

    public static new Result<T> Failure(string errorCode, string errorMessage, int statusCode = 400)
    {
        var errorType = statusCode switch
        {
            400 => ErrorType.Validation,
            401 => ErrorType.Unauthorized,
            403 => ErrorType.Forbidden,
            404 => ErrorType.NotFound,
            409 => ErrorType.Conflict,
            413 => ErrorType.PayloadTooLarge,
            422 => ErrorType.UnprocessableEntity,
            429 => ErrorType.TooManyRequests,
            _ => ErrorType.Failure
        };

        return new Result<T>(new Error(errorCode, errorMessage, errorType));
    }

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
