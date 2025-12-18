using EasyAppDev.Blazor.AutoComplete.Configuration;
using EasyAppDev.Blazor.AutoComplete.Utilities;

namespace EasyAppDev.Blazor.AutoComplete;

/// <summary>
/// Partial class containing configuration application logic.
/// This provides a manual fallback implementation when the source generator
/// is not available or fails to generate code.
/// </summary>
/// <remarks>
/// To disable this fallback when using the ConfigurationApplierGenerator,
/// define the AUTOCOMPLETE_USE_GENERATED_CONFIG symbol in your project.
/// </remarks>
public partial class AutoComplete<TItem>
{
#if !AUTOCOMPLETE_USE_GENERATED_CONFIG
    /// <summary>
    /// Fallback implementation for applying configuration.
    /// This is used when the ConfigurationApplierGenerator does not generate code.
    /// The generated partial method takes precedence when available.
    /// </summary>
    /// <remarks>
    /// This method ensures 100% coverage of all configuration properties.
    /// If you add new properties to AutoCompleteConfig, add them here too.
    /// </remarks>
    partial void ApplyConfigurationGenerated(AutoCompleteConfig<TItem> config)
    {
        // Data properties
        if (config.Items != null)
        {
            Items = config.Items;
        }

        if (config.DataSource != null)
        {
            DataSource = config.DataSource;
        }

        if (config.Value != null)
        {
            Value = config.Value;
        }

        if (config.ValueChanged.HasDelegate)
        {
            ValueChanged = config.ValueChanged;
        }

        // Display properties
        if (config.TextField != null)
        {
            TextField = config.TextField;
            _textFieldAccessor = ExpressionCompiler.CompileOrNull(TextField);
        }

        if (config.SearchFields != null)
        {
            SearchFields = config.SearchFields;
            _searchFieldsAccessor = ExpressionCompiler.CompileFieldsOrNull(SearchFields);
        }

        if (config.Placeholder != null)
        {
            Placeholder = config.Placeholder;
        }

        Theme = config.Theme;
        BootstrapTheme = config.BootstrapTheme;
        ThemePreset = config.ThemePreset;
        Size = config.Size;
        EnableThemeTransitions = config.EnableThemeTransitions;
        RightToLeft = config.RightToLeft;

        if (config.ThemeOverrides != null)
        {
            ThemeOverrides = config.ThemeOverrides;
        }

        // Behavior properties
        MinSearchLength = config.MinSearchLength;
        MaxDisplayedItems = config.MaxDisplayedItems;
        DebounceMs = config.DebounceMs;
        AllowClear = config.AllowClear;
        Disabled = config.Disabled;
        CloseOnSelect = config.CloseOnSelect;

        // Filtering properties
        FilterStrategy = config.FilterStrategy;

        if (config.CustomFilter != null)
        {
            CustomFilter = config.CustomFilter;
        }

        // Virtualization properties
        Virtualize = config.Virtualize;
        VirtualizationThreshold = config.VirtualizationThreshold;
        ItemHeight = config.ItemHeight;

        // Grouping properties
        if (config.GroupBy != null)
        {
            GroupBy = config.GroupBy;
            _groupByAccessor = GroupBy.Compile();
        }

        if (config.GroupTemplate != null)
        {
            GroupTemplate = config.GroupTemplate;
        }

        // Display mode properties
        DisplayMode = config.DisplayMode;

        if (config.DescriptionField != null)
        {
            DescriptionField = config.DescriptionField;
            _descriptionFieldAccessor = ExpressionCompiler.CompileOrNull(DescriptionField);
        }

        if (config.BadgeField != null)
        {
            BadgeField = config.BadgeField;
            _badgeFieldAccessor = ExpressionCompiler.CompileOrNull(BadgeField);
        }

        if (config.IconField != null)
        {
            IconField = config.IconField;
            _iconFieldAccessor = ExpressionCompiler.CompileOrNull(IconField);
        }

        if (config.SubtitleField != null)
        {
            SubtitleField = config.SubtitleField;
            _subtitleFieldAccessor = ExpressionCompiler.CompileOrNull(SubtitleField);
        }

        if (!string.IsNullOrEmpty(config.BadgeClass))
        {
            BadgeClass = config.BadgeClass;
        }

        // Template properties
        if (config.ItemTemplate != null)
        {
            ItemTemplate = config.ItemTemplate;
        }

        if (config.NoResultsTemplate != null)
        {
            NoResultsTemplate = config.NoResultsTemplate;
        }

        if (config.LoadingTemplate != null)
        {
            LoadingTemplate = config.LoadingTemplate;
        }

        if (config.HeaderTemplate != null)
        {
            HeaderTemplate = config.HeaderTemplate;
        }

        if (config.FooterTemplate != null)
        {
            FooterTemplate = config.FooterTemplate;
        }

        // Accessibility properties
        if (config.AriaLabel != null)
        {
            AriaLabel = config.AriaLabel;
        }

        // Re-initialize dependent services after configuration changes
        ReinitializeAfterConfigurationChange();
    }
#endif

    /// <summary>
    /// Re-initializes services that depend on configuration values.
    /// Called after configuration is applied to ensure consistency.
    /// </summary>
    private void ReinitializeAfterConfigurationChange()
    {
        // Re-initialize filter engine if strategy changed
        InitializeFilterEngine();

        // Re-initialize display mode renderer if mode changed
        InitializeDisplayModeRenderer();

        // Update debouncer if interval changed (use factory for consistency)
        _debouncer?.Dispose();
        _debouncer = _serviceFactory.CreateDebouncer(DebounceMs);
    }
}
