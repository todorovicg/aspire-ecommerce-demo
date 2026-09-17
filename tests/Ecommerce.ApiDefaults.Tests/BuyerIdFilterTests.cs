using Ecommerce.ApiDefaults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.ApiDefaults.Tests;

public sealed class BuyerIdFilterTests
{
    [Fact]
    public async Task InvokeAsync_WithInvalidBuyerId_ReturnsBadRequestEnvelope()
    {
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), BuyerId.Invalid);
        var filter = new BuyerIdFilter();

        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>("next"));

        var badRequest = Assert.IsType<BadRequest<ApiResponse>>(result);
        Assert.NotNull(badRequest.Value?.Error);
        Assert.Equal("common.buyer_id_invalid", badRequest.Value.Error.Code);
    }

    [Fact]
    public async Task InvokeAsync_WithValidBuyerId_CallsNext()
    {
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), new BuyerId(Guid.NewGuid()));
        var filter = new BuyerIdFilter();

        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>("next"));

        Assert.Equal("next", result);
    }
}
