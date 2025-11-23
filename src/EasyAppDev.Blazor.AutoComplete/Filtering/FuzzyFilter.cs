namespace EasyAppDev.Blazor.AutoComplete.Filtering;

/// <summary>
/// Filters items using fuzzy matching based on Levenshtein distance.
/// This provides typo-tolerant search capabilities.
/// </summary>
/// <typeparam name="TItem">The type of items to filter</typeparam>
public class FuzzyFilter<TItem> : IFilterEngine<TItem>
{
    private readonly int _maxDistance;

    /// <summary>
    /// Initializes a new instance of the <see cref="FuzzyFilter{TItem}"/> class.
    /// </summary>
    /// <param name="maxDistance">Maximum Levenshtein distance to consider a match. Default is 2.</param>
    public FuzzyFilter(int maxDistance = 2)
    {
        _maxDistance = maxDistance;
    }

    /// <summary>
    /// Filters the items using fuzzy matching.
    /// </summary>
    /// <param name="items">The items to filter</param>
    /// <param name="searchText">The text to search for</param>
    /// <param name="textSelector">Function to extract searchable text from each item</param>
    /// <returns>Filtered and sorted collection of items by edit distance</returns>
    public IEnumerable<TItem> Filter(
        IEnumerable<TItem> items,
        string searchText,
        Func<TItem, string> textSelector)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return items;
        }

        var searchLower = searchText.ToLowerInvariant();

        return items
            .Select(item => new
            {
                Item = item,
                Text = textSelector(item),
                Distance = CalculateBestFuzzyMatch(searchLower, textSelector(item) ?? string.Empty)
            })
            .Where(x => x.Distance <= _maxDistance)
            .OrderBy(x => x.Distance)
            .Select(x => x.Item);
    }

    /// <summary>
    /// Filters items using fuzzy matching across multiple fields.
    /// Finds the minimum edit distance across all fields for each item.
    /// </summary>
    /// <param name="items">The items to filter</param>
    /// <param name="searchText">The text to search for</param>
    /// <param name="fieldsSelector">Function to extract multiple text fields from an item</param>
    /// <returns>Filtered and sorted collection of items by minimum edit distance</returns>
    public IEnumerable<TItem> FilterMultiField(
        IEnumerable<TItem> items,
        string searchText,
        Func<TItem, string[]> fieldsSelector)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return items;
        }

        var searchLower = searchText.ToLowerInvariant();

        return items
            .Select(item =>
            {
                var fields = fieldsSelector(item) ?? Array.Empty<string>();

                // Calculate minimum distance across all fields
                var minDistance = int.MaxValue;
                foreach (var field in fields.Where(f => !string.IsNullOrWhiteSpace(f)))
                {
                    var distance = CalculateBestFuzzyMatch(searchLower, field);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }

                return new
                {
                    Item = item,
                    MinDistance = minDistance
                };
            })
            .Where(x => x.MinDistance <= _maxDistance)
            .OrderBy(x => x.MinDistance)
            .Select(x => x.Item);
    }

    /// <summary>
    /// Calculates the best fuzzy match by checking the search term against the whole text
    /// and individual words within the text.
    /// </summary>
    /// <param name="searchText">The search text (already lowercased)</param>
    /// <param name="targetText">The target text to search in</param>
    /// <returns>The minimum Levenshtein distance found</returns>
    private int CalculateBestFuzzyMatch(string searchText, string targetText)
    {
        if (string.IsNullOrEmpty(targetText))
        {
            return int.MaxValue;
        }

        var targetLower = targetText.ToLowerInvariant();

        // Check distance against the whole text
        var wholeDistance = LevenshteinDistance(searchText, targetLower);

        // Also check distance against each word in the text
        var words = targetLower.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        var minWordDistance = words.Length > 0
            ? words.Min(word => LevenshteinDistance(searchText, word))
            : int.MaxValue;

        // Return the minimum distance found
        return Math.Min(wholeDistance, minWordDistance);
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two strings.
    /// </summary>
    /// <param name="source">The source string</param>
    /// <param name="target">The target string</param>
    /// <returns>The edit distance between the strings</returns>
    private static int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
        {
            return target?.Length ?? 0;
        }

        if (string.IsNullOrEmpty(target))
        {
            return source.Length;
        }

        var sourceLength = source.Length;
        var targetLength = target.Length;
        var distance = new int[sourceLength + 1, targetLength + 1];

        for (var i = 0; i <= sourceLength; distance[i, 0] = i++) { }
        for (var j = 0; j <= targetLength; distance[0, j] = j++) { }

        for (var i = 1; i <= sourceLength; i++)
        {
            for (var j = 1; j <= targetLength; j++)
            {
                var cost = target[j - 1] == source[i - 1] ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[sourceLength, targetLength];
    }
}
