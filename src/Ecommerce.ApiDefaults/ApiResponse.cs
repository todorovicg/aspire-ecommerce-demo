using System.Diagnostics;

namespace Ecommerce.ApiDefaults;

public sealed record ApiError(string Code, string Message, object? Details, string? TraceId);

public sealed record ApiResponse<T>(bool Success, T? Data, ApiError? Error);

public sealed record ApiResponse(bool Success, object? Data, ApiError? Error)
{
    public static ApiResponse<T> Ok<T>(T data) => new(true, data, null);

    public static ApiResponse Empty() => new(true, null, null);

    public static ApiResponse Fail(string code, string message, object? details = null) =>
        new(false, null, new ApiError(code, message, details, Activity.Current?.Id));
}
