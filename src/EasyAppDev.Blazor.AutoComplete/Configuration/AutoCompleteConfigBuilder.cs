using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using EasyAppDev.Blazor.AutoComplete.Filtering;
using EasyAppDev.Blazor.AutoComplete.Options;
using EasyAppDev.Blazor.AutoComplete.DataSources;
using EasyAppDev.Blazor.AutoComplete.Theming;

namespace EasyAppDev.Blazor.AutoComplete.Configuration;

/// <summary>
/// Fluent builder for configuring AutoComplete component settings.
/// Provides a clean, readable way to set up complex AutoComplete configurations.
/// </summary>
/// <typeparam name="TItem">The type of items in the autocomplete list</typeparam>
public class AutoCompleteConfigBuilder<TItem>
{
    private readonly AutoCompleteConfig<TItem> _config = new();

    #region Data Configuration

    /// <summary>
    /// Sets the collection of items to display in the autocomplete.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithItems(IEnumerable<TItem> items)
    {
        _config.Items = items;
        return this;
    }

    /// <summary>
    /// Sets the async data source for remote data fetching.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithDataSource(IAutoCompleteDataSource<TItem> dataSource)
    {
        _config.DataSource = dataSource;
        return this;
    }

    /// <summary>
    /// Sets the initial selected value.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithValue(TItem? value)
    {
        _config.Value = value;
        return this;
    }

    /// <summary>
    /// Sets the value changed event callback.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithValueChanged(EventCallback<TItem?> valueChanged)
    {
        _config.ValueChanged = valueChanged;
        return this;
    }

    #endregion

    #region Display Configuration

    /// <summary>
    /// Sets the expression to extract the text to display for each item.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithTextField(Expression<Func<TItem, string>> textField)
    {
        _config.TextField = textField;
        return this;
    }

    /// <summary>
    /// Sets the expression to extract multiple fields for searching.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithSearchFields(Expression<Func<TItem, string[]>> searchFields)
    {
        _config.SearchFields = searchFields;
        return this;
    }

    /// <summary>
    /// Sets the placeholder text for the input field.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithPlaceholder(string placeholder)
    {
        _config.Placeholder = placeholder;
        return this;
    }

    /// <summary>
    /// Sets the theme for the component.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithTheme(Theme theme)
    {
        _config.Theme = theme;
        return this;
    }

    /// <summary>
    /// Sets the Bootstrap theme preset for coordinated colors.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithBootstrapTheme(BootstrapTheme bootstrapTheme)
    {
        _config.BootstrapTheme = bootstrapTheme;
        return this;
    }

    /// <summary>
    /// Sets the theme preset to apply (Material, Fluent, Modern, Bootstrap).
    /// Loads corresponding CSS file and applies coordinated design system.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithThemePreset(ThemePreset themePreset)
    {
        _config.ThemePreset = themePreset;
        return this;
    }

    /// <summary>
    /// Sets theme overrides for colors, spacing, typography, and effects.
    /// Provides a structured way to customize the component's appearance.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithThemeOverrides(ThemeOptions themeOverrides)
    {
        _config.ThemeOverrides = themeOverrides;
        return this;
    }

    /// <summary>
    /// Sets the component size variant that adjusts spacing proportionally.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithSize(ComponentSize size)
    {
        _config.Size = size;
        return this;
    }

    /// <summary>
    /// Enables or disables smooth transitions when theme properties change.
    /// Respects user's prefers-reduced-motion setting.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithThemeTransitions(bool enableTransitions = true)
    {
        _config.EnableThemeTransitions = enableTransitions;
        return this;
    }

    /// <summary>
    /// Sets color overrides for the theme.
    /// Convenience method that creates or updates the ThemeOverrides.Colors property.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithColors(ColorOptions colors)
    {
        _config.ThemeOverrides ??= new ThemeOptions();
        _config.ThemeOverrides = _config.ThemeOverrides with { Colors = colors };
        return this;
    }

    /// <summary>
    /// Sets spacing overrides for the theme.
    /// Convenience method that creates or updates the ThemeOverrides.Spacing property.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithSpacing(SpacingOptions spacing)
    {
        _config.ThemeOverrides ??= new ThemeOptions();
        _config.ThemeOverrides = _config.ThemeOverrides with { Spacing = spacing };
        return this;
    }

