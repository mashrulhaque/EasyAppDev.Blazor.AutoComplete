using EasyAppDev.Blazor.AutoComplete.OData;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring OData data sources for AutoComplete components.
/// </summary>
public static class AutoCompleteODataExtensions
{
    /// <summary>
    /// Adds OData support for AutoComplete with configuration from appsettings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration root.</param>
    /// <param name="configSection">The configuration section name. Default is "ODataAutoComplete".</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// Configuration in appsettings.json:
    /// <code>
    /// {
    ///   "ODataAutoComplete": {
    ///     "EndpointUrl": "https://api.example.com/odata/products",
    ///     "Top": 20,
    ///     "FilterStrategy": "Contains",
    ///     "Version": "V4"
    ///   }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddAutoCompleteOData(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "ODataAutoComplete")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new ODataOptions();
        configuration.GetSection(configSection).Bind(options);

        services.AddSingleton(options);

        return services;
    }

    /// <summary>
    /// Adds OData support for AutoComplete with explicit options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The OData configuration options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoCompleteOData(
        this IServiceCollection services,
        ODataOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.EndpointUrl);

        services.AddSingleton(options);

        return services;
    }

    /// <summary>
    /// Adds OData support for AutoComplete with fluent configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="endpointUrl">The OData endpoint URL.</param>
    /// <param name="configure">Optional action to configure additional options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddAutoCompleteOData(
    ///     "https://api.example.com/odata/products",
    ///     options => {
    ///         options.FilterStrategy = ODataFilterStrategy.Contains;
    ///         options.Top = 20;
    ///         options.Version = ODataVersion.V4;
    ///     });
    /// </code>
    /// </example>
    public static IServiceCollection AddAutoCompleteOData(
        this IServiceCollection services,
        string endpointUrl,
        Action<ODataOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(endpointUrl);

        var options = new ODataOptions { EndpointUrl = endpointUrl };
        configure?.Invoke(options);

        return services.AddAutoCompleteOData(options);
    }

    /// <summary>
    /// Adds a typed HttpClient configured for OData requests.
    /// Use this when you want dependency injection to manage the HttpClient lifecycle.
    /// </summary>
    /// <typeparam name="TItem">The type of items returned by the OData endpoint.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The OData configuration options.</param>
    /// <param name="searchFieldName">The OData property name to search in.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoCompleteODataSource<TItem>(
        this IServiceCollection services,
        ODataOptions options,
        string searchFieldName)
        where TItem : class
    {
        return services.AddAutoCompleteODataSource<TItem>(options, new[] { searchFieldName });
    }

    /// <summary>
    /// Adds a typed HttpClient configured for OData requests with multi-field search.
    /// Use this when you want dependency injection to manage the HttpClient lifecycle.
    /// </summary>
    /// <typeparam name="TItem">The type of items returned by the OData endpoint.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The OData configuration options.</param>
    /// <param name="searchFieldNames">The OData property names to search in (combined with OR).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoCompleteODataSource<TItem>(
        this IServiceCollection services,
        ODataOptions options,
        string[] searchFieldNames)
        where TItem : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.EndpointUrl);

        services.AddSingleton(options);

        services.AddHttpClient<ODataDataSource<TItem>>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            // Add any custom headers
            if (options.CustomHeaders != null)
            {
                foreach (var (key, value) in options.CustomHeaders)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
                }
            }
        });

        // Register the data source
        services.AddTransient<ODataDataSource<TItem>>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(typeof(ODataDataSource<TItem>).Name);
            var opts = sp.GetRequiredService<ODataOptions>();

            return new ODataDataSource<TItem>(httpClient, opts, searchFieldNames);
        });

        return services;
    }
}
