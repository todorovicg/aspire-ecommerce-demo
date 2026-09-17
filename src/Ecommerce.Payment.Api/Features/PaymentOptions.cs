using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Payment.Api.Features;

public sealed record PaymentOptions
{
    public const string SectionName = "Payment";

    // Simulated processing time so the payment step is visible in traces and on the orders page
    [Range(typeof(TimeSpan), "00:00:00", "00:01:00")]
    public TimeSpan ProcessingDelay { get; init; } = TimeSpan.FromMilliseconds(1500);
}
