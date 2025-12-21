using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using VectorDataDistanceFunction = Microsoft.Extensions.VectorData.DistanceFunction;
using VectorDataIndexKind = Microsoft.Extensions.VectorData.IndexKind;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.Models;
using EasyAppDev.Blazor.AutoComplete.AI.CosmosDb.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.CosmosDb;

/// <summary>
/// Indexer for adding items to Azure CosmosDB vector store.
/// </summary>
/// <typeparam name="TItem">The item type to index.</typeparam>
public class CosmosDbVectorIndexer<TItem> : IVectorIndexer<TItem>
{
    private readonly CosmosNoSqlCollection<string, CosmosDbVectorRecord> _collection;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly CosmosDbVectorSearchOptions _options;
    private readonly Func<TItem, string> _textSelector;
    private readonly Func<TItem, string>? _idSelector;

    /// <summary>
    /// Creates a new CosmosDB vector indexer.
    /// </summary>
    /// <param name="vectorStore">The Semantic Kernel CosmosDB NoSQL vector store.</param>
    /// <param name="embeddingGenerator">The embedding generator.</param>
    /// <param name="options">Provider configuration options.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID from items. If null, uses GUID.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public CosmosDbVectorIndexer(
        CosmosNoSqlVectorStore vectorStore,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        CosmosDbVectorSearchOptions options,
        Func<TItem, string> textSelector,
        Func<TItem, string>? idSelector = null)
    {
        ArgumentNullException.ThrowIfNull(vectorStore);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(textSelector);

        _embeddingGenerator = embeddingGenerator;
        _options = options;
        _textSelector = textSelector;
        _idSelector = idSelector;

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
    public event EventHandler<IndexingProgressEventArgs>? ProgressChanged;

    /// <inheritdoc />
    public async Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
    {
        await _collection.EnsureCollectionExistsAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task IndexAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await IndexAsync([item], cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task IndexAsync(
        IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
            return;

        var totalCount = itemList.Count;
        var processedCount = 0;
        var successfulCount = 0;
        var failedCount = 0;

        // Process in batches
        foreach (var batch in itemList.Chunk(_options.IndexBatchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Extract texts for embedding
                var texts = batch.Select(_textSelector).ToList();

                // Generate embeddings for batch
                var embeddings = await _embeddingGenerator
                    .GenerateAsync(texts, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var embeddingsList = embeddings.ToList();

                // Create records for batch upsert
                var records = batch.Select((item, index) => CosmosDbVectorRecord.Create(
                    id: _idSelector?.Invoke(item) ?? Guid.NewGuid().ToString(),
                    item: item,
                    text: texts[index],
                    embedding: embeddingsList[index].Vector
                )).ToList();

                // Batch upsert records
                await _collection.UpsertAsync(records, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                processedCount += batch.Length;
                successfulCount += batch.Length;
            }
            catch (Exception)
            {
                processedCount += batch.Length;
                failedCount += batch.Length;
                throw;
            }

            // Report progress
            OnProgressChanged(new IndexingProgressEventArgs
            {
                TotalItems = totalCount,
                ProcessedItems = processedCount,
                SuccessfulItems = successfulCount,
                FailedItems = failedCount,
                Message = $"Indexed {processedCount} of {totalCount} items"
            });
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string itemId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);

        await _collection.DeleteAsync(itemId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Raises the ProgressChanged event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    protected virtual void OnProgressChanged(IndexingProgressEventArgs args)
    {
        ProgressChanged?.Invoke(this, args);
    }
}
