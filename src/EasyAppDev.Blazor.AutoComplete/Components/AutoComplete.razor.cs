using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Linq.Expressions;
using EasyAppDev.Blazor.AutoComplete.Filtering;
using EasyAppDev.Blazor.AutoComplete.Options;
using EasyAppDev.Blazor.AutoComplete.Utilities;
using EasyAppDev.Blazor.AutoComplete.Accessibility;
using EasyAppDev.Blazor.AutoComplete.DataSources;
using EasyAppDev.Blazor.AutoComplete.Configuration;
using EasyAppDev.Blazor.AutoComplete.Theming;
using EasyAppDev.Blazor.AutoComplete.Rendering;

namespace EasyAppDev.Blazor.AutoComplete;

/// <summary>
/// A high-performance autocomplete component with support for virtualization,
/// theming, and AI-powered search.
/// </summary>
/// <typeparam name="TItem">The type of items in the autocomplete list</typeparam>
public partial class AutoComplete<TItem> : ComponentBase, IAsyncDisposable
{
    #region Parameters

    /// <summary>
    /// Fluent configuration object for the AutoComplete component.
    /// When provided, individual parameters are ignored and the configuration is used instead.
    /// Use AutoCompleteConfig&lt;TItem&gt;.Create() to build a configuration.
    /// </summary>
    [Parameter]
    public AutoCompleteConfig<TItem>? Config { get; set; }

    /// <summary>
    /// The collection of items to display in the autocomplete.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>
    /// The currently selected value.
    /// </summary>
    [Parameter]
    public TItem? Value { get; set; }

    /// <summary>
    /// Event callback for when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TItem?> ValueChanged { get; set; }

    /// <summary>
    /// Expression to extract the text to display for each item.
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, string>>? TextField { get; set; }

    /// <summary>
    /// Expression to extract multiple fields for searching across multiple properties.
    /// When specified, the search will match if any of the fields contain the search term.
    /// This is useful for searching across name, description, tags, etc.
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, string[]>>? SearchFields { get; set; }

    /// <summary>
    /// Placeholder text for the input field.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Minimum number of characters required before searching.
    /// </summary>
    [Parameter]
    public int MinSearchLength { get; set; } = 1;

    /// <summary>
    /// Maximum number of items to display in the dropdown.
    /// </summary>
    [Parameter]
    public int MaxDisplayedItems { get; set; } = 100;

    /// <summary>
    /// Whether to show a clear button when text is entered.
    /// </summary>
    [Parameter]
    public bool AllowClear { get; set; } = true;

    /// <summary>
    /// Whether the component is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// The theme to apply to the component.
    /// </summary>
    [Parameter]
    public Theme Theme { get; set; } = Theme.Auto;

    /// <summary>
    /// Bootstrap theme preset to apply coordinated colors to the component.
    /// This applies Bootstrap 5 color schemes to input borders, focus states,
    /// selected items, badges, and other visual elements.
    /// </summary>
    [Parameter]
    public BootstrapTheme BootstrapTheme { get; set; } = BootstrapTheme.Default;

    /// <summary>
    /// Theme preset to apply. Loads corresponding CSS file and applies coordinated design system.
    /// Set to None to use custom properties only.
    /// </summary>
    [Parameter]
    public ThemePreset ThemePreset { get; set; } = ThemePreset.None;

    /// <summary>
    /// Component size variant that adjusts spacing proportionally.
    /// </summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Default;

    /// <summary>
    /// Enable smooth transitions when theme properties change.
    /// Respects user's prefers-reduced-motion setting.
    /// </summary>
    [Parameter]
    public bool EnableThemeTransitions { get; set; } = true;

    /// <summary>
    /// Theme overrides for colors, spacing, typography, and effects.
    /// Provides a structured way to customize the component's appearance.
    /// Individual CSS custom properties can be overridden via nested options.
    /// Note: Individual theme parameters (PrimaryColor, BorderRadius, etc.) take precedence over ThemeOverrides.
    /// </summary>
    [Parameter]
    public ThemeOptions? ThemeOverrides { get; set; }

