namespace EasyAppDev.Blazor.AutoComplete.AI.Models;

/// <summary>
/// Configuration options for <see cref="DataSources.VectorSearchDataSource{TItem}"/>.
/// </summary>
public class VectorSearchDataSourceOptions
{
    /// <summary>
    /// Maximum number of results to return from search. Default: 20.
    /// </summary>
    public int MaxResults { get; set; } = 20;

    /// <summary>
    /// Minimum similarity score threshold. Default: null (no threshold).
    /// For similarity metrics (Cosine, DotProduct), results below this are excluded.
    /// For distance metrics (Euclidean, etc.), results above this are excluded.
    /// </summary>
    public float? MinSimilarityScore { get; set; }

    /// <summary>
    /// Distance function for similarity calculation. Default: Cosine.
    /// </summary>
    public DistanceFunction DistanceFunction { get; set; } = DistanceFunction.Cosine;

    /// <summary>
    /// Enable hybrid vector + keyword search. Default: false.
    /// Only supported by Azure AI Search and CosmosDB providers.
    /// </summary>
    public bool EnableHybridSearch { get; set; }

    /// <summary>
    /// Query embedding cache duration. Default: 15 minutes.
    /// Caching reduces API costs during rapid typing.
    /// </summary>
    public TimeSpan QueryCacheDuration { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Maximum number of query embeddings to cache. Default: 1000.
    /// </summary>
    public int MaxQueryCacheSize { get; set; } = 1000;
}
