using EasyAppDev.Blazor.AutoComplete.Options;

namespace EasyAppDev.Blazor.AutoComplete.Theming;

/// <summary>
/// Interface for managing theme-related operations for the AutoComplete component.
/// Handles CSS custom property generation and theme class generation.
/// </summary>
public interface IThemeManager
{
    /// <summary>
    /// Builds inline CSS custom property string from theme options.
    /// Returns an empty string if no theme overrides are provided.
    /// </summary>
    /// <param name="options">Theme options containing color, spacing, typography, and effect overrides.</param>
    /// <returns>CSS custom property string (e.g., "--ebd-ac-primary: #007bff; --ebd-ac-bg: #ffffff").</returns>
    string BuildCustomPropertyStyle(ThemeOptions? options);

    /// <summary>
    /// Generates CSS class name for the Theme enum.
    /// </summary>
    /// <param name="theme">The theme (Light, Dark, Auto).</param>
    /// <returns>CSS class name (e.g., "ebd-ac-theme-light").</returns>
    string GetThemeClass(Theme theme);

    /// <summary>
    /// Generates CSS class name for the BootstrapTheme enum.
    /// </summary>
    /// <param name="theme">The Bootstrap theme variant.</param>
    /// <returns>CSS class name (e.g., "ebd-ac-bs-primary") or empty string for Default.</returns>
    string GetBootstrapThemeClass(BootstrapTheme theme);

    /// <summary>
    /// Generates CSS class name for the ThemePreset enum.
    /// </summary>
    /// <param name="preset">The theme preset.</param>
    /// <returns>CSS class name (e.g., "ebd-ac-theme-preset-material") or empty string for None.</returns>
    string GetThemePresetClass(ThemePreset preset);

    /// <summary>
    /// Generates CSS class name for the ComponentSize enum.
    /// </summary>
    /// <param name="size">The component size.</param>
    /// <returns>CSS class name (e.g., "ebd-ac-size-compact").</returns>
    string GetSizeClass(ComponentSize size);
}
