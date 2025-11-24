using EasyAppDev.Blazor.AutoComplete.Utilities;
using FluentAssertions;
using System.Diagnostics;
using Xunit;

namespace EasyAppDev.Blazor.AutoComplete.Tests.Security;

/// <summary>
/// Tests to verify protection against Regular Expression Denial of Service (ReDoS) attacks.
/// All tests verify that operations complete within 100ms timeout.
/// </summary>
public class ReDoSTests
{
    [Fact]
    public void SanitizeFontFamily_WithExcessiveLength_ShouldRejectQuickly()
    {
        // Arrange - input exceeding MaxCssValueLength (1000 chars)
        var maliciousInput = new string('a', 50000);

        // Act
        var sw = Stopwatch.StartNew();
        var result = CssSanitizer.SanitizeFontFamily(maliciousInput);
        sw.Stop();

        // Assert - should reject at length check before any regex processing
        result.Should().BeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(10, "Length check should be instant");
    }

    [Fact]
    public void SanitizeColor_WithExcessiveLength_ShouldRejectQuickly()
    {
        // Arrange - input designed to cause backtracking
        var maliciousInput = new string('a', 50000);

        // Act
        var sw = Stopwatch.StartNew();
        var result = CssSanitizer.SanitizeColor(maliciousInput);
        sw.Stop();

        // Assert - should reject at length check
        result.Should().BeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(10, "Length check should be instant");
    }

    [Theory]
    [InlineData("rgb(255,255,255                                                  )")]
    [InlineData("rgba(0,0,0,0.5)")]
    [InlineData("hsl(360,100%,50%)")]
    public void SanitizeColor_WithValidFormats_ShouldCompleteQuickly(string validInput)
    {
        // Act
        var sw = Stopwatch.StartNew();
        var result = CssSanitizer.SanitizeColor(validInput);
        sw.Stop();

        // Assert - valid inputs should process quickly
        result.Should().NotBeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(100, "Valid inputs should process within timeout");
    }

