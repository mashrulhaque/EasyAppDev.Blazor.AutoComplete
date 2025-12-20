using System.Net.Http.Json;
using System.Text.Json;
using EasyAppDev.Blazor.AutoComplete.DataSources;
using Microsoft.Extensions.Logging;

namespace EasyAppDev.Blazor.AutoComplete.OData;

/// <summary>
/// OData-based data source for AutoComplete component.
/// Translates search text to OData $filter queries and fetches results from an OData endpoint.
/// </summary>
/// <typeparam name="TItem">The type of items to retrieve.</typeparam>
public class ODataDataSource<TItem> : IAutoCompleteDataSource<TItem> where TItem : class
{
    private readonly HttpClient _httpClient;
    private readonly ODataOptions _options;
    private readonly ODataQueryBuilder _queryBuilder;
    private readonly ODataResponseParser<TItem> _responseParser;
    private readonly Func<TItem, string>? _textSelector;
    private readonly string[] _searchFieldNames;
    private readonly ILogger? _logger;

    private string? _lastError;

    /// <summary>
    /// Gets the last error message that occurred during search, if any.
    /// </summary>
    public string? LastError => _lastError;

    /// <summary>
    /// Event raised when an error occurs during search.
    /// </summary>
    public event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// Creates an OData data source for single-field search.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making requests.</param>
    /// <param name="options">OData configuration options.</param>
    /// <param name="searchFieldName">The OData property name to search in.</param>
    /// <param name="textSelector">Optional text selector for fuzzy client-side re-ranking.</param>
    /// <param name="logger">Optional logger for debugging.</param>
    public ODataDataSource(
        HttpClient httpClient,
        ODataOptions options,
        string searchFieldName,
        Func<TItem, string>? textSelector = null,
        ILogger? logger = null)
        : this(httpClient, options, new[] { searchFieldName }, textSelector, logger)
    {
    }

    /// <summary>
    /// Creates an OData data source for multi-field search.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making requests.</param>
    /// <param name="options">OData configuration options.</param>
    /// <param name="searchFieldNames">The OData property names to search in (combined with OR).</param>
    /// <param name="textSelector">Optional text selector for fuzzy client-side re-ranking.</param>
    /// <param name="logger">Optional logger for debugging.</param>
    public ODataDataSource(
        HttpClient httpClient,
        ODataOptions options,
        string[] searchFieldNames,
        Func<TItem, string>? textSelector = null,
        ILogger? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _searchFieldNames = searchFieldNames ?? throw new ArgumentNullException(nameof(searchFieldNames));
        _textSelector = textSelector;
        _logger = logger;

        if (searchFieldNames.Length == 0)
        {
            throw new ArgumentException("At least one search field name is required.", nameof(searchFieldNames));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(options.EndpointUrl, nameof(options.EndpointUrl));

        _queryBuilder = new ODataQueryBuilder(options);
        _responseParser = new ODataResponseParser<TItem>(options);

        // Configure HttpClient timeout if not already set
        if (_httpClient.Timeout == TimeSpan.Zero || _httpClient.Timeout == Timeout.InfiniteTimeSpan)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TItem>> SearchAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        // Clear previous error
        _lastError = null;

        // Return empty for null/whitespace search
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return Enumerable.Empty<TItem>();
        }

        // Check minimum search length
        if (searchText.Length < _options.MinSearchLength)
        {
            _logger?.LogDebug(
                "Search text '{SearchText}' is shorter than minimum length {MinLength}",
                searchText,
                _options.MinSearchLength);
            return Enumerable.Empty<TItem>();
        }

        try
        {
            // Build query URL
            var url = _queryBuilder.BuildQuery(searchText, _searchFieldNames);

            _logger?.LogDebug("OData query: {Url}", url);

            // Create request with custom headers
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (_options.CustomHeaders != null)
            {
                foreach (var (key, value) in _options.CustomHeaders)
                {
                    request.Headers.TryAddWithoutValidation(key, value);
                }
            }

            // Execute request
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _lastError = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
                _logger?.LogError(
                    "OData request failed with status {StatusCode}: {Content}",
                    response.StatusCode,
                    errorContent);
                ErrorOccurred?.Invoke(this, _lastError);
                return Enumerable.Empty<TItem>();
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            // Parse response
            var odataResponse = _responseParser.Parse(json);

            _logger?.LogDebug(
                "OData returned {Count} items (total: {Total})",
                odataResponse.Items.Count,
                odataResponse.TotalCount);

            // Apply client-side fuzzy re-ranking if needed
            if (_options.FilterStrategy == ODataFilterStrategy.FuzzyFallback &&
                _textSelector != null)
            {
                return ApplyFuzzyRanking(odataResponse.Items, searchText);
            }

            return odataResponse.Items;
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation (e.g., debounce, user typed again)
            _logger?.LogDebug("OData search cancelled for '{SearchText}'", searchText);
            throw;
        }
        catch (HttpRequestException ex)
        {
            _lastError = $"Network error: {ex.Message}";
            _logger?.LogError(ex, "OData network request failed");
            ErrorOccurred?.Invoke(this, _lastError);
            return Enumerable.Empty<TItem>();
        }
        catch (JsonException ex)
        {
            _lastError = $"Failed to parse response: {ex.Message}";
            _logger?.LogError(ex, "OData response parsing failed");
            ErrorOccurred?.Invoke(this, _lastError);
            return Enumerable.Empty<TItem>();
        }
        catch (Exception ex)
        {
            _lastError = $"Unexpected error: {ex.Message}";
            _logger?.LogError(ex, "OData search failed unexpectedly");
            ErrorOccurred?.Invoke(this, _lastError);
            return Enumerable.Empty<TItem>();
        }
    }

