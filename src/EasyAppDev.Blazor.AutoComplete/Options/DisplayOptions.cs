using System.Linq.Expressions;

namespace EasyAppDev.Blazor.AutoComplete.Options;

/// <summary>
/// Options for configuring item display in the AutoComplete component.
/// Groups all display mode-related parameters into a single options object.
/// </summary>
/// <typeparam name="TItem">The type of items in the autocomplete list.</typeparam>
public class DisplayOptions<TItem>
{
    /// <summary>
    /// Gets or sets the display mode for rendering items.
    /// Default is Custom (uses ItemTemplate or falls back to Simple).
    /// </summary>
    public ItemDisplayMode Mode { get; set; } = ItemDisplayMode.Custom;

    /// <summary>
    /// Gets or sets the expression to extract description text.
    /// Used by: TitleWithDescription, TitleDescriptionBadge, IconTitleDescription, Card.
    /// </summary>
    public Expression<Func<TItem, string>>? DescriptionField { get; set; }

    /// <summary>
    /// Gets or sets the badge options for display modes that show badges.
    /// </summary>
    public BadgeOptions<TItem>? Badge { get; set; }

    /// <summary>
    /// Gets or sets the expression to extract icon/emoji text.
    /// Used by: IconWithTitle, IconTitleDescription, Card.
    /// </summary>
    public Expression<Func<TItem, string>>? IconField { get; set; }

    /// <summary>
    /// Gets or sets the expression to extract subtitle text.
    /// Used by: Card mode.
    /// </summary>
    public Expression<Func<TItem, string>>? SubtitleField { get; set; }
}

/// <summary>
/// Options for configuring badge display in the AutoComplete component.
/// Groups badge-related parameters into a single options object.
/// </summary>
/// <typeparam name="TItem">The type of items in the autocomplete list.</typeparam>
public class BadgeOptions<TItem>
{
    /// <summary>
    /// Gets or sets the expression to extract badge text.
    /// Used by: TitleWithBadge, TitleDescriptionBadge, Card.
    /// </summary>
    public Expression<Func<TItem, string>>? Field { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for badges.
    /// Default is "badge bg-primary" (Bootstrap 5 style).
    /// </summary>
    public string CssClass { get; set; } = "badge bg-primary";

    /// <summary>
    /// Gets or sets a function to dynamically determine badge CSS class per item.
    /// If provided, takes precedence over CssClass.
    /// </summary>
    public Func<TItem, string>? CssClassSelector { get; set; }
}

/// <summary>
/// Options for configuring virtualization behavior.
/// Groups virtualization-related parameters into a single options object.
/// </summary>
public class VirtualizationOptions
{
    /// <summary>
    /// Gets or sets whether virtualization is enabled.
    /// Default is false.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the minimum number of items before virtualization kicks in.
    /// Default is 100 items.
    /// </summary>
    public int Threshold { get; set; } = 100;

    /// <summary>
    /// Gets or sets the height of each item in pixels.
    /// Required for accurate virtualization calculations.
    /// Default is 40px.
    /// </summary>
    public float ItemHeight { get; set; } = 40f;
}

/// <summary>
/// Options for configuring debounce behavior.
/// </summary>
public class DebounceOptions
{
    /// <summary>
    /// Gets or sets the debounce delay in milliseconds.
    /// Set to 0 to disable debouncing.
    /// Default is 300ms.
    /// </summary>
    public int DelayMs { get; set; } = 300;

    /// <summary>
    /// Gets or sets whether to cancel pending operations when new input arrives.
    /// Default is true.
    /// </summary>
    public bool CancelOnNewInput { get; set; } = true;
}

/// <summary>
/// Options for configuring security-related behavior.
/// </summary>
public class SecurityOptions
{
    /// <summary>
    /// Gets or sets the maximum allowed search text length.
    /// Values exceeding this limit will be truncated.
    /// Default is 500, maximum is 2000.
    /// </summary>
    public int MaxSearchLength { get; set; } = 500;
}