    // Individual theme parameters (convenience properties that override ThemeOverrides)

    /// <summary>
    /// Primary brand color (e.g., "#007bff"). Overrides ThemeOverrides.Colors.Primary.
    /// </summary>
    [Parameter]
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// Background color for input and dropdown (e.g., "#ffffff"). Overrides ThemeOverrides.Colors.Background.
    /// </summary>
    [Parameter]
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// Primary text color (e.g., "#212529"). Overrides ThemeOverrides.Colors.Text.
    /// </summary>
    [Parameter]
    public string? TextColor { get; set; }

    /// <summary>
    /// Border color (e.g., "#ced4da"). Overrides ThemeOverrides.Colors.Border.
    /// </summary>
    [Parameter]
    public string? BorderColor { get; set; }

    /// <summary>
    /// Hover state background color (e.g., "#f8f9fa"). Overrides ThemeOverrides.Colors.Hover.
    /// </summary>
    [Parameter]
    public string? HoverColor { get; set; }

    /// <summary>
    /// Selected item background color (e.g., "#e7f1ff"). Overrides ThemeOverrides.Colors.Selected.
    /// </summary>
    [Parameter]
    public string? SelectedColor { get; set; }

    /// <summary>
    /// Border radius for rounded corners (e.g., "4px", "0.375rem"). Overrides ThemeOverrides.Spacing.BorderRadius.
    /// </summary>
    [Parameter]
    public string? BorderRadius { get; set; }

    /// <summary>
    /// Font family stack (e.g., "Roboto, sans-serif"). Overrides ThemeOverrides.Typography.FontFamily.
    /// </summary>
    [Parameter]
    public string? FontFamily { get; set; }

    /// <summary>
    /// Base font size (e.g., "14px", "1rem"). Overrides ThemeOverrides.Typography.FontSize.
    /// </summary>
    [Parameter]
    public string? FontSize { get; set; }

    /// <summary>
    /// Dropdown shadow (e.g., "0 4px 6px rgba(0, 0, 0, 0.1)"). Overrides ThemeOverrides.Effects.DropdownShadow.
    /// </summary>
    [Parameter]
    public string? DropdownShadow { get; set; }

    /// <summary>
    /// Whether to use right-to-left text direction.
    /// </summary>
    [Parameter]
    public bool RightToLeft { get; set; } = false;

    /// <summary>
    /// Custom template for rendering items.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Custom template for rendering when no results are found.
    /// </summary>
    [Parameter]
    public RenderFragment? NoResultsTemplate { get; set; }

    /// <summary>
    /// Debounce delay in milliseconds before filtering. Set to 0 to disable debouncing.
    /// Default is 300ms.
    /// </summary>
    [Parameter]
    public int DebounceMs { get; set; } = 300;

    /// <summary>
    /// Maximum allowed length for search text to prevent memory exhaustion attacks.
    /// Default is 500 characters. Maximum allowed value is 2000.
    /// Values exceeding this limit will be truncated.
    /// </summary>
    [Parameter]
    public int MaxSearchLength { get; set; } = 500;

    /// <summary>
    /// Whether to enable virtualization for large datasets.
    /// </summary>
    [Parameter]
    public bool Virtualize { get; set; } = false;

    /// <summary>
    /// Minimum number of items before virtualization is enabled.
    /// Default is 100 items.
    /// </summary>
    [Parameter]
    public int VirtualizationThreshold { get; set; } = 100;

    /// <summary>
    /// Height of each item in pixels (required for virtualization).
    /// Default is 40px.
    /// </summary>
    [Parameter]
    public float ItemHeight { get; set; } = 40f;

    /// <summary>
    /// The filtering strategy to use.
    /// </summary>
    [Parameter]
    public FilterStrategy FilterStrategy { get; set; } = FilterStrategy.StartsWith;

