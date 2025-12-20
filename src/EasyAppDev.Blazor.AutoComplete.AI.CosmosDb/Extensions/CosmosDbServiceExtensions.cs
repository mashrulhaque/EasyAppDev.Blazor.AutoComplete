using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.CosmosDb.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.CosmosDb.Extensions;

/// <summary>
/// Extension methods for registering Azure CosmosDB vector search services.
/// </summary>
public static class CosmosDbServiceExtensions
{
    /// <summary>
    /// Adds Azure CosmosDB as the vector search provider.
    /// </summary>
    /// <typeparam name="TItem">The item type to search.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configureOptions is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    public static IServiceCollection AddAutoCompleteCosmosDbProvider<TItem>(
        this IServiceCollection services,
        Action<CosmosDbVectorSearchOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Build options using the configure delegate
        var options = new CosmosDbVectorSearchOptions();
        configureOptions(options);
        ValidateOptions(options);

        // Register options
        services.AddSingleton(options);

        // Register CosmosDB client with System.Text.Json serializer (required for VectorData)
        services.AddSingleton<CosmosClient>(sp =>
        {
            var opts = sp.GetRequiredService<CosmosDbVectorSearchOptions>();
            return new CosmosClient(opts.ConnectionString, new CosmosClientOptions
            {
                UseSystemTextJsonSerializerWithOptions = JsonSerializerOptions.Default
            });
        });

        // Register Database
        services.AddSingleton<Database>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var opts = sp.GetRequiredService<CosmosDbVectorSearchOptions>();
            return client.GetDatabase(opts.DatabaseName);
        });

        // Register Container
        services.AddSingleton<Container>(sp =>
        {
            var database = sp.GetRequiredService<Database>();
            var opts = sp.GetRequiredService<CosmosDbVectorSearchOptions>();
            return database.GetContainer(opts.ContainerName);
        });

        // Register Semantic Kernel CosmosDB NoSQL vector store
        services.AddSingleton<CosmosNoSqlVectorStore>(sp =>
        {
            var database = sp.GetRequiredService<Database>();
            return new CosmosNoSqlVectorStore(database);
        });

        // Register our provider
        services.AddScoped<IVectorSearchProvider<TItem>, CosmosDbVectorSearchProvider<TItem>>();

        return services;
    }

    /// <summary>
    /// Adds Azure CosmosDB vector indexer for batch indexing operations.
    /// </summary>
    /// <typeparam name="TItem">The item type to index.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID from items.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or textSelector is null.</exception>
    public static IServiceCollection AddAutoCompleteCosmosDbIndexer<TItem>(
        this IServiceCollection services,
        Func<TItem, string> textSelector,
        Func<TItem, string>? idSelector = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(textSelector);

        services.AddScoped<IVectorIndexer<TItem>>(sp =>
        {
            var vectorStore = sp.GetRequiredService<CosmosNoSqlVectorStore>();
            var embeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var options = sp.GetRequiredService<CosmosDbVectorSearchOptions>();

            return new CosmosDbVectorIndexer<TItem>(
                vectorStore,
                embeddingGenerator,
                options,
                textSelector,
                idSelector);
        });

        return services;
    }

    /// <summary>
    /// Adds Azure CosmosDB vector search with both provider and indexer.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID from items.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoCompleteCosmosDb<TItem>(
        this IServiceCollection services,
        Action<CosmosDbVectorSearchOptions> configureOptions,
        Func<TItem, string> textSelector,
        Func<TItem, string>? idSelector = null)
    {
        services.AddAutoCompleteCosmosDbProvider<TItem>(configureOptions);
        services.AddAutoCompleteCosmosDbIndexer(textSelector, idSelector);

        return services;
    }

    private static void ValidateOptions(CosmosDbVectorSearchOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            errors.Add("ConnectionString is required");

        if (string.IsNullOrWhiteSpace(options.DatabaseName))
            errors.Add("DatabaseName is required");

        if (string.IsNullOrWhiteSpace(options.ContainerName))
            errors.Add("ContainerName is required");

        if (options.EmbeddingDimensions <= 0)
            errors.Add("EmbeddingDimensions must be positive");

        if (options.IndexBatchSize <= 0)
            errors.Add("IndexBatchSize must be positive");

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Invalid CosmosDbVectorSearchOptions: {string.Join(", ", errors)}",
                nameof(options));
        }
    }
}
