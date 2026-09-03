namespace TinyRateLimiter;

// Reference type (class)
public class ClientState
{
    // Fine-grained lock object per client
    public object Lock { get; } = new();

    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
}
