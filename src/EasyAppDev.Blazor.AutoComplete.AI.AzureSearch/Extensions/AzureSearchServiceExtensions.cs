using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Extensions;

/// <summary>
/// Extension methods for registering Azure AI Search vector search services.
/// </summary>
public static class AzureSearchServiceExtensions
{
    /// <summary>
    /// Adds Azure AI Search as the vector search provider.
    /// </summary>
    /// <typeparam name="TItem">The item type to search.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configureOptions is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    public static IServiceCollection AddAutoCompleteAzureSearchProvider<TItem>(
        this IServiceCollection services,
        Action<AzureSearchVectorSearchOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Build options using the configure delegate
        var options = new AzureSearchVectorSearchOptions();
        configureOptions(options);
        ValidateOptions(options);

        // Register options
        services.AddSingleton(options);

        // Register Azure Search client for the specific index
        services.AddSingleton<SearchClient>(sp =>
        {
            var opts = sp.GetRequiredService<AzureSearchVectorSearchOptions>();
            return new SearchClient(
                new Uri(opts.Endpoint),
                opts.IndexName,
                new AzureKeyCredential(opts.ApiKey));
        });

        // Register Semantic Kernel Azure AI Search vector store
        services.AddSingleton<AzureAISearchVectorStore>(sp =>
        {
            var opts = sp.GetRequiredService<AzureSearchVectorSearchOptions>();
            return new AzureAISearchVectorStore(
                new SearchIndexClient(
                    new Uri(opts.Endpoint),
                    new AzureKeyCredential(opts.ApiKey)));
        });

        // Register our provider
        services.AddScoped<IVectorSearchProvider<TItem>, AzureSearchVectorSearchProvider<TItem>>();

        return services;
    }

    /// <summary>
    /// Adds Azure AI Search vector indexer for batch indexing operations.
    /// </summary>
    /// <typeparam name="TItem">The item type to index.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="titleSelector">Optional function to extract title from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID from items.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or textSelector is null.</exception>
    public static IServiceCollection AddAutoCompleteAzureSearchIndexer<TItem>(
        this IServiceCollection services,
        Func<TItem, string> textSelector,
        Func<TItem, string>? titleSelector = null,
        Func<TItem, string>? idSelector = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(textSelector);

        services.AddScoped<IVectorIndexer<TItem>>(sp =>
        {
            var vectorStore = sp.GetRequiredService<AzureAISearchVectorStore>();
            var searchClient = sp.GetRequiredService<SearchClient>();
            var embeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var options = sp.GetRequiredService<AzureSearchVectorSearchOptions>();

            return new AzureSearchVectorIndexer<TItem>(
                vectorStore,
                searchClient,
                embeddingGenerator,
                options,
                textSelector,
                titleSelector,
                idSelector);
        });

        return services;
    }

    /// <summary>
    /// Adds Azure AI Search vector search with both provider and indexer.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <param name="textSelector">Function to extract searchable text from items.</param>
    /// <param name="titleSelector">Optional function to extract title from items.</param>
    /// <param name="idSelector">Optional function to extract unique ID from items.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoCompleteAzureSearch<TItem>(
        this IServiceCollection services,
        Action<AzureSearchVectorSearchOptions> configureOptions,
        Func<TItem, string> textSelector,
        Func<TItem, string>? titleSelector = null,
        Func<TItem, string>? idSelector = null)
    {
        services.AddAutoCompleteAzureSearchProvider<TItem>(configureOptions);
        services.AddAutoCompleteAzureSearchIndexer(textSelector, titleSelector, idSelector);

        return services;
    }

    private static void ValidateOptions(AzureSearchVectorSearchOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Endpoint))
            errors.Add("Endpoint is required");

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            errors.Add("ApiKey is required");

        if (string.IsNullOrWhiteSpace(options.IndexName))
            errors.Add("IndexName is required");

        if (options.EmbeddingDimensions <= 0)
            errors.Add("EmbeddingDimensions must be positive");

        if (options.IndexBatchSize <= 0)
            errors.Add("IndexBatchSize must be positive");

        if (options.EnableSemanticRanking && string.IsNullOrWhiteSpace(options.SemanticConfigurationName))
            errors.Add("SemanticConfigurationName is required when EnableSemanticRanking is true");

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Invalid AzureSearchVectorSearchOptions: {string.Join(", ", errors)}",
                nameof(options));
        }
    }
}
