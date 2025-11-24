using EasyAppDev.Blazor.AutoComplete.Security;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EasyAppDev.Blazor.AutoComplete.Tests.Security;

/// <summary>
/// Tests for security logging functionality.
/// Verifies that security events are logged with correct event IDs, severity, and structured data.
/// </summary>
public class SecurityLoggingTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly SecurityEventLogger _securityLogger;
    private readonly List<LogEntry> _logEntries;

    public SecurityLoggingTests()
    {
        _mockLogger = new Mock<ILogger>();
        _logEntries = new List<LogEntry>();

        // Capture all log calls
        _mockLogger
            .Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(new InvocationAction(invocation =>
            {
                var logLevel = (LogLevel)invocation.Arguments[0];
                var eventId = (EventId)invocation.Arguments[1];
                var state = invocation.Arguments[2];
                var exception = (Exception?)invocation.Arguments[3];
                var formatter = invocation.Arguments[4];

                var message = formatter?.GetType()
                    .GetMethod("Invoke")?
                    .Invoke(formatter, new[] { state, exception })?.ToString() ?? string.Empty;

                _logEntries.Add(new LogEntry(logLevel, eventId, message, exception));
            }));

        _securityLogger = new SecurityEventLogger(_mockLogger.Object);
    }

    [Fact]
    public void LogCssSanitizationFailure_ShouldLogWithCorrectEventId()
    {
        // Act
        _securityLogger.LogCssSanitizationFailure(
            "Color",
            "javascript:alert(1)",
            "Contains dangerous pattern");

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        entry.Level.Should().Be(LogLevel.Warning);
        entry.EventId.Should().Be(SecurityEventId.CssSanitizationFailed);
        entry.Message.Should().Contain("[SECURITY]");
        entry.Message.Should().Contain("Color");
        entry.Message.Should().Contain("javascript");
        entry.Message.Should().Contain("dangerous pattern");
    }

    [Fact]
    public void LogExcessiveInputLength_ShouldLogWithCorrectDetails()
    {
        // Act
        _securityLogger.LogExcessiveInputLength(
            "CSS Color",
            5000,
            200,
            "User theme customization");

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        entry.Level.Should().Be(LogLevel.Warning);
        entry.EventId.Should().Be(SecurityEventId.ExcessiveInputLength);
        entry.Message.Should().Contain("5000");
        entry.Message.Should().Contain("200");
        entry.Message.Should().Contain("CSS Color");
    }

    [Fact]
    public void LogReDoSAttempt_ShouldLogAsError()
    {
        // Act
        _securityLogger.LogReDoSAttempt(
            @"[\w\s\-,'""]+",
            new string('a', 10000),
            TimeSpan.FromMilliseconds(100));

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        entry.Level.Should().Be(LogLevel.Error);
        entry.EventId.Should().Be(SecurityEventId.ReDoSAttempt);
        entry.Message.Should().Contain("ReDoS");
        entry.Message.Should().Contain("10000"); // Input length
        entry.Message.Should().Contain("100"); // Timeout in ms
    }

    [Fact]
    public void LogRateLimitExceeded_ShouldLogWithTokenInfo()
    {
        // Act
        var nextRefill = DateTime.UtcNow.AddMinutes(1);
        _securityLogger.LogRateLimitExceeded(
            "SemanticSearch",
            0,
            nextRefill);

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        entry.Level.Should().Be(LogLevel.Warning);
        entry.EventId.Should().Be(SecurityEventId.RateLimitExceeded);
        entry.Message.Should().Contain("SemanticSearch");
        entry.Message.Should().Contain("0"); // Available tokens
    }

    [Fact]
    public void LogCachePressure_HighUsage_ShouldLogWarning()
    {
        // Act
        _securityLogger.LogCachePressure(
            "ItemCache",
            8500,
            10000,
            0.85);

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        entry.Level.Should().Be(LogLevel.Warning);
        entry.EventId.Should().Be(SecurityEventId.CachePressure);
        entry.Message.Should().Contain("HIGH");
        entry.Message.Should().Contain("85"); // Percentage
    }

    [Fact]
    public void LogCachePressure_CriticalUsage_ShouldLogCritical()
    {
        // Act
        _securityLogger.LogCachePressure(
            "QueryCache",
            9800,
            10000,
            0.98);

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        entry.Level.Should().Be(LogLevel.Critical);
        entry.EventId.Should().Be(SecurityEventId.CachePressure);
        entry.Message.Should().Contain("CRITICAL");
        entry.Message.Should().Contain("98"); // Percentage
    }

    [Fact]
    public void LogMemoryPressure_HighUsage_ShouldLogWarning()
    {
        // Act
        _securityLogger.LogMemoryPressure(
            512, // 512 MB available
            0.85,
            "Clearing query cache");

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        entry.Level.Should().Be(LogLevel.Warning);
        entry.EventId.Should().Be(SecurityEventId.MemoryPressure);
        entry.Message.Should().Contain("512");
        entry.Message.Should().Contain("Clearing query cache");
    }

    [Fact]
    public void LogMemoryPressure_CriticalUsage_ShouldLogCritical()
    {
        // Act
        _securityLogger.LogMemoryPressure(
            128, // Only 128 MB available
            0.95,
            "Emergency cache clear");

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        entry.Level.Should().Be(LogLevel.Critical);
        entry.EventId.Should().Be(SecurityEventId.MemoryPressure);
    }

    [Fact]
    public void LogSuspiciousInput_ShouldTruncateLongInput()
    {
        // Arrange
        var longInput = new string('x', 500);

        // Act
        _securityLogger.LogSuspiciousInput(
            "SearchText",
            "SQL injection",
            longInput);

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        entry.Level.Should().Be(LogLevel.Warning);
        entry.EventId.Should().Be(SecurityEventId.SuspiciousInput);
        entry.Message.Should().Contain("SQL injection");
        entry.Message.Length.Should().BeLessThan(600); // Truncated + formatting
    }

    [Fact]
    public void LogEmbeddingValidationFailed_WithDimensions_ShouldLogBoth()
    {
        // Act
        _securityLogger.LogEmbeddingValidationFailed(
            "Dimension mismatch",
            expectedDimension: 1536,
            actualDimension: 512);

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        entry.Level.Should().Be(LogLevel.Warning);
        entry.EventId.Should().Be(SecurityEventId.EmbeddingValidationFailed);
        entry.Message.Should().Contain("1536");
        entry.Message.Should().Contain("512");
        entry.Message.Should().Contain("Dimension mismatch");
    }

    [Fact]
    public void LogEmbeddingValidationFailed_WithoutDimensions_ShouldLogReason()
    {
        // Act
        _securityLogger.LogEmbeddingValidationFailed("Null embedding returned");

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        entry.Level.Should().Be(LogLevel.Warning);
        entry.EventId.Should().Be(SecurityEventId.EmbeddingValidationFailed);
        entry.Message.Should().Contain("Null embedding");
    }

    [Fact]
    public void LogHttpsViolation_ShouldLogAsError()
    {
        // Act
        _securityLogger.LogHttpsViolation(
            "http://insecure-api.example.com/data",
            "RemoteDataSource");

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        entry.Level.Should().Be(LogLevel.Error);
        entry.EventId.Should().Be(SecurityEventId.HttpsViolation);
        entry.Message.Should().Contain("HTTPS");
        entry.Message.Should().Contain("http://insecure");
        entry.Message.Should().Contain("RemoteDataSource");
    }

    [Fact]
    public void SecurityLogger_WithNullLogger_ShouldNotThrow()
    {
        // Arrange
        var logger = new SecurityEventLogger(null);

        // Act & Assert - should not throw
        var act = () =>
        {
            logger.LogCssSanitizationFailure("Color", "test", "reason");
            logger.LogExcessiveInputLength("Test", 1000, 200);
            logger.LogReDoSAttempt("pattern", "input", TimeSpan.FromMilliseconds(100));
            logger.LogRateLimitExceeded("Component", 0, DateTime.UtcNow);
            logger.LogCachePressure("Cache", 100, 200, 0.5);
            logger.LogMemoryPressure(1024, 0.8, "action");
            logger.LogSuspiciousInput("Type", "Pattern", "Input");
            logger.LogEmbeddingValidationFailed("Reason");
            logger.LogHttpsViolation("http://test.com", "Component");
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void SecurityEventId_AllEvents_ShouldHaveUniqueIds()
    {
        // Arrange
        var eventIds = new[]
        {
            SecurityEventId.CssSanitizationFailed,
            SecurityEventId.ExcessiveInputLength,
            SecurityEventId.ReDoSAttempt,
            SecurityEventId.RateLimitExceeded,
            SecurityEventId.CachePressure,
            SecurityEventId.MemoryPressure,
            SecurityEventId.SuspiciousInput,
            SecurityEventId.EmbeddingValidationFailed,
            SecurityEventId.HttpsViolation
        };

        // Act & Assert - all IDs should be unique
        eventIds.Select(e => e.Id).Should().OnlyHaveUniqueItems();

        // All IDs should be in the 9000-9999 range (security events)
        eventIds.Should().AllSatisfy(e => e.Id.Should().BeInRange(9000, 9999));
    }

    [Fact]
    public void LogCssSanitizationFailure_ShouldTruncateLongValues()
    {
        // Arrange
        var longValue = new string('x', 500);

        // Act
        _securityLogger.LogCssSanitizationFailure(
            "Color",
            longValue,
            "Too long");

        // Assert
        _logEntries.Should().HaveCount(1);
        var entry = _logEntries[0];
        // Message should be truncated (not 500+ chars in the value portion)
        entry.Message.Length.Should().BeLessThan(400);
        entry.Message.Should().Contain("...");
    }

    // Helper class to capture log entries
    private record LogEntry(LogLevel Level, EventId EventId, string Message, Exception? Exception);
}
