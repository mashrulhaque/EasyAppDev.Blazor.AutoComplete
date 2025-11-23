using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using EasyAppDev.Blazor.AutoComplete.Filtering;
using EasyAppDev.Blazor.AutoComplete.Options;
using EasyAppDev.Blazor.AutoComplete.DataSources;
using EasyAppDev.Blazor.AutoComplete.Theming;

namespace EasyAppDev.Blazor.AutoComplete.Configuration;

/// <summary>
/// Configuration class for AutoComplete component. Use AutoCompleteConfigBuilder to create instances.
/// </summary>
/// <typeparam name="TItem">The type of items in the autocomplete list</typeparam>
public class AutoCompleteConfig<TItem>
{
    #region Data Properties

    /// <summary>
    /// The collection of items to display in the autocomplete.
    /// </summary>
    public IEnumerable<TItem>? Items { get; internal set; }

    /// <summary>
    /// Async data source for remote data fetching.
    /// </summary>
    public IAutoCompleteDataSource<TItem>? DataSource { get; internal set; }

    /// <summary>
    /// The currently selected value.
    /// </summary>
    public TItem? Value { get; internal set; }

    /// <summary>
    /// Event callback for when the value changes.
    /// </summary>
    public EventCallback<TItem?> ValueChanged { get; internal set; }

    #endregion

    #region Display Properties

    /// <summary>
    /// Expression to extract the text to display for each item.
    /// </summary>
    public Expression<Func<TItem, string>>? TextField { get; internal set; }

    /// <summary>
    /// Expression to extract multiple fields for searching.
    /// </summary>
    public Expression<Func<TItem, string[]>>? SearchFields { get; internal set; }

    /// <summary>
    /// Placeholder text for the input field.
    /// </summary>
    public string? Placeholder { get; internal set; }

    /// <summary>
    /// The theme to apply to the component.
    /// </summary>
    public Theme Theme { get; internal set; } = Theme.Auto;

    /// <summary>
    /// Bootstrap theme preset to apply coordinated colors to the component.
    /// </summary>
    public BootstrapTheme BootstrapTheme { get; internal set; } = BootstrapTheme.Default;

    /// <summary>
    /// Theme preset to apply. Loads corresponding CSS file and applies coordinated design system.
    /// Set to None to use custom properties only.
    /// </summary>
    public ThemePreset ThemePreset { get; internal set; } = ThemePreset.None;

    /// <summary>
    /// Theme overrides for colors, spacing, typography, and effects.
    /// Provides a structured way to customize the component's appearance.
    /// </summary>
    public ThemeOptions? ThemeOverrides { get; internal set; }

    /// <summary>
    /// Component size variant that adjusts spacing proportionally.
    /// </summary>
    public ComponentSize Size { get; internal set; } = ComponentSize.Default;

    /// <summary>
    /// Enable smooth transitions when theme properties change.
    /// Respects user's prefers-reduced-motion setting.
    /// </summary>
    public bool EnableThemeTransitions { get; internal set; } = true;

    /// <summary>
    /// Whether to use right-to-left text direction.
    /// </summary>
    public bool RightToLeft { get; internal set; } = false;

    #endregion

    #region Behavior Properties

    /// <summary>
    /// Minimum number of characters required before searching.
    /// </summary>
    public int MinSearchLength { get; internal set; } = 1;

    /// <summary>
    /// Maximum number of items to display in the dropdown.
    /// </summary>
    public int MaxDisplayedItems { get; internal set; } = 100;

    /// <summary>
    /// Debounce delay in milliseconds before filtering.
    /// </summary>
    public int DebounceMs { get; internal set; } = 300;

    /// <summary>
    /// Whether to show a clear button when text is entered.
    /// </summary>
    public bool AllowClear { get; internal set; } = true;

    /// <summary>
    /// Whether the component is disabled.
    /// </summary>
    public bool Disabled { get; internal set; } = false;

    /// <summary>
    /// Whether to close the dropdown when an item is selected.
    /// </summary>
    public bool CloseOnSelect { get; internal set; } = true;

    #endregion

    #region Filtering Properties

    /// <summary>
    /// The filtering strategy to use.
    /// </summary>
    public FilterStrategy FilterStrategy { get; internal set; } = FilterStrategy.StartsWith;

    /// <summary>
    /// Custom filter engine when FilterStrategy is set to Custom.
    /// </summary>
    public IFilterEngine<TItem>? CustomFilter { get; internal set; }

    #endregion

    #region Virtualization Properties

    /// <summary>
    /// Whether to enable virtualization for large datasets.
    /// </summary>
    public bool Virtualize { get; internal set; } = false;

    /// <summary>
    /// Minimum number of items before virtualization is enabled.
    /// </summary>
    public int VirtualizationThreshold { get; internal set; } = 100;

    /// <summary>
    /// Height of each item in pixels (required for virtualization).
    /// </summary>
    public float ItemHeight { get; internal set; } = 40f;

    #endregion

    #region Grouping Properties

    /// <summary>
    /// Expression to group items by a specific property.
    /// </summary>
    public Expression<Func<TItem, object>>? GroupBy { get; internal set; }

    /// <summary>
    /// Custom template for rendering group headers.
    /// </summary>
    public RenderFragment<IGrouping<object, TItem>>? GroupTemplate { get; internal set; }

    #endregion

    #region Display Mode Properties

    /// <summary>
    /// Display mode for rendering items.
    /// </summary>
    public ItemDisplayMode DisplayMode { get; internal set; } = ItemDisplayMode.Custom;

    /// <summary>
    /// Expression to extract description text for built-in display modes.
    /// </summary>
    public Expression<Func<TItem, string>>? DescriptionField { get; internal set; }

    /// <summary>
    /// Expression to extract badge text for built-in display modes.
    /// </summary>
    public Expression<Func<TItem, string>>? BadgeField { get; internal set; }

    /// <summary>
    /// Expression to extract icon/emoji text for built-in display modes.
    /// </summary>
    public Expression<Func<TItem, string>>? IconField { get; internal set; }

    /// <summary>
    /// Expression to extract subtitle text for built-in display modes.
    /// </summary>
    public Expression<Func<TItem, string>>? SubtitleField { get; internal set; }

    /// <summary>
    /// CSS class for badges in built-in display modes.
    /// </summary>
    public string BadgeClass { get; internal set; } = "badge bg-primary";

    #endregion

    #region Template Properties

    /// <summary>
    /// Custom template for rendering items.
    /// </summary>
    public RenderFragment<TItem>? ItemTemplate { get; internal set; }

    /// <summary>
    /// Custom template for rendering when no results are found.
    /// </summary>
    public RenderFragment? NoResultsTemplate { get; internal set; }

    /// <summary>
    /// Custom template for rendering a loading indicator.
    /// </summary>
    public RenderFragment? LoadingTemplate { get; internal set; }

    /// <summary>
    /// Custom template for rendering a header above the list.
    /// </summary>
    public RenderFragment? HeaderTemplate { get; internal set; }

    /// <summary>
    /// Custom template for rendering a footer below the list.
    /// </summary>
    public RenderFragment? FooterTemplate { get; internal set; }

    #endregion

    #region Accessibility Properties

    /// <summary>
    /// ARIA label for the component (for accessibility).
    /// </summary>
    public string? AriaLabel { get; internal set; }

    #endregion

    /// <summary>
    /// Creates a new builder for configuring AutoComplete settings.
    /// </summary>
    public static AutoCompleteConfigBuilder<TItem> Create()
        => new AutoCompleteConfigBuilder<TItem>();
}
