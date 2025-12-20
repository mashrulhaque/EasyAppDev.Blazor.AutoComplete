namespace EasyAppDev.Blazor.AutoComplete.AI.Models;

/// <summary>
/// Event arguments for reporting indexing progress during batch operations.
/// </summary>
public class IndexingProgressEventArgs : EventArgs
{
    /// <summary>
    /// Total number of items to index.
    /// </summary>
    public int TotalItems { get; init; }

    /// <summary>
    /// Number of items processed so far.
    /// </summary>
    public int ProcessedItems { get; init; }

    /// <summary>
    /// Number of items successfully indexed.
    /// </summary>
    public int SuccessfulItems { get; init; }

    /// <summary>
    /// Number of items that failed to index.
    /// </summary>
    public int FailedItems { get; init; }

    /// <summary>
    /// Current progress percentage (0-100).
    /// </summary>
    public double ProgressPercentage => TotalItems > 0 ? (double)ProcessedItems / TotalItems * 100 : 0;

    /// <summary>
    /// Indicates whether the indexing operation is complete.
    /// </summary>
    public bool IsComplete => ProcessedItems >= TotalItems;

    /// <summary>
    /// Optional message describing the current operation.
    /// </summary>
    public string? Message { get; init; }
}
