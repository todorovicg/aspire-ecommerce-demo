using System.ComponentModel.DataAnnotations;
using Ecommerce.ApiDefaults;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.Payment.Api.Features.Authorize;

public sealed record AuthorizeRequest(
    Guid OrderId,
    [property: Range(0.01, 1_000_000)] decimal Amount,
    [property: Required, StringLength(3, MinimumLength = 3)] string Currency,
    [property: Required, RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be 16 digits")] string CardNumber,
    [property: Required, StringLength(100, MinimumLength = 2)] string CardHolder);

public sealed record AuthorizeResponse(Guid PaymentId, string Status, string? Reason);

public static class AuthorizeEndpoint
{
    public const string Approved = "Approved";
    public const string Declined = "Declined";

    public static RouteGroupBuilder MapAuthorize(this RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync).WithName("AuthorizePayment");
        return group;
    }

    public static async Task<Ok<ApiResponse<AuthorizeResponse>>> HandleAsync(
        AuthorizeRequest request,
        PaymentService service,
        CancellationToken cancellationToken)
    {
        var result = await service.AuthorizeAsync(request.OrderId, request.Amount, request.Currency, request.CardNumber, cancellationToken);
        return ApiResults.Ok(new AuthorizeResponse(result.PaymentId, result.Approved ? Approved : Declined, result.Reason));
    }
}
