namespace EasyAppDev.Blazor.AutoComplete.Filtering;

/// <summary>
/// Base class for filter engines providing common null checking and case conversion.
/// </summary>
/// <typeparam name="TItem">Type of items to filter</typeparam>
public abstract class FilterEngineBase<TItem> : IFilterEngine<TItem>
{
    /// <summary>
    /// Filters items based on search text using the specified matching strategy.
    /// </summary>
    /// <param name="items">The items to filter</param>
    /// <param name="searchText">The text to search for</param>
    /// <param name="textSelector">Function to extract searchable text from each item</param>
    /// <returns>Filtered collection of items</returns>
    public IEnumerable<TItem> Filter(
        IEnumerable<TItem> items,
        string searchText,
        Func<TItem, string> textSelector)
    {
        // Common null/empty check
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return items;
        }

        // Common case conversion
        var searchLower = searchText.ToLowerInvariant();

        // Delegate to specific matching strategy
        return FilterCore(items, searchLower, textSelector);
    }

    /// <summary>
    /// Implements the specific matching strategy.
    /// Guaranteed that searchText is non-null, non-empty, and lowercase.
    /// </summary>
    /// <param name="items">The items to filter</param>
    /// <param name="searchTextLower">The lowercase search text (non-null, non-empty)</param>
    /// <param name="textSelector">Function to extract searchable text from each item</param>
    /// <returns>Filtered collection of items</returns>
    protected abstract IEnumerable<TItem> FilterCore(
        IEnumerable<TItem> items,
        string searchTextLower,
        Func<TItem, string> textSelector);

    /// <summary>
    /// Helper to get lowercase text from item with null safety.
    /// </summary>
    /// <param name="item">The item to extract text from</param>
    /// <param name="textSelector">Function to extract searchable text from the item</param>
    /// <returns>Lowercase text or null if the text is null</returns>
    protected static string? GetItemTextLower(TItem item, Func<TItem, string> textSelector)
    {
        return textSelector(item)?.ToLowerInvariant();
    }
}
