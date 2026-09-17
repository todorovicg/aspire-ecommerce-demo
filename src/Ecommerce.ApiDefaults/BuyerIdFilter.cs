using Microsoft.AspNetCore.Http;

namespace Ecommerce.ApiDefaults;

// Answers 400 once for every endpoint in a group when the X-Buyer-Id header is missing or malformed
public sealed class BuyerIdFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var buyerId = context.Arguments.OfType<BuyerId>().FirstOrDefault();
        if (buyerId is { IsValid: false })
        {
            return ValueTask.FromResult<object?>(ApiResults.BadRequest(
                CommonErrorCodes.BuyerIdInvalid,
                $"Header [{BuyerId.HeaderName}] must carry a valid buyer id"));
        }

        return next(context);
    }
}
