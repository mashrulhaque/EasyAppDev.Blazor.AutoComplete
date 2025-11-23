using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

// NOTE: Extension methods are placed in Microsoft.Extensions.DependencyInjection namespace
// to enable IntelliSense discovery when using IServiceCollection. This follows the pattern
// established by .NET Core libraries (AddDbContext, AddHttpClient, etc.).
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring semantic search services for AutoComplete components.
/// </summary>
public static class AutoCompleteSemanticSearchExtensions
{
    /// <summary>
    /// Adds OpenAI embedding generator for semantic search in AutoComplete components.
    /// Reads configuration from the "OpenAI" section by default.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="configSection">Configuration section name (default: "OpenAI")</param>
    /// <returns>The service collection for chaining</returns>
    /// <example>
    /// <code>
    /// // In Program.cs
    /// builder.Services.AddAutoCompleteSemanticSearch(builder.Configuration);
    ///
    /// // In appsettings.json
    /// {
    ///   "OpenAI": {
    ///     "ApiKey": "sk-...",
    ///     "Model": "text-embedding-3-small"
    ///   }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddAutoCompleteSemanticSearch(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "OpenAI")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var apiKey = configuration[$"{configSection}:ApiKey"];
        var model = configuration[$"{configSection}:Model"] ?? "text-embedding-3-small";

        if (!string.IsNullOrEmpty(apiKey))
        {
#pragma warning disable SKEXP0010, SKEXP0001 // Type is for evaluation purposes only
            services.AddOpenAIEmbeddingGenerator(
                modelId: model,
                apiKey: apiKey);
#pragma warning restore SKEXP0010, SKEXP0001
        }

        return services;
    }

    /// <summary>
    /// Adds OpenAI embedding generator for semantic search with explicit API key.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="apiKey">OpenAI API key</param>
    /// <param name="model">Model ID (default: "text-embedding-3-small")</param>
    /// <returns>The service collection for chaining</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddAutoCompleteSemanticSearch(
    ///     apiKey: "sk-...",
    ///     model: "text-embedding-3-small");
    /// </code>
    /// </example>
    public static IServiceCollection AddAutoCompleteSemanticSearch(
        this IServiceCollection services,
        string apiKey,
        string model = "text-embedding-3-small")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

#pragma warning disable SKEXP0010, SKEXP0001
        services.AddOpenAIEmbeddingGenerator(
            modelId: model,
            apiKey: apiKey);
#pragma warning restore SKEXP0010, SKEXP0001

        return services;
    }

    /// <summary>
    /// Adds Azure OpenAI embedding generator for semantic search.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="endpoint">Azure OpenAI endpoint URL</param>
    /// <param name="apiKey">Azure OpenAI API key</param>
    /// <param name="deploymentName">Deployment name for the embedding model</param>
    /// <returns>The service collection for chaining</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddAutoCompleteSemanticSearchWithAzure(
    ///     endpoint: "https://my-resource.openai.azure.com/",
    ///     apiKey: builder.Configuration["Azure:ApiKey"]!,
    ///     deploymentName: "text-embedding-ada-002");
    /// </code>
    /// </example>
    public static IServiceCollection AddAutoCompleteSemanticSearchWithAzure(
        this IServiceCollection services,
        string endpoint,
        string apiKey,
        string deploymentName)
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

        return services;
    }

    // TODO: Add Ollama support when the connector package is available
    // /// <summary>
    // /// Adds Ollama embedding generator for semantic search (local/self-hosted).
    // /// </summary>
    // /// <param name="services">The service collection</param>
    // /// <param name="ollamaEndpoint">Ollama endpoint URI (default: http://localhost:11434)</param>
    // /// <param name="modelId">Model ID (default: "nomic-embed-text")</param>
    // /// <returns>The service collection for chaining</returns>
    // /// <example>
    // /// <code>
    // /// // Use default local Ollama instance
    // /// builder.Services.AddAutoCompleteSemanticSearchWithOllama();
    // ///
    // /// // Or specify custom endpoint
    // /// builder.Services.AddAutoCompleteSemanticSearchWithOllama(
    // ///     ollamaEndpoint: new Uri("http://my-server:11434"),
    // ///     modelId: "nomic-embed-text");
    // /// </code>
    // /// </example>
    // public static IServiceCollection AddAutoCompleteSemanticSearchWithOllama(
    //     this IServiceCollection services,
    //     Uri? ollamaEndpoint = null,
    //     string modelId = "nomic-embed-text")
    // {
    //     ArgumentNullException.ThrowIfNull(services);
    //
    //     ollamaEndpoint ??= new Uri("http://localhost:11434");
    //
    // #pragma warning disable SKEXP0070
    //     services.AddOllamaEmbeddingGenerator(
    //         endpoint: ollamaEndpoint,
    //         modelId: modelId);
    // #pragma warning restore SKEXP0070
    //
    //     return services;
    // }
}
