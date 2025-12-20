using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Npgsql;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Extensions;

/// <summary>
/// Extension methods for registering PostgreSQL vector search services.
/// </summary>
public static class PostgresServiceExtensions
{
    /// <summary>
    /// Adds PostgreSQL (pgvector) as the vector search provider using configuration from appsettings.json.
    /// </summary>
    /// <typeparam name="TItem">The item type to search.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="configSection">The configuration section name. Default: "VectorSearch:PostgreSQL".</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
    /// <exception cref="ArgumentException">Thrown when configuration section is missing or options are invalid.</exception>
    /// <example>
    /// appsettings.json:
    /// {
    ///   "VectorSearch": {
    ///     "PostgreSQL": {
    ///       "ConnectionString": "Host=localhost;Database=myapp;Username=user;Password=pass",
    ///       "CollectionName": "products",
    ///       "EmbeddingDimensions": 1536
    ///     }
    ///   }
    /// }
    /// </example>
    public static IServiceCollection AddAutoCompletePostgresProvider<TItem>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "VectorSearch:PostgreSQL")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(configSection);
        if (!section.Exists())
        {
            throw new ArgumentException(
                $"Configuration section '{configSection}' not found. " +
                "Ensure your appsettings.json contains the PostgreSQL vector search configuration.",
                nameof(configSection));
        }

        var options = new PostgresVectorSearchOptions();
        section.Bind(options);

        return services.AddAutoCompletePostgresProvider<TItem>(opts =>
        {
            opts.ConnectionString = options.ConnectionString;
            opts.CollectionName = options.CollectionName;
            opts.EmbeddingDimensions = options.EmbeddingDimensions;
            opts.DistanceFunction = options.DistanceFunction;
            opts.Schema = options.Schema;
            opts.IndexBatchSize = options.IndexBatchSize;
            opts.CreateHnswIndex = options.CreateHnswIndex;
            opts.HnswM = options.HnswM;
            opts.HnswEfConstruction = options.HnswEfConstruction;
        });
    }

    /// <summary>
    /// Adds PostgreSQL (pgvector) as the vector search provider.
    /// </summary>
    /// <typeparam name="TItem">The item type to search.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configureOptions is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    public static IServiceCollection AddAutoCompletePostgresProvider<TItem>(
        this IServiceCollection services,
        Action<PostgresVectorSearchOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Build options using the configure delegate
        var options = new PostgresVectorSearchOptions();
        configureOptions(options);
        ValidateOptions(options);

        // Register options
        services.AddSingleton(options);

        // Register Npgsql data source (connection pooling)
        services.AddSingleton<NpgsqlDataSource>(sp =>
        {
            var opts = sp.GetRequiredService<PostgresVectorSearchOptions>();
            var builder = new NpgsqlDataSourceBuilder(opts.ConnectionString);
            builder.UseVector();  // Enable pgvector support
            return builder.Build();
        });

        // Register Semantic Kernel PostgreSQL vector store
        services.AddSingleton<PostgresVectorStore>(sp =>
        {
            var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
            return new PostgresVectorStore(dataSource, ownsDataSource: false);
        });

        // Register our provider
        services.AddScoped<IVectorSearchProvider<TItem>, PostgresVectorSearchProvider<TItem>>();

        return services;
    }

    /// <summary>
    /// Adds PostgreSQL vector indexer for batch indexing operations.
    /// </summary>
    /// <typeparam name="TItem">The item type to index.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID from items.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or textSelector is null.</exception>
    public static IServiceCollection AddAutoCompletePostgresIndexer<TItem>(
        this IServiceCollection services,
        Func<TItem, string> textSelector,
        Func<TItem, string>? idSelector = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(textSelector);

        services.AddScoped<IVectorIndexer<TItem>>(sp =>
        {
            var vectorStore = sp.GetRequiredService<PostgresVectorStore>();
            var embeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var options = sp.GetRequiredService<PostgresVectorSearchOptions>();

            return new PostgresVectorIndexer<TItem>(
                vectorStore,
                embeddingGenerator,
                options,
                textSelector,
                idSelector);
        });

        return services;
    }

    /// <summary>
    /// Adds PostgreSQL vector search with both provider and indexer using configuration from appsettings.json.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID from items.</param>
    /// <param name="configSection">The configuration section name. Default: "VectorSearch:PostgreSQL".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoCompletePostgres<TItem>(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<TItem, string> textSelector,
        Func<TItem, string>? idSelector = null,
        string configSection = "VectorSearch:PostgreSQL")
    {
        services.AddAutoCompletePostgresProvider<TItem>(configuration, configSection);
        services.AddAutoCompletePostgresIndexer(textSelector, idSelector);

        return services;
    }

    /// <summary>
    /// Adds PostgreSQL vector search with both provider and indexer.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID from items.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoCompletePostgres<TItem>(
        this IServiceCollection services,
        Action<PostgresVectorSearchOptions> configureOptions,
        Func<TItem, string> textSelector,
        Func<TItem, string>? idSelector = null)
    {
        services.AddAutoCompletePostgresProvider<TItem>(configureOptions);
        services.AddAutoCompletePostgresIndexer(textSelector, idSelector);

        return services;
    }

    private static void ValidateOptions(PostgresVectorSearchOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            errors.Add("ConnectionString is required");

        if (string.IsNullOrWhiteSpace(options.CollectionName))
            errors.Add("CollectionName is required");

        if (options.EmbeddingDimensions <= 0)
            errors.Add("EmbeddingDimensions must be positive");

        if (options.IndexBatchSize <= 0)
            errors.Add("IndexBatchSize must be positive");

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Invalid PostgresVectorSearchOptions: {string.Join(", ", errors)}",
                nameof(options));
        }
    }

}
