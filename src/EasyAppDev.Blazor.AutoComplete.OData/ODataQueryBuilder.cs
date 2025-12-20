using System.Text;

namespace EasyAppDev.Blazor.AutoComplete.OData;

/// <summary>
/// Builds OData query strings from search parameters.
/// Supports both OData v3 and v4 syntax.
/// </summary>
public class ODataQueryBuilder
{
    private readonly ODataOptions _options;

    /// <summary>
    /// Creates a new OData query builder with the specified options.
    /// </summary>
    /// <param name="options">OData configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public ODataQueryBuilder(ODataOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Builds the complete query URL for a single-field search.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <param name="fieldName">The field name to search in.</param>
    /// <returns>The complete URL with OData query parameters.</returns>
    public string BuildQuery(string searchText, string fieldName)
    {
        return BuildQuery(searchText, new[] { fieldName });
    }

    /// <summary>
    /// Builds the complete query URL for multi-field search.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <param name="fieldNames">The field names to search in (combined with OR).</param>
    /// <returns>The complete URL with OData query parameters.</returns>
    public string BuildQuery(string searchText, string[] fieldNames)
    {
        ArgumentNullException.ThrowIfNull(fieldNames);

        if (fieldNames.Length == 0)
        {
            throw new ArgumentException("At least one field name is required.", nameof(fieldNames));
        }

        var queryParts = new List<string>();

        // Build $filter
        var filter = BuildFilter(searchText, fieldNames);
        if (!string.IsNullOrEmpty(filter))
        {
            queryParts.Add($"$filter={Uri.EscapeDataString(filter)}");
        }

        // Add $top
        if (_options.Top > 0)
        {
            queryParts.Add($"$top={_options.Top}");
        }

        // Add $select
        if (_options.Select?.Length > 0)
        {
            queryParts.Add($"$select={string.Join(",", _options.Select)}");
        }

        // Add $orderby
        if (!string.IsNullOrEmpty(_options.OrderBy))
        {
            queryParts.Add($"$orderby={Uri.EscapeDataString(_options.OrderBy)}");
        }

        // Add $count
        if (_options.IncludeCount)
        {
            queryParts.Add("$count=true");
        }

        var queryString = string.Join("&", queryParts);
        var separator = _options.EndpointUrl.Contains('?') ? "&" : "?";

        return $"{_options.EndpointUrl}{separator}{queryString}";
    }

    /// <summary>
    /// Builds the $filter portion of the query.
    /// </summary>
    private string BuildFilter(string searchText, string[] fieldNames)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            // Return just the additional filter if no search text
            return _options.AdditionalFilter ?? string.Empty;
        }

        // Escape single quotes in search text (OData standard: ' becomes '')
        var escapedSearch = EscapeODataString(searchText);

        // Build filter for each field
        var fieldFilters = fieldNames.Select(field =>
            BuildFieldFilter(field, escapedSearch)).ToList();

        // Combine with OR
        var searchFilter = fieldFilters.Count == 1
            ? fieldFilters[0]
            : $"({string.Join(" or ", fieldFilters)})";

        // Combine with additional static filter
        if (!string.IsNullOrEmpty(_options.AdditionalFilter))
        {
            return $"({searchFilter}) and ({_options.AdditionalFilter})";
        }

        return searchFilter;
    }

    /// <summary>
    /// Builds a filter expression for a single field.
    /// </summary>
    private string BuildFieldFilter(string fieldName, string escapedSearch)
    {
        var field = _options.CaseInsensitive
            ? $"tolower({fieldName})"
            : fieldName;

        var value = _options.CaseInsensitive
            ? escapedSearch.ToLowerInvariant()
            : escapedSearch;

        return _options.FilterStrategy switch
        {
            ODataFilterStrategy.StartsWith => $"startswith({field},'{value}')",
            ODataFilterStrategy.Contains => BuildContainsFilter(field, value),
            ODataFilterStrategy.FuzzyFallback => BuildContainsFilter(field, value),
            _ => $"startswith({field},'{value}')"
        };
    }

    /// <summary>
    /// Builds a contains filter using the appropriate syntax for the OData version.
    /// </summary>
    private string BuildContainsFilter(string field, string value)
    {
        return _options.Version switch
        {
            ODataVersion.V3 => $"substringof('{value}',{field})",
            ODataVersion.V4 => $"contains({field},'{value}')",
            _ => $"contains({field},'{value}')"
        };
    }

    /// <summary>
    /// Escapes special characters in the search text for OData queries.
    /// </summary>
    /// <param name="value">The value to escape.</param>
    /// <returns>The escaped value safe for OData queries.</returns>
    public static string EscapeODataString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        // OData uses doubled single quotes for escaping
        // O'Brien -> O''Brien
        return value.Replace("'", "''");
    }
}
