namespace EasyAppDev.Blazor.AutoComplete;

/// <summary>
/// Centralized constants used throughout the AutoComplete component library.
/// Provides a single source of truth for magic numbers, timeouts, and CSS class names.
/// </summary>
public static class AutoCompleteConstants
{
    #region Security Limits

    /// <summary>
    /// Absolute maximum search length to prevent memory exhaustion attacks.
    /// </summary>
    public const int AbsoluteMaxSearchLength = 2000;

    /// <summary>
    /// Default maximum search length for user input.
    /// </summary>
    public const int DefaultMaxSearchLength = 500;

    /// <summary>
    /// Maximum allowed distance for fuzzy matching (Levenshtein distance).
    /// </summary>
    public const int MaxFuzzyDistance = 2;

    /// <summary>
    /// Maximum character length per dimension for Levenshtein calculation (~4MB memory limit).
    /// </summary>
    public const int MaxLevenshteinDimensionLength = 1000;

    #endregion

    #region Timing

    /// <summary>
    /// Default debounce interval in milliseconds for the core component.
    /// </summary>
    public const int DefaultDebounceMs = 300;

    /// <summary>
    /// Default debounce interval in milliseconds for AI semantic search.
    /// </summary>
    public const int DefaultAIDebounceMs = 500;

    /// <summary>
    /// Delay before closing dropdown on focus out (allows click events to register).
    /// </summary>
    public const int FocusOutDelayMs = 200;

    /// <summary>
    /// Timeout for CSS regex pattern matching to prevent ReDoS attacks.
    /// </summary>
    public const int CssRegexTimeoutMs = 100;

    #endregion

    #region Display Defaults

    /// <summary>
    /// Default minimum number of characters before search triggers.
    /// </summary>
    public const int DefaultMinSearchLength = 1;

    /// <summary>
    /// Default maximum number of items to display in the dropdown.
    /// </summary>
    public const int DefaultMaxDisplayedItems = 100;

    /// <summary>
    /// Default item height in pixels for virtualization.
    /// </summary>
    public const float DefaultItemHeight = 40f;

    /// <summary>
    /// Default virtualization threshold (minimum items before virtualization enables).
    /// </summary>
    public const int DefaultVirtualizationThreshold = 100;

    #endregion

    #region AI/Caching Defaults

    /// <summary>
    /// Default similarity threshold for semantic search (0.0 to 1.0).
    /// </summary>
    public const float DefaultSimilarityThreshold = 0.15f;

    /// <summary>
    /// Default TTL for item embedding cache in minutes.
    /// </summary>
    public const int DefaultItemCacheTtlMinutes = 60;

    /// <summary>
    /// Default TTL for query embedding cache in minutes.
    /// </summary>
    public const int DefaultQueryCacheTtlMinutes = 15;

    /// <summary>
    /// Default maximum items in embedding cache.
    /// </summary>
    public const int DefaultMaxCachedItems = 10000;

    /// <summary>
    /// Default maximum queries in embedding cache.
    /// </summary>
    public const int DefaultMaxCachedQueries = 1000;

    /// <summary>
    /// Default memory pressure threshold for cache eviction (0.0 to 1.0).
    /// </summary>
    public const float DefaultMemoryPressureThreshold = 0.85f;

    /// <summary>
    /// Cache cleanup interval in minutes.
    /// </summary>
    public const int CacheCleanupIntervalMinutes = 5;

    #endregion

    #region CSS Class Prefixes

    /// <summary>
    /// Base CSS class prefix for all AutoComplete components.
    /// </summary>
    public const string CssPrefix = "ebd-ac";

    /// <summary>
    /// CSS class for the container element.
    /// </summary>
    public const string CssContainer = "ebd-ac-container";

    /// <summary>
    /// CSS class for the input wrapper element.
    /// </summary>
    public const string CssInputWrapper = "ebd-ac-input-wrapper";

    /// <summary>
    /// CSS class for the dropdown element.
    /// </summary>
    public const string CssDropdown = "ebd-ac-dropdown";

    /// <summary>
    /// CSS class for the listbox element.
    /// </summary>
    public const string CssListbox = "ebd-ac-listbox";

    /// <summary>
    /// CSS class for individual list items.
    /// </summary>
    public const string CssItem = "ebd-ac-item";

    /// <summary>
    /// CSS class for selected items.
    /// </summary>
    public const string CssSelected = "ebd-ac-selected";

    /// <summary>
    /// CSS class for keyboard-selected items.
    /// </summary>
    public const string CssKeyboardSelected = "ebd-ac-keyboard-selected";

    /// <summary>
    /// CSS class for invalid state (validation errors).
    /// </summary>
    public const string CssInvalid = "ebd-ac-invalid";

    /// <summary>
    /// CSS class for loading state.
    /// </summary>
    public const string CssLoading = "ebd-ac-loading";

    /// <summary>
    /// CSS class for disabled state.
    /// </summary>
    public const string CssDisabled = "ebd-ac-disabled";

    /// <summary>
    /// CSS class prefix for theme variants.
    /// </summary>
    public const string CssThemePrefix = "ebd-ac-theme";

    /// <summary>
    /// CSS class prefix for Bootstrap theme variants.
    /// </summary>
    public const string CssBootstrapThemePrefix = "ebd-ac-bs";

    /// <summary>
    /// CSS class prefix for size variants.
    /// </summary>
    public const string CssSizePrefix = "ebd-ac-size";

    /// <summary>
    /// CSS class for theme transitions.
    /// </summary>
    public const string CssThemeTransitions = "ebd-ac-theme-transitions";

    #endregion

    #region CSS Custom Property Prefixes

    /// <summary>
    /// Prefix for all CSS custom properties.
    /// </summary>
    public const string CssVarPrefix = "--ebd-ac";

    /// <summary>
    /// CSS custom property for primary color.
    /// </summary>
    public const string CssVarPrimary = "--ebd-ac-primary";

    /// <summary>
    /// CSS custom property for background color.
    /// </summary>
    public const string CssVarBackground = "--ebd-ac-bg";

    /// <summary>
    /// CSS custom property for text color.
    /// </summary>
    public const string CssVarText = "--ebd-ac-text";

    /// <summary>
    /// CSS custom property for border color.
    /// </summary>
    public const string CssVarBorder = "--ebd-ac-border";

    #endregion

    #region Default Styles

    /// <summary>
    /// Default CSS class for badge styling (Bootstrap 5).
    /// </summary>
    public const string DefaultBadgeClass = "badge bg-primary";

    #endregion
}
