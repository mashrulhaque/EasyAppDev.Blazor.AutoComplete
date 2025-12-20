using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Pinecone;
using Sdk = Pinecone;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.Pinecone.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.Pinecone.Extensions;

/// <summary>
/// Extension methods for registering Pinecone vector search services.
/// </summary>
public static class PineconeServiceExtensions
{
    /// <summary>
    /// Adds Pinecone as the vector search provider.
    /// </summary>
    /// <typeparam name="TItem">The item type to search.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configureOptions is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    public static IServiceCollection AddAutoCompletePineconeProvider<TItem>(
        this IServiceCollection services,
        Action<PineconeVectorSearchOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Build options using the configure delegate
        var options = new PineconeVectorSearchOptions();
        configureOptions(options);
        ValidateOptions(options);

        // Register options
        services.AddSingleton(options);

        // Register Pinecone client
        services.AddSingleton<Sdk.PineconeClient>(sp =>
        {
            var opts = sp.GetRequiredService<PineconeVectorSearchOptions>();
            return new Sdk.PineconeClient(opts.ApiKey);
        });

        // Register Semantic Kernel Pinecone vector store
        services.AddSingleton<PineconeVectorStore>(sp =>
        {
            var client = sp.GetRequiredService<Sdk.PineconeClient>();
            return new PineconeVectorStore(client);
        });

        // Register our provider
        services.AddScoped<IVectorSearchProvider<TItem>, PineconeVectorSearchProvider<TItem>>();

        return services;
    }

    /// <summary>
    /// Adds Pinecone vector indexer for batch indexing operations.
    /// </summary>
    /// <typeparam name="TItem">The item type to index.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID from items.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or textSelector is null.</exception>
    public static IServiceCollection AddAutoCompletePineconeIndexer<TItem>(
        this IServiceCollection services,
        Func<TItem, string> textSelector,
        Func<TItem, string>? idSelector = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(textSelector);

        services.AddScoped<IVectorIndexer<TItem>>(sp =>
        {
            var vectorStore = sp.GetRequiredService<PineconeVectorStore>();
            var embeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var options = sp.GetRequiredService<PineconeVectorSearchOptions>();

            return new PineconeVectorIndexer<TItem>(
                vectorStore,
                embeddingGenerator,
                options,
                textSelector,
                idSelector);
        });

        return services;
    }

    /// <summary>
    /// Adds Pinecone vector search with both provider and indexer.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID from items.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoCompletePinecone<TItem>(
        this IServiceCollection services,
        Action<PineconeVectorSearchOptions> configureOptions,
        Func<TItem, string> textSelector,
        Func<TItem, string>? idSelector = null)
    {
        services.AddAutoCompletePineconeProvider<TItem>(configureOptions);
        services.AddAutoCompletePineconeIndexer(textSelector, idSelector);

        return services;
    }

    private static void ValidateOptions(PineconeVectorSearchOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            errors.Add("ApiKey is required");

        if (string.IsNullOrWhiteSpace(options.IndexName))
            errors.Add("IndexName is required");

        if (options.EmbeddingDimensions <= 0)
            errors.Add("EmbeddingDimensions must be positive");

        if (options.IndexBatchSize <= 0)
            errors.Add("IndexBatchSize must be positive");

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Invalid PineconeVectorSearchOptions: {string.Join(", ", errors)}",
                nameof(options));
        }
    }
}
