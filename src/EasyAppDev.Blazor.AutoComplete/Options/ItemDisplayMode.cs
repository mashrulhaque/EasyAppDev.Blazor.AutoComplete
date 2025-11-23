namespace EasyAppDev.Blazor.AutoComplete.Options;

/// <summary>
/// Specifies how items should be displayed in the autocomplete dropdown.
/// Built-in modes eliminate the need for custom ItemTemplate markup for common scenarios.
/// </summary>
public enum ItemDisplayMode
{
    /// <summary>
    /// User provides a custom ItemTemplate (default behavior for backwards compatibility).
    /// When ItemTemplate is null, falls back to Simple mode.
    /// </summary>
    Custom,

    /// <summary>
    /// Simple text display - just shows the TextField value.
    /// Best for: Simple lists where only the main text is needed.
    /// </summary>
    Simple,

    /// <summary>
    /// Two-line display: Title (bold) + Description (muted below).
    /// Best for: Lists with supplementary information.
    /// Requires: DescriptionField parameter.
    /// </summary>
    TitleWithDescription,

    /// <summary>
    /// Title with right-aligned badge.
    /// Best for: Items with status, price, or category indicators.
    /// Requires: BadgeField parameter.
    /// </summary>
    TitleWithBadge,

    /// <summary>
    /// Title + Description below + Badge on right (most comprehensive).
    /// Best for: Rich item displays with multiple pieces of information.
    /// Requires: DescriptionField, BadgeField parameters.
    /// </summary>
    TitleDescriptionBadge,

    /// <summary>
    /// Icon/emoji on left + Title.
    /// Best for: Lists with visual indicators (emojis, icons).
    /// Requires: IconField parameter.
    /// </summary>
    IconWithTitle,

    /// <summary>
    /// Icon/emoji + Title + Description below.
    /// Best for: Rich lists with icons and details.
    /// Requires: IconField, DescriptionField parameters.
    /// </summary>
    IconTitleDescription,

    /// <summary>
    /// Card-style display with all available fields.
    /// Best for: Maximum information density.
    /// Uses: All available field parameters.
    /// </summary>
    Card
}
