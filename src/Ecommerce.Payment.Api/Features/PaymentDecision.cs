namespace Ecommerce.Payment.Api.Features;

// Test card rule: any number ending in 0002 is declined, everything else is approved
public static class PaymentDecision
{
    public const string DeclinedSuffix = "0002";
    public const string DeclinedReason = "card_declined";

    public static bool IsDeclined(string cardNumber) => cardNumber.EndsWith(DeclinedSuffix, StringComparison.Ordinal);
}
