using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using VectorDataDistanceFunction = Microsoft.Extensions.VectorData.DistanceFunction;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.AzureSearch;

/// <summary>
/// Vector search provider using Azure AI Search.
/// Supports: Cosine, Euclidean, DotProduct distance functions + Hybrid search + Semantic ranking.
/// </summary>
/// <typeparam name="TItem">The item type to search.</typeparam>
public class AzureSearchVectorSearchProvider<TItem> : IVectorSearchProvider<TItem>
{
    private readonly AzureAISearchCollection<string, AzureSearchVectorRecord> _collection;
    private readonly SearchClient _searchClient;
    private readonly AzureSearchVectorSearchOptions _options;

    /// <summary>
    /// Creates a new Azure AI Search vector search provider.
    /// </summary>
    /// <param name="vectorStore">The Semantic Kernel Azure AI Search vector store.</param>
    /// <param name="searchClient">The Azure Search client for hybrid queries.</param>
    /// <param name="options">Provider configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options contain invalid values.</exception>
    public AzureSearchVectorSearchProvider(
        AzureAISearchVectorStore vectorStore,
        SearchClient searchClient,
        AzureSearchVectorSearchOptions options)
    {
        ArgumentNullException.ThrowIfNull(vectorStore);
        ArgumentNullException.ThrowIfNull(searchClient);
        ArgumentNullException.ThrowIfNull(options);

        ValidateOptions(options);

        _searchClient = searchClient;
        _options = options;

        // Create record definition with runtime-configured dimensions and distance function
        var definition = CreateRecordDefinition(options);
        _collection = vectorStore.GetCollection<string, AzureSearchVectorRecord>(options.IndexName, definition);
    }

    /// <summary>
    /// Creates a VectorStoreCollectionDefinition with runtime-configured dimensions and distance function.
    /// This overrides the hardcoded values in the AzureSearchVectorRecord attributes.
    /// </summary>
    private static VectorStoreCollectionDefinition CreateRecordDefinition(AzureSearchVectorSearchOptions options)
    {
        return new VectorStoreCollectionDefinition
        {
            Properties =
            [
                new VectorStoreKeyProperty("Id", typeof(string)),
                new VectorStoreDataProperty("ItemJson", typeof(string)),
                new VectorStoreDataProperty("Content", typeof(string)),
                new VectorStoreDataProperty("Title", typeof(string)),
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

        // Use hybrid search if enabled and text query is provided
        if (_options.EnableHybridSearch && !string.IsNullOrWhiteSpace(options.TextQuery))
        {
            return await HybridSearchAsync(
                queryEmbedding,
                options.TextQuery,
                options,
                cancellationToken).ConfigureAwait(false);
        }

        // Pure vector search
        return await VectorSearchAsync(
            queryEmbedding,
            options,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<IEnumerable<AI.Models.VectorSearchResult<TItem>>> VectorSearchAsync(
        ReadOnlyMemory<float> queryEmbedding,
        AI.Models.VectorSearchOptions options,
        CancellationToken cancellationToken)
    {
        var searchOptions = new Microsoft.Extensions.VectorData.VectorSearchOptions<AzureSearchVectorRecord>
        {
            IncludeVectors = false
        };

        var results = new List<AI.Models.VectorSearchResult<TItem>>();

        await foreach (var result in _collection
            .SearchAsync(queryEmbedding, top: options.MaxResults, searchOptions, cancellationToken)
            .ConfigureAwait(false))
        {
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

    private async Task<IEnumerable<AI.Models.VectorSearchResult<TItem>>> HybridSearchAsync(
        ReadOnlyMemory<float> queryEmbedding,
        string textQuery,
        AI.Models.VectorSearchOptions options,
        CancellationToken cancellationToken)
    {
        // Use Azure Search SDK directly for hybrid search
        var searchOptions = new SearchOptions
        {
            Size = options.MaxResults,
            VectorSearch = new VectorSearchOptions
            {
                Queries =
                {
                    new VectorizedQuery(queryEmbedding.ToArray())
                    {
                        KNearestNeighborsCount = options.MaxResults,
                        Fields = { _options.VectorFieldName }
                    }
                }
            },
            QueryType = SearchQueryType.Simple,
            SearchMode = SearchMode.Any
        };

        // Add semantic ranking if enabled
        if (_options.EnableSemanticRanking && !string.IsNullOrEmpty(_options.SemanticConfigurationName))
        {
            searchOptions.QueryType = SearchQueryType.Semantic;
            searchOptions.SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = _options.SemanticConfigurationName
            };
        }

        var response = await _searchClient
            .SearchAsync<AzureSearchVectorRecord>(textQuery, searchOptions, cancellationToken)
            .ConfigureAwait(false);

        var results = new List<AI.Models.VectorSearchResult<TItem>>();

        await foreach (var result in response.Value.GetResultsAsync()
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            var score = (float)(result.Score ?? 0);

            if (options.MinScore.HasValue && score < options.MinScore.Value)
                continue;

            var item = result.Document.GetItem<TItem>();
            if (item is null)
                continue;

            results.Add(new AI.Models.VectorSearchResult<TItem>
            {
                Item = item,
                Score = score,
                Id = result.Document.Id
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
    public async Task<long> GetItemCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _searchClient
                .GetDocumentCountAsync(cancellationToken)
                .ConfigureAwait(false);
            return response.Value;
        }
        catch
        {
            return -1;
        }
    }

    private static void ValidateOptions(AzureSearchVectorSearchOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new ArgumentException("Endpoint is required", nameof(options));

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("ApiKey is required", nameof(options));

        if (string.IsNullOrWhiteSpace(options.IndexName))
            throw new ArgumentException("IndexName is required", nameof(options));

        if (options.EmbeddingDimensions <= 0)
            throw new ArgumentException("EmbeddingDimensions must be positive", nameof(options));

        if (options.IndexBatchSize <= 0)
            throw new ArgumentException("IndexBatchSize must be positive", nameof(options));

        if (options.EnableSemanticRanking && string.IsNullOrWhiteSpace(options.SemanticConfigurationName))
            throw new ArgumentException(
                "SemanticConfigurationName is required when EnableSemanticRanking is true",
                nameof(options));
    }
}
