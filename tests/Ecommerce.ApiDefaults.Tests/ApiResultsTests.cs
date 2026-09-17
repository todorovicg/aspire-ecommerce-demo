using Ecommerce.ApiDefaults;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.ApiDefaults.Tests;

public sealed class ApiResultsTests
{
    [Fact]
    public void Ok_WrapsDataInSuccessEnvelope()
    {
        var result = ApiResults.Ok(42);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.Success);
        Assert.Equal(42, result.Value.Data);
    }

    [Fact]
    public void Accepted_WrapsDataInSuccessEnvelope()
    {
        var result = ApiResults.Accepted("order-1");

        Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.Success);
        Assert.Equal("order-1", result.Value.Data);
    }

    [Fact]
    public void Empty_IsOkWithNullData()
    {
        var result = ApiResults.Empty();

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.Success);
        Assert.Null(result.Value.Data);
    }

    [Fact]
    public void BadRequest_CarriesCodeMessageAndDetails()
    {
        var details = new { Field = "Name" };

        var result = ApiResults.BadRequest("common.buyer_id_invalid", "Header [X-Buyer-Id] is missing", details);

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.NotNull(result.Value?.Error);
        Assert.Equal("common.buyer_id_invalid", result.Value.Error.Code);
        Assert.Equal("Header [X-Buyer-Id] is missing", result.Value.Error.Message);
        Assert.Same(details, result.Value.Error.Details);
    }

    [Fact]
    public void NotFound_CarriesCodeAndMessage()
    {
        var result = ApiResults.NotFound("catalog.product_not_found", "Product [1] not found");

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.NotNull(result.Value?.Error);
        Assert.False(result.Value.Success);
        Assert.Equal("catalog.product_not_found", result.Value.Error.Code);
    }

    [Fact]
    public void Conflict_CarriesCodeMessageAndDetails()
    {
        var details = new[] { "a", "b" };

        var result = ApiResults.Conflict("basket.product_out_of_stock", "Product [Mug] has [0] units available", details);

        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.NotNull(result.Value?.Error);
        Assert.Equal("basket.product_out_of_stock", result.Value.Error.Code);
        Assert.Same(details, result.Value.Error.Details);
    }

    [Fact]
    public void ServiceUnavailable_Uses503AndFailureEnvelope()
    {
        var result = ApiResults.ServiceUnavailable("basket.broker_unavailable", "Message broker is unavailable");

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, result.StatusCode);
        Assert.NotNull(result.Value?.Error);
        Assert.False(result.Value.Success);
        Assert.Equal("basket.broker_unavailable", result.Value.Error.Code);
    }
}
