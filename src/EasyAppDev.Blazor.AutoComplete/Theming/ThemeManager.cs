using EasyAppDev.Blazor.AutoComplete.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyAppDev.Blazor.AutoComplete.Theming;

/// <summary>
/// Manages theme-related operations for the AutoComplete component.
/// Handles CSS custom property generation and theme class generation.
/// All themes are loaded statically via autocomplete.themes.css (no dynamic loading).
/// </summary>
public class ThemeManager
{
    /// <summary>
    /// Initializes a new instance of the ThemeManager class.
    /// </summary>
    public ThemeManager()
    {
    }

    /// <summary>
    /// Builds inline CSS custom property string from theme options.
    /// Returns an empty string if no theme overrides are provided.
    /// </summary>
    /// <param name="options">Theme options containing color, spacing, typography, and effect overrides.</param>
    /// <returns>CSS custom property string (e.g., "--ebd-ac-primary: #007bff; --ebd-ac-bg: #ffffff").</returns>
    public string BuildCustomPropertyStyle(ThemeOptions? options)
    {
        if (options == null)
        {
            return string.Empty;
        }

        var props = new List<string>();

        // Color properties
        if (options.Colors != null)
        {
            var colors = options.Colors;

            if (colors.Primary != null)
            {
                props.Add($"--ebd-ac-primary: {colors.Primary}");
            }

            if (colors.Background != null)
            {
                props.Add($"--ebd-ac-bg: {colors.Background}");
            }

            if (colors.Text != null)
            {
                props.Add($"--ebd-ac-text: {colors.Text}");
            }

            if (colors.TextSecondary != null)
            {
                props.Add($"--ebd-ac-text-secondary: {colors.TextSecondary}");
            }

            if (colors.Border != null)
            {
                props.Add($"--ebd-ac-border: {colors.Border}");
            }

            if (colors.BorderFocus != null)
            {
                props.Add($"--ebd-ac-border-focus: {colors.BorderFocus}");
            }

            // Auto-generate hover color from primary color if not explicitly set
            if (colors.Hover != null)
            {
                props.Add($"--ebd-ac-hover: {colors.Hover}");
            }
            else if (colors.Primary != null)
            {
                // Generate a very light tint of the primary color for hover (90% lighter)
                var autoHoverColor = ColorHelper.Lighten(colors.Primary, 0.9);
                props.Add($"--ebd-ac-hover: {autoHoverColor}");
            }

            // Auto-generate selected color from primary color if not explicitly set
            if (colors.Selected != null)
            {
                props.Add($"--ebd-ac-selected: {colors.Selected}");
            }
            else if (colors.Primary != null)
            {
                // Generate a lighter tint of the primary color for selected state (85% lighter)
                var autoSelectedColor = ColorHelper.Lighten(colors.Primary, 0.85);
                props.Add($"--ebd-ac-selected: {autoSelectedColor}");
            }

            if (colors.SelectedText != null)
            {
                props.Add($"--ebd-ac-selected-text: {colors.SelectedText}");
            }

            if (colors.Disabled != null)
            {
                props.Add($"--ebd-ac-disabled: {colors.Disabled}");
            }

            if (colors.Error != null)
            {
                props.Add($"--ebd-ac-error: {colors.Error}");
            }

            if (colors.Shadow != null)
            {
                props.Add($"--ebd-ac-shadow: {colors.Shadow}");
            }

            if (colors.DropdownBackground != null)
            {
                props.Add($"--ebd-ac-dropdown-bg: {colors.DropdownBackground}");
            }

            if (colors.Focus != null)
            {
                props.Add($"--ebd-ac-focus: {colors.Focus}");
            }

            if (colors.Placeholder != null)
            {
                props.Add($"--ebd-ac-placeholder: {colors.Placeholder}");
            }
        }

        // Spacing properties
        if (options.Spacing != null)
        {
            var spacing = options.Spacing;

            if (spacing.InputPadding != null)
            {
                props.Add($"--ebd-ac-input-padding: {spacing.InputPadding}");
            }

            if (spacing.ItemPadding != null)
            {
                props.Add($"--ebd-ac-item-padding: {spacing.ItemPadding}");
            }

            if (spacing.BorderRadius != null)
            {
                props.Add($"--ebd-ac-border-radius: {spacing.BorderRadius}");
            }

            if (spacing.DropdownGap != null)
            {
                props.Add($"--ebd-ac-dropdown-gap: {spacing.DropdownGap}");
            }

            if (spacing.ItemGap != null)
            {
                props.Add($"--ebd-ac-item-gap: {spacing.ItemGap}");
            }

            if (spacing.GroupHeaderPadding != null)
            {
                props.Add($"--ebd-ac-group-padding: {spacing.GroupHeaderPadding}");
            }

            if (spacing.MaxHeight != null)
            {
                props.Add($"--ebd-ac-dropdown-max-height: {spacing.MaxHeight}");
            }

            if (spacing.MinWidth != null)
            {
                props.Add($"--ebd-ac-min-width: {spacing.MinWidth}");
            }

            if (spacing.ListPadding != null)
            {
                props.Add($"--ebd-ac-list-padding: {spacing.ListPadding}");
            }

            if (spacing.IconSize != null)
            {
                props.Add($"--ebd-ac-icon-size: {spacing.IconSize}");
            }
        }

        // Typography properties
        if (options.Typography != null)
        {
            var typography = options.Typography;

            if (typography.FontFamily != null)
            {
                props.Add($"--ebd-ac-font-family: {typography.FontFamily}");
            }

            if (typography.FontSize != null)
            {
                props.Add($"--ebd-ac-font-size: {typography.FontSize}");
            }

            if (typography.LineHeight != null)
            {
                props.Add($"--ebd-ac-line-height: {typography.LineHeight}");
            }

            if (typography.FontWeight != null)
            {
                props.Add($"--ebd-ac-font-weight: {typography.FontWeight}");
            }

            if (typography.DescriptionFontSize != null)
            {
                props.Add($"--ebd-ac-description-font-size: {typography.DescriptionFontSize}");
            }

            if (typography.BadgeFontSize != null)
            {
                props.Add($"--ebd-ac-badge-font-size: {typography.BadgeFontSize}");
            }

            if (typography.GroupHeaderFontSize != null)
            {
                props.Add($"--ebd-ac-group-font-size: {typography.GroupHeaderFontSize}");
            }

            if (typography.LetterSpacing != null)
            {
                props.Add($"--ebd-ac-letter-spacing: {typography.LetterSpacing}");
            }
        }

        // Effect properties
        if (options.Effects != null)
        {
            var effects = options.Effects;

            if (effects.FocusShadow != null)
            {
                props.Add($"--ebd-ac-focus-shadow: {effects.FocusShadow}");
            }

            if (effects.DropdownShadow != null)
            {
                props.Add($"--ebd-ac-dropdown-shadow: {effects.DropdownShadow}");
            }

            if (effects.TransitionDuration != null)
            {
                props.Add($"--ebd-ac-transition-duration: {effects.TransitionDuration}");
            }

            if (effects.BorderWidth != null)
            {
                props.Add($"--ebd-ac-border-width: {effects.BorderWidth}");
            }

            if (effects.TransitionTiming != null)
            {
                props.Add($"--ebd-ac-transition-timing: {effects.TransitionTiming}");
            }
        }

        return props.Any() ? string.Join("; ", props) : string.Empty;
    }


