using Microsoft.Extensions.AI;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.Models;
using EasyAppDev.Blazor.AutoComplete.DataSources;

namespace EasyAppDev.Blazor.AutoComplete.AI.DataSources;

/// <summary>
/// Data source that delegates vector search to external providers.
/// Only the query is embedded; items are pre-indexed in the vector database.
/// </summary>
/// <typeparam name="TItem">The item type to search.</typeparam>
public class VectorSearchDataSource<TItem> : IAutoCompleteDataSource<TItem>, IDisposable
    where TItem : notnull
{
    private readonly IVectorSearchProvider<TItem> _provider;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly EmbeddingCache<string> _queryCache;
    private readonly VectorSearchDataSourceOptions _options;
    private bool _disposed;

    /// <summary>
    /// Creates a new VectorSearchDataSource.
    /// </summary>
    /// <param name="provider">The vector search provider.</param>
    /// <param name="embeddingGenerator">The embedding generator for query vectorization.</param>
    /// <param name="options">Configuration options.</param>
    public VectorSearchDataSource(
        IVectorSearchProvider<TItem> provider,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        VectorSearchDataSourceOptions? options = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        _options = options ?? new VectorSearchDataSourceOptions();

        _queryCache = new EmbeddingCache<string>(
            _options.QueryCacheDuration,
            _options.MaxQueryCacheSize);
    }

    /// <summary>
    /// Query cache hit rate for monitoring (0.0 to 1.0).
    /// </summary>
    public double QueryCacheHitRate => _queryCache.HitRate;

    /// <summary>
    /// Number of cached query embeddings.
    /// </summary>
    public int CachedQueryCount => _queryCache.Count;

    /// <summary>
    /// Total number of cache hits.
    /// </summary>
    public long CacheHits => _queryCache.Hits;

    /// <summary>
    /// Total number of cache misses.
    /// </summary>
    public long CacheMisses => _queryCache.Misses;

    /// <inheritdoc />
    public async Task<IEnumerable<TItem>> SearchAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (string.IsNullOrWhiteSpace(searchText))
            return Enumerable.Empty<TItem>();

        // 1. Normalize query
        var normalizedQuery = searchText.Trim().ToLowerInvariant();

        // 2. Get or generate query embedding (cached)
        var queryEmbedding = await GetOrGenerateEmbeddingAsync(
            normalizedQuery,
            cancellationToken).ConfigureAwait(false);

        // 3. Build search options
        var searchOptions = new VectorSearchOptions
        {
            MaxResults = _options.MaxResults,
            MinScore = _options.MinSimilarityScore,
            DistanceFunction = _options.DistanceFunction,
            EnableHybridSearch = _options.EnableHybridSearch,
            TextQuery = _options.EnableHybridSearch ? searchText : null
        };

        // 4. Delegate search to provider
        var results = await _provider.SearchAsync(
            queryEmbedding.Vector,
            searchOptions,
            cancellationToken).ConfigureAwait(false);

        // 5. Return items
        return results.Select(r => r.Item);
    }

    /// <summary>
    /// Checks if the underlying vector store is available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the vector store is reachable.</returns>
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _provider.IsAvailableAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the count of indexed items.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of items indexed.</returns>
    public Task<long> GetItemCountAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _provider.GetItemCountAsync(cancellationToken);
    }

    /// <summary>
    /// Clears the query cache.
    /// </summary>
    public Task ClearCacheAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _queryCache.ClearAsync();
    }

    /// <summary>
    /// Removes expired entries from the query cache.
    /// </summary>
    public Task CleanupCacheAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _queryCache.CleanupExpiredAsync();
    }

    private async Task<Embedding<float>> GetOrGenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken)
    {
        return await _queryCache.GetOrCreateAsync(
            text,
            async () =>
            {
                var embeddings = await _embeddingGenerator.GenerateAsync(
                    new[] { text },
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                return embeddings.FirstOrDefault()
                    ?? throw new InvalidOperationException("Failed to generate embedding for query");
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _queryCache.Dispose();
            }
            _disposed = true;
        }
    }
}