    /// <summary>
    /// Applies fuzzy ranking to results for FuzzyFallback strategy.
    /// Uses a simple scoring algorithm based on string matching.
    /// </summary>
    private IEnumerable<TItem> ApplyFuzzyRanking(List<TItem> items, string searchText)
    {
        if (_textSelector == null || items.Count == 0)
        {
            return items;
        }

        var searchLower = searchText.ToLowerInvariant();

        return items
            .Select(item => new
            {
                Item = item,
                Text = _textSelector(item)?.ToLowerInvariant() ?? string.Empty
            })
            .Select(x => new
            {
                x.Item,
                Score = CalculateFuzzyScore(x.Text, searchLower)
            })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Item);
    }

    /// <summary>
    /// Calculates a simple fuzzy score for client-side ranking.
    /// Higher scores indicate better matches.
    /// </summary>
    private static float CalculateFuzzyScore(string text, string search)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(search))
        {
            return 0f;
        }

        // Exact match
        if (text == search)
        {
            return 1.0f;
        }

        // Starts with (prefix match)
        if (text.StartsWith(search, StringComparison.Ordinal))
        {
            return 0.9f;
        }

        // Contains as substring
        if (text.Contains(search, StringComparison.Ordinal))
        {
            return 0.7f;
        }

        // Word boundary match (search appears after a space)
        if (text.Contains($" {search}", StringComparison.Ordinal))
        {
            return 0.8f;
        }

        // Character matching score (for typo tolerance)
        var matchCount = search.Count(c => text.Contains(c));
        var matchRatio = (float)matchCount / search.Length;

        // Bonus for sequential character matches
        var sequentialBonus = CalculateSequentialMatchBonus(text, search);

        return (matchRatio * 0.4f) + (sequentialBonus * 0.2f);
    }

    /// <summary>
    /// Calculates a bonus score for sequential character matches.
    /// </summary>
    private static float CalculateSequentialMatchBonus(string text, string search)
    {
        var maxSequential = 0;
        var currentSequential = 0;
        var textIndex = 0;

        foreach (var c in search)
        {
            var foundIndex = text.IndexOf(c, textIndex);
            if (foundIndex >= 0 && foundIndex == textIndex)
            {
                currentSequential++;
                textIndex = foundIndex + 1;
            }
            else if (foundIndex >= 0)
            {
                maxSequential = Math.Max(maxSequential, currentSequential);
                currentSequential = 1;
                textIndex = foundIndex + 1;
            }
            else
            {
                maxSequential = Math.Max(maxSequential, currentSequential);
                currentSequential = 0;
            }
        }

        maxSequential = Math.Max(maxSequential, currentSequential);
        return search.Length > 0 ? (float)maxSequential / search.Length : 0f;
    }
}
