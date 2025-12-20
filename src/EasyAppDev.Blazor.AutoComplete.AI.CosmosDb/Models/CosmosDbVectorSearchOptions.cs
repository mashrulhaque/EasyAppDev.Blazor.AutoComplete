using EasyAppDev.Blazor.AutoComplete.AI.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.CosmosDb.Models;

/// <summary>
/// Configuration options for Azure CosmosDB vector search provider.
/// </summary>
public class CosmosDbVectorSearchOptions
{
    /// <summary>
    /// CosmosDB connection string.
    /// Example: AccountEndpoint=https://myaccount.documents.azure.com:443/;AccountKey=...
    /// </summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>
    /// Database name.
    /// </summary>
    public string DatabaseName { get; set; } = "";

    /// <summary>
    /// Container name.
    /// </summary>
    public string ContainerName { get; set; } = "";

    /// <summary>
    /// Dimension of the embedding vectors. Default: 1536 (OpenAI text-embedding-3-small).
    /// Must match the embedding model's output dimension.
    /// </summary>
    public int EmbeddingDimensions { get; set; } = 1536;

    /// <summary>
    /// Distance function for similarity search. Default: Cosine.
    /// CosmosDB supports: Cosine, Euclidean, DotProduct.
    /// </summary>
    public DistanceFunction DistanceFunction { get; set; } = DistanceFunction.Cosine;

    /// <summary>
    /// Batch size for indexing operations. Default: 100.
    /// </summary>
    public int IndexBatchSize { get; set; } = 100;

    /// <summary>
    /// Partition key path. Default: /id.
    /// </summary>
    public string PartitionKeyPath { get; set; } = "/id";

    /// <summary>
    /// Vector indexing policy type. Default: quantizedFlat.
    /// Options: flat, quantizedFlat, diskANN.
    /// - flat: Exact search, suitable for small datasets.
    /// - quantizedFlat: Compressed vectors for memory efficiency.
    /// - diskANN: High-performance approximate search using DiskANN algorithm.
    /// </summary>
    public string VectorIndexType { get; set; } = "quantizedFlat";
}
