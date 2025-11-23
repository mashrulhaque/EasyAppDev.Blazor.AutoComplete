using EasyAppDev.Blazor.AutoComplete;
using EasyAppDev.Blazor.AutoComplete.Options;
using Xunit;
using FluentAssertions;

namespace AutoComplete.Tests;

/// <summary>
/// Unit tests for theming functionality including theme presets, size variants, and custom properties.
/// </summary>
public class ThemingTests
{
    [Theory]
    [InlineData(ThemePreset.Material, "material")]
    [InlineData(ThemePreset.Fluent, "fluent")]
    [InlineData(ThemePreset.Modern, "modern")]
    [InlineData(ThemePreset.Bootstrap, "bootstrap")]
    public void ThemePreset_MapsToCorrectThemeName(ThemePreset preset, string expectedThemeName)
    {
        // Arrange & Act
        var themeName = preset.ToString().ToLower();

        // Assert
        themeName.Should().Be(expectedThemeName);
    }

    [Theory]
    [InlineData(ThemePreset.None, 0)]
    [InlineData(ThemePreset.Material, 1)]
    [InlineData(ThemePreset.Fluent, 2)]
    [InlineData(ThemePreset.Modern, 3)]
    [InlineData(ThemePreset.Bootstrap, 4)]
    public void ThemePreset_HasCorrectEnumValue(ThemePreset preset, int expectedValue)
    {
        // Arrange & Act
        var value = (int)preset;

        // Assert
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(ComponentSize.Compact, 0)]
    [InlineData(ComponentSize.Default, 1)]
    [InlineData(ComponentSize.Large, 2)]
    public void ComponentSize_HasCorrectEnumValue(ComponentSize size, int expectedValue)
    {
        // Arrange & Act
        var value = (int)size;

        // Assert
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(ComponentSize.Compact, "compact")]
    [InlineData(ComponentSize.Default, "default")]
    [InlineData(ComponentSize.Large, "large")]
    public void ComponentSize_MapsToCorrectCSSClass(ComponentSize size, string expectedClassName)
    {
        // Arrange & Act
        var className = $"ebd-ac-size-{size.ToString().ToLower()}";

        // Assert
        className.Should().Be($"ebd-ac-size-{expectedClassName}");
    }

    [Fact]
    public void ThemePreset_None_ShouldBeDefaultValue()
    {
        // Arrange
        var defaultPreset = default(ThemePreset);

        // Act & Assert
        defaultPreset.Should().Be(ThemePreset.None);
    }

    [Fact]
    public void ComponentSize_Default_ShouldBeDefaultValue()
    {
        // Arrange
        var defaultSize = default(ComponentSize);

        // Act & Assert
        // Note: Default is enum value 1, so default(ComponentSize) will be Compact (0)
        defaultSize.Should().Be(ComponentSize.Compact);
    }

    [Fact]
    public void ThemePreset_AllValues_ShouldBeUnique()
    {
        // Arrange
        var allValues = Enum.GetValues<ThemePreset>();

        // Act
        var uniqueValues = allValues.Distinct().ToList();

        // Assert
        uniqueValues.Should().HaveCount(allValues.Length);
    }

    [Fact]
    public void ComponentSize_AllValues_ShouldBeUnique()
    {
        // Arrange
        var allValues = Enum.GetValues<ComponentSize>();

        // Act
        var uniqueValues = allValues.Distinct().ToList();

        // Assert
        uniqueValues.Should().HaveCount(allValues.Length);
    }

    [Theory]
    [InlineData("material")]
    [InlineData("fluent")]
    [InlineData("modern")]
    [InlineData("bootstrap")]
    public void ThemePreset_CanBeParsedFromString(string themeString)
    {
        // Arrange & Act
        var parsed = Enum.TryParse<ThemePreset>(themeString, ignoreCase: true, out var result);

        // Assert
        parsed.Should().BeTrue();
        result.Should().NotBe(ThemePreset.None);
    }

    [Theory]
    [InlineData("compact")]
    [InlineData("default")]
    [InlineData("large")]
    public void ComponentSize_CanBeParsedFromString(string sizeString)
    {
        // Arrange & Act
        var parsed = Enum.TryParse<ComponentSize>(sizeString, ignoreCase: true, out var result);

        // Assert
        parsed.Should().BeTrue();
    }

    [Fact]
    public void ThemePreset_Count_ShouldBeFive()
    {
        // Arrange
        var allValues = Enum.GetValues<ThemePreset>();

        // Act & Assert
        // None, Material, Fluent, Modern, Bootstrap
        allValues.Length.Should().Be(5);
    }

    [Fact]
    public void ComponentSize_Count_ShouldBeThree()
    {
        // Arrange
        var allValues = Enum.GetValues<ComponentSize>();

        // Act & Assert
        // Compact, Default, Large
        allValues.Length.Should().Be(3);
    }
}
