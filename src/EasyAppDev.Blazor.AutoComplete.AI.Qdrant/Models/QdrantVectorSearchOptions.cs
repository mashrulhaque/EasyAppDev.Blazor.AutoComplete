using EasyAppDev.Blazor.AutoComplete.AI.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.Qdrant.Models;

/// <summary>
/// Configuration options for Qdrant vector search provider.
/// </summary>
public class QdrantVectorSearchOptions
{
    /// <summary>
    /// Qdrant server host. Default: localhost.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Qdrant gRPC port. Default: 6334.
    /// </summary>
    public int Port { get; set; } = 6334;

    /// <summary>
    /// Use HTTPS. Default: false for local, true for cloud.
    /// </summary>
    public bool Https { get; set; }

    /// <summary>
    /// API key for Qdrant Cloud. Optional for self-hosted instances.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Collection name.
    /// </summary>
    public string CollectionName { get; set; } = "";

    /// <summary>
    /// Dimension of the embedding vectors. Default: 1536 (OpenAI text-embedding-3-small).
    /// Must match the embedding model's output dimension.
    /// </summary>
    public int EmbeddingDimensions { get; set; } = 1536;

    /// <summary>
    /// Distance function for similarity search. Default: Cosine.
    /// Qdrant supports: Cosine, Euclidean, DotProduct.
    /// </summary>
    public DistanceFunction DistanceFunction { get; set; } = DistanceFunction.Cosine;

    /// <summary>
    /// Batch size for indexing operations. Default: 100.
    /// </summary>
    public int IndexBatchSize { get; set; } = 100;

    /// <summary>
    /// Whether to create an HNSW index on collection creation.
    /// Default: true. HNSW provides fast approximate nearest neighbor search.
    /// </summary>
    public bool CreateHnswIndex { get; set; } = true;
}
