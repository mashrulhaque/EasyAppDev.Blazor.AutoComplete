using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.Qdrant.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.Qdrant.Extensions;

/// <summary>
/// Extension methods for registering Qdrant vector search services.
/// </summary>
public static class QdrantServiceExtensions
{
    /// <summary>
    /// Adds Qdrant as the vector search provider.
    /// </summary>
    /// <typeparam name="TItem">The item type to search.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configureOptions is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    public static IServiceCollection AddAutoCompleteQdrantProvider<TItem>(
        this IServiceCollection services,
        Action<QdrantVectorSearchOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Build options using the configure delegate
        var options = new QdrantVectorSearchOptions();
        configureOptions(options);
        ValidateOptions(options);

        // Register options
        services.AddSingleton(options);

        // Register Qdrant client
        services.AddSingleton<QdrantClient>(sp =>
        {
            var opts = sp.GetRequiredService<QdrantVectorSearchOptions>();
            return new QdrantClient(
                host: opts.Host,
                port: opts.Port,
                https: opts.Https,
                apiKey: opts.ApiKey);
        });

        // Register Semantic Kernel Qdrant vector store
        services.AddSingleton<QdrantVectorStore>(sp =>
        {
            var client = sp.GetRequiredService<QdrantClient>();
            return new QdrantVectorStore(client, ownsClient: false);
        });

        // Register our provider
        services.AddScoped<IVectorSearchProvider<TItem>, QdrantVectorSearchProvider<TItem>>();

        return services;
    }

    /// <summary>
    /// Adds Qdrant vector indexer for batch indexing operations.
    /// </summary>
    /// <typeparam name="TItem">The item type to index.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID (GUID) from items.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or textSelector is null.</exception>
    public static IServiceCollection AddAutoCompleteQdrantIndexer<TItem>(
        this IServiceCollection services,
        Func<TItem, string> textSelector,
        Func<TItem, Guid>? idSelector = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(textSelector);

        services.AddScoped<IVectorIndexer<TItem>>(sp =>
        {
            var vectorStore = sp.GetRequiredService<QdrantVectorStore>();
            var embeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var options = sp.GetRequiredService<QdrantVectorSearchOptions>();

            return new QdrantVectorIndexer<TItem>(
                vectorStore,
                embeddingGenerator,
                options,
                textSelector,
                idSelector);
        });

        return services;
    }

    /// <summary>
    /// Adds Qdrant vector search with both provider and indexer.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID (GUID) from items.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoCompleteQdrant<TItem>(
        this IServiceCollection services,
        Action<QdrantVectorSearchOptions> configureOptions,
        Func<TItem, string> textSelector,
        Func<TItem, Guid>? idSelector = null)
    {
        services.AddAutoCompleteQdrantProvider<TItem>(configureOptions);
        services.AddAutoCompleteQdrantIndexer(textSelector, idSelector);

        return services;
    }

    private static void ValidateOptions(QdrantVectorSearchOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
            errors.Add("Host is required");

        if (string.IsNullOrWhiteSpace(options.CollectionName))
            errors.Add("CollectionName is required");

        if (options.Port <= 0 || options.Port > 65535)
            errors.Add("Port must be between 1 and 65535");

        if (options.EmbeddingDimensions <= 0)
            errors.Add("EmbeddingDimensions must be positive");

        if (options.IndexBatchSize <= 0)
            errors.Add("IndexBatchSize must be positive");

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Invalid QdrantVectorSearchOptions: {string.Join(", ", errors)}",
                nameof(options));
        }
    }
}
