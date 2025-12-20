using EasyAppDev.Blazor.AutoComplete.AI.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Models;

/// <summary>
/// Configuration options for Azure AI Search vector search provider.
/// </summary>
public class AzureSearchVectorSearchOptions
{
    /// <summary>
    /// Azure AI Search service endpoint.
    /// </summary>
    /// <example>https://my-search-service.search.windows.net</example>
    public string Endpoint { get; set; } = "";

    /// <summary>
    /// Azure AI Search API key.
    /// Use admin key for indexing, query key for search-only.
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// Name of the search index to use.
    /// Will be created if it doesn't exist when using the indexer.
    /// </summary>
    public string IndexName { get; set; } = "";

    /// <summary>
    /// Dimension of the embedding vectors. Default: 1536 (OpenAI text-embedding-3-small).
    /// Must match the embedding model's output dimension.
    /// </summary>
    public int EmbeddingDimensions { get; set; } = 1536;

    /// <summary>
    /// Distance function for similarity search. Default: Cosine.
    /// Azure AI Search supports: Cosine, Euclidean, DotProduct.
    /// </summary>
    public DistanceFunction DistanceFunction { get; set; } = DistanceFunction.Cosine;

    /// <summary>
    /// Enable hybrid search (vector + keyword). Default: true.
    /// When enabled, combines vector similarity with BM25 keyword ranking.
    /// </summary>
    public bool EnableHybridSearch { get; set; } = true;

    /// <summary>
    /// Enable semantic ranking. Default: false.
    /// Requires semantic search capability on your Azure AI Search service (Standard tier or higher).
    /// </summary>
    public bool EnableSemanticRanking { get; set; }

    /// <summary>
    /// Semantic configuration name. Required if EnableSemanticRanking is true.
    /// </summary>
    public string? SemanticConfigurationName { get; set; }

    /// <summary>
    /// Name of the vector field in the index. Default: "Embedding".
    /// Must match the property name in the index schema.
    /// </summary>
    public string VectorFieldName { get; set; } = "Embedding";

    /// <summary>
    /// Name of the content field for keyword search. Default: "Content".
    /// Must match the property name in the index schema.
    /// </summary>
    public string ContentFieldName { get; set; } = "Content";

    /// <summary>
    /// Batch size for indexing operations. Default: 100.
    /// Azure AI Search supports up to 1000 documents per batch.
    /// </summary>
    public int IndexBatchSize { get; set; } = 100;
}
