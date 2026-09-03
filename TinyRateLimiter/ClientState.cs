namespace TinyRateLimiter;

// Reference type (class)
internal class ClientState
{
    // Fine-grained lock object per client
    internal object Lock { get; } = new();

    // Sliding Window Log: tracks request timestamps within the active window
    internal Queue<DateTimeOffset> RequestTimestamps { get; } = new();
}