    /// <summary>
    /// Sets typography overrides for the theme.
    /// Convenience method that creates or updates the ThemeOverrides.Typography property.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithTypography(TypographyOptions typography)
    {
        _config.ThemeOverrides ??= new ThemeOptions();
        _config.ThemeOverrides = _config.ThemeOverrides with { Typography = typography };
        return this;
    }

    /// <summary>
    /// Sets effect overrides for the theme (shadows, transitions).
    /// Convenience method that creates or updates the ThemeOverrides.Effects property.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithEffects(EffectOptions effects)
    {
        _config.ThemeOverrides ??= new ThemeOptions();
        _config.ThemeOverrides = _config.ThemeOverrides with { Effects = effects };
        return this;
    }

    /// <summary>
    /// Enables right-to-left text direction.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithRightToLeft(bool rightToLeft = true)
    {
        _config.RightToLeft = rightToLeft;
        return this;
    }

    #endregion

    #region Behavior Configuration

    /// <summary>
    /// Sets the minimum number of characters required before searching.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithMinSearchLength(int minSearchLength)
    {
        _config.MinSearchLength = minSearchLength;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of items to display in the dropdown.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithMaxDisplayedItems(int maxDisplayedItems)
    {
        _config.MaxDisplayedItems = maxDisplayedItems;
        return this;
    }

    /// <summary>
    /// Sets the debounce delay in milliseconds before filtering.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithDebounce(int debounceMs)
    {
        _config.DebounceMs = debounceMs;
        return this;
    }

    /// <summary>
    /// Enables or disables the clear button.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithAllowClear(bool allowClear = true)
    {
        _config.AllowClear = allowClear;
        return this;
    }

    /// <summary>
    /// Enables or disables the component.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithDisabled(bool disabled = true)
    {
        _config.Disabled = disabled;
        return this;
    }

    /// <summary>
    /// Sets whether to close the dropdown when an item is selected.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithCloseOnSelect(bool closeOnSelect = true)
    {
        _config.CloseOnSelect = closeOnSelect;
        return this;
    }

    #endregion

    #region Filtering Configuration

    /// <summary>
    /// Sets the filtering strategy to use.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithFilterStrategy(FilterStrategy strategy)
    {
        _config.FilterStrategy = strategy;
        return this;
    }

    /// <summary>
    /// Sets a custom filter engine.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithCustomFilter(IFilterEngine<TItem> customFilter)
    {
        _config.CustomFilter = customFilter;
        _config.FilterStrategy = FilterStrategy.Custom;
        return this;
    }

    #endregion

    #region Virtualization Configuration

    /// <summary>
    /// Enables virtualization with the specified threshold and item height.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithVirtualization(int threshold = 100, float itemHeight = 40f)
    {
        _config.Virtualize = true;
        _config.VirtualizationThreshold = threshold;
        _config.ItemHeight = itemHeight;
        return this;
    }

    /// <summary>
    /// Disables virtualization.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithoutVirtualization()
    {
        _config.Virtualize = false;
        return this;
    }

    #endregion

    #region Grouping Configuration

    /// <summary>
    /// Sets the expression to group items by a specific property.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithGrouping(Expression<Func<TItem, object>> groupBy)
    {
        _config.GroupBy = groupBy;
        return this;
    }

    /// <summary>
    /// Sets a custom template for rendering group headers.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithGroupTemplate(RenderFragment<IGrouping<object, TItem>> groupTemplate)
    {
        _config.GroupTemplate = groupTemplate;
        return this;
    }

    #endregion

    #region Display Mode Configuration

    /// <summary>
    /// Sets the display mode for rendering items.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithDisplayMode(ItemDisplayMode displayMode)
    {
        _config.DisplayMode = displayMode;
        return this;
    }

    /// <summary>
    /// Configures title with description display mode.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithTitleAndDescription(
        Expression<Func<TItem, string>> descriptionField)
    {
        _config.DisplayMode = ItemDisplayMode.TitleWithDescription;
        _config.DescriptionField = descriptionField;
        return this;
    }