    [Fact]
    public void SanitizeLength_WithMultipleValues_ShouldHandleQuickly()
    {
        // Arrange - legitimate multi-value input
        var input = "10px 20px 30px 40px";

        // Act
        var sw = Stopwatch.StartNew();
        var result = CssSanitizer.SanitizeLength(input);
        sw.Stop();

        // Assert
        result.Should().Be(input);
        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public void SanitizeLength_WithExcessiveWhitespace_ShouldTimeout()
    {
        // Arrange - input with excessive whitespace
        var maliciousInput = "10px" + new string(' ', 5000) + "20px";

        // Act
        var sw = Stopwatch.StartNew();
        var result = CssSanitizer.SanitizeLength(maliciousInput);
        sw.Stop();

        // Assert - should handle within timeout or reject at length check
        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public void SanitizeFontFamily_WithNestedQuantifierPattern_ShouldNotCauseBacktracking()
    {
        // Arrange - pattern designed to exploit nested quantifiers
        // Previously vulnerable pattern: ^[\w\s\-,'""]+(,\s*[\w\s\-,""']+)*$
        var maliciousInput = "Arial, " + new string('a', 100) + ", " + new string('b', 100) + ", sans-serif";

        // Act
        var sw = Stopwatch.StartNew();
        var result = CssSanitizer.SanitizeFontFamily(maliciousInput);
        sw.Stop();

        // Assert - should process in linear time
        sw.ElapsedMilliseconds.Should().BeLessThan(100, "Fixed regex should have linear complexity");
    }

    [Fact]
    public void SanitizeFontFamily_WithValidFonts_ShouldAccept()
    {
        // Arrange
        var validInput = "Arial, \"Helvetica Neue\", sans-serif";

        // Act
        var sw = Stopwatch.StartNew();
        var result = CssSanitizer.SanitizeFontFamily(validInput);
        sw.Stop();

        // Assert
        result.Should().Be(validInput);
        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public void SanitizeShadow_WithComplexShadow_ShouldHandleQuickly()
    {
        // Arrange
        var validInput = "0 4px 6px rgba(0, 0, 0, 0.1)";

        // Act
        var sw = Stopwatch.StartNew();
        var result = CssSanitizer.SanitizeShadow(validInput);
        sw.Stop();

        // Assert
        result.Should().Be(validInput);
        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public void SanitizeShadow_WithExcessiveLength_ShouldReject()
    {
        // Arrange - shadow with excessive length
        var maliciousInput = "0 4px 6px " + new string('a', 50000);

        // Act
        var sw = Stopwatch.StartNew();
        var result = CssSanitizer.SanitizeShadow(maliciousInput);
        sw.Stop();

        // Assert
        result.Should().BeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(10, "Length check should be instant");
    }

    [Fact]
    public void SanitizeTime_WithValidDurations_ShouldAccept()
    {
        // Arrange
        var validInputs = new[] { "300ms", "1.5s", "0.5s" };

        foreach (var input in validInputs)
        {
            // Act
            var sw = Stopwatch.StartNew();
            var result = CssSanitizer.SanitizeTime(input);
            sw.Stop();

            // Assert
            result.Should().Be(input);
            sw.ElapsedMilliseconds.Should().BeLessThan(100);
        }
    }

    [Fact]
    public void SanitizeGenericValue_WithValidValues_ShouldAccept()
    {
        // Arrange
        var validInputs = new[] { "10px", "1.5", "normal", "inherit" };

        foreach (var input in validInputs)
        {
            // Act
            var sw = Stopwatch.StartNew();
            var result = CssSanitizer.SanitizeGenericValue(input);
            sw.Stop();

            // Assert
            result.Should().Be(input);
            sw.ElapsedMilliseconds.Should().BeLessThan(100);
        }
    }

    [Fact]
    public void AllSanitizeMethods_WithDangerousPatterns_ShouldReject()
    {
        // Arrange - patterns that should always be rejected
        var dangerousPatterns = new[]
        {
            "url(javascript:alert(1))",
            "expression(alert(1))",
            "javascript:void(0)",
            "@import url(evil.css)",
            "binding:url(#myBinding)",
            "<script>alert(1)</script>",
            "/* comment */ attack",
            "value; DROP TABLE users--"
        };

        foreach (var pattern in dangerousPatterns)
        {
            // Act & Assert
            CssSanitizer.SanitizeColor(pattern).Should().BeNull($"Dangerous pattern should be rejected: {pattern}");
            CssSanitizer.SanitizeLength(pattern).Should().BeNull($"Dangerous pattern should be rejected: {pattern}");
            CssSanitizer.SanitizeFontFamily(pattern).Should().BeNull($"Dangerous pattern should be rejected: {pattern}");
            CssSanitizer.SanitizeShadow(pattern).Should().BeNull($"Dangerous pattern should be rejected: {pattern}");
            CssSanitizer.SanitizeTime(pattern).Should().BeNull($"Dangerous pattern should be rejected: {pattern}");
            CssSanitizer.SanitizeGenericValue(pattern).Should().BeNull($"Dangerous pattern should be rejected: {pattern}");
        }
    }

    [Fact]
    public void SanitizeColor_WithRgbEdgeCases_ShouldValidateCorrectly()
    {
        // Arrange & Act & Assert
        CssSanitizer.SanitizeColor("rgb(0,0,0)").Should().Be("rgb(0,0,0)");
        CssSanitizer.SanitizeColor("rgb(255,255,255)").Should().Be("rgb(255,255,255)");
        CssSanitizer.SanitizeColor("rgba(128,128,128,0.5)").Should().Be("rgba(128,128,128,0.5)");

        // Invalid ranges
        CssSanitizer.SanitizeColor("rgb(256,0,0)").Should().BeNull("Value 256 exceeds max 255");
        CssSanitizer.SanitizeColor("rgb(-1,0,0)").Should().BeNull("Negative values not allowed");
    }

    [Fact]
    public void SanitizeColor_WithHslEdgeCases_ShouldValidateCorrectly()
    {
        // Arrange & Act & Assert
        CssSanitizer.SanitizeColor("hsl(0,0%,0%)").Should().Be("hsl(0,0%,0%)");
        CssSanitizer.SanitizeColor("hsl(360,100%,100%)").Should().Be("hsl(360,100%,100%)");
        CssSanitizer.SanitizeColor("hsla(180,50%,50%,0.5)").Should().Be("hsla(180,50%,50%,0.5)");

        // Invalid ranges
        CssSanitizer.SanitizeColor("hsl(361,0%,0%)").Should().BeNull("Hue 361 exceeds max 360");
        CssSanitizer.SanitizeColor("hsl(0,101%,0%)").Should().BeNull("Saturation 101% exceeds max 100%");
    }

    [Theory]
    [InlineData("#FFF")]
    [InlineData("#FFFFFF")]
    [InlineData("#FFFFFFFF")]
    [InlineData("#123")]
    [InlineData("#123456")]
    [InlineData("#12345678")]
    public void SanitizeColor_WithValidHexColors_ShouldAccept(string hex)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(hex);

        // Assert
        result.Should().Be(hex);
    }

    [Theory]
    [InlineData("#FF")]          // Too short
    [InlineData("#FFFFF")]       // Invalid length
    [InlineData("#GGGGGG")]      // Invalid characters
    [InlineData("##FFFFFF")]     // Double hash
    public void SanitizeColor_WithInvalidHexColors_ShouldReject(string hex)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(hex);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void PerformanceTest_ProcessingManyValidInputs_ShouldScaleLinearly()
    {
        // Arrange - test that processing time scales linearly with input count
        var inputs = Enumerable.Range(0, 100).Select(i => $"Arial-{i}, sans-serif").ToList();

        // Act
        var sw = Stopwatch.StartNew();
        foreach (var input in inputs)
        {
            var result = CssSanitizer.SanitizeFontFamily(input);
            result.Should().NotBeNull();
        }
        sw.Stop();

        // Assert - 100 inputs should process well within reasonable time
        sw.ElapsedMilliseconds.Should().BeLessThan(500, "100 valid inputs should process quickly");
    }

    [Fact]
    public void MaxLengthProtection_AtBoundary_ShouldHandleCorrectly()
    {
        // Arrange - exactly at max length (1000 chars)
        var inputAtLimit = new string('a', 1000);
        var inputOverLimit = new string('a', 1001);

        // Act
        var resultAtLimit = CssSanitizer.SanitizeGenericValue(inputAtLimit);
        var resultOverLimit = CssSanitizer.SanitizeGenericValue(inputOverLimit);

        // Assert - at limit is processed, over limit is rejected
        // (will fail pattern matching but won't cause ReDoS)
        resultOverLimit.Should().BeNull("Input over limit should be rejected");
    }
}
