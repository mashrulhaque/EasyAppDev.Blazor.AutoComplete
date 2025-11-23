using EasyAppDev.Blazor.AutoComplete.DataSources;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Numerics.Tensors;

namespace EasyAppDev.Blazor.AutoComplete.AI;

/// <summary>
/// Provides high-performance semantic search functionality for AutoComplete using AI embeddings.
/// Uses SIMD-accelerated cosine similarity and intelligent caching for optimal performance.
/// </summary>
/// <typeparam name="TItem">The type of items to search.</typeparam>
public class SemanticSearchDataSource<TItem> : IAutoCompleteDataSource<TItem> where TItem : notnull
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IEnumerable<TItem> _items;
    private readonly Func<TItem, string> _textSelector;
    private readonly EmbeddingCache<TItem> _itemCache;
    private readonly EmbeddingCache<string> _queryCache;
    private readonly float _minSimilarityScore;
    private readonly int? _maxResults;
    private readonly ILogger? _logger;
    private readonly HybridSearchOptions _hybridOptions;
    private string? _lastError;
    private readonly Dictionary<TItem, float> _lastSearchScores = new();
    private readonly SemaphoreSlim _cleanupLock = new(1, 1);
    private DateTime _lastCleanup = DateTime.UtcNow;

    /// <summary>
    /// Gets the last error message that occurred during search, if any.
    /// </summary>
    public string? LastError => _lastError;

    /// <summary>
    /// Event raised when an error occurs during search.
    /// </summary>
    public event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticSearchDataSource{TItem}"/> class.
    /// </summary>
    /// <param name="items">The collection of items to search through.</param>
    /// <param name="textSelector">Function to extract text from an item for embedding generation.</param>
    /// <param name="embeddingGenerator">The embedding generator to use for creating embeddings.</param>
    /// <param name="similarityThreshold">Minimum cosine similarity score (0-1) for results. Default is 0.5 (tuned for text-embedding-3-small).</param>
    /// <param name="maxResults">Maximum number of results to return. If null, returns all matching results.</param>
    /// <param name="itemCacheDuration">Duration to cache item embeddings. Default is 1 hour.</param>
    /// <param name="queryCacheDuration">Duration to cache query embeddings. Default is 15 minutes.</param>
    /// <param name="maxItemCacheSize">Maximum number of item embeddings to cache. Default is 10,000. Use -1 for unlimited.</param>
    /// <param name="maxQueryCacheSize">Maximum number of query embeddings to cache. Default is 1,000. Use -1 for unlimited.</param>
    /// <param name="preWarmCache">If true, generates embeddings for all items at initialization. Default is false.</param>
    /// <param name="hybridOptions">Options for hybrid search (combining semantic + text matching). If null, uses defaults.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    public SemanticSearchDataSource(
        IEnumerable<TItem> items,
        Func<TItem, string> textSelector,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        float similarityThreshold = 0.5f,
        int? maxResults = null,
        TimeSpan? itemCacheDuration = null,
        TimeSpan? queryCacheDuration = null,
        int maxItemCacheSize = 10_000,
        int maxQueryCacheSize = 1_000,
        bool preWarmCache = false,
        HybridSearchOptions? hybridOptions = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);
        ArgumentNullException.ThrowIfNull(textSelector);

        if (similarityThreshold is < 0f or > 1f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(similarityThreshold),
                "Similarity score must be between 0 and 1.");
        }

        if (maxResults is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxResults),
                "Max results must be greater than 0.");
        }

        _items = items;
        _embeddingGenerator = embeddingGenerator;
        _textSelector = textSelector;
        _minSimilarityScore = similarityThreshold;
        _maxResults = maxResults;
        _logger = logger;
        _hybridOptions = hybridOptions ?? new HybridSearchOptions();

        // Initialize caches with LRU eviction
        _itemCache = new EmbeddingCache<TItem>(
            itemCacheDuration ?? TimeSpan.FromHours(1),
            maxItemCacheSize);

        _queryCache = new EmbeddingCache<string>(
            queryCacheDuration ?? TimeSpan.FromMinutes(15),
            maxQueryCacheSize);

        if (preWarmCache)
        {
            _ = PreWarmCacheAsync(progress: null);
        }
    }

    /// <summary>
    /// Initializes the data source by pre-generating embeddings for all items in parallel batches.
    /// </summary>
    /// <param name="progress">Optional progress reporter (reports percentage 0-100).</param>
    /// <param name="batchSize">Number of items to process in each parallel batch. Default is 50.</param>
    /// <param name="maxParallelism">Maximum number of concurrent embedding requests. Default is 4 (respects API rate limits).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the initialization operation.</returns>
    public async Task InitializeAsync(
        IProgress<double>? progress = null,
        int batchSize = 50,
        int maxParallelism = 4,
        CancellationToken cancellationToken = default)
    {
        await PreWarmCacheAsync(progress, batchSize, maxParallelism, cancellationToken);
    }

    /// <summary>
    /// Gets the similarity score for an item from the last search.
    /// </summary>
    /// <param name="item">The item to get the score for.</param>
    /// <returns>The similarity score (0-1) or 0 if the item was not in the last search results.</returns>
    public float GetSimilarityScore(TItem item)
    {
        return _lastSearchScores.TryGetValue(item, out var score) ? score : 0f;
    }

    /// <summary>
    /// Pre-generates embeddings for all items to improve first search performance.
    /// Uses parallel batching for optimal throughput while respecting API rate limits.
    /// </summary>
    private async Task PreWarmCacheAsync(
        IProgress<double>? progress = null,
        int batchSize = 50,
        int maxParallelism = 4,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var itemsList = _items.ToList();
            var totalItems = itemsList.Count;
            _logger?.LogInformation("Pre-warming embedding cache for {Count} items with parallelism={Parallelism}", totalItems, maxParallelism);

            var processedCount = 0;
            var lockObj = new object();

            // Process in parallel batches
            var batches = itemsList
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.item).ToList());

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxParallelism,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(batches, options, async (batch, ct) =>
            {
                foreach (var item in batch)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        await _itemCache.GetOrCreateAsync(
                            item,
                            async () =>
                            {
                                var result = await _embeddingGenerator.GenerateAsync(
                                    [_textSelector(item)],
                                    cancellationToken: ct);
                                return result[0];
                            },
                            ct);

                        // Update progress atomically
                        lock (lockObj)
                        {
                            processedCount++;
                            if (progress != null && totalItems > 0)
                            {
                                var percentage = (double)processedCount / totalItems * 100.0;
                                progress.Report(percentage);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to pre-warm cache for item");
                    }
                }
            });

            _logger?.LogInformation(
                "Cache pre-warming completed. Cached {Count} items. Hit rate: {HitRate:P2}",
                _itemCache.Count,
                _itemCache.HitRate);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during cache pre-warming");
        }
    }

    /// <summary>
    /// Searches for items semantically similar to the search text.
    /// Uses SIMD-accelerated cosine similarity and intelligent caching for optimal performance.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Items ordered by semantic similarity (highest first).</returns>
    public async Task<IEnumerable<TItem>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            _lastError = null;
            return Enumerable.Empty<TItem>();
        }

        try
        {
            // Periodic cache cleanup (every 5 minutes)
            await PerformPeriodicCleanupAsync();

            // Generate embedding for search query (with caching for repeated searches)
            Embedding<float> queryEmbedding;
            try
            {
                queryEmbedding = await _queryCache.GetOrCreateAsync(
                    searchText,
                    async () =>
                    {
                        var result = await _embeddingGenerator.GenerateAsync(
                            [searchText],
                            cancellationToken: cancellationToken);
                        return result[0];
                    },
                    cancellationToken);

                _lastError = null; // Clear any previous errors on success
            }
            catch (OperationCanceledException)
            {
                // Cancellation is expected during rapid typing (debounce) - silently return empty results
                _logger?.LogDebug("Search for '{SearchText}' was canceled (likely due to debounce)", searchText);
                return Enumerable.Empty<TItem>();
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                _lastError = $"Failed to generate embedding: {errorMessage}";
                _logger?.LogError(ex, "Failed to generate embedding for search text: {SearchText}", searchText);
                ErrorOccurred?.Invoke(this, _lastError);
                return Enumerable.Empty<TItem>();
            }

            var results = new List<(TItem Item, float Similarity)>();

            // Compute similarities for all items
            foreach (var item in _items)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    // Get or generate embedding for item
                    var itemEmbedding = await _itemCache.GetOrCreateAsync(
                        item,
                        async () =>
                        {
                            try
                            {
                                var result = await _embeddingGenerator.GenerateAsync(
                                    [_textSelector(item)],
                                    cancellationToken: cancellationToken);
                                return result[0];
                            }
                            catch (OperationCanceledException)
                            {
                                // Propagate cancellation to stop processing
                                throw;
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogWarning(ex, "Failed to generate embedding for item");
                                throw;
                            }
                        },
                        cancellationToken);

                    // Calculate cosine similarity using SIMD-accelerated TensorPrimitives
                    // This is 3-5x faster than manual loop and uses ReadOnlySpan to avoid allocations
                    // Note: We get the span fresh each iteration to avoid storing it across await boundaries
                    var similarity = TensorPrimitives.CosineSimilarity(queryEmbedding.Vector.Span, itemEmbedding.Vector.Span);

                    if (similarity >= _minSimilarityScore)
                    {
                        results.Add((item, similarity));
                    }
                }
                catch (OperationCanceledException)
                {
                    // Search was canceled - stop processing and return partial results
                    _logger?.LogDebug("Item processing canceled at item iteration");
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "Skipping item due to embedding generation error");
                    // Skip items that fail embedding generation
                    continue;
                }
            }

            _logger?.LogDebug(
                "Semantic search returned {Count} results for query: {SearchText}. Query cache hit rate: {QueryHitRate:P2}, Item cache hit rate: {ItemHitRate:P2}",
                results.Count,
                searchText,
                _queryCache.HitRate,
                _itemCache.HitRate);

            // Hybrid search: If semantic search returns few results and query matches criteria,
            // also include traditional text matches (for partial words, acronyms, etc.)
            if (_hybridOptions.Enabled &&
                results.Count < _hybridOptions.MinResultsThreshold &&
                searchText.Length <= _hybridOptions.MaxQueryLengthForTextMatching)
            {
                var searchLower = searchText.ToLowerInvariant();
                var textMatches = _items
                    .Where(item => !results.Any(r => EqualityComparer<TItem>.Default.Equals(r.Item, item)))
                    .Select(item => new
                    {
                        Item = item,
                        Text = _textSelector(item).ToLowerInvariant()
                    })
                    .Where(x => x.Text.Contains(searchLower))
                    .Select(x => (x.Item, Similarity: _hybridOptions.TextMatchScore))
                    .ToList();

                if (textMatches.Any())
                {
                    _logger?.LogDebug(
                        "Hybrid search: Added {Count} text-based matches for query: {SearchText}",
                        textMatches.Count,
                        searchText);
                    results.AddRange(textMatches);
                }
            }

            // Order by similarity (highest first) and apply max results limit
            var orderedResults = results.OrderByDescending(x => x.Similarity);

            var finalResults = _maxResults.HasValue
                ? orderedResults.Take(_maxResults.Value).ToList()
                : orderedResults.ToList();

            // Store scores for GetSimilarityScore() method
            _lastSearchScores.Clear();
            foreach (var (item, similarity) in finalResults)
            {
                _lastSearchScores[item] = similarity;
            }

            return finalResults.Select(x => x.Item);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error during semantic search");
            _lastSearchScores.Clear();
            return Enumerable.Empty<TItem>();
        }
    }

    /// <summary>
    /// Performs periodic cache cleanup to remove expired entries.
    /// Uses throttling to prevent excessive cleanup calls.
    /// </summary>
    private async Task PerformPeriodicCleanupAsync()
    {
        var timeSinceLastCleanup = DateTime.UtcNow - _lastCleanup;
        if (timeSinceLastCleanup < TimeSpan.FromMinutes(5))
        {
            return;
        }

        // Try to acquire cleanup lock without blocking
        if (!await _cleanupLock.WaitAsync(0))
        {
            return; // Another thread is already cleaning up
        }

        try
        {
            await _itemCache.CleanupExpiredAsync();
            await _queryCache.CleanupExpiredAsync();
            _lastCleanup = DateTime.UtcNow;

            _logger?.LogDebug(
                "Cache cleanup completed. Item cache: {ItemCount} items, Query cache: {QueryCount} queries",
                _itemCache.Count,
                _queryCache.Count);
        }
        finally
        {
            _cleanupLock.Release();
        }
    }

    /// <summary>
    /// Clears both item and query embedding caches.
    /// </summary>
    public async Task ClearCacheAsync()
    {
        await _itemCache.ClearAsync();
        await _queryCache.ClearAsync();
    }

    /// <summary>
    /// Cleans up expired embeddings from both caches.
    /// </summary>
    public async Task CleanupExpiredCacheAsync()
    {
        await _itemCache.CleanupExpiredAsync();
        await _queryCache.CleanupExpiredAsync();
    }

    /// <summary>
    /// Gets the number of cached item embeddings.
    /// </summary>
    public int CachedItemCount => _itemCache.Count;

    /// <summary>
    /// Gets the number of cached query embeddings.
    /// </summary>
    public int CachedQueryCount => _queryCache.Count;

    /// <summary>
    /// Gets the item cache hit rate (0.0 to 1.0).
    /// </summary>
    public double ItemCacheHitRate => _itemCache.HitRate;

    /// <summary>
    /// Gets the query cache hit rate (0.0 to 1.0).
    /// </summary>
    public double QueryCacheHitRate => _queryCache.HitRate;

    /// <summary>
    /// Resets cache statistics for both item and query caches.
    /// </summary>
    public void ResetCacheStatistics()
    {
        _itemCache.ResetStatistics();
        _queryCache.ResetStatistics();
    }
}

/// <summary>
/// Options for configuring hybrid search (combining semantic and traditional text matching).
/// </summary>
public class HybridSearchOptions
{
    /// <summary>
    /// Gets or sets whether hybrid search is enabled. Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum number of semantic results before triggering text matching fallback. Default is 3.
    /// </summary>
    public int MinResultsThreshold { get; set; } = 3;

    /// <summary>
    /// Gets or sets the maximum query length (in characters) for text matching fallback. Default is 10.
    /// Short queries benefit more from text matching (e.g., "mobi" for "mobile").
    /// </summary>
    public int MaxQueryLengthForTextMatching { get; set; } = 10;

    /// <summary>
    /// Gets or sets the similarity score assigned to text-based matches. Default is 0.4 (lower than typical semantic matches).
    /// Must be between 0 and 1.
    /// </summary>
    public float TextMatchScore { get; set; } = 0.4f;
}
