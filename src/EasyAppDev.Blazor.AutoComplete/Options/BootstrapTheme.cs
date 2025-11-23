namespace EasyAppDev.Blazor.AutoComplete.Options;

/// <summary>
/// Bootstrap theme presets for the AutoComplete component.
/// These themes map to Bootstrap 5 color schemes and apply coordinated styling
/// to input borders, focus states, badges, and other visual elements.
/// </summary>
public enum BootstrapTheme
{
    /// <summary>
    /// Default theme with neutral colors (gray/blue accents).
    /// Uses standard Bootstrap colors without specific theme application.
    /// </summary>
    Default,

    /// <summary>
    /// Primary theme (typically blue in Bootstrap).
    /// Applies primary color to input focus, selected items, and badges.
    /// </summary>
    Primary,

    /// <summary>
    /// Secondary theme (typically gray in Bootstrap).
    /// Applies secondary color for a more subtle, professional appearance.
    /// </summary>
    Secondary,

    /// <summary>
    /// Success theme (typically green in Bootstrap).
    /// Good for confirmation or positive selection contexts.
    /// </summary>
    Success,

    /// <summary>
    /// Danger theme (typically red in Bootstrap).
    /// Useful for deletion or critical selection scenarios.
    /// </summary>
    Danger,

    /// <summary>
    /// Warning theme (typically yellow/orange in Bootstrap).
    /// Suitable for caution or attention-needed contexts.
    /// </summary>
    Warning,

    /// <summary>
    /// Info theme (typically cyan in Bootstrap).
    /// Good for informational or data-driven contexts.
    /// </summary>
    Info,

    /// <summary>
    /// Light theme (typically light gray in Bootstrap).
    /// Minimal visual impact, good for subtle interfaces.
    /// </summary>
    Light,

    /// <summary>
    /// Dark theme (typically dark gray/black in Bootstrap).
    /// Strong contrast, good for emphasis or dark mode interfaces.
    /// </summary>
    Dark
}
