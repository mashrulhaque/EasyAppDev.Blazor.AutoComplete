using EasyAppDev.Blazor.AutoComplete.AI.RateLimiting;
using FluentAssertions;
using Xunit;

namespace EasyAppDev.Blazor.AutoComplete.Tests.Security;

/// <summary>
/// Tests for rate limiting functionality.
/// Verifies token bucket algorithm, concurrency, and graceful degradation.
/// </summary>
public class RateLimitTests
{
    [Fact]
    public async Task RateLimiter_WithinLimit_ShouldAllowRequests()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequestsPerMinute: 60);

        // Act & Assert - 60 requests should succeed
        for (int i = 0; i < 60; i++)
        {
            var result = await limiter.TryAcquireAsync();
            result.Should().BeTrue($"Request {i + 1} should succeed");
        }

        limiter.Dispose();
    }

    [Fact]
    public async Task RateLimiter_ExceedingLimit_ShouldRejectRequests()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequestsPerMinute: 10);

        // Act - consume all tokens
        for (int i = 0; i < 10; i++)
        {
            await limiter.TryAcquireAsync();
        }

        // Assert - 11th request should fail
        var result = await limiter.TryAcquireAsync();
        result.Should().BeFalse("11th request should be rejected");

        limiter.Dispose();
    }

    [Fact]
    public async Task RateLimiter_AfterRefill_ShouldAllowRequestsAgain()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequestsPerMinute: 60);

        // Consume all tokens
        for (int i = 0; i < 60; i++)
        {
            await limiter.TryAcquireAsync();
        }

        // Act - wait for refill (1 second = 1 token for 60 RPM)
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Assert - should have at least 1 token available (possibly 2)
        limiter.AvailableTokens.Should().BeGreaterOrEqualTo(1);

        var result = await limiter.TryAcquireAsync();
        result.Should().BeTrue("Request should succeed after refill");

        limiter.Dispose();
    }

    [Fact]
    public async Task RateLimiter_ConcurrentRequests_ShouldBeThreadSafe()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequestsPerMinute: 100);

        // Act - 200 concurrent requests
        var tasks = Enumerable.Range(0, 200)
            .Select(_ => limiter.TryAcquireAsync())
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert - exactly 100 should succeed
        results.Count(r => r).Should().Be(100, "Only 100 out of 200 concurrent requests should succeed");

        limiter.Dispose();
    }

    [Fact]
    public async Task RateLimiter_AcquireAsync_ShouldWaitForToken()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequestsPerMinute: 60);

        // Consume all tokens
        for (int i = 0; i < 60; i++)
        {
            await limiter.TryAcquireAsync();
        }

        // Act - AcquireAsync should wait for refill
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await limiter.AcquireAsync(timeout: TimeSpan.FromSeconds(2));
        sw.Stop();

        // Assert - should have waited approximately 1 second (1 token refill time for 60 RPM)
        sw.Elapsed.Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(900),
            "Should wait for token refill");
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1.5),
            "Should not wait much longer than refill time");

        limiter.Dispose();
    }

    [Fact]
    public async Task RateLimiter_AcquireAsync_ShouldThrowOnTimeout()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequestsPerMinute: 1); // Very slow refill

        // Consume the single token
        await limiter.TryAcquireAsync();

        // Act & Assert - should throw when timeout expires
        var act = async () => await limiter.AcquireAsync(timeout: TimeSpan.FromMilliseconds(100));

        await act.Should().ThrowAsync<RateLimitExceededException>()
            .WithMessage("*Rate limit*exceeded*");

        limiter.Dispose();
    }

    [Fact]
    public void RateLimiter_Constructor_ShouldRejectInvalidRate()
    {
        // Act & Assert
        var act = () => new RateLimiter(maxRequestsPerMinute: 0);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxRequestsPerMinute");

        var act2 = () => new RateLimiter(maxRequestsPerMinute: -1);
        act2.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxRequestsPerMinute");
    }

    [Fact]
    public async Task RateLimiter_AvailableTokens_ShouldReflectCurrentState()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequestsPerMinute: 10);

        // Act & Assert - initial state
        limiter.AvailableTokens.Should().Be(10, "Should start with full bucket");

        // Consume 5 tokens
        for (int i = 0; i < 5; i++)
        {
            await limiter.TryAcquireAsync();
        }

        limiter.AvailableTokens.Should().Be(5, "Should have 5 tokens remaining");

        limiter.Dispose();
    }

    [Fact]
    public void RateLimiter_GetNextRefillTime_ShouldReturnFutureTime()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequestsPerMinute: 60);

        // Act
        var nextRefill = limiter.GetNextRefillTime();

        // Assert
        nextRefill.Should().BeAfter(DateTime.UtcNow.AddMilliseconds(-100),
            "Next refill should be in the future or very recent past");

        limiter.Dispose();
    }

    [Fact]
    public async Task RateLimiter_Reset_ShouldRefillBucket()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequestsPerMinute: 10);

        // Consume all tokens
        for (int i = 0; i < 10; i++)
        {
            await limiter.TryAcquireAsync();
        }

        limiter.AvailableTokens.Should().Be(0, "Bucket should be empty");

        // Act
        limiter.Reset();

        // Assert
        limiter.AvailableTokens.Should().Be(10, "Bucket should be full after reset");

        limiter.Dispose();
    }

    [Fact]
    public void RateLimiter_Dispose_ShouldPreventFurtherUse()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequestsPerMinute: 60);
        limiter.Dispose();

        // Act & Assert
        var act = async () => await limiter.TryAcquireAsync();
        act.Should().ThrowAsync<ObjectDisposedException>();

        var act2 = () => limiter.AvailableTokens;
        act2.Should().Throw<ObjectDisposedException>();

        var act3 = () => limiter.Reset();
        act3.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public async Task RateLimiter_MultipleAcquire_ShouldHandleCancellation()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequestsPerMinute: 1);
        await limiter.TryAcquireAsync(); // Consume the token

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        var act = async () => await limiter.AcquireAsync(
            timeout: TimeSpan.FromSeconds(10),
            cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();

        limiter.Dispose();
    }

    [Fact]
    public async Task RateLimiter_StressTest_ShouldMaintainAccuracy()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequestsPerMinute: 100);
        var successCount = 0;
        var failureCount = 0;

        // Act - Run 500 requests across 10 parallel tasks
        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            for (int i = 0; i < 50; i++)
            {
                if (await limiter.TryAcquireAsync())
                {
                    Interlocked.Increment(ref successCount);
                }
                else
                {
                    Interlocked.Increment(ref failureCount);
                }
            }
        });

        await Task.WhenAll(tasks);

        // Assert
        successCount.Should().Be(100, "Exactly 100 requests should succeed");
        failureCount.Should().Be(400, "Exactly 400 requests should fail");

        limiter.Dispose();
    }
}
