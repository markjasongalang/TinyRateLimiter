
using TinyRateLimiter;

public class Program
{
    public static async Task  Main(string[] args)
    {
        var limiter = new RateLimiter(
            limit: 5,
            window: TimeSpan.FromSeconds(10));

        Console.WriteLine("Firing 100 parallel requests for 'user-123'...");

        var allowedCount = 0;
        var rejectedCount = 0;

        // Force 100 threads to run AllowRequest simultaneously
        IEnumerable<Task> tasks = Enumerable.Range(1, 100).Select(_ => Task.Run(() =>
        {
            if (limiter.AllowRequest("user-123"))
            {
                Interlocked.Increment(ref allowedCount);
            }
            else
            {
                Interlocked.Increment(ref rejectedCount);
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        Console.WriteLine("\n--- Results ---");
        Console.WriteLine($"Allowed: {allowedCount} (Expected: 5)");
        Console.WriteLine($"Rejected: {rejectedCount} (Expected: 95)");
    }
}
