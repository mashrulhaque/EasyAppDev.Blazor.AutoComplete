using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;
using VectorDataDistanceFunction = Microsoft.Extensions.VectorData.DistanceFunction;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.PostgreSql;

/// <summary>
/// Vector search provider using PostgreSQL with pgvector extension.
/// Supports: Cosine, Euclidean (L2), DotProduct, Manhattan (L1) distance functions.
/// </summary>
/// <typeparam name="TItem">The item type to search.</typeparam>
public class PostgresVectorSearchProvider<TItem> : IVectorSearchProvider<TItem>
{
    private readonly PostgresCollection<string, VectorRecord> _collection;
    private readonly PostgresVectorSearchOptions _options;

    /// <summary>
    /// Creates a new PostgreSQL vector search provider.
    /// </summary>
    /// <param name="vectorStore">The Semantic Kernel PostgreSQL vector store.</param>
    /// <param name="options">Provider configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when vectorStore or options is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options contain invalid values.</exception>
    public PostgresVectorSearchProvider(
        PostgresVectorStore vectorStore,
        PostgresVectorSearchOptions options)
    {
        ArgumentNullException.ThrowIfNull(vectorStore);
        ArgumentNullException.ThrowIfNull(options);

        ValidateOptions(options);

        _options = options;

        // Create record definition with runtime-configured dimensions and distance function
        var definition = CreateRecordDefinition(options);
        _collection = vectorStore.GetCollection<string, VectorRecord>(options.CollectionName, definition);
    }

    /// <summary>
    /// Creates a VectorStoreCollectionDefinition with runtime-configured dimensions and distance function.
    /// This overrides the hardcoded values in the VectorRecord attributes.
    /// </summary>
    private static VectorStoreCollectionDefinition CreateRecordDefinition(PostgresVectorSearchOptions options)
    {
        return new VectorStoreCollectionDefinition
        {
            Properties =
            [
                new VectorStoreKeyProperty("Id", typeof(string)),
                new VectorStoreDataProperty("ItemJson", typeof(string)),
                new VectorStoreDataProperty("Text", typeof(string)),
                new VectorStoreVectorProperty("Embedding", typeof(ReadOnlyMemory<float>), options.EmbeddingDimensions)
                {
                    DistanceFunction = MapDistanceFunction(options.DistanceFunction)
                }
            ]
        };
    }

    /// <summary>
    /// Maps the library's DistanceFunction enum to Semantic Kernel's DistanceFunction string.
    /// </summary>
    private static string MapDistanceFunction(AI.Models.DistanceFunction distanceFunction)
    {
        return distanceFunction switch
        {
            AI.Models.DistanceFunction.Cosine => VectorDataDistanceFunction.CosineSimilarity,
            AI.Models.DistanceFunction.Euclidean => VectorDataDistanceFunction.EuclideanDistance,
            AI.Models.DistanceFunction.DotProduct => VectorDataDistanceFunction.DotProductSimilarity,
            AI.Models.DistanceFunction.Manhattan => VectorDataDistanceFunction.ManhattanDistance,
            _ => VectorDataDistanceFunction.CosineSimilarity
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AI.Models.VectorSearchResult<TItem>>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding,
        AI.Models.VectorSearchOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var searchOptions = new Microsoft.Extensions.VectorData.VectorSearchOptions<VectorRecord>
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
                Id = result.Record.Id
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
        // Note: Semantic Kernel doesn't expose count directly.
        // Return -1 to indicate "not available".
        return Task.FromResult(-1L);
    }

    private static void ValidateOptions(PostgresVectorSearchOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new ArgumentException("ConnectionString is required", nameof(options));

        if (string.IsNullOrWhiteSpace(options.CollectionName))
            throw new ArgumentException("CollectionName is required", nameof(options));

        if (options.EmbeddingDimensions <= 0)
            throw new ArgumentException("EmbeddingDimensions must be positive", nameof(options));

        if (options.IndexBatchSize <= 0)
            throw new ArgumentException("IndexBatchSize must be positive", nameof(options));
    }
}
