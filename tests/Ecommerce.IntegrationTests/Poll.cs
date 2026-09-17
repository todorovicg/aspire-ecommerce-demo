namespace Ecommerce.IntegrationTests;

public static class Poll
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    // Returns the last observed value even when the condition never held, so assertions report what the system actually did
    public static async Task<T> UntilAsync<T>(Func<Task<T>> read, Func<T, bool> condition, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        var value = await read();

        while (!condition(value) && DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(Interval, cancellationToken);
            value = await read();
        }

        return value;
    }
}
