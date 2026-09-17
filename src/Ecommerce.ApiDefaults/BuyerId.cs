using Microsoft.AspNetCore.Http;

namespace Ecommerce.ApiDefaults;

// Bound from the X-Buyer-Id header; an invalid or missing header yields Invalid so BuyerIdFilter can answer 400
public sealed record BuyerId(Guid Value)
{
    public const string HeaderName = "X-Buyer-Id";

    public static readonly BuyerId Invalid = new(Guid.Empty);

    public bool IsValid => Value != Guid.Empty;

    public static ValueTask<BuyerId?> BindAsync(HttpContext context)
    {
        var header = context.Request.Headers[HeaderName].ToString();
        var buyerId = Guid.TryParse(header, out var value) && value != Guid.Empty ? new BuyerId(value) : Invalid;
        return ValueTask.FromResult<BuyerId?>(buyerId);
    }
}
