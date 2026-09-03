# TinyRateLimiter

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)

A lightweight, in-memory per-client sliding window rate limiter written in C#. Ships as a console application with three built-in demos that showcase concurrency safety, time-based window boundaries, and multi-client isolation.

## Features

- Sliding Window Log algorithm for precise per-request timestamp tracking
- Per-client rate limiting with isolated state and independent quotas
- Fine-grained locking that only contends within the same client
- TimeProvider abstraction enabling deterministic testing without real clock delays
- Thread-safe by design using ConcurrentDictionary and per-client locks
- Zero NuGet dependencies in the core rate limiter logic

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later

### Run

```bash
git clone https://github.com/your-username/TinyRateLimiter.git
cd TinyRateLimiter
dotnet run --project TinyRateLimiter
```

## Project Structure

```
TinyRateLimiter/
├── TinyRateLimiter.slnx
├── LICENSE.txt
└── TinyRateLimiter/
    ├── IRateLimiter.cs                   # Rate limiter contract
    ├── ClientState.cs                    # Per-client state (lock + timestamp queue)
    ├── SlidingWindowRateLimiter.cs       # Core sliding window implementation
    ├── Program.cs                        # Entry point with three demos
    └── TinyRateLimiter.csproj
```

## How It Works

The rate limiter uses the Sliding Window Log algorithm. Each client gets a queue of request timestamps. When a new request arrives:

1. The system computes the window boundary by subtracting the configured duration from the current time.
2. Expired timestamps older than the window boundary are evicted from the front of the queue.
3. If the remaining count is at or above the limit, the request is rejected.
4. Otherwise, the current timestamp is enqueued and the request is allowed.

All of this happens under a per-client lock, so different clients never block each other.

### Usage

```csharp
using TinyRateLimiter;

var limiter = new SlidingWindowRateLimiter(
    limit: 5,
    window: TimeSpan.FromSeconds(10));

if (limiter.AllowRequest("user-123"))
{
    // Process the request
}
else
{
    // Rate limit exceeded
}
```

For deterministic testing, inject a `FakeTimeProvider`:

```csharp
using Microsoft.Extensions.Time.Testing;

var fakeTime = new FakeTimeProvider();
var limiter = new SlidingWindowRateLimiter(
    limit: 5,
    window: TimeSpan.FromSeconds(10),
    timeProvider: fakeTime);

// Use limiter, then advance the clock
fakeTime.Advance(TimeSpan.FromSeconds(11));
```

## Demos

**Demo 1 - Concurrency:** Fires 100 parallel tasks against a single client with a limit of 5. Validates that exactly 5 are allowed and 95 are rejected under concurrent load.

**Demo 2 - Sliding Window Boundaries:** Uses a FakeTimeProvider to advance the clock in controlled steps, proving that capacity only resets after the full window has elapsed.

**Demo 3 - Multi-Client Isolation:** Sends requests from two different clients to confirm that one client reaching its limit does not affect the other.

## Architecture Sequence Diagram

![TinyRateLimiter Architecture](https://www.plantuml.com/plantuml/proxy?src=https://raw.githubusercontent.com/markjasongalang/TinyRateLimiter/master/docs/architecture.puml&fmt=svg)

## License

This project is licensed under the [MIT License](LICENSE.txt).
