using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.ApiDefaults;

public static class ApiResults
{
    public static Ok<ApiResponse<T>> Ok<T>(T data) =>
        TypedResults.Ok(ApiResponse.Ok(data));

    public static Accepted<ApiResponse<T>> Accepted<T>(T data) =>
        TypedResults.Accepted((string?)null, ApiResponse.Ok(data));

    public static Ok<ApiResponse> Empty() =>
        TypedResults.Ok(ApiResponse.Empty());

    public static BadRequest<ApiResponse> BadRequest(string code, string message, object? details = null) =>
        TypedResults.BadRequest(ApiResponse.Fail(code, message, details));

    public static NotFound<ApiResponse> NotFound(string code, string message) =>
        TypedResults.NotFound(ApiResponse.Fail(code, message));

    public static Conflict<ApiResponse> Conflict(string code, string message, object? details = null) =>
        TypedResults.Conflict(ApiResponse.Fail(code, message, details));

    public static JsonHttpResult<ApiResponse> ServiceUnavailable(string code, string message) =>
        TypedResults.Json(ApiResponse.Fail(code, message), statusCode: StatusCodes.Status503ServiceUnavailable);
}
