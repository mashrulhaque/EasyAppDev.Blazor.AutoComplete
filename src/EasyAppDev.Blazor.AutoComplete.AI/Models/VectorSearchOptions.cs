namespace EasyAppDev.Blazor.AutoComplete.AI.Models;

/// <summary>
/// Options for vector similarity search operations.
/// </summary>
public record VectorSearchOptions
{
    /// <summary>
    /// Maximum number of results to return. Default: 20.
    /// </summary>
    public int MaxResults { get; init; } = 20;

    /// <summary>
    /// Minimum similarity score threshold. Default: null (no threshold).
    /// For similarity metrics (Cosine, DotProduct), results below this are excluded.
    /// For distance metrics (Euclidean, etc.), results above this are excluded.
    /// </summary>
    public float? MinScore { get; init; }

    /// <summary>
    /// Distance function for similarity calculation. Default: Cosine.
    /// </summary>
    public DistanceFunction DistanceFunction { get; init; } = DistanceFunction.Cosine;

    /// <summary>
    /// Enable hybrid vector + keyword search. Default: false.
    /// Only supported by Azure AI Search and CosmosDB providers.
    /// </summary>
    public bool EnableHybridSearch { get; init; }

    /// <summary>
    /// Text query for hybrid search (when EnableHybridSearch is true).
    /// This is the original text before embedding, used for keyword matching.
    /// </summary>
    public string? TextQuery { get; init; }
}
