namespace EasyAppDev.Blazor.AutoComplete.AI.Models;

/// <summary>
/// Result from a vector similarity search.
/// </summary>
/// <typeparam name="TItem">The item type.</typeparam>
public record VectorSearchResult<TItem>
{
    /// <summary>
    /// The matched item from the vector store.
    /// </summary>
    public required TItem Item { get; init; }

    /// <summary>
    /// Similarity or distance score.
    /// Interpretation depends on the distance function used:
    /// - Cosine, DotProduct: Higher is more similar.
    /// - Euclidean, Manhattan, Hamming, Jaccard: Lower is more similar.
    /// </summary>
    public required float Score { get; init; }

    /// <summary>
    /// Optional item identifier from the vector store.
    /// </summary>
    public string? Id { get; init; }
}
