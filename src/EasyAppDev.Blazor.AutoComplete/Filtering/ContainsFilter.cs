namespace EasyAppDev.Blazor.AutoComplete.Filtering;

/// <summary>
/// Filters items where the text contains the search text (case-insensitive).
/// This provides more flexible matching than StartsWith filtering.
/// </summary>
/// <typeparam name="TItem">The type of items to filter</typeparam>
public class ContainsFilter<TItem> : FilterEngineBase<TItem>
{
    /// <summary>
    /// Filters items where the text contains the search text.
    /// </summary>
    protected override IEnumerable<TItem> FilterCore(
        IEnumerable<TItem> items,
        string searchTextLower,
        Func<TItem, string> textSelector)
    {
        return items.Where(item =>
        {
            var itemText = GetItemTextLower(item, textSelector);
            return itemText?.Contains(searchTextLower) ?? false;
        });
    }

    /// <summary>
    /// Filters items where ANY field contains the search text.
    /// Optimized for multi-field search - checks each field independently.
    /// </summary>
    public IEnumerable<TItem> FilterMultiField(
        IEnumerable<TItem> items,
        string searchText,
        Func<TItem, string[]> fieldsSelector)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return items;
        }

        // Security: Prevent performance degradation from excessively long search strings
        const int MaxSearchLength = 2000;
        if (searchText.Length > MaxSearchLength)
        {
            searchText = searchText.Substring(0, MaxSearchLength);
        }

        var searchLower = searchText.ToLowerInvariant();

        return items.Where(item =>
        {
            var fields = fieldsSelector(item);
            if (fields == null || fields.Length == 0)
            {
                return false;
            }

            // Match if ANY field contains search text
            return fields.Any(field =>
                field?.ToLowerInvariant().Contains(searchLower) ?? false);
        });
    }
}
