// Copyright (c) EasyAppDev. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace EasyAppDev.Blazor.AutoComplete.Security;

/// <summary>
/// Security event IDs for structured logging.
/// </summary>
/// <remarks>
/// <para>
/// These event IDs enable filtering, alerting, and monitoring of security-related events
/// in logging systems. They follow the convention of using IDs in the 9000-9999 range
/// for security events.
/// </para>
/// <para>
/// <b>Event ID Ranges:</b>
/// <list type="bullet">
/// <item>9001-9003: Input validation failures (CSS, excessive length, ReDoS)</item>
/// <item>9004-9006: Resource exhaustion (rate limits, cache pressure, memory)</item>
/// <item>9007-9009: Security violations (suspicious input, embedding validation, HTTPS)</item>
/// </list>
/// </para>
/// </remarks>
public static class SecurityEventId
{
    /// <summary>
    /// CSS sanitization failed due to invalid or dangerous input.
    /// </summary>
    /// <remarks>
    /// <b>Severity:</b> Warning<br/>
    /// <b>Common Causes:</b>
    /// <list type="bullet">
    /// <item>Custom property injection attempts (e.g., "--ebd-ac-primary")</item>
    /// <item>JavaScript injection via url() or expression()</item>
    /// <item>Encoded dangerous patterns (Unicode dashes, URL encoding)</item>
    /// <item>Invalid color/size/spacing values</item>
    /// </list>
    /// </remarks>
    public static readonly EventId CssSanitizationFailed = new(9001, "CssSanitizationFailed");

    /// <summary>
    /// Input exceeded maximum allowed length.
    /// </summary>
    /// <remarks>
    /// <b>Severity:</b> Warning<br/>
    /// <b>Common Causes:</b>
    /// <list type="bullet">
    /// <item>CSS value longer than 200 characters</item>
    /// <item>Search text longer than 1000 characters</item>
    /// <item>Embedding text longer than configured limit</item>
    /// <item>Potential denial-of-service attempts</item>
    /// </list>
    /// </remarks>
    public static readonly EventId ExcessiveInputLength = new(9002, "ExcessiveInputLength");

    /// <summary>
    /// Potential ReDoS (Regular Expression Denial of Service) attack detected.
    /// </summary>
    /// <remarks>
    /// <b>Severity:</b> Error<br/>
    /// <b>Description:</b> Regex timeout expired during pattern matching, indicating
    /// either a ReDoS attack attempt or legitimate complex input that requires optimization.
    /// </remarks>
    public static readonly EventId ReDoSAttempt = new(9003, "ReDoSAttempt");

    /// <summary>
    /// Rate limit exceeded for API calls.
    /// </summary>
    /// <remarks>
    /// <b>Severity:</b> Warning<br/>
    /// <b>Common Causes:</b>
    /// <list type="bullet">
    /// <item>Legitimate rapid user typing</item>
    /// <item>Cost amplification attack</item>
    /// <item>Automated scraping or testing</item>
    /// <item>Misconfigured rate limits (too low)</item>
    /// </list>
    /// </remarks>
    public static readonly EventId RateLimitExceeded = new(9004, "RateLimitExceeded");

    /// <summary>
    /// Cache approaching or at capacity.
    /// </summary>
    /// <remarks>
    /// <b>Severity:</b> Warning (high usage) or Critical (at capacity)<br/>
    /// <b>Common Causes:</b>
    /// <list type="bullet">
    /// <item>Large dataset with many unique searches</item>
    /// <item>Cache size too small for workload</item>
    /// <item>Memory-based DoS attempt</item>
    /// <item>Missing cache eviction policy</item>
    /// </list>
    /// </remarks>
    public static readonly EventId CachePressure = new(9005, "CachePressure");

    /// <summary>
    /// System memory pressure detected.
    /// </summary>
    /// <remarks>
    /// <b>Severity:</b> Warning or Critical<br/>
    /// <b>Description:</b> Available memory is low and may impact application performance
    /// or stability. Consider reducing cache sizes or dataset sizes.
    /// </remarks>
    public static readonly EventId MemoryPressure = new(9006, "MemoryPressure");

    /// <summary>
    /// Suspicious input pattern detected.
    /// </summary>
    /// <remarks>
    /// <b>Severity:</b> Warning<br/>
    /// <b>Common Causes:</b>
    /// <list type="bullet">
    /// <item>SQL injection attempt patterns</item>
    /// <item>XSS payload patterns</item>
    /// <item>Path traversal attempts (../, etc.)</item>
    /// <item>Command injection patterns</item>
    /// </list>
    /// </remarks>
    public static readonly EventId SuspiciousInput = new(9007, "SuspiciousInput");

    /// <summary>
    /// Embedding validation failed (null, empty, or invalid dimensions).
    /// </summary>
    /// <remarks>
    /// <b>Severity:</b> Warning<br/>
    /// <b>Common Causes:</b>
    /// <list type="bullet">
    /// <item>API returned null or empty embedding</item>
    /// <item>Incorrect embedding model configuration</item>
    /// <item>Dimension mismatch between cached and new embeddings</item>
    /// <item>Corrupted cache data</item>
    /// </list>
    /// </remarks>
    public static readonly EventId EmbeddingValidationFailed = new(9008, "EmbeddingValidationFailed");

    /// <summary>
    /// HTTPS enforcement violation (HTTP used instead of HTTPS).
    /// </summary>
    /// <remarks>
    /// <b>Severity:</b> Error<br/>
    /// <b>Description:</b> A remote data source is using HTTP instead of HTTPS,
    /// potentially exposing data to man-in-the-middle attacks.
    /// </remarks>
    public static readonly EventId HttpsViolation = new(9009, "HttpsViolation");
}
