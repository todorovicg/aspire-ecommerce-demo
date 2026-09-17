using System.Text.Json;
using Ecommerce.ApiDefaults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.ApiDefaults.Tests;

public sealed class ApiEnvelopeProblemDetailsWriterTests
{
    [Fact]
    public void CanWrite_AlwaysReturnsTrue()
    {
        var writer = new ApiEnvelopeProblemDetailsWriter();
        var context = new ProblemDetailsContext
        {
            HttpContext = new DefaultHttpContext(),
            ProblemDetails = new ProblemDetails { Status = 418 },
        };

        Assert.True(writer.CanWrite(context));
    }

    [Fact]
    public async Task WriteAsync_WithValidationProblem_WritesValidationFailedEnvelopeWithFieldErrors()
    {
        var errors = new Dictionary<string, string[]> { ["Name"] = ["The Name field is required."] };
        var problem = new HttpValidationProblemDetails(errors) { Status = StatusCodes.Status400BadRequest };

        var (status, root) = await WriteAsync(problem);

        Assert.Equal(400, status);
        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
        var error = root.GetProperty("error");
        Assert.Equal("common.validation_failed", error.GetProperty("code").GetString());
        Assert.Equal("Request validation failed", error.GetProperty("message").GetString());
        Assert.Equal("The Name field is required.", error.GetProperty("details").GetProperty("Name")[0].GetString());
        Assert.False(string.IsNullOrEmpty(error.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task WriteAsync_WithPlain400_WritesBadRequestEnvelope()
    {
        var problem = new ProblemDetails { Status = StatusCodes.Status400BadRequest };

        var (status, root) = await WriteAsync(problem);

        Assert.Equal(400, status);
        Assert.Equal("common.bad_request", root.GetProperty("error").GetProperty("code").GetString());
        Assert.Equal("The request could not be read", root.GetProperty("error").GetProperty("message").GetString());
    }

    [Fact]
    public async Task WriteAsync_With404_WritesNotFoundEnvelopeNamingThePath()
    {
        var problem = new ProblemDetails { Status = StatusCodes.Status404NotFound };

        var (status, root) = await WriteAsync(problem, "/api/nope");

        Assert.Equal(404, status);
        Assert.Equal("common.not_found", root.GetProperty("error").GetProperty("code").GetString());
        Assert.Equal("No endpoint matches [/api/nope]", root.GetProperty("error").GetProperty("message").GetString());
    }

    [Fact]
    public async Task WriteAsync_With500_WritesInternalErrorEnvelope()
    {
        var problem = new ProblemDetails { Status = StatusCodes.Status500InternalServerError };

        var (status, root) = await WriteAsync(problem);

        Assert.Equal(500, status);
        Assert.Equal("common.internal_error", root.GetProperty("error").GetProperty("code").GetString());
        Assert.Equal("An unexpected error occurred", root.GetProperty("error").GetProperty("message").GetString());
    }

    [Theory]
    [InlineData(502)]
    [InlineData(503)]
    [InlineData(504)]
    public async Task WriteAsync_WithGatewayStatus_WritesUpstreamUnavailableEnvelope(int gatewayStatus)
    {
        var problem = new ProblemDetails { Status = gatewayStatus };

        var (status, root) = await WriteAsync(problem);

        Assert.Equal(gatewayStatus, status);
        Assert.Equal("common.upstream_unavailable", root.GetProperty("error").GetProperty("code").GetString());
        Assert.Equal("An upstream service is unavailable", root.GetProperty("error").GetProperty("message").GetString());
    }

    [Fact]
    public async Task WriteAsync_WithoutStatus_UsesResponseStatusCode()
    {
        var problem = new ProblemDetails();

        var (status, root) = await WriteAsync(problem, responseStatus: StatusCodes.Status405MethodNotAllowed);

        Assert.Equal(405, status);
        Assert.Equal("common.method_not_allowed", root.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task WriteAsync_WithUnmappedStatus_UsesGenericCodeAndTrimsTitle()
    {
        var problem = new ProblemDetails { Status = 418, Title = "I am a teapot." };

        var (status, root) = await WriteAsync(problem);

        Assert.Equal(418, status);
        Assert.Equal("common.http_418", root.GetProperty("error").GetProperty("code").GetString());
        Assert.Equal("I am a teapot", root.GetProperty("error").GetProperty("message").GetString());
    }

    [Fact]
    public async Task WriteAsync_WithCodeExtension_PrefersThatCode()
    {
        var problem = new ProblemDetails { Status = 409, Detail = "Basket is empty" };
        problem.Extensions["code"] = "basket.checkout_empty";

        var (status, root) = await WriteAsync(problem);

        Assert.Equal(409, status);
        Assert.Equal("basket.checkout_empty", root.GetProperty("error").GetProperty("code").GetString());
        Assert.Equal("Basket is empty", root.GetProperty("error").GetProperty("message").GetString());
    }

    private static async Task<(int Status, JsonElement Root)> WriteAsync(
        ProblemDetails problem,
        string path = "/api/test",
        int responseStatus = StatusCodes.Status200OK)
    {
        var http = new DefaultHttpContext();
        http.Request.Path = path;
        http.Response.StatusCode = responseStatus;
        http.Response.Body = new MemoryStream();
        var writer = new ApiEnvelopeProblemDetailsWriter();

        await writer.WriteAsync(new ProblemDetailsContext { HttpContext = http, ProblemDetails = problem });

        http.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(
            http.Response.Body,
            cancellationToken: TestContext.Current.CancellationToken);
        return (http.Response.StatusCode, document.RootElement.Clone());
    }
}
