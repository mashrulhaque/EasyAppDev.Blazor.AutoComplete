using EasyAppDev.Blazor.AutoComplete.Options;
using EasyAppDev.Blazor.AutoComplete.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyAppDev.Blazor.AutoComplete.Theming;

/// <summary>
/// Manages theme-related operations for the AutoComplete component.
/// Handles CSS custom property generation and theme class generation.
/// All themes are loaded statically via autocomplete.themes.css (no dynamic loading).
/// </summary>
public class ThemeManager : IThemeManager
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

        // Color properties (with security sanitization)
        if (options.Colors != null)
        {
            var colors = options.Colors;

            var sanitizedPrimary = CssSanitizer.SanitizeColor(colors.Primary);
            if (sanitizedPrimary != null)
            {
                props.Add($"--ebd-ac-primary: {sanitizedPrimary}");
            }

            var sanitizedBg = CssSanitizer.SanitizeColor(colors.Background);
            if (sanitizedBg != null)
            {
                props.Add($"--ebd-ac-bg: {sanitizedBg}");
            }

            var sanitizedText = CssSanitizer.SanitizeColor(colors.Text);
            if (sanitizedText != null)
            {
                props.Add($"--ebd-ac-text: {sanitizedText}");
            }

            var sanitizedTextSecondary = CssSanitizer.SanitizeColor(colors.TextSecondary);
            if (sanitizedTextSecondary != null)
            {
                props.Add($"--ebd-ac-text-secondary: {sanitizedTextSecondary}");
            }

            var sanitizedBorder = CssSanitizer.SanitizeColor(colors.Border);
            if (sanitizedBorder != null)
            {
                props.Add($"--ebd-ac-border: {sanitizedBorder}");
            }

            var sanitizedBorderFocus = CssSanitizer.SanitizeColor(colors.BorderFocus);
            if (sanitizedBorderFocus != null)
            {
                props.Add($"--ebd-ac-border-focus: {sanitizedBorderFocus}");
            }

            // Auto-generate hover color from primary color if not explicitly set
            var sanitizedHover = CssSanitizer.SanitizeColor(colors.Hover);
            if (sanitizedHover != null)
            {
                props.Add($"--ebd-ac-hover: {sanitizedHover}");
            }
            else if (sanitizedPrimary != null)
            {
                // Generate a very light tint of the primary color for hover (90% lighter)
                var autoHoverColor = ColorHelper.Lighten(sanitizedPrimary, 0.9);
                var sanitizedAutoHover = CssSanitizer.SanitizeColor(autoHoverColor);
                if (sanitizedAutoHover != null)
                {
                    props.Add($"--ebd-ac-hover: {sanitizedAutoHover}");
                }
            }

            // Auto-generate selected color from primary color if not explicitly set
            var sanitizedSelected = CssSanitizer.SanitizeColor(colors.Selected);
            if (sanitizedSelected != null)
            {
                props.Add($"--ebd-ac-selected: {sanitizedSelected}");
            }
            else if (sanitizedPrimary != null)
            {
                // Generate a lighter tint of the primary color for selected state (85% lighter)
                var autoSelectedColor = ColorHelper.Lighten(sanitizedPrimary, 0.85);
                var sanitizedAutoSelected = CssSanitizer.SanitizeColor(autoSelectedColor);
                if (sanitizedAutoSelected != null)
                {
                    props.Add($"--ebd-ac-selected: {sanitizedAutoSelected}");
                }
            }

            var sanitizedSelectedText = CssSanitizer.SanitizeColor(colors.SelectedText);
            if (sanitizedSelectedText != null)
            {
                props.Add($"--ebd-ac-selected-text: {sanitizedSelectedText}");
            }

            var sanitizedDisabled = CssSanitizer.SanitizeColor(colors.Disabled);
            if (sanitizedDisabled != null)
            {
                props.Add($"--ebd-ac-disabled: {sanitizedDisabled}");
            }

            var sanitizedError = CssSanitizer.SanitizeColor(colors.Error);
            if (sanitizedError != null)
            {
                props.Add($"--ebd-ac-error: {sanitizedError}");
            }

            var sanitizedShadow = CssSanitizer.SanitizeColor(colors.Shadow);
            if (sanitizedShadow != null)
            {
                props.Add($"--ebd-ac-shadow: {sanitizedShadow}");
            }

            var sanitizedDropdownBg = CssSanitizer.SanitizeColor(colors.DropdownBackground);
            if (sanitizedDropdownBg != null)
            {
                props.Add($"--ebd-ac-dropdown-bg: {sanitizedDropdownBg}");
            }

            var sanitizedFocus = CssSanitizer.SanitizeColor(colors.Focus);
            if (sanitizedFocus != null)
            {
                props.Add($"--ebd-ac-focus: {sanitizedFocus}");
            }

            var sanitizedPlaceholder = CssSanitizer.SanitizeColor(colors.Placeholder);
            if (sanitizedPlaceholder != null)
            {
                props.Add($"--ebd-ac-placeholder: {sanitizedPlaceholder}");
            }
        }

        // Spacing properties (with security sanitization)
        if (options.Spacing != null)
        {
            var spacing = options.Spacing;

            var sanitizedInputPadding = CssSanitizer.SanitizeLength(spacing.InputPadding);
            if (sanitizedInputPadding != null)
            {
                props.Add($"--ebd-ac-input-padding: {sanitizedInputPadding}");
            }

            var sanitizedItemPadding = CssSanitizer.SanitizeLength(spacing.ItemPadding);
            if (sanitizedItemPadding != null)
            {
                props.Add($"--ebd-ac-item-padding: {sanitizedItemPadding}");
            }

            var sanitizedBorderRadius = CssSanitizer.SanitizeLength(spacing.BorderRadius);
            if (sanitizedBorderRadius != null)
            {
                props.Add($"--ebd-ac-border-radius: {sanitizedBorderRadius}");
            }

            var sanitizedDropdownGap = CssSanitizer.SanitizeLength(spacing.DropdownGap);
            if (sanitizedDropdownGap != null)
            {
                props.Add($"--ebd-ac-dropdown-gap: {sanitizedDropdownGap}");
            }

            var sanitizedItemGap = CssSanitizer.SanitizeLength(spacing.ItemGap);
            if (sanitizedItemGap != null)
            {
                props.Add($"--ebd-ac-item-gap: {sanitizedItemGap}");
            }

            var sanitizedGroupPadding = CssSanitizer.SanitizeLength(spacing.GroupHeaderPadding);
            if (sanitizedGroupPadding != null)
            {
                props.Add($"--ebd-ac-group-padding: {sanitizedGroupPadding}");
            }

            var sanitizedMaxHeight = CssSanitizer.SanitizeLength(spacing.MaxHeight);
            if (sanitizedMaxHeight != null)
            {
                props.Add($"--ebd-ac-dropdown-max-height: {sanitizedMaxHeight}");
            }

            var sanitizedMinWidth = CssSanitizer.SanitizeLength(spacing.MinWidth);
            if (sanitizedMinWidth != null)
            {
                props.Add($"--ebd-ac-min-width: {sanitizedMinWidth}");
            }

            var sanitizedListPadding = CssSanitizer.SanitizeLength(spacing.ListPadding);
            if (sanitizedListPadding != null)
            {
                props.Add($"--ebd-ac-list-padding: {sanitizedListPadding}");
            }

            var sanitizedIconSize = CssSanitizer.SanitizeLength(spacing.IconSize);
            if (sanitizedIconSize != null)
            {
                props.Add($"--ebd-ac-icon-size: {sanitizedIconSize}");
            }
        }

        // Typography properties (with security sanitization)
        if (options.Typography != null)
        {
            var typography = options.Typography;

            var sanitizedFontFamily = CssSanitizer.SanitizeFontFamily(typography.FontFamily);
            if (sanitizedFontFamily != null)
            {
                props.Add($"--ebd-ac-font-family: {sanitizedFontFamily}");
            }

            var sanitizedFontSize = CssSanitizer.SanitizeLength(typography.FontSize);
            if (sanitizedFontSize != null)
            {
                props.Add($"--ebd-ac-font-size: {sanitizedFontSize}");
            }

            var sanitizedLineHeight = CssSanitizer.SanitizeGenericValue(typography.LineHeight);
            if (sanitizedLineHeight != null)
            {
                props.Add($"--ebd-ac-line-height: {sanitizedLineHeight}");
            }

            var sanitizedFontWeight = CssSanitizer.SanitizeGenericValue(typography.FontWeight);
            if (sanitizedFontWeight != null)
            {
                props.Add($"--ebd-ac-font-weight: {sanitizedFontWeight}");
            }

            var sanitizedDescFontSize = CssSanitizer.SanitizeLength(typography.DescriptionFontSize);
            if (sanitizedDescFontSize != null)
            {
                props.Add($"--ebd-ac-description-font-size: {sanitizedDescFontSize}");
            }

            var sanitizedBadgeFontSize = CssSanitizer.SanitizeLength(typography.BadgeFontSize);
            if (sanitizedBadgeFontSize != null)
            {
                props.Add($"--ebd-ac-badge-font-size: {sanitizedBadgeFontSize}");
            }

            var sanitizedGroupFontSize = CssSanitizer.SanitizeLength(typography.GroupHeaderFontSize);
            if (sanitizedGroupFontSize != null)
            {
                props.Add($"--ebd-ac-group-font-size: {sanitizedGroupFontSize}");
            }

            var sanitizedLetterSpacing = CssSanitizer.SanitizeLength(typography.LetterSpacing);
            if (sanitizedLetterSpacing != null)
            {
                props.Add($"--ebd-ac-letter-spacing: {sanitizedLetterSpacing}");
            }
        }

        // Effect properties (with security sanitization)
        if (options.Effects != null)
        {
            var effects = options.Effects;

            var sanitizedFocusShadow = CssSanitizer.SanitizeShadow(effects.FocusShadow);
            if (sanitizedFocusShadow != null)
            {
                props.Add($"--ebd-ac-focus-shadow: {sanitizedFocusShadow}");
            }

            var sanitizedDropdownShadow = CssSanitizer.SanitizeShadow(effects.DropdownShadow);
            if (sanitizedDropdownShadow != null)
            {
                props.Add($"--ebd-ac-dropdown-shadow: {sanitizedDropdownShadow}");
            }

            var sanitizedTransitionDuration = CssSanitizer.SanitizeTime(effects.TransitionDuration);
            if (sanitizedTransitionDuration != null)
            {
                props.Add($"--ebd-ac-transition-duration: {sanitizedTransitionDuration}");
            }

            var sanitizedBorderWidth = CssSanitizer.SanitizeLength(effects.BorderWidth);
            if (sanitizedBorderWidth != null)
            {
                props.Add($"--ebd-ac-border-width: {sanitizedBorderWidth}");
            }

            var sanitizedTransitionTiming = CssSanitizer.SanitizeGenericValue(effects.TransitionTiming);
            if (sanitizedTransitionTiming != null)
            {
                props.Add($"--ebd-ac-transition-timing: {sanitizedTransitionTiming}");
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
