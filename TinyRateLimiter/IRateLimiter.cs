namespace TinyRateLimiter;

public interface IRateLimiter
{
    bool AllowRequest(string clientId);
}
