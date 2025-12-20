using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
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
        _collection = vectorStore.GetCollection<string, CosmosDbVectorRecord>(options.ContainerName);
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

                // Create and upsert records one at a time (CosmosDB batch upsert)
                foreach (var (item, index) in batch.Select((item, index) => (item, index)))
                {
                    var record = CosmosDbVectorRecord.Create(
                        id: _idSelector?.Invoke(item) ?? Guid.NewGuid().ToString(),
                        item: item,
                        text: texts[index],
                        embedding: embeddingsList[index].Vector);

                    await _collection.UpsertAsync(record, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }

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
