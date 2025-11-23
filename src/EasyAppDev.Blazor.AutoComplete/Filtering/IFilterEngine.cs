namespace EasyAppDev.Blazor.AutoComplete.Filtering;

/// <summary>
/// Interface for implementing custom filtering strategies.
/// </summary>
/// <typeparam name="TItem">The type of items to filter</typeparam>
public interface IFilterEngine<TItem>
{
    /// <summary>
    /// Filters items based on a single text field.
    /// </summary>
    /// <param name="items">The items to filter</param>
    /// <param name="searchText">The search text</param>
    /// <param name="textSelector">Function to extract text from an item</param>
    /// <returns>Filtered items</returns>
    IEnumerable<TItem> Filter(
        IEnumerable<TItem> items,
        string searchText,
        Func<TItem, string> textSelector);

    /// <summary>
    /// Filters items based on multiple text fields.
    /// Default implementation combines all fields and uses Filter().
    /// Override for optimized multi-field matching.
    /// </summary>
    /// <param name="items">The items to filter</param>
    /// <param name="searchText">The search text</param>
    /// <param name="fieldsSelector">Function to extract multiple text fields from an item</param>
    /// <returns>Filtered items</returns>
    IEnumerable<TItem> FilterMultiField(
        IEnumerable<TItem> items,
        string searchText,
        Func<TItem, string[]> fieldsSelector)
    {
        // Default: combine all fields into single text and use Filter()
        return Filter(items, searchText, item =>
        {
            var fields = fieldsSelector(item);
            return string.Join(" ", fields?.Where(f => !string.IsNullOrWhiteSpace(f)) ?? Array.Empty<string>());
        });
    }
}
