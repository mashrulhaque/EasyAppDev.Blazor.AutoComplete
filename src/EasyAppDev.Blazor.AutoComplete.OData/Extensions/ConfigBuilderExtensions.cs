using EasyAppDev.Blazor.AutoComplete.Configuration;
using EasyAppDev.Blazor.AutoComplete.OData;

namespace EasyAppDev.Blazor.AutoComplete;

/// <summary>
/// Extension methods for configuring OData data sources via the fluent builder.
/// </summary>
public static class ODataConfigBuilderExtensions
{
    /// <summary>
    /// Configures the AutoComplete to use an OData data source.
    /// </summary>
    /// <typeparam name="TItem">The type of items.</typeparam>
    /// <param name="builder">The config builder.</param>
    /// <param name="dataSource">The OData data source instance.</param>
    /// <returns>The builder for chaining.</returns>
    public static AutoCompleteConfigBuilder<TItem> WithODataSource<TItem>(
        this AutoCompleteConfigBuilder<TItem> builder,
        ODataDataSource<TItem> dataSource)
        where TItem : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(dataSource);

        return builder.WithDataSource(dataSource);
    }

    /// <summary>
    /// Configures the AutoComplete to use an OData data source with inline options.
    /// </summary>
    /// <typeparam name="TItem">The type of items.</typeparam>
    /// <param name="builder">The config builder.</param>
    /// <param name="httpClient">The HTTP client for making requests.</param>
    /// <param name="endpointUrl">The OData endpoint URL.</param>
    /// <param name="searchFieldName">The OData property name to search in.</param>
    /// <param name="configure">Optional action to configure additional options.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example>
    /// <code>
    /// var config = AutoCompleteConfig&lt;Product&gt;.Create()
    ///     .WithODataSource(httpClient, "https://api.example.com/odata/products", "Name",
    ///         opts => {
    ///             opts.FilterStrategy = ODataFilterStrategy.Contains;
    ///             opts.Top = 20;
    ///         })
    ///     .Build();
    /// </code>
    /// </example>
    public static AutoCompleteConfigBuilder<TItem> WithODataSource<TItem>(
        this AutoCompleteConfigBuilder<TItem> builder,
        HttpClient httpClient,
        string endpointUrl,
        string searchFieldName,
        Action<ODataOptions>? configure = null)
        where TItem : class
    {
        return builder.WithODataSource(httpClient, endpointUrl, new[] { searchFieldName }, configure);
    }

    /// <summary>
    /// Configures the AutoComplete to use an OData data source with multi-field search.
    /// </summary>
    /// <typeparam name="TItem">The type of items.</typeparam>
    /// <param name="builder">The config builder.</param>
    /// <param name="httpClient">The HTTP client for making requests.</param>
    /// <param name="endpointUrl">The OData endpoint URL.</param>
    /// <param name="searchFieldNames">The OData property names to search in (combined with OR).</param>
    /// <param name="configure">Optional action to configure additional options.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example>
    /// <code>
    /// var config = AutoCompleteConfig&lt;Product&gt;.Create()
    ///     .WithODataSource(httpClient, "https://api.example.com/odata/products",
    ///         new[] { "Name", "Description", "Category" },
    ///         opts => {
    ///             opts.FilterStrategy = ODataFilterStrategy.Contains;
    ///             opts.Version = ODataVersion.V3;
    ///         })
    ///     .Build();
    /// </code>
    /// </example>
    public static AutoCompleteConfigBuilder<TItem> WithODataSource<TItem>(
        this AutoCompleteConfigBuilder<TItem> builder,
        HttpClient httpClient,
        string endpointUrl,
        string[] searchFieldNames,
        Action<ODataOptions>? configure = null)
        where TItem : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(endpointUrl);
        ArgumentNullException.ThrowIfNull(searchFieldNames);

        var options = new ODataOptions { EndpointUrl = endpointUrl };
        configure?.Invoke(options);

        var dataSource = new ODataDataSource<TItem>(httpClient, options, searchFieldNames);

        return builder.WithDataSource(dataSource);
    }

    /// <summary>
    /// Configures the AutoComplete to use an OData data source with full options.
    /// </summary>
    /// <typeparam name="TItem">The type of items.</typeparam>
    /// <param name="builder">The config builder.</param>
    /// <param name="httpClient">The HTTP client for making requests.</param>
    /// <param name="options">The OData configuration options.</param>
    /// <param name="searchFieldNames">The OData property names to search in (combined with OR).</param>
    /// <param name="textSelector">Optional text selector for fuzzy client-side re-ranking.</param>
    /// <returns>The builder for chaining.</returns>
    public static AutoCompleteConfigBuilder<TItem> WithODataSource<TItem>(
        this AutoCompleteConfigBuilder<TItem> builder,
        HttpClient httpClient,
        ODataOptions options,
        string[] searchFieldNames,
        Func<TItem, string>? textSelector = null)
        where TItem : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(searchFieldNames);

        var dataSource = new ODataDataSource<TItem>(httpClient, options, searchFieldNames, textSelector);

        return builder.WithDataSource(dataSource);
    }
}
