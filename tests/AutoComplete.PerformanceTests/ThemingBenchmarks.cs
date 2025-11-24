using BenchmarkDotNet.Attributes;
using EasyAppDev.Blazor.AutoComplete.Options;
using EasyAppDev.Blazor.AutoComplete.Theming;

namespace EasyAppDev.Blazor.AutoComplete.PerformanceTests;

/// <summary>
/// Benchmarks for theme operations.
/// Tests theme application and CSS generation performance.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class ThemingBenchmarks
{
    private ThemeOptions _complexTheme = null!;
    private ThemeOptions _simpleTheme = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Create a complex theme with all properties set
        _complexTheme = new ThemeOptions
        {
            Colors = new ColorOptions
            {
                Primary = "#007bff",
                Background = "#ffffff",
                Text = "#212529",
                Border = "#dee2e6",
                Hover = "#e9ecef",
                Selected = "#0056b3"
            },
            Spacing = new SpacingOptions
            {
                InputPadding = "0.5rem 1rem",
                ItemPadding = "0.5rem 1rem",
                BorderRadius = "0.25rem",
                DropdownGap = "0.25rem"
            },
            Typography = new TypographyOptions
            {
                FontFamily = "system-ui, sans-serif",
                FontSize = "1rem",
                LineHeight = "1.5",
                FontWeight = "400"
            },
            Effects = new EffectOptions
            {
                FocusShadow = "0 0 0 0.2rem rgba(0, 123, 255, 0.25)",
                DropdownShadow = "0 0.5rem 1rem rgba(0, 0, 0, 0.15)",
                TransitionDuration = "0.15s"
            }
        };

        // Create a simple theme with minimal properties
        _simpleTheme = new ThemeOptions
        {
            Colors = new ColorOptions
            {
                Primary = "#007bff"
            }
        };
    }

    [Benchmark(Baseline = true)]
    public void BuildCustomPropertyStyle_SimpleTheme()
    {
        var themeManager = new ThemeManager();
        var css = themeManager.BuildCustomPropertyStyle(_simpleTheme);
    }

    [Benchmark]
    public void BuildCustomPropertyStyle_ComplexTheme()
    {
        var themeManager = new ThemeManager();
        var css = themeManager.BuildCustomPropertyStyle(_complexTheme);
    }

    [Benchmark]
    public void GenerateBootstrapThemeClasses()
    {
        // Benchmark generation of all 9 Bootstrap theme variants
        var themes = new[]
        {
            BootstrapTheme.Default,
            BootstrapTheme.Primary,
            BootstrapTheme.Secondary,
            BootstrapTheme.Success,
            BootstrapTheme.Danger,
            BootstrapTheme.Warning,
            BootstrapTheme.Info,
            BootstrapTheme.Light,
            BootstrapTheme.Dark
        };

        foreach (var theme in themes)
        {
            var className = GetBootstrapThemeClass(theme);
        }
    }

    [Benchmark]
    public void GenerateThemeClasses()
    {
        // Benchmark generation of theme classes for all modes
        var themes = new[]
        {
            Theme.Auto,
            Theme.Light,
            Theme.Dark
        };

        foreach (var theme in themes)
        {
            var className = GetThemeClass(theme);
        }
    }

    private static string GetBootstrapThemeClass(BootstrapTheme theme)
    {
        return theme switch
        {
            BootstrapTheme.Default => string.Empty,
            BootstrapTheme.Primary => "ebd-ac-bs-primary",
            BootstrapTheme.Secondary => "ebd-ac-bs-secondary",
            BootstrapTheme.Success => "ebd-ac-bs-success",
            BootstrapTheme.Danger => "ebd-ac-bs-danger",
            BootstrapTheme.Warning => "ebd-ac-bs-warning",
            BootstrapTheme.Info => "ebd-ac-bs-info",
            BootstrapTheme.Light => "ebd-ac-bs-light",
            BootstrapTheme.Dark => "ebd-ac-bs-dark",
            _ => string.Empty
        };
    }

    private static string GetThemeClass(Theme theme)
    {
        return theme switch
        {
            Theme.Auto => string.Empty,
            Theme.Light => "ebd-ac-theme-light",
            Theme.Dark => "ebd-ac-theme-dark",
            _ => string.Empty
        };
    }
}
