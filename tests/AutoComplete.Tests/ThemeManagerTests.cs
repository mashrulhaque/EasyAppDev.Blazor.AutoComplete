using EasyAppDev.Blazor.AutoComplete.Theming;
using EasyAppDev.Blazor.AutoComplete.Options;
using FluentAssertions;
using Microsoft.JSInterop;
using Moq;

namespace AutoComplete.Tests;

/// <summary>
/// Unit tests for ThemeManager service.
/// </summary>
public class ThemeManagerTests
{
    private readonly Mock<IJSRuntime> _mockJsRuntime;
    private readonly ThemeManager _themeManager;

    public ThemeManagerTests()
    {
        _mockJsRuntime = new Mock<IJSRuntime>();
        _themeManager = new ThemeManager(_mockJsRuntime.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenJSRuntimeIsNull()
    {
        // Act
        Action act = () => new ThemeManager(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("jsRuntime");
    }

    #endregion

    #region BuildCustomPropertyStyle Tests

    [Fact]
    public void BuildCustomPropertyStyle_ReturnsEmptyString_WhenOptionsIsNull()
    {
        // Act
        var result = _themeManager.BuildCustomPropertyStyle(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildCustomPropertyStyle_ReturnsEmptyString_WhenAllPropertiesAreNull()
    {
        // Arrange
        var options = new ThemeOptions();

        // Act
        var result = _themeManager.BuildCustomPropertyStyle(options);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildCustomPropertyStyle_BuildsColorProperties_Correctly()
    {
        // Arrange
        var options = new ThemeOptions
        {
            Colors = new ColorOptions
            {
                Primary = "#007bff",
                Background = "#ffffff",
                Text = "#212529",
                Border = "#ced4da"
            }
        };

        // Act
        var result = _themeManager.BuildCustomPropertyStyle(options);

        // Assert
        result.Should().Contain("--ebd-ac-primary: #007bff");
        result.Should().Contain("--ebd-ac-bg: #ffffff");
        result.Should().Contain("--ebd-ac-text: #212529");
        result.Should().Contain("--ebd-ac-border: #ced4da");
    }

    [Fact]
    public void BuildCustomPropertyStyle_BuildsSpacingProperties_Correctly()
    {
        // Arrange
        var options = new ThemeOptions
        {
            Spacing = new SpacingOptions
            {
                InputPadding = "12px 16px",
                ItemPadding = "8px 12px",
                BorderRadius = "8px",
                DropdownGap = "4px"
            }
        };

        // Act
        var result = _themeManager.BuildCustomPropertyStyle(options);

        // Assert
        result.Should().Contain("--ebd-ac-input-padding: 12px 16px");
        result.Should().Contain("--ebd-ac-item-padding: 8px 12px");
        result.Should().Contain("--ebd-ac-border-radius: 8px");
        result.Should().Contain("--ebd-ac-dropdown-gap: 4px");
    }

    [Fact]
    public void BuildCustomPropertyStyle_BuildsTypographyProperties_Correctly()
    {
        // Arrange
        var options = new ThemeOptions
        {
            Typography = new TypographyOptions
            {
                FontFamily = "Roboto, sans-serif",
                FontSize = "14px",
                LineHeight = "1.5",
                FontWeight = "500"
            }
        };

        // Act
        var result = _themeManager.BuildCustomPropertyStyle(options);

        // Assert
        result.Should().Contain("--ebd-ac-font-family: Roboto, sans-serif");
        result.Should().Contain("--ebd-ac-font-size: 14px");
        result.Should().Contain("--ebd-ac-line-height: 1.5");
        result.Should().Contain("--ebd-ac-font-weight: 500");
    }

    [Fact]
    public void BuildCustomPropertyStyle_BuildsEffectProperties_Correctly()
    {
        // Arrange
        var options = new ThemeOptions
        {
            Effects = new EffectOptions
            {
                FocusShadow = "0 0 0 3px rgba(0, 0, 0, 0.1)",
                DropdownShadow = "0 4px 8px rgba(0, 0, 0, 0.15)",
                TransitionDuration = "200ms",
                BorderWidth = "2px"
            }
        };

        // Act
        var result = _themeManager.BuildCustomPropertyStyle(options);

        // Assert
        result.Should().Contain("--ebd-ac-focus-shadow: 0 0 0 3px rgba(0, 0, 0, 0.1)");
        result.Should().Contain("--ebd-ac-dropdown-shadow: 0 4px 8px rgba(0, 0, 0, 0.15)");
        result.Should().Contain("--ebd-ac-transition-duration: 200ms");
        result.Should().Contain("--ebd-ac-border-width: 2px");
    }

    [Fact]
    public void BuildCustomPropertyStyle_AutoGeneratesHoverColor_FromPrimaryColor()
    {
        // Arrange
        var options = new ThemeOptions
        {
            Colors = new ColorOptions
            {
                Primary = "#007bff"
                // Hover color not specified - should be auto-generated
            }
        };

        // Act
        var result = _themeManager.BuildCustomPropertyStyle(options);

        // Assert
        result.Should().Contain("--ebd-ac-primary: #007bff");
        result.Should().Contain("--ebd-ac-hover: #"); // Should have generated hover color
        result.Should().NotContain("--ebd-ac-hover: #007bff"); // Should be lighter
    }

    [Fact]
    public void BuildCustomPropertyStyle_AutoGeneratesSelectedColor_FromPrimaryColor()
    {
        // Arrange
        var options = new ThemeOptions
        {
            Colors = new ColorOptions
            {
                Primary = "#007bff"
                // Selected color not specified - should be auto-generated
            }
        };

        // Act
        var result = _themeManager.BuildCustomPropertyStyle(options);

        // Assert
        result.Should().Contain("--ebd-ac-primary: #007bff");
        result.Should().Contain("--ebd-ac-selected: #"); // Should have generated selected color
        result.Should().NotContain("--ebd-ac-selected: #007bff"); // Should be lighter
    }

    [Fact]
    public void BuildCustomPropertyStyle_UsesExplicitHoverColor_WhenProvided()
    {
        // Arrange
        var options = new ThemeOptions
        {
            Colors = new ColorOptions
            {
                Primary = "#007bff",
                Hover = "#0056b3" // Explicit hover color
            }
        };

        // Act
        var result = _themeManager.BuildCustomPropertyStyle(options);

        // Assert
        result.Should().Contain("--ebd-ac-hover: #0056b3");
    }

    [Fact]
    public void BuildCustomPropertyStyle_CombinesAllCategories_Correctly()
    {
        // Arrange
        var options = new ThemeOptions
        {
            Colors = new ColorOptions { Primary = "#007bff" },
            Spacing = new SpacingOptions { BorderRadius = "8px" },
            Typography = new TypographyOptions { FontFamily = "Arial" },
            Effects = new EffectOptions { TransitionDuration = "300ms" }
        };

        // Act
        var result = _themeManager.BuildCustomPropertyStyle(options);

        // Assert
        result.Should().Contain("--ebd-ac-primary: #007bff");
        result.Should().Contain("--ebd-ac-border-radius: 8px");
        result.Should().Contain("--ebd-ac-font-family: Arial");
        result.Should().Contain("--ebd-ac-transition-duration: 300ms");
    }

    [Fact]
    public void BuildCustomPropertyStyle_SeparatesPropertiesWithSemicolon()
    {
        // Arrange
        var options = new ThemeOptions
        {
            Colors = new ColorOptions
            {
                Primary = "#007bff",
                Background = "#ffffff"
            }
        };

        // Act
        var result = _themeManager.BuildCustomPropertyStyle(options);

        // Assert
        result.Should().Contain("; ");
    }

    #endregion

    #region ApplyThemeAsync Tests

    [Fact]
    public async Task ApplyThemeAsync_ReturnsNull_WhenPresetIsNone()
    {
        // Act
        var result = await _themeManager.ApplyThemeAsync(ThemePreset.None);

        // Assert
        result.Should().BeNull();
    }

    // Note: JS interop behavior is tested via integration tests in ThemingComponentTests
    // The fallback styles are tested via GetFallbackThemeStyle tests below

    #endregion

    #region GetThemeClass Tests

    [Theory]
    [InlineData(Theme.Light, "ebd-ac-theme-light")]
    [InlineData(Theme.Dark, "ebd-ac-theme-dark")]
    [InlineData(Theme.Auto, "ebd-ac-theme-auto")]
    public void GetThemeClass_ReturnsCorrectClass_ForEachTheme(Theme theme, string expectedClass)
    {
        // Act
        var result = _themeManager.GetThemeClass(theme);

        // Assert
        result.Should().Be(expectedClass);
    }

    [Fact]
    public void GetThemeClass_ReturnsAutoClass_ForInvalidTheme()
    {
        // Arrange
        var invalidTheme = (Theme)999;

        // Act
        var result = _themeManager.GetThemeClass(invalidTheme);

        // Assert
        result.Should().Be("ebd-ac-theme-auto");
    }

    #endregion

    #region GetBootstrapThemeClass Tests

    [Theory]
    [InlineData(BootstrapTheme.Default, "")]
    [InlineData(BootstrapTheme.Primary, "ebd-ac-bs-primary")]
    [InlineData(BootstrapTheme.Secondary, "ebd-ac-bs-secondary")]
    [InlineData(BootstrapTheme.Success, "ebd-ac-bs-success")]
    [InlineData(BootstrapTheme.Danger, "ebd-ac-bs-danger")]
    [InlineData(BootstrapTheme.Warning, "ebd-ac-bs-warning")]
    [InlineData(BootstrapTheme.Info, "ebd-ac-bs-info")]
    [InlineData(BootstrapTheme.Light, "ebd-ac-bs-light")]
    [InlineData(BootstrapTheme.Dark, "ebd-ac-bs-dark")]
    public void GetBootstrapThemeClass_ReturnsCorrectClass_ForEachTheme(BootstrapTheme theme, string expectedClass)
    {
        // Act
        var result = _themeManager.GetBootstrapThemeClass(theme);

        // Assert
        result.Should().Be(expectedClass);
    }

    [Fact]
    public void GetBootstrapThemeClass_ReturnsEmptyString_ForInvalidTheme()
    {
        // Arrange
        var invalidTheme = (BootstrapTheme)999;

        // Act
        var result = _themeManager.GetBootstrapThemeClass(invalidTheme);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetThemePresetClass Tests

    [Theory]
    [InlineData(ThemePreset.None, "")]
    [InlineData(ThemePreset.Material, "ebd-ac-theme-preset-material")]
    [InlineData(ThemePreset.Fluent, "ebd-ac-theme-preset-fluent")]
    [InlineData(ThemePreset.Modern, "ebd-ac-theme-preset-modern")]
    [InlineData(ThemePreset.Bootstrap, "ebd-ac-theme-preset-bootstrap")]
    public void GetThemePresetClass_ReturnsCorrectClass_ForEachPreset(ThemePreset preset, string expectedClass)
    {
        // Act
        var result = _themeManager.GetThemePresetClass(preset);

        // Assert
        result.Should().Be(expectedClass);
    }

    [Fact]
    public void GetThemePresetClass_ReturnsEmptyString_ForInvalidPreset()
    {
        // Arrange
        var invalidPreset = (ThemePreset)999;

        // Act
        var result = _themeManager.GetThemePresetClass(invalidPreset);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetSizeClass Tests

    [Theory]
    [InlineData(ComponentSize.Default, "ebd-ac-size-default")]
    [InlineData(ComponentSize.Compact, "ebd-ac-size-compact")]
    [InlineData(ComponentSize.Large, "ebd-ac-size-large")]
    public void GetSizeClass_ReturnsCorrectClass_ForEachSize(ComponentSize size, string expectedClass)
    {
        // Act
        var result = _themeManager.GetSizeClass(size);

        // Assert
        result.Should().Be(expectedClass);
    }

    #endregion

    #region GetFallbackThemeStyle Tests

    [Fact]
    public void GetFallbackThemeStyle_ReturnsMaterialStyle_ForMaterialPreset()
    {
        // Act
        var result = ThemeManager.GetFallbackThemeStyle(ThemePreset.Material);

        // Assert
        result.Should().Contain("--ebd-ac-primary: #6200EE");
        result.Should().Contain("--ebd-ac-bg: #FFFFFF");
        result.Should().Contain("--ebd-ac-text: #1C1B1F");
        result.Should().Contain("--ebd-ac-border-radius: 4px");
    }

    [Fact]
    public void GetFallbackThemeStyle_ReturnsFluentStyle_ForFluentPreset()
    {
        // Act
        var result = ThemeManager.GetFallbackThemeStyle(ThemePreset.Fluent);

        // Assert
        result.Should().Contain("--ebd-ac-primary: #0078D4");
        result.Should().Contain("--ebd-ac-bg: #FFFFFF");
        result.Should().Contain("--ebd-ac-text: #323130");
        result.Should().Contain("--ebd-ac-border-radius: 2px");
    }

    [Fact]
    public void GetFallbackThemeStyle_ReturnsModernStyle_ForModernPreset()
    {
        // Act
        var result = ThemeManager.GetFallbackThemeStyle(ThemePreset.Modern);

        // Assert
        result.Should().Contain("--ebd-ac-primary: #2563EB");
        result.Should().Contain("--ebd-ac-bg: #FFFFFF");
        result.Should().Contain("--ebd-ac-text: #111827");
        result.Should().Contain("--ebd-ac-border-radius: 2px");
    }

    [Fact]
    public void GetFallbackThemeStyle_ReturnsBootstrapStyle_ForBootstrapPreset()
    {
        // Act
        var result = ThemeManager.GetFallbackThemeStyle(ThemePreset.Bootstrap);

        // Assert
        result.Should().Contain("--ebd-ac-primary: #0D6EFD");
        result.Should().Contain("--ebd-ac-bg: #FFFFFF");
        result.Should().Contain("--ebd-ac-text: #212529");
        result.Should().Contain("--ebd-ac-border-radius: 0.375rem");
    }

    [Fact]
    public void GetFallbackThemeStyle_ReturnsEmptyString_ForNonePreset()
    {
        // Act
        var result = ThemeManager.GetFallbackThemeStyle(ThemePreset.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetFallbackThemeStyle_ReturnsEmptyString_ForInvalidPreset()
    {
        // Arrange
        var invalidPreset = (ThemePreset)999;

        // Act
        var result = ThemeManager.GetFallbackThemeStyle(invalidPreset);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region All Color Properties Tests

    [Fact]
    public void BuildCustomPropertyStyle_IncludesAllColorProperties_WhenProvided()
    {
        // Arrange
        var options = new ThemeOptions
        {
            Colors = new ColorOptions
            {
                Primary = "#007bff",
                Background = "#ffffff",
                Text = "#212529",
                TextSecondary = "#6c757d",
                Border = "#ced4da",
                BorderFocus = "#86b7fe",
                Hover = "#f8f9fa",
                Selected = "#e7f1ff",
                SelectedText = "#0a58ca",
                Disabled = "#e9ecef",
                Error = "#dc3545",
                Shadow = "rgba(0,0,0,0.1)",
                DropdownBackground = "#fff",
                Focus = "#0d6efd",
                Placeholder = "#6c757d"
            }
        };

        // Act
        var result = _themeManager.BuildCustomPropertyStyle(options);

        // Assert
        result.Should().Contain("--ebd-ac-primary: #007bff");
        result.Should().Contain("--ebd-ac-bg: #ffffff");
        result.Should().Contain("--ebd-ac-text: #212529");
        result.Should().Contain("--ebd-ac-text-secondary: #6c757d");
        result.Should().Contain("--ebd-ac-border: #ced4da");
        result.Should().Contain("--ebd-ac-border-focus: #86b7fe");
        result.Should().Contain("--ebd-ac-hover: #f8f9fa");
        result.Should().Contain("--ebd-ac-selected: #e7f1ff");
        result.Should().Contain("--ebd-ac-selected-text: #0a58ca");
        result.Should().Contain("--ebd-ac-disabled: #e9ecef");
        result.Should().Contain("--ebd-ac-error: #dc3545");
        result.Should().Contain("--ebd-ac-shadow: rgba(0,0,0,0.1)");
        result.Should().Contain("--ebd-ac-dropdown-bg: #fff");
        result.Should().Contain("--ebd-ac-focus: #0d6efd");
        result.Should().Contain("--ebd-ac-placeholder: #6c757d");
    }

    #endregion
}
