using EasyAppDev.Blazor.AutoComplete.Utilities;
using FluentAssertions;
using Xunit;

namespace EasyAppDev.Blazor.AutoComplete.Tests.Security;

/// <summary>
/// Tests for CSS sanitizer bypass attempts and edge cases.
/// Verifies that obfuscation techniques and injection attempts are blocked.
/// </summary>
public class CssSanitizerBypassTests
{
    [Theory]
    [InlineData("--ebd-ac-primary")] // Direct custom property
    [InlineData("--test")] // Any custom property
    [InlineData("var(--test)")] // CSS variable reference (contains --)
    public void SanitizeColor_CustomPropertyAttempts_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject custom property: {maliciousValue}");
    }

    [Theory]
    [InlineData("\u2013\u2013ebd-ac-primary")] // En dashes (U+2013)
    [InlineData("\u2014\u2014ebd-ac-primary")] // Em dashes (U+2014)
    [InlineData("\u2015\u2015ebd-ac-primary")] // Horizontal bars (U+2015)
    [InlineData("\uFE58\uFE58ebd-ac-primary")] // Small em dashes
    [InlineData("\uFE63\uFE63ebd-ac-primary")] // Small hyphen-minus
    [InlineData("\uFF0D\uFF0Debd-ac-primary")] // Fullwidth hyphen-minus
    public void SanitizeColor_UnicodeDashVariants_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject Unicode dash variant: U+{((int)maliciousValue[0]):X4}");
    }

    [Theory]
    [InlineData("%2D%2Debd-ac-primary")] // URL encoded --
    [InlineData("%2d%2debd-ac-primary")] // URL encoded -- (lowercase)
    [InlineData("&#45;&#45;ebd-ac-primary")] // HTML entity --
    [InlineData("&#x2d;&#x2d;ebd-ac-primary")] // HTML hex entity --
    public void SanitizeColor_EncodedDashes_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject encoded dashes: {maliciousValue}");
    }

    [Theory]
    [InlineData("url(javascript:alert(1))")] // JavaScript injection
    [InlineData("url('javascript:alert(1)')")] // With quotes
    [InlineData("expression(alert(1))")] // IE expression
    [InlineData("javascript:alert(1)")] // Direct JS
    [InlineData("data:text/html,<script>alert(1)</script>")] // Data URI with script
    public void SanitizeColor_JavaScriptInjection_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject JavaScript injection: {maliciousValue}");
    }

    [Theory]
    [InlineData("red; background: url(evil.com)")] // CSS injection with semicolon
    [InlineData("red } body { background: red")] // Breaking out of block
    [InlineData("red /* comment */ url(evil.com)")] // Comment injection
    [InlineData("@import url(evil.com)")] // Import injection
    public void SanitizeColor_CssInjection_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject CSS injection: {maliciousValue}");
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")] // HTML tag
    [InlineData("<style>body{display:none}</style>")] // Style tag
    [InlineData("red<script>")] // Partial tag
    [InlineData("red>test")] // Just angle brackets
    public void SanitizeColor_HtmlInjection_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject HTML injection: {maliciousValue}");
    }

    [Fact]
    public void SanitizeColor_ExcessiveLength_ShouldReject()
    {
        // Arrange - 201 characters (exceeds MaxCssValueLength of 200)
        var longValue = new string('a', 201);

        // Act
        var result = CssSanitizer.SanitizeColor(longValue);

        // Assert
        result.Should().BeNull("Should reject values exceeding maximum length");
    }

    [Theory]
    [InlineData("#FF0000")] // Valid hex
    [InlineData("rgb(255, 0, 0)")] // Valid RGB
    [InlineData("rgba(255, 0, 0, 0.5)")] // Valid RGBA
    [InlineData("hsl(0, 100%, 50%)")] // Valid HSL
    [InlineData("hsla(0, 100%, 50%, 0.5)")] // Valid HSLA
    [InlineData("red")] // Valid named color
    [InlineData("transparent")] // Valid keyword
    public void SanitizeColor_ValidColors_ShouldAccept(string validValue)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(validValue);

        // Assert
        result.Should().NotBeNull($"Should accept valid color: {validValue}");
    }

    [Theory]
    [InlineData("10px")] // Valid pixel
    [InlineData("1.5em")] // Valid em
    [InlineData("100%")] // Valid percentage
    [InlineData("10px 20px")] // Multiple values
    [InlineData("10px 20px 30px 40px")] // Four values (padding/margin)
    public void SanitizeLength_ValidLengths_ShouldAccept(string validValue)
    {
        // Act
        var result = CssSanitizer.SanitizeLength(validValue);

        // Assert
        result.Should().NotBeNull($"Should accept valid length: {validValue}");
    }

    [Theory]
    [InlineData("10px; background: url(evil.com)")] // Injection attempt
    [InlineData("10px } body { display: none")] // Breaking out
    [InlineData("calc(100% - var(--evil))")] // CSS variable in calc (contains --)
    public void SanitizeLength_InjectionAttempts_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeLength(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject injection: {maliciousValue}");
    }

    [Theory]
    [InlineData("Arial, sans-serif")] // Valid font stack
    [InlineData("'Segoe UI', Tahoma")] // With quotes
    [InlineData("system-ui")] // Generic font
    [InlineData("monospace")] // Generic font
    public void SanitizeFontFamily_ValidFonts_ShouldAccept(string validValue)
    {
        // Act
        var result = CssSanitizer.SanitizeFontFamily(validValue);

        // Assert
        result.Should().NotBeNull($"Should accept valid font: {validValue}");
    }

    [Theory]
    [InlineData("Arial; background: url(evil.com)")] // Injection
    [InlineData("Arial /* comment */ url(evil.com)")] // Comment injection
    [InlineData("'Arial\"><script>alert(1)</script>'")] // Breaking out of quotes
    public void SanitizeFontFamily_InjectionAttempts_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeFontFamily(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject injection: {maliciousValue}");
    }

    [Theory]
    [InlineData("0 4px 6px rgba(0, 0, 0, 0.1)")] // Valid shadow
    [InlineData("0 0 10px #FF0000")] // Valid with hex color
    [InlineData("inset 0 2px 4px rgba(0, 0, 0, 0.2)")] // Valid inset
    [InlineData("none")] // Valid keyword
    public void SanitizeShadow_ValidShadows_ShouldAccept(string validValue)
    {
        // Act
        var result = CssSanitizer.SanitizeShadow(validValue);

        // Assert
        result.Should().NotBeNull($"Should accept valid shadow: {validValue}");
    }

    [Theory]
    [InlineData("0 0 10px red; background: url(evil.com)")] // Injection
    [InlineData("0 0 10px var(--evil-color)")] // CSS variable (contains --)
    public void SanitizeShadow_InjectionAttempts_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeShadow(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject injection: {maliciousValue}");
    }

    [Theory]
    [InlineData("200ms")] // Valid milliseconds
    [InlineData("1.5s")] // Valid seconds
    [InlineData("0.3s")] // Valid fractional
    public void SanitizeTime_ValidTimes_ShouldAccept(string validValue)
    {
        // Act
        var result = CssSanitizer.SanitizeTime(validValue);

        // Assert
        result.Should().NotBeNull($"Should accept valid time: {validValue}");
    }

    [Theory]
    [InlineData("200ms; animation-name: evil")] // Injection
    [InlineData("1s } body { display: none")] // Breaking out
    public void SanitizeTime_InjectionAttempts_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeTime(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject injection: {maliciousValue}");
    }

    [Theory]
    [InlineData("1.5")] // Valid number
    [InlineData("normal")] // Valid keyword
    [InlineData("inherit")] // Valid keyword
    [InlineData("10px")] // Valid length
    public void SanitizeGenericValue_ValidValues_ShouldAccept(string validValue)
    {
        // Act
        var result = CssSanitizer.SanitizeGenericValue(validValue);

        // Assert
        result.Should().NotBeNull($"Should accept valid value: {validValue}");
    }

    [Theory]
    [InlineData("normal; background: red")] // Injection
    [InlineData("1.5 } body { display: none")] // Breaking out
    public void SanitizeGenericValue_InjectionAttempts_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeGenericValue(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject injection: {maliciousValue}");
    }

    [Fact]
    public void SanitizeColor_ReDoSAttempt_ShouldTimeoutAndReject()
    {
        // Arrange - crafted input that could cause catastrophic backtracking
        // This would be vulnerable in poorly written regex but should be protected by:
        // 1. Timeout protection (100ms)
        // 2. Linear-time regex patterns
        var potentialReDoS = new string('a', 1000) + new string('b', 1000);

        // Act
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = CssSanitizer.SanitizeColor(potentialReDoS);
        sw.Stop();

        // Assert
        result.Should().BeNull("Should reject invalid input");
        sw.ElapsedMilliseconds.Should().BeLessThan(200,
            "Should complete quickly even with potential ReDoS input");
    }

    [Fact]
    public void SanitizeFontFamily_ReDoSAttempt_ShouldTimeoutAndReject()
    {
        // Arrange - nested repetition that could cause catastrophic backtracking
        var potentialReDoS = string.Join(",", Enumerable.Repeat("'Arial'", 100)) +
                            new string(',', 1000);

        // Act
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = CssSanitizer.SanitizeFontFamily(potentialReDoS);
        sw.Stop();

        // Assert
        result.Should().BeNull("Should reject invalid input");
        sw.ElapsedMilliseconds.Should().BeLessThan(200,
            "Should complete quickly even with potential ReDoS input");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void SanitizeColor_NullOrWhitespace_ShouldReturnNull(string? emptyValue)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(emptyValue);

        // Assert
        result.Should().BeNull("Should reject null or whitespace");
    }

    [Fact]
    public void SanitizeColor_RgbOutOfRange_ShouldReject()
    {
        // Arrange - RGB values must be 0-255
        var invalidRgb = "rgb(300, 0, 0)";

        // Act
        var result = CssSanitizer.SanitizeColor(invalidRgb);

        // Assert
        result.Should().BeNull("Should reject RGB values > 255");
    }

    [Fact]
    public void SanitizeColor_HslOutOfRange_ShouldReject()
    {
        // Arrange - HSL saturation/lightness must be 0-100%
        var invalidHsl = "hsl(0, 150%, 50%)";

        // Act
        var result = CssSanitizer.SanitizeColor(invalidHsl);

        // Assert
        result.Should().BeNull("Should reject HSL values > 100%");
    }

    [Theory]
    [InlineData("binding:alert(1)")] // IE binding
    [InlineData("-moz-binding:url(evil.xml)")] // Firefox binding
    [InlineData("behavior:url(evil.htc)")] // IE behavior
    public void SanitizeColor_IESpecificInjections_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject IE-specific injection: {maliciousValue}");
    }

    [Theory]
    [InlineData("\\002d\\002debd-ac-primary")] // CSS escape for --
    [InlineData("\\2d\\2debd-ac-primary")] // Short CSS escape
    public void SanitizeColor_CssEscapeSequences_ShouldReject(string maliciousValue)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(maliciousValue);

        // Assert
        result.Should().BeNull($"Should reject CSS escape sequences: {maliciousValue}");
    }
}