    /// <summary>
    /// Custom filter engine when FilterStrategy is set to Custom.
    /// </summary>
    [Parameter]
    public IFilterEngine<TItem>? CustomFilter { get; set; }

    /// <summary>
    /// Expression to group items by a specific property.
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, object>>? GroupBy { get; set; }

    /// <summary>
    /// Custom template for rendering group headers.
    /// </summary>
    [Parameter]
    public RenderFragment<IGrouping<object, TItem>>? GroupTemplate { get; set; }

    /// <summary>
    /// Display mode for rendering items. Built-in modes eliminate the need for custom ItemTemplate markup.
    /// When set to Custom (default), ItemTemplate is used. If ItemTemplate is null, falls back to Simple mode.
    /// </summary>
    [Parameter]
    public ItemDisplayMode DisplayMode { get; set; } = ItemDisplayMode.Custom;

    /// <summary>
    /// Expression to extract description text for built-in display modes.
    /// Used by: TitleWithDescription, TitleDescriptionBadge, IconTitleDescription, Card.
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, string>>? DescriptionField { get; set; }

    /// <summary>
    /// Expression to extract badge text for built-in display modes.
    /// Used by: TitleWithBadge, TitleDescriptionBadge, Card.
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, string>>? BadgeField { get; set; }

    /// <summary>
    /// Expression to extract icon/emoji text for built-in display modes.
    /// Used by: IconWithTitle, IconTitleDescription, Card.
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, string>>? IconField { get; set; }

    /// <summary>
    /// Expression to extract subtitle text for built-in display modes.
    /// Used by: Card mode.
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, string>>? SubtitleField { get; set; }

    /// <summary>
    /// CSS class for badges in built-in display modes.
    /// Default is "badge bg-primary" (Bootstrap 5 style).
    /// </summary>
    [Parameter]
    public string BadgeClass { get; set; } = "badge bg-primary";

    /// <summary>
    /// Async data source for remote data fetching.
    /// If provided, this takes precedence over the Items parameter.
    /// </summary>
    [Parameter]
    public IAutoCompleteDataSource<TItem>? DataSource { get; set; }

    /// <summary>
    /// Custom template for rendering a loading indicator.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    /// <summary>
    /// Custom template for rendering a header above the list.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Custom template for rendering a footer below the list.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Expression for the value (used for validation integration).
    /// </summary>
    [Parameter]
    public Expression<Func<TItem?>>? ValueExpression { get; set; }

    /// <summary>
    /// ARIA label for the component (for accessibility).
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Optional explicit ID for the input element.
    /// Use this to associate an external label via the 'for' attribute.
    /// If not provided, a unique ID is auto-generated.
    /// Example: &lt;label for="product-search"&gt;Search:&lt;/label&gt;
    ///          &lt;AutoComplete InputId="product-search" ... /&gt;
    /// </summary>
    [Parameter]
    public string? InputId { get; set; }

    /// <summary>
    /// Whether to close the dropdown when an item is selected.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool CloseOnSelect { get; set; } = true;

    /// <summary>
    /// Cascading parameter for the EditContext (for validation integration).
    /// </summary>
    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    #endregion

    #region Private Fields

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private string _searchText = string.Empty;
    private bool _isDropdownOpen = false;
    private List<TItem> _filteredItems = new();
    private List<IGrouping<object, TItem>> _groupedItems = new();
    private Func<TItem, string>? _textFieldAccessor;
    private Func<TItem, string>? _descriptionFieldAccessor;
    private Func<TItem, string>? _badgeFieldAccessor;
    private Func<TItem, string>? _iconFieldAccessor;
    private Func<TItem, string>? _subtitleFieldAccessor;
    private Func<TItem, string[]>? _searchFieldsAccessor;
    private IFilterEngine<TItem> _filterEngine = new StartsWithFilter<TItem>();
    private IDisplayModeRenderer<TItem> _displayModeRenderer = null!;
    private DisplayModeRenderContext<TItem> _renderContext = null!;
    private DebounceTimer? _debounceTimer;
    private KeyboardNavigationHandler<TItem>? _keyboardHandler;
    private FieldIdentifier _fieldIdentifier;
    private bool _isLoading = false;
    private CancellationTokenSource? _loadingCancellationTokenSource;
    private bool _shouldPreventDefault = false;

