// Copyright (c) EasyAppDev. All rights reserved.
// Licensed under the MIT License.

namespace EasyAppDev.Blazor.AutoComplete.AI.RateLimiting;

/// <summary>
/// Thread-safe rate limiter using token bucket algorithm.
/// Prevents cost amplification attacks by limiting the rate of API calls.
/// </summary>
/// <remarks>
/// <para>
/// The token bucket algorithm maintains a fixed-size bucket of tokens.
/// Each API call consumes one token. Tokens are refilled at a constant rate.
/// When the bucket is empty, requests are rejected or delayed.
/// </para>
/// <para>
/// <b>Security Features:</b>
/// <list type="bullet">
/// <item>Thread-safe for concurrent access using SemaphoreSlim</item>
/// <item>Prevents API quota exhaustion</item>
/// <item>Protects against cost amplification attacks</item>
/// <item>Configurable per-component or per-user limits</item>
/// </list>
/// </para>
/// </remarks>
public class RateLimiter : IDisposable
{
    private readonly int _maxTokens;
    private readonly TimeSpan _refillInterval;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private int _currentTokens;
    private DateTime _lastRefill;
    private bool _disposed;

    /// <summary>
    /// Initializes a new rate limiter.
    /// </summary>
    /// <param name="maxRequestsPerMinute">Maximum requests allowed per minute. Must be greater than 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxRequestsPerMinute is less than or equal to 0.</exception>
    /// <example>
    /// <code>
    /// // Allow 60 requests per minute (1 per second)
    /// var limiter = new RateLimiter(maxRequestsPerMinute: 60);
    ///
    /// // Allow 10 requests per minute (conservative for multi-user apps)
    /// var limiter = new RateLimiter(maxRequestsPerMinute: 10);
    /// </code>
    /// </example>
    public RateLimiter(int maxRequestsPerMinute)
    {
        if (maxRequestsPerMinute <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxRequestsPerMinute),
                "Must be greater than 0");
        }

        _maxTokens = maxRequestsPerMinute;
        _currentTokens = maxRequestsPerMinute;
        _refillInterval = TimeSpan.FromMinutes(1.0 / maxRequestsPerMinute);
        _lastRefill = DateTime.UtcNow;
    }

    /// <summary>
    /// Attempts to acquire a token for making a request.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token acquired, false if rate limit exceeded</returns>
    /// <remarks>
    /// This method returns immediately. If no tokens are available, it returns false.
    /// Use <see cref="AcquireAsync"/> if you want to wait for a token to become available.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown if the rate limiter has been disposed.</exception>
    public async Task<bool> TryAcquireAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            RefillTokens();

            if (_currentTokens > 0)
            {
                _currentTokens--;
                return true;
            }

            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Waits until a token is available, then acquires it.
    /// </summary>
    /// <param name="timeout">Maximum time to wait. If null, waits indefinitely.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="RateLimitExceededException">Thrown if timeout expires before a token becomes available.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the rate limiter has been disposed.</exception>
    /// <remarks>
    /// This method uses adaptive backoff to minimize CPU usage while waiting.
    /// It calculates the next refill time and waits until then before retrying.
    /// </remarks>
    public async Task AcquireAsync(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var deadline = timeout.HasValue
            ? DateTime.UtcNow + timeout.Value
            : DateTime.MaxValue;

        while (DateTime.UtcNow < deadline)
        {
            if (await TryAcquireAsync(cancellationToken))
            {
                return;
            }

            // Wait before retry (adaptive backoff)
            var waitTime = GetNextRefillTime() - DateTime.UtcNow;
            if (waitTime > TimeSpan.Zero)
            {
                await Task.Delay(waitTime, cancellationToken);
            }
        }

        throw new RateLimitExceededException(
            $"Rate limit of {_maxTokens} requests per minute exceeded. " +
            $"Timeout of {timeout} expired.");
    }

    /// <summary>
    /// Gets the current number of available tokens.
    /// </summary>
    /// <remarks>
    /// This property automatically refills tokens based on elapsed time.
    /// It is thread-safe and can be called concurrently.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown if the rate limiter has been disposed.</exception>
    public int AvailableTokens
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            _lock.Wait();
            try
            {
                RefillTokens();
                return _currentTokens;
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    /// <summary>
    /// Gets the time when the next token will be available.
    /// </summary>
    /// <remarks>
    /// This is useful for displaying wait time to users or for implementing custom retry logic.
    /// </remarks>
    public DateTime GetNextRefillTime()
    {
        return _lastRefill + _refillInterval;
    }

    /// <summary>
    /// Refills tokens based on elapsed time since last refill.
    /// </summary>
    /// <remarks>
    /// This method is called internally by TryAcquireAsync and AvailableTokens.
    /// It uses integer division to ensure tokens are added in discrete units.
    /// </remarks>
    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var elapsed = now - _lastRefill;
        var tokensToAdd = (int)(elapsed / _refillInterval);

        if (tokensToAdd > 0)
        {
            _currentTokens = Math.Min(_maxTokens, _currentTokens + tokensToAdd);
            _lastRefill = now;
        }
    }

    /// <summary>
    /// Resets the rate limiter to its initial state (for testing).
    /// </summary>
    /// <remarks>
    /// This method refills the bucket to maximum capacity and resets the refill timer.
    /// It should only be used in testing scenarios.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown if the rate limiter has been disposed.</exception>
    public void Reset()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _lock.Wait();
        try
        {
            _currentTokens = _maxTokens;
            _lastRefill = DateTime.UtcNow;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Disposes the rate limiter and releases associated resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }
}
