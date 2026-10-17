namespace NasaSpaceApps.Application.Common;

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
