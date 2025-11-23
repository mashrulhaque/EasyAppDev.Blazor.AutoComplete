using FluentAssertions;
using EasyAppDev.Blazor.AutoComplete.Configuration;
using EasyAppDev.Blazor.AutoComplete.Options;
using EasyAppDev.Blazor.AutoComplete.Theming;
using Xunit;

namespace EasyAppDev.Blazor.AutoComplete.Tests;

/// <summary>
/// Tests for AutoCompleteConfigBuilder fluent API
/// </summary>
public class ConfigurationBuilderTests
{
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    [Fact]
    public void Builder_ShouldCreateEmptyConfig_WhenNoMethodsCalled()
    {
        // Arrange & Act
        var config = AutoCompleteConfig<TestItem>.Create().Build();

        // Assert
        config.Should().NotBeNull();
        config.Items.Should().BeNull();
        config.Theme.Should().Be(Theme.Auto);
        config.BootstrapTheme.Should().Be(BootstrapTheme.Default);
    }

    [Fact]
    public void WithSize_ShouldSetSizeProperty()
    {
        // Arrange & Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithSize(ComponentSize.Large)
            .Build();

        // Assert
        config.Size.Should().Be(ComponentSize.Large);
    }

    [Fact]
    public void WithSize_ShouldSupportCompactSize()
    {
        // Arrange & Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithSize(ComponentSize.Compact)
            .Build();

        // Assert
        config.Size.Should().Be(ComponentSize.Compact);
    }

    [Fact]
    public void WithSize_ShouldSupportDefaultSize()
    {
        // Arrange & Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithSize(ComponentSize.Default)
            .Build();

        // Assert
        config.Size.Should().Be(ComponentSize.Default);
    }

    [Fact]
    public void WithThemeTransitions_ShouldEnableTransitions_WhenCalledWithTrue()
    {
        // Arrange & Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithThemeTransitions(true)
            .Build();

        // Assert
        config.EnableThemeTransitions.Should().BeTrue();
    }

    [Fact]
    public void WithThemeTransitions_ShouldDisableTransitions_WhenCalledWithFalse()
    {
        // Arrange & Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithThemeTransitions(false)
            .Build();

        // Assert
        config.EnableThemeTransitions.Should().BeFalse();
    }

    [Fact]
    public void WithThemeTransitions_ShouldEnableTransitions_WhenCalledWithoutParameter()
    {
        // Arrange & Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithThemeTransitions()
            .Build();

        // Assert
        config.EnableThemeTransitions.Should().BeTrue();
    }

