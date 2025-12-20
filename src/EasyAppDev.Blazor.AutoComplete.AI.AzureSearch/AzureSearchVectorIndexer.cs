using Azure.Search.Documents;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.Models;
using EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.AzureSearch;

/// <summary>
/// Indexer for adding items to Azure AI Search index.
/// </summary>
/// <typeparam name="TItem">The item type to index.</typeparam>
public class AzureSearchVectorIndexer<TItem> : IVectorIndexer<TItem>
{
    private readonly AzureAISearchCollection<string, AzureSearchVectorRecord> _collection;
    private readonly SearchClient _searchClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly AzureSearchVectorSearchOptions _options;
    private readonly Func<TItem, string> _textSelector;
    private readonly Func<TItem, string>? _titleSelector;
    private readonly Func<TItem, string>? _idSelector;

    /// <summary>
    /// Creates a new Azure AI Search vector indexer.
    /// </summary>
    /// <param name="vectorStore">The Semantic Kernel Azure AI Search vector store.</param>
    /// <param name="searchClient">The Azure Search client for document operations.</param>
    /// <param name="embeddingGenerator">The embedding generator.</param>
    /// <param name="options">Provider configuration options.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="titleSelector">Optional function to extract title from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID from items. If null, uses GUID.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public AzureSearchVectorIndexer(
        AzureAISearchVectorStore vectorStore,
        SearchClient searchClient,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        AzureSearchVectorSearchOptions options,
        Func<TItem, string> textSelector,
        Func<TItem, string>? titleSelector = null,
        Func<TItem, string>? idSelector = null)
    {
        ArgumentNullException.ThrowIfNull(vectorStore);
        ArgumentNullException.ThrowIfNull(searchClient);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(textSelector);

        _searchClient = searchClient;
        _embeddingGenerator = embeddingGenerator;
        _options = options;
        _textSelector = textSelector;
        _titleSelector = titleSelector;
        _idSelector = idSelector;
        _collection = vectorStore.GetCollection<string, AzureSearchVectorRecord>(options.IndexName);
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

                // Create records
                var records = batch.Select((item, index) => AzureSearchVectorRecord.Create(
                    id: _idSelector?.Invoke(item) ?? Guid.NewGuid().ToString(),
                    item: item,
                    content: texts[index],
                    title: _titleSelector?.Invoke(item),
                    embedding: embeddingsList[index].Vector
                )).ToList();

                // Upsert batch using SearchClient for better control
                await _searchClient
                    .MergeOrUploadDocumentsAsync(records, cancellationToken: cancellationToken)
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

        await _searchClient
            .DeleteDocumentsAsync("Id", new[] { itemId }, cancellationToken: cancellationToken)
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
