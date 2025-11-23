namespace EasyAppDev.Blazor.AutoComplete.Theming;

/// <summary>
/// Represents CSS custom property overrides for component theming.
/// All properties are optional and override the default theme values.
/// </summary>
/// <example>
/// <code>
/// var theme = new ThemeOptions
/// {
///     Colors = new ColorOptions { Primary = "#007bff", Background = "#ffffff" },
///     Spacing = new SpacingOptions { BorderRadius = "8px", InputPadding = "10px 12px" },
///     Typography = new TypographyOptions { FontFamily = "Roboto, sans-serif" },
///     Effects = new EffectOptions { TransitionDuration = "300ms" }
/// };
/// </code>
/// </example>
public record ThemeOptions
{
    /// <summary>
    /// Color overrides for the component.
    /// </summary>
    public ColorOptions? Colors { get; init; }

    /// <summary>
    /// Spacing overrides for the component.
    /// </summary>
    public SpacingOptions? Spacing { get; init; }

    /// <summary>
    /// Typography overrides for the component.
    /// </summary>
    public TypographyOptions? Typography { get; init; }

    /// <summary>
    /// Effect overrides for the component (shadows, transitions).
    /// </summary>
    public EffectOptions? Effects { get; init; }
}

/// <summary>
/// Color-related theme overrides.
/// </summary>
public record ColorOptions
{
    /// <summary>
    /// Primary brand color (e.g., "#007bff").
    /// </summary>
    public string? Primary { get; init; }

    /// <summary>
    /// Background color for input and dropdown (e.g., "#ffffff").
    /// </summary>
    public string? Background { get; init; }

    /// <summary>
    /// Primary text color (e.g., "#212529").
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Secondary/muted text color for descriptions (e.g., "#6c757d").
    /// </summary>
    public string? TextSecondary { get; init; }

    /// <summary>
    /// Default border color (e.g., "#ced4da").
    /// </summary>
    public string? Border { get; init; }

    /// <summary>
    /// Border color when focused (e.g., "#86b7fe").
    /// </summary>
    public string? BorderFocus { get; init; }

    /// <summary>
    /// Hover state background color (e.g., "#f8f9fa").
    /// </summary>
    public string? Hover { get; init; }

    /// <summary>
    /// Selected item background color (e.g., "#e7f1ff").
    /// </summary>
    public string? Selected { get; init; }

    /// <summary>
    /// Selected item text color (e.g., "#0a58ca").
    /// </summary>
    public string? SelectedText { get; init; }

    /// <summary>
    /// Disabled state color (e.g., "#e9ecef").
    /// </summary>
    public string? Disabled { get; init; }

    /// <summary>
    /// Error/invalid state color (e.g., "#dc3545").
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Shadow color - RGBA recommended (e.g., "rgba(0, 0, 0, 0.1)").
    /// </summary>
    public string? Shadow { get; init; }

    /// <summary>
    /// Dropdown background color (e.g., "#ffffff").
    /// </summary>
    public string? DropdownBackground { get; init; }

    /// <summary>
    /// Focus ring color (e.g., "#0d6efd").
    /// </summary>
    public string? Focus { get; init; }

    /// <summary>
    /// Placeholder text color (e.g., "#6c757d").
    /// </summary>
    public string? Placeholder { get; init; }
}

/// <summary>
/// Spacing-related theme overrides.
/// </summary>
public record SpacingOptions
{
    /// <summary>
    /// Input field padding (e.g., "8px 32px 8px 12px").
    /// </summary>
    public string? InputPadding { get; init; }

    /// <summary>
    /// Dropdown item padding (e.g., "8px 12px").
    /// </summary>
    public string? ItemPadding { get; init; }

    /// <summary>
    /// Border radius for rounded corners (e.g., "4px", "0.375rem").
    /// </summary>
    public string? BorderRadius { get; init; }

    /// <summary>
    /// Gap between input and dropdown (e.g., "4px").
    /// </summary>
    public string? DropdownGap { get; init; }

    /// <summary>
    /// Gap between items in the dropdown (e.g., "2px").
    /// </summary>
    public string? ItemGap { get; init; }

    /// <summary>
    /// Padding for group headers (e.g., "8px 12px").
    /// </summary>
    public string? GroupHeaderPadding { get; init; }

    /// <summary>
    /// Maximum height of dropdown (e.g., "300px", "20rem").
    /// </summary>
    public string? MaxHeight { get; init; }

    /// <summary>
    /// Minimum width of dropdown (e.g., "200px").
    /// </summary>
    public string? MinWidth { get; init; }

    /// <summary>
    /// Padding inside dropdown list (e.g., "4px 0").
    /// </summary>
    public string? ListPadding { get; init; }

    /// <summary>
    /// Icon size for display modes (e.g., "20px").
    /// </summary>
    public string? IconSize { get; init; }
}

/// <summary>
/// Typography-related theme overrides.
/// </summary>
public record TypographyOptions
{
    /// <summary>
    /// Font family stack (e.g., "Roboto, sans-serif", "-apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif").
    /// </summary>
    public string? FontFamily { get; init; }

    /// <summary>
    /// Base font size (e.g., "14px", "1rem").
    /// </summary>
    public string? FontSize { get; init; }

    /// <summary>
    /// Line height (e.g., "1.5", "20px").
    /// </summary>
    public string? LineHeight { get; init; }

    /// <summary>
    /// Font weight (e.g., "400", "normal", "600", "bold").
    /// </summary>
    public string? FontWeight { get; init; }

    /// <summary>
    /// Font size for descriptions in display modes (e.g., "12px", "0.875rem").
    /// </summary>
    public string? DescriptionFontSize { get; init; }

    /// <summary>
    /// Font size for badges in display modes (e.g., "11px", "0.75rem").
    /// </summary>
    public string? BadgeFontSize { get; init; }

    /// <summary>
    /// Font size for group headers (e.g., "12px", "0.875rem").
    /// </summary>
    public string? GroupHeaderFontSize { get; init; }

    /// <summary>
    /// Letter spacing (e.g., "0.5px", "normal").
    /// </summary>
    public string? LetterSpacing { get; init; }
}

/// <summary>
/// Effect-related theme overrides (shadows, transitions).
/// </summary>
public record EffectOptions
{
    /// <summary>
    /// Focus ring shadow (e.g., "0 0 0 3px rgba(59, 130, 246, 0.1)").
    /// </summary>
    public string? FocusShadow { get; init; }

    /// <summary>
    /// Dropdown shadow (e.g., "0 4px 6px rgba(0, 0, 0, 0.1)", "0 10px 15px -3px rgba(0, 0, 0, 0.1)").
    /// </summary>
    public string? DropdownShadow { get; init; }

    /// <summary>
    /// Transition duration (e.g., "200ms", "0.2s").
    /// </summary>
    public string? TransitionDuration { get; init; }

    /// <summary>
    /// Border width (e.g., "1px", "2px").
    /// </summary>
    public string? BorderWidth { get; init; }

    /// <summary>
    /// Transition timing function (e.g., "ease", "cubic-bezier(0.4, 0, 0.2, 1)").
    /// </summary>
    public string? TransitionTiming { get; init; }
}
