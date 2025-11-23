namespace EasyAppDev.Blazor.AutoComplete.Theming;

/// <summary>
/// Individual theme property overrides
/// All properties are nullable - only set properties will override theme preset values
/// </summary>
public sealed class ThemeProperties
{
    #region Colors (12 properties)

    /// <summary>
    /// Primary brand color
    /// </summary>
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// Background color for input and dropdown
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// Primary text color
    /// </summary>
    public string? TextColor { get; set; }

    /// <summary>
    /// Secondary/muted text color
    /// </summary>
    public string? TextSecondaryColor { get; set; }

    /// <summary>
    /// Default border color
    /// </summary>
    public string? BorderColor { get; set; }

    /// <summary>
    /// Border color when focused
    /// </summary>
    public string? BorderFocusColor { get; set; }

    /// <summary>
    /// Hover state background color
    /// </summary>
    public string? HoverColor { get; set; }

    /// <summary>
    /// Selected item background color
    /// </summary>
    public string? SelectedColor { get; set; }

    /// <summary>
    /// Selected item text color
    /// </summary>
    public string? SelectedTextColor { get; set; }

    /// <summary>
    /// Disabled state color
    /// </summary>
    public string? DisabledColor { get; set; }

    /// <summary>
    /// Error/invalid state color
    /// </summary>
    public string? ErrorColor { get; set; }

    /// <summary>
    /// Shadow color (RGBA recommended)
    /// </summary>
    public string? ShadowColor { get; set; }

    #endregion

    #region Spacing (8 properties)

    /// <summary>
    /// Input field padding (e.g., "8px 32px 8px 12px")
    /// </summary>
    public string? InputPadding { get; set; }

    /// <summary>
    /// Dropdown item padding (e.g., "8px 12px")
    /// </summary>
    public string? ItemPadding { get; set; }

    /// <summary>
    /// Border radius for rounded corners (e.g., "4px")
    /// </summary>
    public string? BorderRadius { get; set; }

    /// <summary>
    /// Gap between input and dropdown (e.g., "4px")
    /// </summary>
    public string? DropdownGap { get; set; }

    /// <summary>
    /// Maximum height of dropdown (e.g., "300px")
    /// </summary>
    public string? DropdownMaxHeight { get; set; }

    /// <summary>
    /// Padding inside dropdown list (e.g., "4px 0")
    /// </summary>
    public string? ListPadding { get; set; }

    /// <summary>
    /// Padding for group headers (e.g., "8px 12px")
    /// </summary>
    public string? GroupPadding { get; set; }

    /// <summary>
    /// Icon size (e.g., "20px")
    /// </summary>
    public string? IconSize { get; set; }

    #endregion

    #region Typography (6 properties)

    /// <summary>
    /// Font family stack (e.g., "Roboto, sans-serif")
    /// </summary>
    public string? FontFamily { get; set; }

    /// <summary>
    /// Base font size (e.g., "14px")
    /// </summary>
    public string? FontSize { get; set; }

    /// <summary>
    /// Line height (e.g., "1.5" or "20px")
    /// </summary>
    public string? LineHeight { get; set; }

    /// <summary>
    /// Font weight (e.g., "400" or "normal")
    /// </summary>
    public string? FontWeight { get; set; }

    /// <summary>
    /// Font size for group headers (e.g., "12px")
    /// </summary>
    public string? GroupHeaderFontSize { get; set; }

    /// <summary>
    /// Letter spacing (e.g., "0.5px" or "normal")
    /// </summary>
    public string? LetterSpacing { get; set; }

    #endregion

    #region Effects (4 properties)

    /// <summary>
    /// Focus ring shadow (e.g., "0 0 0 3px rgba(59, 130, 246, 0.1)")
    /// </summary>
    public string? FocusShadow { get; set; }

    /// <summary>
    /// Dropdown shadow (e.g., "0 4px 6px rgba(0, 0, 0, 0.1)")
    /// </summary>
    public string? DropdownShadow { get; set; }

    /// <summary>
    /// Transition duration (e.g., "200ms" or "0.2s")
    /// </summary>
    public string? TransitionDuration { get; set; }

    /// <summary>
    /// Transition timing function (e.g., "ease" or "cubic-bezier(0.4, 0, 0.2, 1)")
    /// </summary>
    public string? TransitionTiming { get; set; }

    #endregion
}
