// Copyright (c) EasyAppDev. All rights reserved.
// Licensed under the MIT License.

namespace EasyAppDev.Blazor.AutoComplete.AI.RateLimiting;

/// <summary>
/// Exception thrown when rate limit is exceeded and the timeout expires while waiting for a token.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown by <see cref="RateLimiter.AcquireAsync"/> when the specified timeout
/// expires before a token becomes available. It indicates that the rate limit has been exceeded
/// and the caller should handle this gracefully (e.g., show error message, retry later, etc.).
/// </para>
/// <para>
/// <b>Security Context:</b>
/// This exception is part of the defense against cost amplification attacks. When rate limits
/// are exceeded, it prevents further API calls that could result in unexpected costs or
/// API quota exhaustion.
/// </para>
/// </remarks>
[Serializable]
public class RateLimitExceededException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitExceededException"/> class.
    /// </summary>
    public RateLimitExceededException()
        : base("Rate limit exceeded.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitExceededException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RateLimitExceededException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitExceededException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or null if no inner exception is specified.</param>
    public RateLimitExceededException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
