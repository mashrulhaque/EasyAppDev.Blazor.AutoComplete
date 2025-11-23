using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using EasyAppDev.Blazor.AutoComplete.Options;

namespace EasyAppDev.Blazor.AutoComplete.AI;

/// <summary>
/// A high-performance semantic search autocomplete component that uses AI embeddings
/// for intelligent search. Handles all dependency injection, initialization, error handling,
/// and disposal automatically.
/// </summary>
/// <typeparam name="TItem">The type of items to search</typeparam>
public partial class SemanticAutoComplete<TItem> : ComponentBase, IDisposable where TItem : notnull
{
    #region Dependency Injection

    [Inject] private IEmbeddingGenerator<string, Embedding<float>>? EmbeddingGenerator { get; set; }
    [Inject] private ILogger<SemanticAutoComplete<TItem>>? Logger { get; set; }

    #endregion

    #region Parameters - Data

    /// <summary>
    /// The collection of items to search through.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>
    /// Expression to extract the text field to display for each item.
    /// If not specified and SearchFields is provided, uses the first search field.
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, string>>? TextField { get; set; }

    /// <summary>
    /// Expression to extract multiple fields for semantic search.
    /// The fields will be concatenated for embedding generation.
    /// Example: @(doc => new[] { doc.Title, doc.Description, doc.Tags })
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, string[]>>? SearchFields { get; set; }

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

    #endregion

    #region Parameters - Display

    /// <summary>
    /// Placeholder text for the input field.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Custom template for rendering items in the dropdown.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Custom template for rendering when no results are found.
    /// </summary>
    [Parameter]
    public RenderFragment? NoResultsTemplate { get; set; }

    /// <summary>
    /// Custom template for rendering while loading results.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    /// <summary>
    /// Custom template for the dropdown header.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Custom template for the dropdown footer.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// The theme to apply to the component.
    /// </summary>
    [Parameter]
    public Theme Theme { get; set; } = Theme.Auto;

    #endregion

    #region Parameters - Behavior

    /// <summary>
    /// Minimum number of characters required before searching.
    /// Default is 3 (optimized for semantic search to reduce API calls).
    /// </summary>
    [Parameter]
    public int MinSearchLength { get; set; } = 3;

    /// <summary>
    /// Debounce delay in milliseconds before searching.
    /// Default is 500ms (optimized for semantic search to reduce API calls).
    /// </summary>
    [Parameter]
    public int DebounceMs { get; set; } = 500;

    /// <summary>
    /// Minimum cosine similarity score (0-1) for results.
    /// Default is 0.15 (tuned for text-embedding-3-small).
    /// Lower values return more results but may be less relevant.
    /// </summary>
    [Parameter]
    public float SimilarityThreshold { get; set; } = 0.15f;

    /// <summary>
    /// Maximum number of results to return from semantic search.
    /// If null, returns all matching results.
    /// </summary>
    [Parameter]
    public int? MaxResults { get; set; }

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
    /// Maximum number of items to display in the dropdown.
    /// </summary>
    [Parameter]
    public int MaxDisplayedItems { get; set; } = 100;

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
    /// ARIA label for accessibility.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    #endregion

    #region Parameters - Cache Configuration

    /// <summary>
    /// Duration to cache item embeddings. Default is 1 hour.
    /// Longer durations reduce API costs but use more memory.
    /// </summary>
    [Parameter]
    public TimeSpan ItemCacheDuration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Duration to cache query embeddings. Default is 15 minutes.
    /// </summary>
    [Parameter]
    public TimeSpan QueryCacheDuration { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Maximum number of item embeddings to cache. Default is 10,000.
    /// Use -1 for unlimited.
    /// </summary>
    [Parameter]
    public int MaxItemCacheSize { get; set; } = 10_000;

    /// <summary>
    /// Maximum number of query embeddings to cache. Default is 1,000.
    /// Use -1 for unlimited.
    /// </summary>
    [Parameter]
    public int MaxQueryCacheSize { get; set; } = 1_000;

    /// <summary>
    /// If true, generates embeddings for all items at initialization.
    /// This improves first search performance but increases startup time.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool PreWarmCache { get; set; } = false;

    #endregion

    #region Parameters - UI Customization

    /// <summary>
    /// Whether to show cache status below the autocomplete.
    /// Displays cached item count and any errors.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowCacheStatus { get; set; } = true;

    /// <summary>
    /// Whether to show a warning message if embedding generator is not configured.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowConfigurationWarning { get; set; } = true;

    #endregion

    #region Private Fields

    private SemanticSearchDataSource<TItem>? _dataSource;
    private System.Threading.Timer? _cacheRefreshTimer;

    #endregion

    #region Lifecycle Methods

    protected override void OnInitialized()
    {
        if (EmbeddingGenerator == null)
        {
            Logger?.LogWarning(
                "EmbeddingGenerator is null - semantic search will not be available. " +
                "Add services.AddAutoCompleteSemanticSearch(configuration) to Program.cs");
            return;
        }

        if (Items == null)
        {
            Logger?.LogWarning("Items collection is null - semantic search cannot be initialized");
            return;
        }

        try
        {
            var textSelector = BuildTextSelector();

            _dataSource = new SemanticSearchDataSource<TItem>(
                items: Items,
                textSelector: textSelector,
                embeddingGenerator: EmbeddingGenerator,
                similarityThreshold: SimilarityThreshold,
                maxResults: MaxResults,
                itemCacheDuration: ItemCacheDuration,
                queryCacheDuration: QueryCacheDuration,
                maxItemCacheSize: MaxItemCacheSize,
                maxQueryCacheSize: MaxQueryCacheSize,
                preWarmCache: PreWarmCache,
                hybridOptions: null, // Use defaults
                logger: Logger);

            // Auto-wire error handling to trigger UI updates
            _dataSource.ErrorOccurred += (sender, errorMessage) =>
            {
                InvokeAsync(StateHasChanged);
            };

            Logger?.LogInformation(
                "SemanticAutoComplete initialized with {Count} items, threshold={Threshold}, minSearchLength={MinLength}, debounce={Debounce}ms",
                Items.Count(),
                SimilarityThreshold,
                MinSearchLength,
                DebounceMs);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to initialize SemanticAutoComplete");
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && _dataSource != null && ShowCacheStatus)
        {
            // Auto-setup cache refresh timer to update cache count display
            _cacheRefreshTimer = new System.Threading.Timer(
                _ => InvokeAsync(StateHasChanged),
                null,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(2));
        }
    }

    public void Dispose()
    {
        _cacheRefreshTimer?.Dispose();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Builds the text selector function from either SearchFields or TextField parameter.
    /// </summary>
    private Func<TItem, string> BuildTextSelector()
    {
        if (SearchFields != null)
        {
            var compiled = SearchFields.Compile();
            return item =>
            {
                var fields = compiled(item);
                return string.Join(" ", fields.Where(f => !string.IsNullOrWhiteSpace(f)));
            };
        }
        else if (TextField != null)
        {
            return TextField.Compile();
        }
        else
        {
            throw new InvalidOperationException(
                "Either TextField or SearchFields must be specified for SemanticAutoComplete. " +
                "Example: SearchFields=\"@(doc => new[] { doc.Title, doc.Description })\" " +
                "or TextField=\"@(doc => doc.Title)\"");
        }
    }

    /// <summary>
    /// Handles value changes from the inner AutoComplete component.
    /// </summary>
    private async Task OnValueChanged(TItem? newValue)
    {
        Value = newValue;
        await ValueChanged.InvokeAsync(newValue);
    }

    #endregion
}
