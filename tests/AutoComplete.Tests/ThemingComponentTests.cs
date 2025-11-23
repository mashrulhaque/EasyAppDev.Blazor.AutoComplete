using EasyAppDev.Blazor.AutoComplete;
using EasyAppDev.Blazor.AutoComplete.Options;
using EasyAppDev.Blazor.AutoComplete.Theming;
using Microsoft.AspNetCore.Components;
using Bunit.JSInterop;

namespace AutoComplete.Tests;

/// <summary>
/// Component tests for theming functionality using bUnit.
/// </summary>
public class ThemingComponentTests : TestContext
{
    private readonly List<string> _testCountries = new()
    {
        "United States",
        "Canada",
        "Mexico",
        "Brazil",
        "United Kingdom"
    };

    [Theory]
    [InlineData(ComponentSize.Compact, "ebd-ac-size-compact")]
    [InlineData(ComponentSize.Default, "ebd-ac-size-default")]
    [InlineData(ComponentSize.Large, "ebd-ac-size-large")]
    public void Component_AppliesSizeClass(ComponentSize size, string expectedClass)
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.Size, size));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        container.ClassList.Should().Contain(expectedClass);
    }

    [Fact]
    public void Component_AppliesTransitionsClass_WhenEnabled()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.EnableThemeTransitions, true));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        container.ClassList.Should().Contain("ebd-ac-theme-transitions");
    }

    [Fact]
    public void Component_DoesNotApplyTransitionsClass_WhenDisabled()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.EnableThemeTransitions, false));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        container.ClassList.Should().NotContain("ebd-ac-theme-transitions");
    }

    [Fact]
    public void Component_AppliesInlineCustomProperties_Colors()
    {
        // Arrange
        var themeOptions = new ThemeOptions
        {
            Colors = new ColorOptions
            {
                Primary = "#FF6B6B",
                Background = "#FFFFFF",
                Text = "#333333"
            }
        };

        // Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.ThemeOverrides, themeOptions));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");
        style.Should().Contain("--ebd-ac-primary: #FF6B6B");
        style.Should().Contain("--ebd-ac-bg: #FFFFFF");
        style.Should().Contain("--ebd-ac-text: #333333");
    }

    [Fact]
    public void Component_AppliesInlineCustomProperties_Spacing()
    {
        // Arrange
        var themeOptions = new ThemeOptions
        {
            Spacing = new SpacingOptions
            {
                BorderRadius = "8px",
                InputPadding = "12px 16px",
                ItemPadding = "8px 12px"
            }
        };

        // Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.ThemeOverrides, themeOptions));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");
        style.Should().Contain("--ebd-ac-border-radius: 8px");
        style.Should().Contain("--ebd-ac-input-padding: 12px 16px");
        style.Should().Contain("--ebd-ac-item-padding: 8px 12px");
    }

    [Fact]
    public void Component_AppliesInlineCustomProperties_Typography()
    {
        // Arrange
        var themeOptions = new ThemeOptions
        {
            Typography = new TypographyOptions
            {
                FontFamily = "Roboto, sans-serif",
                FontSize = "14px",
                FontWeight = "500"
            }
        };

        // Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.ThemeOverrides, themeOptions));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");
        style.Should().Contain("--ebd-ac-font-family: Roboto, sans-serif");
        style.Should().Contain("--ebd-ac-font-size: 14px");
        style.Should().Contain("--ebd-ac-font-weight: 500");
    }

    [Fact]
    public void Component_AppliesInlineCustomProperties_Effects()
    {
        // Arrange
        var themeOptions = new ThemeOptions
        {
            Effects = new EffectOptions
            {
                FocusShadow = "0 0 0 3px rgba(0, 0, 0, 0.1)",
                DropdownShadow = "0 4px 8px rgba(0, 0, 0, 0.15)",
                TransitionDuration = "200ms"
            }
        };

        // Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.ThemeOverrides, themeOptions));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");
        style.Should().Contain("--ebd-ac-focus-shadow: 0 0 0 3px rgba(0, 0, 0, 0.1)");
        style.Should().Contain("--ebd-ac-dropdown-shadow: 0 4px 8px rgba(0, 0, 0, 0.15)");
        style.Should().Contain("--ebd-ac-transition-duration: 200ms");
    }

    [Fact]
    public void Component_DoesNotApplyCustomProperties_WhenNull()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");

        // Style attribute should either be null/empty or only contain size class variables
        if (!string.IsNullOrEmpty(style))
        {
            // If there's a style, it should not contain custom color overrides
            style.Should().NotContain("#");
        }
    }

    [Fact]
    public void Component_CombinesSizeClassAndCustomProperties()
    {
        // Arrange
        var themeOptions = new ThemeOptions
        {
            Colors = new ColorOptions { Primary = "#007bff" },
            Spacing = new SpacingOptions { BorderRadius = "12px" }
        };

        // Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.Size, ComponentSize.Large)
            .Add(p => p.ThemeOverrides, themeOptions));

        // Assert
        var container = cut.Find(".ebd-ac-container");

        // Should have size class
        container.ClassList.Should().Contain("ebd-ac-size-large");

        // Should have custom properties
        var style = container.GetAttribute("style");
        style.Should().Contain("--ebd-ac-primary: #007bff");
        style.Should().Contain("--ebd-ac-border-radius: 12px");
    }

    [Fact]
    public void Component_AppliesDefaultSize_WhenNotSpecified()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        container.ClassList.Should().Contain("ebd-ac-size-default");
    }

    [Fact]
    public void Component_EnablesTransitions_ByDefault()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        container.ClassList.Should().Contain("ebd-ac-theme-transitions");
    }

    [Fact]
    public async Task Component_LoadsThemeViaJS_WhenPresetSpecified()
    {
        // Arrange
        var jsInterop = JSInterop.SetupVoid("EasyAppDevAutoComplete.loadTheme", "material");

        // Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.ThemePreset, ThemePreset.Material));

        await Task.Delay(100); // Allow async operations to complete

        // Assert
        var invocations = JSInterop.Invocations["EasyAppDevAutoComplete.loadTheme"];
        invocations.Should().HaveCountGreaterThan(0);
        invocations[0].Arguments[0].Should().Be("material");
    }

    [Theory]
    [InlineData(ThemePreset.Material, "material")]
    [InlineData(ThemePreset.Fluent, "fluent")]
    [InlineData(ThemePreset.Modern, "modern")]
    [InlineData(ThemePreset.Bootstrap, "bootstrap")]
    public async Task Component_LoadsCorrectThemeFile_ForEachPreset(ThemePreset preset, string expectedThemeName)
    {
        // Arrange
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupVoid("EasyAppDevAutoComplete.loadTheme", expectedThemeName);

        // Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.ThemePreset, preset));

        await Task.Delay(100); // Allow async operations to complete

        // Assert
        var invocations = JSInterop.Invocations["EasyAppDevAutoComplete.loadTheme"];
        invocations.Should().HaveCountGreaterThan(0);
        invocations[0].Arguments[0].Should().Be(expectedThemeName);
    }

    [Fact]
    public async Task Component_DoesNotLoadTheme_WhenPresetIsNone()
    {
        // Arrange
        JSInterop.Mode = JSRuntimeMode.Loose; // Allow JS calls but track them

        // Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.ThemePreset, ThemePreset.None));

        await Task.Delay(100); // Allow async operations to complete

        // Assert
        // Verify component renders without calling loadTheme
        // Count all JS invocations - should be 0 since ThemePreset.None shouldn't trigger any
        var allInvocations = JSInterop.Invocations.Count;
        allInvocations.Should().Be(0, "ThemePreset.None should not trigger any JS calls");
    }

    [Fact]
    public void Component_AppliesMultipleCustomProperties_Together()
    {
        // Arrange
        var themeOptions = new ThemeOptions
        {
            Colors = new ColorOptions
            {
                Primary = "#FF6B6B",
                Background = "#FFFFFF"
            },
            Spacing = new SpacingOptions
            {
                BorderRadius = "8px",
                InputPadding = "12px 16px"
            },
            Typography = new TypographyOptions
            {
                FontFamily = "Inter, sans-serif",
                FontSize = "16px"
            },
            Effects = new EffectOptions
            {
                TransitionDuration = "300ms"
            }
        };

        // Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.ThemeOverrides, themeOptions));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");

        style.Should().Contain("--ebd-ac-primary: #FF6B6B");
        style.Should().Contain("--ebd-ac-bg: #FFFFFF");
        style.Should().Contain("--ebd-ac-border-radius: 8px");
        style.Should().Contain("--ebd-ac-font-family: Inter, sans-serif");
        style.Should().Contain("--ebd-ac-font-size: 16px");
        style.Should().Contain("--ebd-ac-input-padding: 12px 16px");
        style.Should().Contain("--ebd-ac-transition-duration: 300ms");
    }

    // ========== Individual Theme Parameters Tests (New Hybrid API) ==========

    [Fact]
    public void Component_AppliesIndividualThemeParameter_PrimaryColor()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.PrimaryColor, "#FF0000"));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");
        style.Should().Contain("--ebd-ac-primary: #FF0000");
    }

    [Fact]
    public void Component_AppliesMultipleIndividualThemeParameters()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.PrimaryColor, "#FF0000")
            .Add(p => p.BackgroundColor, "#FFFFFF")
            .Add(p => p.BorderRadius, "8px")
            .Add(p => p.FontSize, "16px"));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");
        style.Should().Contain("--ebd-ac-primary: #FF0000");
        style.Should().Contain("--ebd-ac-bg: #FFFFFF");
        style.Should().Contain("--ebd-ac-border-radius: 8px");
        style.Should().Contain("--ebd-ac-font-size: 16px");
    }

    [Fact]
    public void Component_IndividualParametersOverrideThemeOptions()
    {
        // Arrange
        var themeOptions = new ThemeOptions
        {
            Colors = new ColorOptions
            {
                Primary = "#0000FF", // Blue (should be overridden)
                Background = "#F0F0F0"
            },
            Spacing = new SpacingOptions
            {
                BorderRadius = "4px" // Should be overridden
            }
        };

        // Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.ThemeOverrides, themeOptions)
            .Add(p => p.PrimaryColor, "#FF0000") // Red (overrides blue)
            .Add(p => p.BorderRadius, "12px")); // Overrides 4px

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");

        // Individual parameters should win
        style.Should().Contain("--ebd-ac-primary: #FF0000");
        style.Should().Contain("--ebd-ac-border-radius: 12px");

        // Non-overridden ThemeOptions values should still apply
        style.Should().Contain("--ebd-ac-bg: #F0F0F0");
    }

    [Fact]
    public void Component_PreservesThemeOptionsAdvancedProperties_WhenUsingIndividualParams()
    {
        // Arrange
        var themeOptions = new ThemeOptions
        {
            Colors = new ColorOptions
            {
                Primary = "#0000FF",
                TextSecondary = "#666666", // Advanced property not available as individual param
                BorderFocus = "#007bff" // Advanced property not available as individual param
            }
        };

        // Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.ThemeOverrides, themeOptions)
            .Add(p => p.PrimaryColor, "#FF0000")); // Override only primary

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");

        // Overridden value
        style.Should().Contain("--ebd-ac-primary: #FF0000");

        // Advanced properties should be preserved
        style.Should().Contain("--ebd-ac-text-secondary: #666666");
        style.Should().Contain("--ebd-ac-border-focus: #007bff");
    }

    [Fact]
    public void Component_AllIndividualColorParameters_MapToCorrectCssVariables()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.PrimaryColor, "#FF0000")
            .Add(p => p.BackgroundColor, "#FFFFFF")
            .Add(p => p.TextColor, "#333333")
            .Add(p => p.BorderColor, "#DDDDDD")
            .Add(p => p.HoverColor, "#F5F5F5")
            .Add(p => p.SelectedColor, "#E3F2FD"));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");
        style.Should().Contain("--ebd-ac-primary: #FF0000");
        style.Should().Contain("--ebd-ac-bg: #FFFFFF");
        style.Should().Contain("--ebd-ac-text: #333333");
        style.Should().Contain("--ebd-ac-border: #DDDDDD");
        style.Should().Contain("--ebd-ac-hover: #F5F5F5");
        style.Should().Contain("--ebd-ac-selected: #E3F2FD");
    }

    [Fact]
    public void Component_AllIndividualSpacingAndTypographyParameters_MapToCorrectCssVariables()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.BorderRadius, "6px")
            .Add(p => p.FontFamily, "Arial, sans-serif")
            .Add(p => p.FontSize, "14px")
            .Add(p => p.DropdownShadow, "0 2px 4px rgba(0,0,0,0.1)"));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");
        style.Should().Contain("--ebd-ac-border-radius: 6px");
        style.Should().Contain("--ebd-ac-font-family: Arial, sans-serif");
        style.Should().Contain("--ebd-ac-font-size: 14px");
        style.Should().Contain("--ebd-ac-dropdown-shadow: 0 2px 4px rgba(0,0,0,0.1)");
    }

    [Fact]
    public void Component_HandlesNullIndividualParameters_Gracefully()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.PrimaryColor, null)
            .Add(p => p.BackgroundColor, null));

        // Assert - Should render without errors
        var container = cut.Find(".ebd-ac-container");
        container.Should().NotBeNull();
    }

    [Fact]
    public void Component_MergesAllThreeOptionsCategories_WithIndividualParams()
    {
        // Arrange
        var themeOptions = new ThemeOptions
        {
            Colors = new ColorOptions { Primary = "#0000FF" },
            Spacing = new SpacingOptions { InputPadding = "10px 14px" },
            Typography = new TypographyOptions { LineHeight = "1.6" },
            Effects = new EffectOptions { TransitionDuration = "200ms" }
        };

        // Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.ThemeOverrides, themeOptions)
            .Add(p => p.PrimaryColor, "#FF0000") // Override color
            .Add(p => p.BorderRadius, "8px") // Add spacing param
            .Add(p => p.FontFamily, "Roboto")); // Add typography param

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");

        // Overridden color
        style.Should().Contain("--ebd-ac-primary: #FF0000");

        // Original ThemeOptions values
        style.Should().Contain("--ebd-ac-input-padding: 10px 14px");
        style.Should().Contain("--ebd-ac-line-height: 1.6");
        style.Should().Contain("--ebd-ac-transition-duration: 200ms");

        // New individual params
        style.Should().Contain("--ebd-ac-border-radius: 8px");
        style.Should().Contain("--ebd-ac-font-family: Roboto");
    }

    [Fact]
    public void Component_OnlyIndividualParameters_NoThemeOptions()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<string>>(parameters => parameters
            .Add(p => p.Items, _testCountries)
            .Add(p => p.PrimaryColor, "#FF0000")
            .Add(p => p.FontSize, "16px"));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        var style = container.GetAttribute("style");
        style.Should().Contain("--ebd-ac-primary: #FF0000");
        style.Should().Contain("--ebd-ac-font-size: 16px");
    }
}
