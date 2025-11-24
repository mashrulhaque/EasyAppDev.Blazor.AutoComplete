// Copyright (c) EasyAppDev. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace EasyAppDev.Blazor.AutoComplete.Security;

/// <summary>
/// Centralized security event logging for the AutoComplete component.
/// </summary>
/// <remarks>
/// <para>
/// This class provides structured logging for security-related events, enabling
/// monitoring, alerting, and incident response. All security events are logged with
/// a [SECURITY] prefix for easy filtering.
/// </para>
/// <para>
/// <b>Integration:</b>
/// This logger can be configured at application startup and used throughout the
/// component library to log security events consistently.
/// </para>
/// <para>
/// <b>Best Practices:</b>
/// <list type="bullet">
/// <item>Log security events but avoid logging sensitive data (PII, credentials, etc.)</item>
/// <item>Truncate long inputs to prevent log injection attacks</item>
/// <item>Use structured logging parameters for queryability</item>
/// <item>Configure appropriate log levels (Warning, Error, Critical)</item>
/// </list>
/// </para>
/// </remarks>
public class SecurityEventLogger
{
    private readonly ILogger? _logger;
    private const int MaxLogValueLength = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityEventLogger"/> class.
    /// </summary>
    /// <param name="logger">Optional logger instance. If null, logging is disabled.</param>
    public SecurityEventLogger(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs a CSS sanitization failure.
    /// </summary>
    /// <param name="propertyType">Type of CSS property that failed (e.g., "Color", "Spacing", "Size")</param>
    /// <param name="attemptedValue">The value that was attempted (will be truncated for logging)</param>
    /// <param name="reason">Optional reason for failure</param>
    public void LogCssSanitizationFailure(
        string propertyType,
        string attemptedValue,
        string? reason = null)
    {
        _logger?.LogWarning(
            SecurityEventId.CssSanitizationFailed,
            "[SECURITY] CSS sanitization failed. " +
            "Property: {PropertyType}, " +
            "AttemptedValue (truncated): {Value}, " +
            "Reason: {Reason}",
            propertyType,
            TruncateForLogging(attemptedValue),
            reason ?? "Invalid format");
    }

    /// <summary>
    /// Logs excessive input length detected.
    /// </summary>
    /// <param name="inputType">Type of input (e.g., "CSS Color", "Search Text", "Embedding Text")</param>
    /// <param name="actualLength">Actual length of the input</param>
    /// <param name="maxLength">Maximum allowed length</param>
    /// <param name="context">Optional context information</param>
    public void LogExcessiveInputLength(
        string inputType,
        int actualLength,
        int maxLength,
        string? context = null)
    {
        _logger?.LogWarning(
            SecurityEventId.ExcessiveInputLength,
            "[SECURITY] Excessive input length detected. " +
            "Type: {InputType}, " +
            "Length: {ActualLength} (max: {MaxLength}), " +
            "Context: {Context}",
            inputType,
            actualLength,
            maxLength,
            context ?? "N/A");
    }

    /// <summary>
    /// Logs a potential ReDoS (Regular Expression Denial of Service) attack.
    /// </summary>
    /// <param name="pattern">The regex pattern that timed out</param>
    /// <param name="input">The input that caused the timeout (length only, not content)</param>
    /// <param name="timeout">The timeout duration that expired</param>
    public void LogReDoSAttempt(
        string pattern,
        string input,
        TimeSpan timeout)
    {
        _logger?.LogError(
            SecurityEventId.ReDoSAttempt,
            "[SECURITY] Potential ReDoS attack detected. " +
            "Pattern: {Pattern}, " +
            "InputLength: {Length}, " +
            "Timeout: {Timeout}ms",
            pattern,
            input.Length,
            timeout.TotalMilliseconds);
    }

    /// <summary>
    /// Logs rate limit exceeded event.
    /// </summary>
    /// <param name="component">Component name (e.g., "SemanticSearch", "AutoComplete")</param>
    /// <param name="availableTokens">Number of tokens currently available</param>
    /// <param name="nextRefill">Time when next token will be available</param>
    public void LogRateLimitExceeded(
        string component,
        int availableTokens,
        DateTime nextRefill)
    {
        _logger?.LogWarning(
            SecurityEventId.RateLimitExceeded,
            "[SECURITY] Rate limit exceeded. " +
            "Component: {Component}, " +
            "AvailableTokens: {Tokens}, " +
            "NextRefill: {NextRefill:yyyy-MM-dd HH:mm:ss}",
            component,
            availableTokens,
            nextRefill);
    }

    /// <summary>
    /// Logs cache pressure event (approaching or at capacity).
    /// </summary>
    /// <param name="cacheType">Type of cache (e.g., "ItemCache", "QueryCache")</param>
    /// <param name="currentCount">Current number of items in cache</param>
    /// <param name="maxCount">Maximum cache capacity</param>
    /// <param name="usagePercent">Cache usage as a percentage (0.0 to 1.0)</param>
    public void LogCachePressure(
        string cacheType,
        int currentCount,
        int maxCount,
        double usagePercent)
    {
        var level = usagePercent >= 0.95 ? LogLevel.Critical : LogLevel.Warning;

        _logger?.Log(
            level,
            SecurityEventId.CachePressure,
            "[SECURITY] Cache pressure {Level}. " +
            "Cache: {CacheType}, " +
            "Usage: {Usage:P1} ({Current}/{Max})",
            level == LogLevel.Critical ? "CRITICAL" : "HIGH",
            cacheType,
            usagePercent,
            currentCount,
            maxCount);
    }

    /// <summary>
    /// Logs memory pressure event.
    /// </summary>
    /// <param name="availableMemoryMB">Available memory in megabytes</param>
    /// <param name="usagePercent">Memory usage as a percentage (0.0 to 1.0)</param>
    /// <param name="action">Action taken in response (e.g., "Clearing cache", "Reducing dataset")</param>
    public void LogMemoryPressure(
        long availableMemoryMB,
        double usagePercent,
        string action)
    {
        var level = usagePercent >= 0.90 ? LogLevel.Critical : LogLevel.Warning;

        _logger?.Log(
            level,
            SecurityEventId.MemoryPressure,
            "[SECURITY] Memory pressure detected. " +
            "Available: {AvailableMB}MB, " +
            "Usage: {Usage:P1}, " +
            "Action: {Action}",
            availableMemoryMB,
            usagePercent,
            action);
    }

    /// <summary>
    /// Logs suspicious input pattern detected.
    /// </summary>
    /// <param name="inputType">Type of input (e.g., "SearchText", "Parameter")</param>
    /// <param name="pattern">Suspicious pattern detected (e.g., "SQL injection", "XSS")</param>
    /// <param name="input">The input that triggered the detection (truncated)</param>
    public void LogSuspiciousInput(
        string inputType,
        string pattern,
        string input)
    {
        _logger?.LogWarning(
            SecurityEventId.SuspiciousInput,
            "[SECURITY] Suspicious input detected. " +
            "InputType: {InputType}, " +
            "Pattern: {Pattern}, " +
            "Input (truncated): {Input}",
            inputType,
            pattern,
            TruncateForLogging(input));
    }

    /// <summary>
    /// Logs embedding validation failure.
    /// </summary>
    /// <param name="reason">Reason for validation failure</param>
    /// <param name="expectedDimension">Expected embedding dimension (if applicable)</param>
    /// <param name="actualDimension">Actual embedding dimension (if applicable)</param>
    public void LogEmbeddingValidationFailed(
        string reason,
        int? expectedDimension = null,
        int? actualDimension = null)
    {
        if (expectedDimension.HasValue && actualDimension.HasValue)
        {
            _logger?.LogWarning(
                SecurityEventId.EmbeddingValidationFailed,
                "[SECURITY] Embedding validation failed. " +
                "Reason: {Reason}, " +
                "Expected dimension: {Expected}, " +
                "Actual dimension: {Actual}",
                reason,
                expectedDimension.Value,
                actualDimension.Value);
        }
        else
        {
            _logger?.LogWarning(
                SecurityEventId.EmbeddingValidationFailed,
                "[SECURITY] Embedding validation failed. Reason: {Reason}",
                reason);
        }
    }

    /// <summary>
    /// Logs HTTPS enforcement violation.
    /// </summary>
    /// <param name="url">The URL that violated HTTPS requirement (truncated)</param>
    /// <param name="component">Component that detected the violation</param>
    public void LogHttpsViolation(
        string url,
        string component)
    {
        _logger?.LogError(
            SecurityEventId.HttpsViolation,
            "[SECURITY] HTTPS enforcement violation. " +
            "Component: {Component}, " +
            "URL (truncated): {Url}",
            component,
            TruncateForLogging(url));
    }

    /// <summary>
    /// Truncates a string for logging to prevent log injection and excessive log sizes.
    /// </summary>
    private string TruncateForLogging(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Length > MaxLogValueLength
            ? value.Substring(0, MaxLogValueLength) + "..."
            : value;
    }
}