    // Theme management
    private ThemeManager? _themeManager;
    private string? _themeStyle;
    private string _themeClasses = string.Empty;

    // ARIA IDs
    private readonly string _componentId = Guid.NewGuid().ToString("N");
    private string _listboxId => $"ebd-ac-listbox-{_componentId}";
    private string _inputIdValue => InputId ?? $"ebd-ac-input-{_componentId}";
    private string _errorId => $"ebd-ac-error-{_componentId}";
    private string _statusId => $"ebd-ac-status-{_componentId}";
    private string GetItemId(int index) => $"ebd-ac-item-{_componentId}-{index}";
    private string? _activeDescendantId => _keyboardHandler?.SelectedIndex >= 0
        ? GetItemId(_keyboardHandler.SelectedIndex)
        : null;

    #endregion

    #region Lifecycle Methods

    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Initialize theme manager
        _themeManager = new ThemeManager();

        // Compile field expressions using ExpressionCompiler utility
        _textFieldAccessor = ExpressionCompiler.CompileOrNull(TextField);
        _searchFieldsAccessor = ExpressionCompiler.CompileFieldsOrNull(SearchFields);
        _descriptionFieldAccessor = ExpressionCompiler.CompileOrNull(DescriptionField);
        _badgeFieldAccessor = ExpressionCompiler.CompileOrNull(BadgeField);
        _iconFieldAccessor = ExpressionCompiler.CompileOrNull(IconField);
        _subtitleFieldAccessor = ExpressionCompiler.CompileOrNull(SubtitleField);

        // Initialize filter engine based on strategy
        _filterEngine = FilterStrategy switch
        {
            FilterStrategy.StartsWith => new StartsWithFilter<TItem>(),
            FilterStrategy.Contains => new ContainsFilter<TItem>(),
            FilterStrategy.Fuzzy => new FuzzyFilter<TItem>(),
            FilterStrategy.Custom when CustomFilter != null => CustomFilter,
            _ => new StartsWithFilter<TItem>()
        };

        // Initialize display mode renderer
        _displayModeRenderer = DisplayModeRendererFactory.GetRenderer<TItem>(DisplayMode);
        _renderContext = new DisplayModeRenderContext<TItem>
        {
            GetItemText = GetItemText,
            GetDescriptionText = GetDescriptionText,
            GetBadgeText = GetBadgeText,
            GetIconText = GetIconText,
            GetSubtitleText = GetSubtitleText,
            BadgeClass = BadgeClass
        };

        // Initialize debounce timer if debouncing is enabled
        if (DebounceMs > 0)
        {
            _debounceTimer = new DebounceTimer(DebounceMs);
        }

        // Initialize keyboard navigation handler
        _keyboardHandler = new KeyboardNavigationHandler<TItem>(_filteredItems);

        // Set up validation if EditContext and ValueExpression are provided
        if (EditContext != null && ValueExpression != null)
        {
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
            EditContext.OnValidationStateChanged += OnValidationStateChanged;
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // Apply configuration from Config parameter if provided
        if (Config != null)
        {
            ApplyConfiguration(Config);
        }

        // Update CSS classes and inline styles
        UpdateThemeClassesAndStyles();

        // Update filtered items when Items parameter changes
        if (!string.IsNullOrEmpty(_searchText) && (Items != null || DataSource != null))
        {
            await FilterItemsAsync();
        }
    }

    /// <summary>
    /// Applies configuration from AutoCompleteConfig to component parameters.
    /// Calls the auto-generated method if available, otherwise uses manual fallback.
    /// </summary>
    private void ApplyConfiguration(AutoCompleteConfig<TItem> config)
    {
        // Call the auto-generated method (generated by ConfigurationApplierGenerator)
        // This ensures all configuration properties are automatically applied
        ApplyConfigurationGenerated(config);
    }

