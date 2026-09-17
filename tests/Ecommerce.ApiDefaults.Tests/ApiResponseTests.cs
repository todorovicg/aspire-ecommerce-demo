using System.Diagnostics;
using Ecommerce.ApiDefaults;

namespace Ecommerce.ApiDefaults.Tests;

public sealed class ApiResponseTests
{
    [Fact]
    public void Ok_WithData_SetsSuccessTrueAndNoError()
    {
        var response = ApiResponse.Ok(new { Name = "Mug" });

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Null(response.Error);
    }

    [Fact]
    public void Empty_SetsSuccessTrueWithNullData()
    {
        var response = ApiResponse.Empty();

        Assert.True(response.Success);
        Assert.Null(response.Data);
        Assert.Null(response.Error);
    }

    [Fact]
    public void Fail_SetsSuccessFalseAndError()
    {
        var response = ApiResponse.Fail("catalog.product_not_found", "Product [1] not found");

        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.NotNull(response.Error);
        Assert.Equal("catalog.product_not_found", response.Error.Code);
        Assert.Equal("Product [1] not found", response.Error.Message);
        Assert.Null(response.Error.Details);
    }

    [Fact]
    public void Fail_WithDetails_KeepsDetails()
    {
        var details = new Dictionary<string, string[]> { ["Name"] = ["Required"] };

        var response = ApiResponse.Fail("common.validation_failed", "Request validation failed", details);

        Assert.NotNull(response.Error);
        Assert.Same(details, response.Error.Details);
    }

    [Fact]
    public void Fail_WithCurrentActivity_SetsTraceIdFromActivity()
    {
        using var source = new ActivitySource("Ecommerce.ApiDefaults.Tests");
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);
        using var activity = source.StartActivity("test");

        var response = ApiResponse.Fail("common.error", "Failed");

        Assert.NotNull(activity);
        Assert.NotNull(response.Error);
        Assert.Equal(activity.Id, response.Error.TraceId);
    }

    [Fact]
    public void Fail_WithoutActivity_LeavesTraceIdNull()
    {
        Activity.Current = null;

        var response = ApiResponse.Fail("common.error", "Failed");

        Assert.NotNull(response.Error);
        Assert.Null(response.Error.TraceId);
    }
}
