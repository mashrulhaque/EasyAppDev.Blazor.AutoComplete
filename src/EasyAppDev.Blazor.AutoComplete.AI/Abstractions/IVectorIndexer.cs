using EasyAppDev.Blazor.AutoComplete.AI.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.Abstractions;

/// <summary>
/// Interface for indexing items into vector databases.
/// Indexing is separate from searching - done during startup, background jobs, or CI/CD pipelines.
/// </summary>
/// <typeparam name="TItem">The item type to index.</typeparam>
public interface IVectorIndexer<TItem>
{
    /// <summary>
    /// Indexes a batch of items into the vector store.
    /// </summary>
    /// <param name="items">Items to index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task IndexAsync(
        IEnumerable<TItem> items,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes a single item into the vector store.
    /// </summary>
    /// <param name="item">Item to index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task IndexAsync(
        TItem item,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an item from the vector store by ID.
    /// </summary>
    /// <param name="itemId">ID of the item to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(
        string itemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the collection/index exists in the vector store, creating it if necessary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnsureCollectionExistsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised to report indexing progress during batch operations.
    /// </summary>
    event EventHandler<IndexingProgressEventArgs>? ProgressChanged;
}
