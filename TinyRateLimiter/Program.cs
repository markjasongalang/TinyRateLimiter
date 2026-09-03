
using TinyRateLimiter;

public class Program
{
    public static void Main(string[] args)
    {
        var limiter = new RateLimiter(5);

        for (var i = 1; i <= 10; i++)
        {
            bool allowed = limiter.AllowRequest();

            Console.WriteLine(
                $"Request {i}: {(allowed ? "Allowed" : "Rejected")}");
        }
    }
}
