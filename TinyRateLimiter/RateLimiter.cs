namespace TinyRateLimiter;

public class RateLimiter
{
    private readonly int _limit;
    private int _requestCount;

    public RateLimiter(int limit)
    {
        _limit = limit;
    }

    public bool AllowRequest()
    {
        if (_requestCount >= _limit)
        {
            return false;
        }

        _requestCount++;

        return true;
    }
}
