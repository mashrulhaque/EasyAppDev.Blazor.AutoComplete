namespace EasyAppDev.Blazor.AutoComplete.OData;

/// <summary>
/// Configuration options for OData data source.
/// </summary>
public class ODataOptions
{
    /// <summary>
    /// The OData endpoint URL (e.g., "https://api.example.com/odata/Products").
    /// </summary>
    public string EndpointUrl { get; set; } = string.Empty;

    /// <summary>
    /// The OData version to use for query syntax. Default is V4.
    /// </summary>
    public ODataVersion Version { get; set; } = ODataVersion.V4;

    /// <summary>
    /// Maximum number of items to return ($top). Default is 100.
    /// </summary>
    public int Top { get; set; } = 100;

    /// <summary>
    /// Fields to select ($select). If empty, all fields are returned.
    /// </summary>
    public string[]? Select { get; set; }

    /// <summary>
    /// Fields to order by ($orderby). If empty, no ordering is applied.
    /// Example: "Name" or "Name desc"
    /// </summary>
    public string? OrderBy { get; set; }

    /// <summary>
    /// Additional static filter to always apply (ANDed with search filter).
    /// Example: "IsActive eq true"
    /// </summary>
    public string? AdditionalFilter { get; set; }

    /// <summary>
    /// Filter strategy to use when generating $filter queries.
    /// </summary>
    public ODataFilterStrategy FilterStrategy { get; set; } = ODataFilterStrategy.StartsWith;

    /// <summary>
    /// Whether to use case-insensitive filtering (wraps fields with tolower()).
    /// Default is true.
    /// </summary>
    public bool CaseInsensitive { get; set; } = true;

    /// <summary>
    /// HTTP request timeout in seconds. Default is 30.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Minimum search length before making API calls. Default is 1.
    /// Searches shorter than this will return empty results without making a request.
    /// </summary>
    public int MinSearchLength { get; set; } = 1;

    /// <summary>
    /// Custom HTTP headers to include in requests (e.g., Authorization).
    /// </summary>
    public Dictionary<string, string>? CustomHeaders { get; set; }

    /// <summary>
    /// JSON property name that contains the results array. Default is "value" (OData standard).
    /// Some endpoints return a direct array without a wrapper property.
    /// </summary>
    public string ResultsPropertyName { get; set; } = "value";

    /// <summary>
    /// Whether to include $count in the response for total count of matching items.
    /// </summary>
    public bool IncludeCount { get; set; } = false;
}

/// <summary>
/// OData protocol version for query syntax.
/// </summary>
public enum ODataVersion
{
    /// <summary>
    /// OData v3 - Uses substringof(needle, haystack) for contains.
    /// </summary>
    V3,

    /// <summary>
    /// OData v4 - Uses contains(haystack, needle) for contains. Default.
    /// </summary>
    V4
}

/// <summary>
/// Filter strategies supported for OData queries.
/// </summary>
public enum ODataFilterStrategy
{
    /// <summary>
    /// startswith(field, 'value') - Fastest, exact prefix match.
    /// Works the same in OData v3 and v4.
    /// </summary>
    StartsWith,

    /// <summary>
    /// contains(field, 'value') in v4, substringof('value', field) in v3 - Substring match anywhere.
    /// </summary>
    Contains,

    /// <summary>
    /// Fuzzy matching is NOT supported by OData natively.
    /// Uses Contains for the server query, then applies client-side Levenshtein distance re-ranking.
    /// </summary>
    FuzzyFallback
}
