namespace EasyAppDev.Blazor.AutoComplete.AI.Models;

/// <summary>
/// Distance functions supported by vector databases for similarity calculation.
/// </summary>
public enum DistanceFunction
{
    /// <summary>
    /// Cosine similarity - measures angle between vectors.
    /// Range: [-1, 1] or [0, 1] depending on provider normalization.
    /// Higher is more similar.
    /// Supported by: all providers.
    /// </summary>
    Cosine,

    /// <summary>
    /// Euclidean distance (L2 norm) - straight-line distance between vectors.
    /// Range: [0, infinity). Lower is more similar.
    /// Supported by: all providers.
    /// </summary>
    Euclidean,

    /// <summary>
    /// Dot product (inner product) - measures alignment and magnitude.
    /// Range: (-infinity, infinity). Higher is more similar.
    /// Supported by: all providers.
    /// </summary>
    DotProduct,

    /// <summary>
    /// Manhattan distance (L1 norm) - sum of absolute differences.
    /// Range: [0, infinity). Lower is more similar.
    /// Supported by: pgvector only.
    /// </summary>
    Manhattan,

    /// <summary>
    /// Hamming distance - count of differing bit positions.
    /// For binary vectors. Range: [0, dimension]. Lower is more similar.
    /// Supported by: pgvector only (binary vectors).
    /// </summary>
    Hamming,

    /// <summary>
    /// Jaccard distance - measures set dissimilarity.
    /// Range: [0, 1]. Lower is more similar.
    /// Supported by: pgvector only (binary vectors).
    /// </summary>
    Jaccard
}
