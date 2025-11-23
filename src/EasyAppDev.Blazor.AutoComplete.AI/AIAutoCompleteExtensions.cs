using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace EasyAppDev.Blazor.AutoComplete.AI;

/// <summary>
/// Extension methods for configuring AI-powered AutoComplete services.
/// </summary>
public static class AIAutoCompleteExtensions
{
    /// <summary>
    /// Adds semantic AutoComplete services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for semantic AutoComplete options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSemanticAutoComplete(
        this IServiceCollection services,
        Action<SemanticAutoCompleteOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new SemanticAutoCompleteOptions();
        configure(options);

        // Validate options
        if (options.EmbeddingGenerator == null)
        {
            throw new InvalidOperationException(
                "EmbeddingGenerator must be configured in SemanticAutoCompleteOptions.");
        }

        // Register embedding generator
        services.AddSingleton(options.EmbeddingGenerator);

        return services;
    }
}

/// <summary>
/// Configuration options for semantic AutoComplete.
/// </summary>
public class SemanticAutoCompleteOptions
{
    /// <summary>
    /// Gets or sets the embedding generator to use for semantic search.
    /// </summary>
    public IEmbeddingGenerator<string, Embedding<float>>? EmbeddingGenerator { get; set; }

    /// <summary>
    /// Gets or sets the duration to cache embeddings. Default is 1 hour.
    /// </summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets the minimum similarity score for results. Default is 0.7.
    /// Range: 0.0 to 1.0, where 1.0 means identical.
    /// </summary>
    public float MinSimilarityScore { get; set; } = 0.7f;
}