    // Partial method declaration - implementation is auto-generated by the
    // ConfigurationApplierGenerator source generator at build time.
    // See: src/EasyAppDev.Blazor.AutoComplete.Generators/ConfigurationApplierGenerator.cs
    partial void ApplyConfigurationGenerated(AutoCompleteConfig<TItem> config);

    #endregion

    #region Event Handlers

    private async Task OnSearchTextChangedAsync()
    {
        // Security: Enforce maximum search length to prevent memory exhaustion
        var effectiveMaxLength = GetEffectiveMaxSearchLength();
        if (_searchText.Length > effectiveMaxLength)
        {
            _searchText = _searchText.Substring(0, effectiveMaxLength);
        }

        if (_searchText.Length >= MinSearchLength)
        {
            if (_debounceTimer != null)
            {
                // Capture the current search text to avoid closure issues
                var currentSearchText = _searchText;
                _debounceTimer.Debounce(async () =>
                {
                    // Only filter if the search text hasn't changed
                    if (_searchText == currentSearchText)
                    {
                        // Set loading state and open dropdown BEFORE filtering
                        _isLoading = true;
                        _isDropdownOpen = true;
                        if (_keyboardHandler != null)
                        {
                            _keyboardHandler.IsOpen = true;
                        }
                        await InvokeAsync(StateHasChanged);

                        await FilterItemsAsync();
                        await InvokeAsync(StateHasChanged);
                    }
                });
            }
            else
            {
                // Set loading state and open dropdown BEFORE filtering
                _isLoading = true;
                _isDropdownOpen = true;
                if (_keyboardHandler != null)
                {
                    _keyboardHandler.IsOpen = true;
                }
                await InvokeAsync(StateHasChanged);

                await FilterItemsAsync();
            }
        }
        else
        {
            // Cancel any pending debounced actions
            _debounceTimer?.Cancel();

            _filteredItems.Clear();
            _groupedItems.Clear();
            _isDropdownOpen = false;
            _isLoading = false;
            _keyboardHandler?.Reset();
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (_keyboardHandler == null)
        {
            return;
        }

        // Only prevent default for navigation keys, not for typing keys
        _shouldPreventDefault = e.Key switch
        {
            "ArrowDown" or "ArrowUp" or "Enter" or "Escape" or
            "Home" or "End" or "PageUp" or "PageDown" => _keyboardHandler.SelectedIndex >= 0 || _isDropdownOpen,
            _ => false
        };

        var action = _keyboardHandler.HandleKeyDown(e);

        switch (action.Type)
        {
            case NavigationActionType.OpenAndSelect:
                _isDropdownOpen = true;
                _keyboardHandler.IsOpen = true;
                await InvokeAsync(StateHasChanged);
                break;

            case NavigationActionType.SelectNext:
            case NavigationActionType.SelectPrevious:
            case NavigationActionType.SelectFirst:
            case NavigationActionType.SelectLast:
                await InvokeAsync(StateHasChanged);
                break;

            case NavigationActionType.ConfirmSelection:
                if (action.SelectedItem is TItem selectedItem)
                {
                    await SelectItem(selectedItem);
                }
                break;

            case NavigationActionType.Close:
                _isDropdownOpen = false;
                _keyboardHandler.Reset();
                await InvokeAsync(StateHasChanged);
                break;
        }
    }

    private void HandleMouseEnter(int index)
    {
        if (_keyboardHandler != null)
        {
            _keyboardHandler.SelectedIndex = index;
            StateHasChanged();
        }
    }

    private void OnFocusIn()
    {
        if (_searchText.Length >= MinSearchLength && _filteredItems.Any())
        {
            _isDropdownOpen = true;
            if (_keyboardHandler != null)
            {
                _keyboardHandler.IsOpen = true;
            }
        }
    }

    private void OnFocusOut()
    {
        // Delay to allow click events on dropdown items
        Task.Delay(200).ContinueWith(_ =>
        {
            _isDropdownOpen = false;
            InvokeAsync(StateHasChanged);
        });
    }

    private async Task SelectItem(TItem item)
    {
        Value = item;
        _searchText = GetItemText(item);

        if (CloseOnSelect)
        {
            _isDropdownOpen = false;
        }

        _keyboardHandler?.Reset();

        await ValueChanged.InvokeAsync(Value);

        // Notify EditContext if validation is enabled
        if (EditContext != null && ValueExpression != null)
        {
            EditContext.NotifyFieldChanged(_fieldIdentifier);
        }
    }

    private void ClearSearch()
    {
        // Cancel any pending debounced actions
        _debounceTimer?.Cancel();

        _searchText = string.Empty;
        _filteredItems.Clear();
        _groupedItems.Clear();
        _isDropdownOpen = false;
        Value = default;
        _keyboardHandler?.Reset();
        ValueChanged.InvokeAsync(Value);

        // Notify EditContext if validation is enabled
        if (EditContext != null && ValueExpression != null)
        {
            EditContext.NotifyFieldChanged(_fieldIdentifier);
        }
    }

    private void OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        StateHasChanged();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets the effective maximum search length, enforcing a hard limit of 2000 characters.
    /// </summary>
    private int GetEffectiveMaxSearchLength()
    {
        const int AbsoluteMaxLength = 2000;
        return Math.Min(MaxSearchLength, AbsoluteMaxLength);
    }

    private async Task FilterItemsAsync()
    {
        // Cancel any ongoing load operation
        _loadingCancellationTokenSource?.Cancel();
        _loadingCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _loadingCancellationTokenSource.Token;

        try
        {
            IEnumerable<TItem> itemsToFilter;

            // Use async data source if available
            if (DataSource != null)
            {
                itemsToFilter = await DataSource.SearchAsync(_searchText, cancellationToken);
            }
            else if (Items != null)
            {
                // Use local filtering - unified path for both single and multi-field search
                if (_searchFieldsAccessor != null)
                {
                    // Multi-field search using filter engine
                    itemsToFilter = _filterEngine.FilterMultiField(
                        Items,
                        _searchText,
                        item => _searchFieldsAccessor(item) ?? Array.Empty<string>());
                }
                else
                {
                    // Single-field search using filter engine
                    itemsToFilter = _filterEngine.Filter(Items, _searchText, GetItemText);
                }
            }
            else
            {
                _filteredItems = new List<TItem>();
                _groupedItems = new List<IGrouping<object, TItem>>();
                _keyboardHandler?.UpdateItems(_filteredItems);
                _isLoading = false;
                return;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                // Don't clear loading state - new search is managing it
                return;
            }

            // Apply grouping if needed
            if (HasGrouping && GroupBy != null)
            {
                var groupSelector = GroupBy.Compile();
                _groupedItems = itemsToFilter
                    .GroupBy(item => groupSelector(item))
                    .ToList();
                _filteredItems = new List<TItem>();
            }
            else
            {
                _filteredItems = itemsToFilter.ToList();
                _groupedItems = new List<IGrouping<object, TItem>>();
            }

            // Update keyboard navigation handler
            _keyboardHandler?.UpdateItems(_filteredItems);

            // Clear loading state after filtering completes
            _isLoading = false;
        }
        catch (OperationCanceledException)
        {
            // Expected when search is cancelled - don't clear loading state
            // because a new search is already running and managing the loading state
            return;
        }
        catch (Exception)
        {
            _isLoading = false;
            _filteredItems = new List<TItem>();
            _groupedItems = new List<IGrouping<object, TItem>>();
            _keyboardHandler?.UpdateItems(_filteredItems);
            await InvokeAsync(StateHasChanged);
            throw;
        }
    }

    private string GetItemText(TItem item) =>
        FieldAccessor.GetTextWithToStringFallback(item, _textFieldAccessor);

    private string? GetDescriptionText(TItem item) =>
        FieldAccessor.GetOptionalText(item, _descriptionFieldAccessor);

    private string? GetBadgeText(TItem item) =>
        FieldAccessor.GetOptionalText(item, _badgeFieldAccessor);

    private string? GetIconText(TItem item) =>
        FieldAccessor.GetOptionalText(item, _iconFieldAccessor);

    private string? GetSubtitleText(TItem item) =>
        FieldAccessor.GetOptionalText(item, _subtitleFieldAccessor);

    private bool IsSelected(TItem item)
    {
        if (Value == null || item == null)
        {
            return false;
        }

        return EqualityComparer<TItem>.Default.Equals(Value, item);
    }

    private bool IsKeyboardSelected(TItem item, int index)
    {
        if (_keyboardHandler == null)
        {
            return false;
        }

        return _keyboardHandler.SelectedIndex == index;
    }

    private bool HasValidationErrors()
    {
        if (EditContext == null || ValueExpression == null)
        {
            return false;
        }

        return EditContext.GetValidationMessages(_fieldIdentifier).Any();
    }

    private string GetValidationClass()
    {
        if (HasValidationErrors())
        {
            return "ebd-ac-invalid";
        }
        return string.Empty;
    }

    private string ThemeClass => _themeManager?.GetThemeClass(Theme) ?? "ebd-ac-theme-auto";

    private string BootstrapThemeClass => _themeManager?.GetBootstrapThemeClass(BootstrapTheme) ?? "";

    private string ThemePresetClass => _themeManager?.GetThemePresetClass(ThemePreset) ?? "";

    private string DirectionAttribute => RightToLeft ? "rtl" : "ltr";

    private bool HasGrouping => GroupBy != null;

    #endregion

    #region Theme Management

    /// <summary>
    /// Merges individual theme parameters with ThemeOverrides.
    /// Individual parameters take precedence over ThemeOverrides properties.
    /// Returns null if no overrides are specified.
    /// </summary>
    private ThemeOptions? GetEffectiveThemeOptions()
    {
        // Check if any individual parameters are set
        var hasIndividualParams = PrimaryColor != null ||
                                 BackgroundColor != null ||
                                 TextColor != null ||
                                 BorderColor != null ||
                                 HoverColor != null ||
                                 SelectedColor != null ||
                                 BorderRadius != null ||
                                 FontFamily != null ||
                                 FontSize != null ||
                                 DropdownShadow != null;

        // If no individual params and no ThemeOverrides, return null
        if (!hasIndividualParams && ThemeOverrides == null)
        {
            return null;
        }

        // Start with ThemeOverrides as base (or empty if null)
        var baseOptions = ThemeOverrides ?? new ThemeOptions();

        // Build merged color options if any color params are set
        ColorOptions? mergedColors = null;
        if (PrimaryColor != null || BackgroundColor != null || TextColor != null ||
            BorderColor != null || HoverColor != null || SelectedColor != null ||
            baseOptions.Colors != null)
        {
            mergedColors = new ColorOptions
            {
                Primary = PrimaryColor ?? baseOptions.Colors?.Primary,
                Background = BackgroundColor ?? baseOptions.Colors?.Background,
                Text = TextColor ?? baseOptions.Colors?.Text,
                Border = BorderColor ?? baseOptions.Colors?.Border,
                Hover = HoverColor ?? baseOptions.Colors?.Hover,
                Selected = SelectedColor ?? baseOptions.Colors?.Selected,
                // Preserve other color options from baseOptions
                TextSecondary = baseOptions.Colors?.TextSecondary,
                BorderFocus = baseOptions.Colors?.BorderFocus,
                SelectedText = baseOptions.Colors?.SelectedText,
                Disabled = baseOptions.Colors?.Disabled,
                Error = baseOptions.Colors?.Error,
                Shadow = baseOptions.Colors?.Shadow,
                DropdownBackground = baseOptions.Colors?.DropdownBackground,
                Focus = baseOptions.Colors?.Focus,
                Placeholder = baseOptions.Colors?.Placeholder
            };
        }

        // Build merged spacing options if any spacing params are set
        SpacingOptions? mergedSpacing = null;
        if (BorderRadius != null || baseOptions.Spacing != null)
        {
            mergedSpacing = new SpacingOptions
            {
                BorderRadius = BorderRadius ?? baseOptions.Spacing?.BorderRadius,
                // Preserve other spacing options from baseOptions
                InputPadding = baseOptions.Spacing?.InputPadding,
                ItemPadding = baseOptions.Spacing?.ItemPadding,
                DropdownGap = baseOptions.Spacing?.DropdownGap,
                ItemGap = baseOptions.Spacing?.ItemGap,
                GroupHeaderPadding = baseOptions.Spacing?.GroupHeaderPadding,
                MaxHeight = baseOptions.Spacing?.MaxHeight,
                MinWidth = baseOptions.Spacing?.MinWidth,
                ListPadding = baseOptions.Spacing?.ListPadding,
                IconSize = baseOptions.Spacing?.IconSize
            };
        }

        // Build merged typography options if any typography params are set
        TypographyOptions? mergedTypography = null;
        if (FontFamily != null || FontSize != null || baseOptions.Typography != null)
        {
            mergedTypography = new TypographyOptions
            {
                FontFamily = FontFamily ?? baseOptions.Typography?.FontFamily,
                FontSize = FontSize ?? baseOptions.Typography?.FontSize,
                // Preserve other typography options from baseOptions
                LineHeight = baseOptions.Typography?.LineHeight,
                FontWeight = baseOptions.Typography?.FontWeight,
                DescriptionFontSize = baseOptions.Typography?.DescriptionFontSize,
                BadgeFontSize = baseOptions.Typography?.BadgeFontSize,
                GroupHeaderFontSize = baseOptions.Typography?.GroupHeaderFontSize,
                LetterSpacing = baseOptions.Typography?.LetterSpacing
            };
        }

        // Build merged effect options if any effect params are set
        EffectOptions? mergedEffects = null;
        if (DropdownShadow != null || baseOptions.Effects != null)
        {
            mergedEffects = new EffectOptions
            {
                DropdownShadow = DropdownShadow ?? baseOptions.Effects?.DropdownShadow,
                // Preserve other effect options from baseOptions
                FocusShadow = baseOptions.Effects?.FocusShadow,
                TransitionDuration = baseOptions.Effects?.TransitionDuration,
                BorderWidth = baseOptions.Effects?.BorderWidth,
                TransitionTiming = baseOptions.Effects?.TransitionTiming
            };
        }

        return new ThemeOptions
        {
            Colors = mergedColors,
            Spacing = mergedSpacing,
            Typography = mergedTypography,
            Effects = mergedEffects
        };
    }

    /// <summary>
    /// Build inline CSS variable overrides and theme classes
    /// </summary>
    private void UpdateThemeClassesAndStyles()
    {
        if (_themeManager == null)
        {
            return;
        }

        // Build CSS classes
        var classes = new List<string>();

        // Add size variant class
        classes.Add(_themeManager.GetSizeClass(Size));

        // Add transitions class if enabled
        if (EnableThemeTransitions)
        {
            classes.Add("ebd-ac-theme-transitions");
        }

        _themeClasses = string.Join(" ", classes);

        // Build inline style with custom property overrides
        // Use GetEffectiveThemeOptions() to merge individual parameters with ThemeOverrides
        var effectiveOptions = GetEffectiveThemeOptions();
        _themeStyle = _themeManager.BuildCustomPropertyStyle(effectiveOptions);
    }

    #endregion

    #region IAsyncDisposable

    /// <summary>
    /// Disposes resources used by the component.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _debounceTimer?.Dispose();
        _loadingCancellationTokenSource?.Cancel();
        _loadingCancellationTokenSource?.Dispose();

        if (EditContext != null)
        {
            EditContext.OnValidationStateChanged -= OnValidationStateChanged;
        }

        await Task.CompletedTask;
    }

    #endregion
}
