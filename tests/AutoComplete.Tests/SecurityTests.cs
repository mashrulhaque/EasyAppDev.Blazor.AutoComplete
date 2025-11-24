using EasyAppDev.Blazor.AutoComplete.Filtering;
using EasyAppDev.Blazor.AutoComplete.Utilities;
using FluentAssertions;
using Xunit;

namespace EasyAppDev.Blazor.AutoComplete.Tests;

/// <summary>
/// Tests for security vulnerabilities and sanitization.
/// </summary>
public class SecurityTests
{
    #region CSS Injection Tests

    [Theory]
    [InlineData("url(javascript:alert(1))", null)] // XSS via url()
    [InlineData("\"; position: fixed; z-index: 999999", null)] // Clickjacking
    [InlineData("red; background: url(http://evil.com)", null)] // Injection with semicolon
    [InlineData("#fff; } body { display: none", null)] // CSS injection
    [InlineData("expression(alert(1))", null)] // IE expression XSS
    [InlineData("<script>alert(1)</script>", null)] // HTML injection
    [InlineData("#fff--custom", null)] // CSS variable injection
    [InlineData("red/**/", null)] // Comment injection
    public void SanitizeColor_RejectsMaliciousInput(string maliciousInput, string? expected)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(maliciousInput);

        // Assert
        result.Should().Be(expected, "malicious CSS should be rejected");
    }

    [Theory]
    [InlineData("#FF0000", "#FF0000")] // Hex 6-digit
    [InlineData("#F00", "#F00")] // Hex 3-digit
    [InlineData("#FF0000FF", "#FF0000FF")] // Hex 8-digit with alpha
    [InlineData("rgb(255, 0, 0)", "rgb(255, 0, 0)")] // RGB
    [InlineData("rgba(255, 0, 0, 0.5)", "rgba(255, 0, 0, 0.5)")] // RGBA
    [InlineData("hsl(0, 100%, 50%)", "hsl(0, 100%, 50%)")] // HSL
    [InlineData("hsla(0, 100%, 50%, 0.5)", "hsla(0, 100%, 50%, 0.5)")] // HSLA
    [InlineData("red", "red")] // Named color
    [InlineData("transparent", "transparent")] // Transparent
    public void SanitizeColor_AcceptsValidInput(string validInput, string expected)
    {
        // Act
        var result = CssSanitizer.SanitizeColor(validInput);

        // Assert
        result.Should().Be(expected, "valid CSS colors should be accepted");
    }

    [Theory]
    [InlineData("10px; position: fixed", null)] // Injection
    [InlineData("calc(100% - 10px)", null)] // calc() not allowed
    [InlineData("var(--custom)", null)] // CSS variable not allowed
    [InlineData("url(image.png)", null)] // URL not allowed
    public void SanitizeLength_RejectsMaliciousInput(string maliciousInput, string? expected)
    {
        // Act
        var result = CssSanitizer.SanitizeLength(maliciousInput);

        // Assert
        result.Should().Be(expected, "malicious length values should be rejected");
    }

    [Theory]
    [InlineData("10px", "10px")]
    [InlineData("1.5em", "1.5em")]
    [InlineData("100%", "100%")]
    [InlineData("50vh", "50vh")]
    [InlineData("0", "0")]
    public void SanitizeLength_AcceptsValidInput(string validInput, string expected)
    {
        // Act
        var result = CssSanitizer.SanitizeLength(validInput);

        // Assert
        result.Should().Be(expected, "valid CSS lengths should be accepted");
    }

    [Theory]
    [InlineData("Arial; } body { display: none", null)] // Injection
    [InlineData("Comic Sans<script>", null)] // HTML injection
    [InlineData("@import url(evil.css)", null)] // Import injection
    public void SanitizeFontFamily_RejectsMaliciousInput(string maliciousInput, string? expected)
    {
        // Act
        var result = CssSanitizer.SanitizeFontFamily(maliciousInput);

        // Assert
        result.Should().Be(expected, "malicious font families should be rejected");
    }

    [Theory]
    [InlineData("Arial, sans-serif", "Arial, sans-serif")]
    [InlineData("'Comic Sans MS', cursive", "'Comic Sans MS', cursive")]
    [InlineData("system-ui", "system-ui")]
    public void SanitizeFontFamily_AcceptsValidInput(string validInput, string expected)
    {
        // Act
        var result = CssSanitizer.SanitizeFontFamily(validInput);

        // Assert
        result.Should().Be(expected, "valid font families should be accepted");
    }

    [Theory]
    [InlineData("0 0 10px url(javascript:alert(1))", null)] // XSS in shadow
    [InlineData("0 0 10px #fff; position: fixed", null)] // Injection after shadow
    public void SanitizeShadow_RejectsMaliciousInput(string maliciousInput, string? expected)
    {
        // Act
        var result = CssSanitizer.SanitizeShadow(maliciousInput);

        // Assert
        result.Should().Be(expected, "malicious shadow values should be rejected");
    }

    [Theory]
    [InlineData("0 4px 6px rgba(0, 0, 0, 0.1)", "0 4px 6px rgba(0, 0, 0, 0.1)")]
    [InlineData("none", "none")]
    [InlineData("0 0 10px #fff", "0 0 10px #fff")]
    [InlineData("2px 2px 5px inset rgba(0,0,0,0.2)", "2px 2px 5px inset rgba(0,0,0,0.2)")]
    public void SanitizeShadow_AcceptsValidInput(string validInput, string expected)
    {
        // Act
        var result = CssSanitizer.SanitizeShadow(validInput);

        // Assert
        result.Should().Be(expected, "valid shadow values should be accepted");
    }

    #endregion

    #region Memory Exhaustion Tests

    [Fact]
    public void FuzzyFilter_TruncatesLongSearchText()
    {
        // Arrange
        var filter = new FuzzyFilter<TestItem>();
        var items = new[]
        {
            new TestItem { Name = "Test" }
        };

        // Create a 10,000 character search string
        var longSearch = new string('a', 10000);

        // Act - Should not throw OutOfMemoryException
        var result = filter.Filter(items, longSearch, x => x.Name);

        // Assert
        result.Should().NotBeNull("filter should handle long search strings safely");
    }

    [Fact]
    public void StartsWithFilter_TruncatesLongSearchText()
    {
        // Arrange
        var filter = new StartsWithFilter<TestItem>();
        var items = new[]
        {
            new TestItem { Name = "Test" }
        };

        // Create a 10,000 character search string
        var longSearch = new string('t', 10000);

        // Act
        var result = filter.Filter(items, longSearch, x => x.Name);

        // Assert
        result.Should().NotBeNull("filter should handle long search strings safely");
    }

    [Fact]
    public void ContainsFilter_TruncatesLongSearchText()
    {
        // Arrange
        var filter = new ContainsFilter<TestItem>();
        var items = new[]
        {
            new TestItem { Name = "Test" }
        };

        // Create a 10,000 character search string
        var longSearch = new string('e', 10000);

        // Act
        var result = filter.Filter(items, longSearch, x => x.Name);

        // Assert
        result.Should().NotBeNull("filter should handle long search strings safely");
    }

    [Fact]
    public void FuzzyFilter_LevenshteinDistance_HandlesLongStrings()
    {
        // Arrange
        var filter = new FuzzyFilter<TestItem>();
        var items = new[]
        {
            new TestItem { Name = new string('a', 5000) } // 5000 char item
        };

        var longSearch = new string('a', 3000); // 3000 char search

        // Act - Should not allocate 15MB+ for a single comparison
        var result = filter.Filter(items, longSearch, x => x.Name).ToList();

        // Assert
        result.Should().NotBeNull("Levenshtein should handle long strings without excessive memory");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ThemeManager_BuildCustomPropertyStyle_SanitizesAllValues()
    {
        // Arrange
        var themeManager = new Theming.ThemeManager();
        var options = new Theming.ThemeOptions
        {
            Colors = new Theming.ColorOptions
            {
                Primary = "\"; position: fixed", // Malicious
                Background = "#FFFFFF" // Valid
            },
            Spacing = new Theming.SpacingOptions
            {
                BorderRadius = "4px; } body { display: none", // Malicious
                InputPadding = "8px" // Valid
            }
        };

        // Act
        var result = themeManager.BuildCustomPropertyStyle(options);

        // Assert
        result.Should().NotContain("position: fixed", "malicious CSS should be stripped");
        result.Should().NotContain("display: none", "malicious CSS should be stripped");
        result.Should().Contain("--ebd-ac-bg: #FFFFFF", "valid CSS should be included");
        result.Should().Contain("--ebd-ac-input-padding: 8px", "valid CSS should be included");
    }

    #endregion

    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
    }
}