    [Fact]
    public void Builder_ShouldSupportMethodChaining_ForAllMethods()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new TestItem { Name = "Test 1", Description = "Desc 1" },
            new TestItem { Name = "Test 2", Description = "Desc 2" }
        };

        // Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithItems(items)
            .WithTextField(x => x.Name)
            .WithPlaceholder("Search...")
            .WithTheme(Theme.Dark)
            .WithBootstrapTheme(BootstrapTheme.Primary)
            .WithThemePreset(ThemePreset.Material)
            .WithSize(ComponentSize.Large)
            .WithThemeTransitions(true)
            .WithMinSearchLength(2)
            .WithMaxDisplayedItems(50)
            .WithDebounce(500)
            .WithAllowClear(true)
            .WithDisabled(false)
            .WithCloseOnSelect(true)
            .WithFilterStrategy(FilterStrategy.Contains)
            .Build();

        // Assert
        config.Items.Should().BeSameAs(items);
        config.TextField.Should().NotBeNull();
        config.Placeholder.Should().Be("Search...");
        config.Theme.Should().Be(Theme.Dark);
        config.BootstrapTheme.Should().Be(BootstrapTheme.Primary);
        config.ThemePreset.Should().Be(ThemePreset.Material);
        config.Size.Should().Be(ComponentSize.Large);
        config.EnableThemeTransitions.Should().BeTrue();
        config.MinSearchLength.Should().Be(2);
        config.MaxDisplayedItems.Should().Be(50);
        config.DebounceMs.Should().Be(500);
        config.AllowClear.Should().BeTrue();
        config.Disabled.Should().BeFalse();
        config.CloseOnSelect.Should().BeTrue();
        config.FilterStrategy.Should().Be(FilterStrategy.Contains);
    }

    [Fact]
    public void Builder_ShouldSetAllThemeProperties()
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
            }
        };

        // Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithThemeOverrides(themeOptions)
            .WithSize(ComponentSize.Large)
            .WithThemeTransitions(false)
            .Build();

        // Assert
        config.ThemeOverrides.Should().BeSameAs(themeOptions);
        config.ThemeOverrides!.Colors!.Primary.Should().Be("#FF6B6B");
        config.ThemeOverrides.Spacing!.BorderRadius.Should().Be("8px");
        config.Size.Should().Be(ComponentSize.Large);
        config.EnableThemeTransitions.Should().BeFalse();
    }

    [Fact]
    public void Builder_ShouldSupportConvenienceThemeMethods()
    {
        // Arrange
        var colors = new ColorOptions { Primary = "#007bff" };
        var spacing = new SpacingOptions { BorderRadius = "4px" };

        // Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithColors(colors)
            .WithSpacing(spacing)
            .WithSize(ComponentSize.Compact)
            .Build();

        // Assert
        config.ThemeOverrides.Should().NotBeNull();
        config.ThemeOverrides!.Colors.Should().BeSameAs(colors);
        config.ThemeOverrides.Spacing.Should().BeSameAs(spacing);
        config.Size.Should().Be(ComponentSize.Compact);
    }

    [Fact]
    public void Builder_ShouldSetAllDisplayModeProperties()
    {
        // Arrange & Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithDisplayMode(ItemDisplayMode.TitleWithDescription)
            .WithDescriptionField(x => x.Description)
            .WithSize(ComponentSize.Default)
            .Build();

        // Assert
        config.DisplayMode.Should().Be(ItemDisplayMode.TitleWithDescription);
        config.DescriptionField.Should().NotBeNull();
        config.Size.Should().Be(ComponentSize.Default);
    }

    [Fact]
    public void Builder_ShouldSetVirtualizationProperties()
    {
        // Arrange & Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithVirtualization(threshold: 1000, itemHeight: 50f)
            .WithSize(ComponentSize.Default)
            .Build();

        // Assert
        config.Virtualize.Should().BeTrue();
        config.VirtualizationThreshold.Should().Be(1000);
        config.ItemHeight.Should().Be(50f);
        config.Size.Should().Be(ComponentSize.Default);
    }

    [Fact]
    public void WithSize_ShouldBeChainableWithOtherMethods()
    {
        // Arrange & Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithTheme(Theme.Light)
            .WithSize(ComponentSize.Large)
            .WithThemeTransitions(true)
            .WithBootstrapTheme(BootstrapTheme.Success)
            .Build();

        // Assert
        config.Theme.Should().Be(Theme.Light);
        config.Size.Should().Be(ComponentSize.Large);
        config.EnableThemeTransitions.Should().BeTrue();
        config.BootstrapTheme.Should().Be(BootstrapTheme.Success);
    }

    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var config = AutoCompleteConfig<TestItem>.Create().Build();

        // Assert
        config.Theme.Should().Be(Theme.Auto);
        config.BootstrapTheme.Should().Be(BootstrapTheme.Default);
        config.ThemePreset.Should().Be(ThemePreset.None);
        config.Size.Should().Be(ComponentSize.Default);
        config.EnableThemeTransitions.Should().BeTrue();
        config.RightToLeft.Should().BeFalse();
        config.MinSearchLength.Should().Be(1);
        config.MaxDisplayedItems.Should().Be(100);
        config.DebounceMs.Should().Be(300);
        config.AllowClear.Should().BeTrue();
        config.Disabled.Should().BeFalse();
        config.CloseOnSelect.Should().BeTrue();
        config.FilterStrategy.Should().Be(FilterStrategy.StartsWith);
        config.Virtualize.Should().BeFalse();
        config.VirtualizationThreshold.Should().Be(100);
        config.ItemHeight.Should().Be(40f);
        config.DisplayMode.Should().Be(ItemDisplayMode.Custom);
        config.BadgeClass.Should().Be("badge bg-primary");
    }

    [Fact]
    public void Builder_ShouldAllowOverridingPreviousValues()
    {
        // Arrange & Act
        var config = AutoCompleteConfig<TestItem>.Create()
            .WithSize(ComponentSize.Compact)
            .WithSize(ComponentSize.Large)  // Override
            .WithThemeTransitions(false)
            .WithThemeTransitions(true)      // Override
            .Build();

        // Assert
        config.Size.Should().Be(ComponentSize.Large, "last value should win");
        config.EnableThemeTransitions.Should().BeTrue("last value should win");
    }
}
