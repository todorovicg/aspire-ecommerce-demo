using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Ordering.Api.Processing;

public sealed record OrderProcessorOptions
{
    public const string SectionName = "Ordering:Processor";

    [Range(typeof(TimeSpan), "00:00:00.100", "00:01:00")]
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromSeconds(1);

    [Range(typeof(TimeSpan), "00:00:01", "00:10:00")]
    public TimeSpan LeaseDuration { get; init; } = TimeSpan.FromSeconds(30);

    [Range(typeof(TimeSpan), "00:00:00", "00:10:00")]
    public TimeSpan PaidToShippedDelay { get; init; } = TimeSpan.FromSeconds(5);

    [Range(typeof(TimeSpan), "00:00:01", "00:10:00")]
    public TimeSpan PaymentRetryDelay { get; init; } = TimeSpan.FromSeconds(5);
}
