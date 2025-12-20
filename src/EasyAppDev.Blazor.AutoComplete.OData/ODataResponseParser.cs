using System.Text.Json;

namespace EasyAppDev.Blazor.AutoComplete.OData;

/// <summary>
/// Parses OData JSON responses into typed collections.
/// </summary>
/// <typeparam name="TItem">The type of items in the response.</typeparam>
public class ODataResponseParser<TItem> where TItem : class
{
    private readonly ODataOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a new OData response parser with the specified options.
    /// </summary>
    /// <param name="options">OData configuration options.</param>
    /// <param name="jsonOptions">Optional JSON serializer options. Defaults to case-insensitive property matching.</param>
    public ODataResponseParser(ODataOptions options, JsonSerializerOptions? jsonOptions = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Parses an OData JSON response string.
    /// </summary>
    /// <param name="json">The JSON response from the OData endpoint.</param>
    /// <returns>Parsed OData response with items and optional count.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public ODataResponse<TItem> Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ODataResponse<TItem>();
        }

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var result = new ODataResponse<TItem>();

        // Handle direct array response first (some endpoints return array directly)
        if (root.ValueKind == JsonValueKind.Array)
        {
            result.Items = DeserializeItems(json);
            return result;
        }

        // Parse count if present (OData v4 uses @odata.count)
        if (root.TryGetProperty("@odata.count", out var countElement))
        {
            result.TotalCount = countElement.GetInt32();
        }
        // Some endpoints use odata.count without @
        else if (root.TryGetProperty("odata.count", out var altCountElement))
        {
            result.TotalCount = altCountElement.GetInt32();
        }

        // Parse items from value array (standard OData response)
        if (root.TryGetProperty(_options.ResultsPropertyName, out var valueElement)
            && valueElement.ValueKind == JsonValueKind.Array)
        {
            result.Items = DeserializeItems(valueElement.GetRawText());
        }

        return result;
    }

    /// <summary>
    /// Deserializes a JSON array string into a list of items.
    /// </summary>
    private List<TItem> DeserializeItems(string jsonArray)
    {
        try
        {
            return JsonSerializer.Deserialize<List<TItem>>(jsonArray, _jsonOptions)
                ?? new List<TItem>();
        }
        catch (JsonException)
        {
            // Return empty list if deserialization fails
            return new List<TItem>();
        }
    }
}

/// <summary>
/// Represents a parsed OData response.
/// </summary>
/// <typeparam name="TItem">The type of items in the response.</typeparam>
public class ODataResponse<TItem>
{
    /// <summary>
    /// The items returned by the OData query.
    /// </summary>
    public List<TItem> Items { get; set; } = new();

    /// <summary>
    /// The total count of matching items (only present if $count=true was requested).
    /// </summary>
    public int? TotalCount { get; set; }
}
