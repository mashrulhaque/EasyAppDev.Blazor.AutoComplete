using EasyAppDev.Blazor.AutoComplete.AI.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.Pinecone.Models;

/// <summary>
/// Configuration options for Pinecone vector search provider.
/// </summary>
public class PineconeVectorSearchOptions
{
    /// <summary>
    /// Pinecone API key.
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// Pinecone index name.
    /// </summary>
    public string IndexName { get; set; } = "";

    /// <summary>
    /// Optional namespace for logical partitioning within the index.
    /// Default: null (uses default namespace).
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Dimension of the embedding vectors. Default: 1536 (OpenAI text-embedding-3-small).
    /// Must match the embedding model's output dimension.
    /// </summary>
    public int EmbeddingDimensions { get; set; } = 1536;

    /// <summary>
    /// Distance function for similarity search. Default: Cosine.
    /// Pinecone supports: Cosine, Euclidean (squared), DotProduct.
    /// </summary>
    public DistanceFunction DistanceFunction { get; set; } = DistanceFunction.Cosine;

    /// <summary>
    /// Batch size for indexing operations. Default: 100.
    /// Pinecone recommends 100-200 vectors per batch.
    /// </summary>
    public int IndexBatchSize { get; set; } = 100;
}
