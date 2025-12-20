using EasyAppDev.Blazor.AutoComplete.AI.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.Abstractions;

/// <summary>
/// Abstraction for vector similarity search.
/// Implementations delegate to external vector databases (PostgreSQL/pgvector, Azure AI Search, Pinecone, Qdrant, etc.).
/// </summary>
/// <typeparam name="TItem">The item type to search.</typeparam>
public interface IVectorSearchProvider<TItem>
{
    /// <summary>
    /// Searches for items similar to the query embedding.
    /// </summary>
    /// <param name="queryEmbedding">The vectorized query. Must match the dimension of indexed embeddings.</param>
    /// <param name="options">Search configuration including max results, minimum score, and distance function.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ranked search results ordered by relevance score (descending for similarity metrics).</returns>
    Task<IEnumerable<VectorSearchResult<TItem>>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding,
        VectorSearchOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the vector store is accessible.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the vector store is reachable and the collection exists.</returns>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of indexed items in the collection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of items currently indexed.</returns>
    Task<long> GetItemCountAsync(CancellationToken cancellationToken = default);
}
