using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using VectorDataDistanceFunction = Microsoft.Extensions.VectorData.DistanceFunction;
using VectorDataIndexKind = Microsoft.Extensions.VectorData.IndexKind;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.CosmosDb.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.CosmosDb;

/// <summary>
/// Vector search provider using Azure CosmosDB with DiskANN vector search.
/// Supports: Cosine, Euclidean, DotProduct distance functions.
/// </summary>
/// <typeparam name="TItem">The item type to search.</typeparam>
public class CosmosDbVectorSearchProvider<TItem> : IVectorSearchProvider<TItem>
{
    private readonly CosmosNoSqlCollection<string, CosmosDbVectorRecord> _collection;
    private readonly Container _container;
    private readonly CosmosDbVectorSearchOptions _options;

    /// <summary>
    /// Creates a new CosmosDB vector search provider.
    /// </summary>
    /// <param name="vectorStore">The Semantic Kernel CosmosDB NoSQL vector store.</param>
    /// <param name="container">The CosmosDB container.</param>
    /// <param name="options">Provider configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when vectorStore, container, or options is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options contain invalid values.</exception>
    public CosmosDbVectorSearchProvider(
        CosmosNoSqlVectorStore vectorStore,
        Container container,
        CosmosDbVectorSearchOptions options)
    {
        ArgumentNullException.ThrowIfNull(vectorStore);
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(options);

        ValidateOptions(options);

        _container = container;
        _options = options;

        // Create record definition with runtime-configured dimensions and distance function
        var definition = CreateRecordDefinition(options);
        _collection = vectorStore.GetCollection<string, CosmosDbVectorRecord>(options.ContainerName, definition);
    }

    /// <summary>
    /// Creates a VectorStoreCollectionDefinition with runtime-configured dimensions and distance function.
    /// This overrides the hardcoded values in the CosmosDbVectorRecord attributes.
    /// </summary>
    private static VectorStoreCollectionDefinition CreateRecordDefinition(CosmosDbVectorSearchOptions options)
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
                    DistanceFunction = MapDistanceFunction(options.DistanceFunction),
                    IndexKind = MapVectorIndexType(options.VectorIndexType)
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
            _ => VectorDataDistanceFunction.CosineSimilarity
        };
    }

    /// <summary>
    /// Maps the VectorIndexType option to Semantic Kernel's IndexKind string.
    /// </summary>
    private static string MapVectorIndexType(string vectorIndexType)
    {
        return vectorIndexType?.ToLowerInvariant() switch
        {
            "flat" => VectorDataIndexKind.Flat,
            "quantizedflat" => VectorDataIndexKind.QuantizedFlat,
            "diskann" => VectorDataIndexKind.DiskAnn,
            _ => VectorDataIndexKind.QuantizedFlat
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AI.Models.VectorSearchResult<TItem>>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding,
        AI.Models.VectorSearchOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var searchOptions = new Microsoft.Extensions.VectorData.VectorSearchOptions<CosmosDbVectorRecord>
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
            await _container.ReadContainerAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<long> GetItemCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
            using var iterator = _container.GetItemQueryIterator<long>(query);

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
                return response.FirstOrDefault();
            }

            return 0;
        }
        catch
        {
            return -1;
        }
    }

    private static void ValidateOptions(CosmosDbVectorSearchOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new ArgumentException("ConnectionString is required", nameof(options));

        if (string.IsNullOrWhiteSpace(options.DatabaseName))
            throw new ArgumentException("DatabaseName is required", nameof(options));

        if (string.IsNullOrWhiteSpace(options.ContainerName))
            throw new ArgumentException("ContainerName is required", nameof(options));

        if (options.EmbeddingDimensions <= 0)
            throw new ArgumentException("EmbeddingDimensions must be positive", nameof(options));

        if (options.IndexBatchSize <= 0)
            throw new ArgumentException("IndexBatchSize must be positive", nameof(options));
    }
}
