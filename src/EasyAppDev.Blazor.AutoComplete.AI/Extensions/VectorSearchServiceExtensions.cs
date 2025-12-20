using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.DataSources;
using EasyAppDev.Blazor.AutoComplete.AI.Models;

// NOTE: Extension methods are placed in Microsoft.Extensions.DependencyInjection namespace
// to enable IntelliSense discovery when using IServiceCollection.
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering vector search services for AutoComplete components.
/// </summary>
public static class AutoCompleteVectorSearchExtensions
{
    /// <summary>
    /// Adds vector search data source with options configuration.
    /// Requires IVectorSearchProvider and IEmbeddingGenerator to be registered separately.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional options configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// // Register provider and embedding generator first, then add data source
    /// builder.Services.AddSingleton&lt;IVectorSearchProvider&lt;Product&gt;&gt;(myProvider);
    /// builder.Services.AddAutoCompleteSemanticSearch(apiKey: "sk-...");
    /// builder.Services.AddAutoCompleteVectorSearch&lt;Product&gt;(options =>
    /// {
    ///     options.MaxResults = 10;
    ///     options.MinSimilarityScore = 0.5f;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddAutoCompleteVectorSearch<TItem>(
        this IServiceCollection services,
        Action<VectorSearchDataSourceOptions>? configureOptions = null)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new VectorSearchDataSourceOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);
        services.AddScoped<VectorSearchDataSource<TItem>>(sp =>
        {
            var provider = sp.GetRequiredService<IVectorSearchProvider<TItem>>();
            var embeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var opts = sp.GetRequiredService<VectorSearchDataSourceOptions>();
            return new VectorSearchDataSource<TItem>(provider, embeddingGenerator, opts);
        });

        return services;
    }

    /// <summary>
    /// Adds vector search data source with OpenAI embedding generator.
    /// Requires IVectorSearchProvider to be registered separately.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="openAiApiKey">OpenAI API key.</param>
    /// <param name="model">Embedding model name. Default: text-embedding-3-small.</param>
    /// <param name="configureOptions">Optional options configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddSingleton&lt;IVectorSearchProvider&lt;Product&gt;&gt;(myPostgresProvider);
    /// builder.Services.AddAutoCompleteVectorSearch&lt;Product&gt;(
    ///     openAiApiKey: "sk-...",
    ///     model: "text-embedding-3-small",
    ///     configureOptions: options =>
    ///     {
    ///         options.MaxResults = 10;
    ///         options.EnableHybridSearch = true;
    ///     });
    /// </code>
    /// </example>
    public static IServiceCollection AddAutoCompleteVectorSearch<TItem>(
        this IServiceCollection services,
        string openAiApiKey,
        string model = "text-embedding-3-small",
        Action<VectorSearchDataSourceOptions>? configureOptions = null)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(openAiApiKey);

#pragma warning disable SKEXP0010, SKEXP0001
        services.AddOpenAIEmbeddingGenerator(
            modelId: model,
            apiKey: openAiApiKey);
#pragma warning restore SKEXP0010, SKEXP0001

        return services.AddAutoCompleteVectorSearch<TItem>(configureOptions);
    }

    /// <summary>
    /// Adds vector search data source with Azure OpenAI embedding generator.
    /// Requires IVectorSearchProvider to be registered separately.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="endpoint">Azure OpenAI endpoint URL.</param>
    /// <param name="apiKey">Azure OpenAI API key.</param>
    /// <param name="deploymentName">Deployment name for the embedding model.</param>
    /// <param name="configureOptions">Optional options configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddSingleton&lt;IVectorSearchProvider&lt;Product&gt;&gt;(myAzureSearchProvider);
    /// builder.Services.AddAutoCompleteVectorSearchWithAzure&lt;Product&gt;(
    ///     endpoint: "https://my-resource.openai.azure.com/",
    ///     apiKey: "...",
    ///     deploymentName: "text-embedding-ada-002",
    ///     configureOptions: options =>
    ///     {
    ///         options.EnableHybridSearch = true;
    ///     });
    /// </code>
    /// </example>
    public static IServiceCollection AddAutoCompleteVectorSearchWithAzure<TItem>(
        this IServiceCollection services,
        string endpoint,
        string apiKey,
        string deploymentName,
        Action<VectorSearchDataSourceOptions>? configureOptions = null)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentName);

#pragma warning disable SKEXP0010, SKEXP0001
        services.AddAzureOpenAIEmbeddingGenerator(
            endpoint: endpoint,
            apiKey: apiKey,
            deploymentName: deploymentName);
#pragma warning restore SKEXP0010, SKEXP0001

        return services.AddAutoCompleteVectorSearch<TItem>(configureOptions);
    }
}
