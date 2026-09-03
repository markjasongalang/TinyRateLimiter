using System.Collections.Concurrent;

namespace TinyRateLimiter;

public class SlidingWindowRateLimiter : IRateLimiter
{
    private readonly int _limit;
    private readonly TimeSpan _window;
    private readonly TimeProvider _timeProvider;

    // Handles dynamic key addition cleanly
    private readonly ConcurrentDictionary<string, ClientState> _clientStates = new();

    public SlidingWindowRateLimiter(int limit, TimeSpan window, TimeProvider? timeProvider = null)
    {
        _limit = limit;
        _window = window;
        _timeProvider = timeProvider ?? TimeProvider.System; 
    }

    public bool AllowRequest(string clientId)
    {
        // Get existing client state or create a new one on demand
        var clientState = _clientStates.GetOrAdd(clientId, _ => new ClientState());

        // Lock only this client's state to ensure atomic checks and updates.
        /*
         * Why Fine-Grained Locking Works Best Here
         * Isolation: Request processing for user-123 locks user-123's state only. user-456 can make requests at the exact same time without waiting.
         * Low Overhead: In C#, lock (monitors) on an uncontended object takes only a few nanoseconds, making it extremely fast for in-memory checks.
         */
        lock (clientState.Lock)
        {
            DateTimeOffset now = _timeProvider.GetUtcNow();
            var windowStart = now.Subtract(_window);

            // Evict timestamps outside the rolling window 
            while (clientState.RequestTimestamps.Count > 0 &&
                   clientState.RequestTimestamps.Peek() <= windowStart) // (starting at the oldest timestamp)
            {
                clientState.RequestTimestamps.Dequeue();
            }

            // Reject if window capacity is reached
            if (clientState.RequestTimestamps.Count >= _limit)
            {
                return false;
            }

            clientState.RequestTimestamps.Enqueue(now);
            return true;
        }
    }
}
