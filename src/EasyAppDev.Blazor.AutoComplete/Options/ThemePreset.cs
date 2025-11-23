namespace EasyAppDev.Blazor.AutoComplete.Options;

/// <summary>
/// Predefined theme presets with coordinated design systems
/// </summary>
public enum ThemePreset
{
    /// <summary>
    /// No preset theme - use custom properties only
    /// </summary>
    None = 0,

    /// <summary>
    /// Material Design 3 theme (Google)
    /// </summary>
    Material = 1,

    /// <summary>
    /// Fluent Design theme (Microsoft Windows 11)
    /// </summary>
    Fluent = 2,

    /// <summary>
    /// Modern minimal theme with flat design
    /// </summary>
    Modern = 3,

    /// <summary>
    /// Bootstrap 5 theme
    /// </summary>
    Bootstrap = 4
}