    /// <summary>
    /// Configures title with badge display mode.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithTitleAndBadge(
        Expression<Func<TItem, string>> badgeField,
        string badgeClass = "badge bg-primary")
    {
        _config.DisplayMode = ItemDisplayMode.TitleWithBadge;
        _config.BadgeField = badgeField;
        _config.BadgeClass = badgeClass;
        return this;
    }

    /// <summary>
    /// Configures title with description and badge display mode.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithTitleDescriptionAndBadge(
        Expression<Func<TItem, string>> descriptionField,
        Expression<Func<TItem, string>> badgeField,
        string badgeClass = "badge bg-primary")
    {
        _config.DisplayMode = ItemDisplayMode.TitleDescriptionBadge;
        _config.DescriptionField = descriptionField;
        _config.BadgeField = badgeField;
        _config.BadgeClass = badgeClass;
        return this;
    }

    /// <summary>
    /// Configures icon with title display mode.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithIconAndTitle(
        Expression<Func<TItem, string>> iconField)
    {
        _config.DisplayMode = ItemDisplayMode.IconWithTitle;
        _config.IconField = iconField;
        return this;
    }

    /// <summary>
    /// Configures icon with title and description display mode.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithIconTitleAndDescription(
        Expression<Func<TItem, string>> iconField,
        Expression<Func<TItem, string>> descriptionField)
    {
        _config.DisplayMode = ItemDisplayMode.IconTitleDescription;
        _config.IconField = iconField;
        _config.DescriptionField = descriptionField;
        return this;
    }

    /// <summary>
    /// Configures card display mode with all fields.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithCardMode(
        Expression<Func<TItem, string>>? iconField = null,
        Expression<Func<TItem, string>>? subtitleField = null,
        Expression<Func<TItem, string>>? descriptionField = null,
        Expression<Func<TItem, string>>? badgeField = null,
        string badgeClass = "badge bg-primary")
    {
        _config.DisplayMode = ItemDisplayMode.Card;
        _config.IconField = iconField;
        _config.SubtitleField = subtitleField;
        _config.DescriptionField = descriptionField;
        _config.BadgeField = badgeField;
        _config.BadgeClass = badgeClass;
        return this;
    }

    /// <summary>
    /// Sets the description field for built-in display modes.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithDescriptionField(Expression<Func<TItem, string>> descriptionField)
    {
        _config.DescriptionField = descriptionField;
        return this;
    }

    /// <summary>
    /// Sets the badge field for built-in display modes.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithBadgeField(Expression<Func<TItem, string>> badgeField, string badgeClass = "badge bg-primary")
    {
        _config.BadgeField = badgeField;
        _config.BadgeClass = badgeClass;
        return this;
    }

    /// <summary>
    /// Sets the icon field for built-in display modes.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithIconField(Expression<Func<TItem, string>> iconField)
    {
        _config.IconField = iconField;
        return this;
    }

    /// <summary>
    /// Sets the subtitle field for built-in display modes.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithSubtitleField(Expression<Func<TItem, string>> subtitleField)
    {
        _config.SubtitleField = subtitleField;
        return this;
    }

    #endregion

    #region Template Configuration

    /// <summary>
    /// Sets a custom template for rendering items.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithItemTemplate(RenderFragment<TItem> itemTemplate)
    {
        _config.ItemTemplate = itemTemplate;
        return this;
    }

    /// <summary>
    /// Sets a custom template for rendering when no results are found.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithNoResultsTemplate(RenderFragment noResultsTemplate)
    {
        _config.NoResultsTemplate = noResultsTemplate;
        return this;
    }

    /// <summary>
    /// Sets a custom template for rendering a loading indicator.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithLoadingTemplate(RenderFragment loadingTemplate)
    {
        _config.LoadingTemplate = loadingTemplate;
        return this;
    }

    /// <summary>
    /// Sets a custom template for rendering a header above the list.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithHeaderTemplate(RenderFragment headerTemplate)
    {
        _config.HeaderTemplate = headerTemplate;
        return this;
    }

    /// <summary>
    /// Sets a custom template for rendering a footer below the list.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithFooterTemplate(RenderFragment footerTemplate)
    {
        _config.FooterTemplate = footerTemplate;
        return this;
    }

    #endregion

    #region Accessibility Configuration

    /// <summary>
    /// Sets the ARIA label for the component.
    /// </summary>
    public AutoCompleteConfigBuilder<TItem> WithAriaLabel(string ariaLabel)
    {
        _config.AriaLabel = ariaLabel;
        return this;
    }

    #endregion

    /// <summary>
    /// Builds and returns the final configuration.
    /// </summary>
    public AutoCompleteConfig<TItem> Build() => _config;
}
