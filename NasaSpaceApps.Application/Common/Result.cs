namespace NasaSpaceApps.Application.Common;

public sealed record AppError(string Code, int HttpStatus, string Message);

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public AppError? Error { get; }

    private Result(bool isSuccess, T? value, AppError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(string code, int httpStatus, string message) =>
        new(false, default, new AppError(code, httpStatus, message));
}

public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public interface ICurrentUser
{
    int? TeamId { get; }
    string? Username { get; }
    string? Role { get; }
}
