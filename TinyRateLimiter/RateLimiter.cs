using System.Collections.Concurrent;

namespace TinyRateLimiter;

public class RateLimiter
{
    private readonly int _limit;
    private readonly TimeSpan _window;

    // Handles dynamic key addition cleanly
    private readonly ConcurrentDictionary<string, ClientState> _clientStates = new();

    public RateLimiter(int limit, TimeSpan window)
    {
        _limit = limit;
        _window = window;
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
            // Check window expiration
            if (DateTime.UtcNow >= clientState.WindowStart.Add(_window))
            {
                clientState.RequestCount = 0;
                clientState.WindowStart = DateTime.UtcNow;
            }

            if (clientState.RequestCount >= _limit)
            {
                return false;
            }

            // SIMULATED RACE CONDITION GAP
            // Multiple threads read RequestCount = 4 before any thread increments it
            Thread.Sleep(1);

            clientState.RequestCount++; // Modifies the object directly in memory

            return true;
        }
    }
}
