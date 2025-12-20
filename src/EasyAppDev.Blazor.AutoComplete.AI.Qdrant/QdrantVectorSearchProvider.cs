using Microsoft.SemanticKernel.Connectors.Qdrant;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.Qdrant.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.Qdrant;

/// <summary>
/// Vector search provider using Qdrant vector database.
/// Supports: Cosine, Euclidean, DotProduct distance functions.
/// </summary>
/// <typeparam name="TItem">The item type to search.</typeparam>
public class QdrantVectorSearchProvider<TItem> : IVectorSearchProvider<TItem>
{
    private readonly QdrantCollection<Guid, QdrantVectorRecord> _collection;
    private readonly QdrantVectorSearchOptions _options;

    /// <summary>
    /// Creates a new Qdrant vector search provider.
    /// </summary>
    /// <param name="vectorStore">The Semantic Kernel Qdrant vector store.</param>
    /// <param name="options">Provider configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when vectorStore or options is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options contain invalid values.</exception>
    public QdrantVectorSearchProvider(
        QdrantVectorStore vectorStore,
        QdrantVectorSearchOptions options)
    {
        ArgumentNullException.ThrowIfNull(vectorStore);
        ArgumentNullException.ThrowIfNull(options);

        ValidateOptions(options);

        _options = options;
        _collection = vectorStore.GetCollection<Guid, QdrantVectorRecord>(options.CollectionName);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AI.Models.VectorSearchResult<TItem>>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding,
        AI.Models.VectorSearchOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var searchOptions = new Microsoft.Extensions.VectorData.VectorSearchOptions<QdrantVectorRecord>
        {
            IncludeVectors = false
        };

        var results = new List<AI.Models.VectorSearchResult<TItem>>();

        await foreach (var result in _collection
            .SearchAsync(queryEmbedding, top: options.MaxResults, searchOptions, cancellationToken)
            .ConfigureAwait(false))
        {
            // Apply minimum score filter
            if (options.MinScore.HasValue && result.Score < options.MinScore.Value)
                continue;

            var item = result.Record.GetItem<TItem>();
            if (item is null)
                continue;

            results.Add(new AI.Models.VectorSearchResult<TItem>
            {
                Item = item,
                Score = (float)(result.Score ?? 0),
                Id = result.Record.Id.ToString()
            });
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _collection.CollectionExistsAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public Task<long> GetItemCountAsync(CancellationToken cancellationToken = default)
    {
        // Note: Semantic Kernel doesn't expose count directly for Qdrant.
        // Return -1 to indicate "not available".
        return Task.FromResult(-1L);
    }

    private static void ValidateOptions(QdrantVectorSearchOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Host))
            throw new ArgumentException("Host is required", nameof(options));

        if (string.IsNullOrWhiteSpace(options.CollectionName))
            throw new ArgumentException("CollectionName is required", nameof(options));

        if (options.Port <= 0 || options.Port > 65535)
            throw new ArgumentException("Port must be between 1 and 65535", nameof(options));

        if (options.EmbeddingDimensions <= 0)
            throw new ArgumentException("EmbeddingDimensions must be positive", nameof(options));

        if (options.IndexBatchSize <= 0)
            throw new ArgumentException("IndexBatchSize must be positive", nameof(options));
    }
}
