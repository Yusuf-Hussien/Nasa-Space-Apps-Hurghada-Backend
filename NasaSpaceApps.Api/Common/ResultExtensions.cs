using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Application.Common;

namespace NasaSpaceApps.Api.Common;

/// <summary>
/// Maps an application <see cref="Result{T}"/> onto an HTTP response.
/// Every controller action returns through here, so the error contract
/// (<c>{ "error": { "code", "message" } }</c>) stays identical across the API.
/// </summary>
public static class ResultExtensions
{
    /// <summary>Returns the value as-is when the result succeeded.</summary>
    public static ActionResult<TResponse> ToHttpResult<TResponse>(
        this Result<TResponse> result, ControllerBase controller)
    {
        return result.IsSuccess
            ? controller.Ok(result.Value)
            : ToError<TResponse>(controller, result.Error!);
    }

    /// <summary>Returns the value mapped through <paramref name="map"/> when the result succeeded.
    /// Use this whenever the service returns a domain entity that must be reshaped
    /// (or had sensitive fields stripped) before it is serialized.</summary>
    public static ActionResult<TResponse> ToHttpResult<TValue, TResponse>(
        this Result<TValue> result, ControllerBase controller, Func<TValue, TResponse> map)
    {
        return result.IsSuccess
            ? controller.Ok(map(result.Value!))
            : ToError<TResponse>(controller, result.Error!);
    }

    private static ActionResult<TResponse> ToError<TResponse>(ControllerBase controller, AppError error) =>
        controller.StatusCode(
            error.HttpStatus,
            new { error = new { code = error.Code, message = error.Message } });
}