    /// <summary>
    /// Generates CSS class name for the Theme enum.
    /// </summary>
    /// <param name="theme">The theme (Light, Dark, Auto).</param>
    /// <returns>CSS class name (e.g., "ebd-ac-theme-light").</returns>
    public string GetThemeClass(Theme theme) => theme switch
    {
        Theme.Light => "ebd-ac-theme-light",
        Theme.Dark => "ebd-ac-theme-dark",
        Theme.Auto => "ebd-ac-theme-auto",
        _ => "ebd-ac-theme-auto"
    };

    /// <summary>
    /// Generates CSS class name for the BootstrapTheme enum.
    /// </summary>
    /// <param name="theme">The Bootstrap theme variant.</param>
    /// <returns>CSS class name (e.g., "ebd-ac-bs-primary") or empty string for Default.</returns>
    public string GetBootstrapThemeClass(BootstrapTheme theme) => theme switch
    {
        BootstrapTheme.Default => "",
        BootstrapTheme.Primary => "ebd-ac-bs-primary",
        BootstrapTheme.Secondary => "ebd-ac-bs-secondary",
        BootstrapTheme.Success => "ebd-ac-bs-success",
        BootstrapTheme.Danger => "ebd-ac-bs-danger",
        BootstrapTheme.Warning => "ebd-ac-bs-warning",
        BootstrapTheme.Info => "ebd-ac-bs-info",
        BootstrapTheme.Light => "ebd-ac-bs-light",
        BootstrapTheme.Dark => "ebd-ac-bs-dark",
        _ => ""
    };

    /// <summary>
    /// Generates CSS class name for the ThemePreset enum.
    /// </summary>
    /// <param name="preset">The theme preset.</param>
    /// <returns>CSS class name (e.g., "ebd-ac-theme-preset-material") or empty string for None.</returns>
    public string GetThemePresetClass(ThemePreset preset) => preset switch
    {
        ThemePreset.Material => "ebd-ac-theme-preset-material",
        ThemePreset.Fluent => "ebd-ac-theme-preset-fluent",
        ThemePreset.Modern => "ebd-ac-theme-preset-modern",
        ThemePreset.Bootstrap => "ebd-ac-theme-preset-bootstrap",
        ThemePreset.None => "",
        _ => ""
    };

    /// <summary>
    /// Generates CSS class name for the ComponentSize enum.
    /// </summary>
    /// <param name="size">The component size.</param>
    /// <returns>CSS class name (e.g., "ebd-ac-size-compact").</returns>
    public string GetSizeClass(ComponentSize size) =>
        $"ebd-ac-size-{size.ToString().ToLower()}";
}
