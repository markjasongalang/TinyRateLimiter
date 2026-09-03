
using Microsoft.Extensions.Time.Testing;
using TinyRateLimiter;

public class Program
{
    public static async Task  Main(string[] args)
    {
        Console.WriteLine("TinyRateLimiter\n");

        var limiter = new SlidingWindowRateLimiter(
            limit: 5,
            window: TimeSpan.FromSeconds(10));

        // Demo 1: Thread-Safety under high-concurrency
        Console.WriteLine("[1] Testing concurency (100 parallel tasks for 'user-123')...");

        var allowedCount = 0;
        var rejectedCount = 0;

        // Run 100 concurrent tasks against the same client.
        // Task.Run() schedules work to the ThreadPool; it doesn't guarantee 100 threads simultaneously
        // entering AllowRequest().
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

        Console.WriteLine($"Allowed: {allowedCount} (Expected strictly: 5)");
        Console.WriteLine($"Rejected: {rejectedCount} (Expected: 95)");

        // Demo 2: Sliding Window Boundary Protection
        Console.WriteLine("\n[2] Testing Rolling Window with Fake Time...");

        var fakeTime = new FakeTimeProvider();
        var testableLimiter = new SlidingWindowRateLimiter(
            limit: 5,
            window: TimeSpan.FromSeconds(10),
            timeProvider: fakeTime);

        // Fill capacity at t = 0s
        for (var i = 1; i <= 5; i++)
        {
            testableLimiter.AllowRequest("user-456");
        }

        Console.WriteLine($"At t=0s, request #6 allowed? {testableLimiter.AllowRequest("user-456")}"); // False

        // Advance fake clock by 6 seconds (window hasn't fully cleared)
        fakeTime.Advance(TimeSpan.FromSeconds(6));
        Console.WriteLine($"At t=6s, request #7 allowed? {testableLimiter.AllowRequest("user-456")}"); // False

        // Advance fake clock past the 10s window
        fakeTime.Advance(TimeSpan.FromSeconds(5)); // Total 11s elapsed
        Console.WriteLine($"At t=11s, request #8 allowed? {testableLimiter.AllowRequest("user-456")}"); // True

        // Demo 3: Multiple Clients
        Console.WriteLine("\n[3] Testing Multiple Clients...");

        for (var i = 0; i < 5; i++)
        {
            limiter.AllowRequest("alice");
        }

        Console.WriteLine($"Alice (Request #6 - Over limit): {limiter.AllowRequest("alice")}");
        Console.WriteLine($"Bob   (Request #1 - Fresh start): {limiter.AllowRequest("bob")}");
    }
}
