using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.ApiDefaults;

// Converts every ProblemDetails the framework produces into the API response envelope
public sealed class ApiEnvelopeProblemDetailsWriter : IProblemDetailsWriter
{
    public bool CanWrite(ProblemDetailsContext context) => true;

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        var http = context.HttpContext;
        var problem = context.ProblemDetails;
        var status = problem.Status ?? http.Response.StatusCode;
        http.Response.StatusCode = status;

        var code = ResolveCode(problem, status);
        var message = ResolveMessage(problem, status, http.Request.Path);
        var details = problem is HttpValidationProblemDetails validation ? validation.Errors : null;
        var traceId = Activity.Current?.Id ?? http.TraceIdentifier;
        var envelope = new ApiResponse(false, null, new ApiError(code, message, details, traceId));

        return new ValueTask(http.Response.WriteAsJsonAsync(envelope, http.RequestAborted));
    }

    private static string ResolveCode(ProblemDetails problem, int status)
    {
        if (problem.Extensions.TryGetValue("code", out var value) && value is string code)
        {
            return code;
        }

        return status switch
        {
            StatusCodes.Status400BadRequest when problem is HttpValidationProblemDetails => CommonErrorCodes.ValidationFailed,
            StatusCodes.Status400BadRequest => CommonErrorCodes.BadRequest,
            StatusCodes.Status404NotFound => CommonErrorCodes.NotFound,
            StatusCodes.Status405MethodNotAllowed => CommonErrorCodes.MethodNotAllowed,
            StatusCodes.Status415UnsupportedMediaType => CommonErrorCodes.UnsupportedMediaType,
            StatusCodes.Status500InternalServerError => CommonErrorCodes.InternalError,
            StatusCodes.Status502BadGateway or StatusCodes.Status503ServiceUnavailable or StatusCodes.Status504GatewayTimeout => CommonErrorCodes.UpstreamUnavailable,
            _ => $"common.http_{status}",
        };
    }

    private static string ResolveMessage(ProblemDetails problem, int status, PathString path)
    {
        if (!string.IsNullOrWhiteSpace(problem.Detail))
        {
            return TrimFullStop(problem.Detail);
        }

        return status switch
        {
            StatusCodes.Status400BadRequest when problem is HttpValidationProblemDetails => "Request validation failed",
            StatusCodes.Status400BadRequest => "The request could not be read",
            StatusCodes.Status404NotFound => $"No endpoint matches [{path}]",
            StatusCodes.Status500InternalServerError => "An unexpected error occurred",
            StatusCodes.Status502BadGateway or StatusCodes.Status503ServiceUnavailable or StatusCodes.Status504GatewayTimeout => "An upstream service is unavailable",
            _ => TrimFullStop(problem.Title ?? $"Request failed with status [{status}]"),
        };
    }

    private static string TrimFullStop(string text) => text.TrimEnd('.');
}
