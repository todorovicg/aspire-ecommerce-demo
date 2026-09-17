using Ecommerce.ApiDefaults;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.ApiDefaults.Tests;

public sealed class BuyerIdTests
{
    [Fact]
    public async Task BindAsync_WithValidHeader_ReturnsBuyerId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[BuyerId.HeaderName] = "b0000000-0000-4000-8000-000000000001";

        var buyerId = await BuyerId.BindAsync(context);

        Assert.NotNull(buyerId);
        Assert.True(buyerId.IsValid);
        Assert.Equal(Guid.Parse("b0000000-0000-4000-8000-000000000001"), buyerId.Value);
    }

    [Fact]
    public async Task BindAsync_WithoutHeader_ReturnsInvalid()
    {
        var buyerId = await BuyerId.BindAsync(new DefaultHttpContext());

        Assert.NotNull(buyerId);
        Assert.False(buyerId.IsValid);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task BindAsync_WithMalformedOrEmptyHeader_ReturnsInvalid(string header)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[BuyerId.HeaderName] = header;

        var buyerId = await BuyerId.BindAsync(context);

        Assert.NotNull(buyerId);
        Assert.False(buyerId.IsValid);
    }
}
