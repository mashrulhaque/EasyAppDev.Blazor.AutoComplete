// This file contains example code for using SemanticSearchDataSource with Ollama (local).
// Add the OllamaSharp NuGet package to use this example.

#if EXAMPLE_CODE
using EasyAppDev.Blazor.AutoComplete.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace AutoComplete.Samples.AIExamples;

/// <summary>
/// Example showing how to configure semantic search with Ollama (local AI).
/// Requires Ollama to be installed and running locally.
/// Install from: https://ollama.ai
/// </summary>
public class OllamaExample
{
    public static void ConfigureServices(IServiceCollection services, string ollamaEndpoint = "http://localhost:11434")
    {
        // Register Ollama embedding generator
        var ollamaClient = new OllamaApiClient(ollamaEndpoint);

        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
            ollamaClient.AsEmbeddingGenerator("nomic-embed-text"));

        // Or use AddSemanticAutoComplete extension
        services.AddSemanticAutoComplete(options =>
        {
            options.EmbeddingGenerator = ollamaClient.AsEmbeddingGenerator("nomic-embed-text");
            options.CacheDuration = TimeSpan.FromDays(7); // Long cache for local embeddings
            options.MinSimilarityScore = 0.65f; // Lower threshold for local models
        });
    }

    // Example Blazor component usage (offline/privacy-focused)
    public class ExampleComponent
    {
        [Inject]
        public required IEmbeddingGenerator<string, Embedding<float>> EmbeddingGenerator { get; set; }

        private List<CustomerTicket> tickets = new()
        {
            new CustomerTicket { Id = 1, Subject = "Login Issue", Description = "Cannot access account" },
            new CustomerTicket { Id = 2, Subject = "Payment Failed", Description = "Transaction declined" },
            new CustomerTicket { Id = 3, Subject = "Feature Request", Description = "Need dark mode support" }
        };

        private SemanticSearchDataSource<CustomerTicket>? semanticDataSource;

        protected async Task OnInitializedAsync()
        {
            // Perfect for privacy-sensitive data - all processing happens locally
            semanticDataSource = new SemanticSearchDataSource<CustomerTicket>(
                items: tickets,
                textSelector: t => $"{t.Subject}: {t.Description}",
                embeddingGenerator: EmbeddingGenerator,
                similarityThreshold: 0.65f,
                cacheDuration: TimeSpan.FromDays(7));

            // Optional: Pre-compute embeddings
            await semanticDataSource.InitializeAsync();
        }

        // Use in AutoComplete:
        // <AutoComplete TItem="CustomerTicket"
        //               DataSource="@semanticDataSource"
        //               TextField="@(t => t.Subject)" />
    }

    public class CustomerTicket
    {
        public int Id { get; set; }
        public required string Subject { get; set; }
        public required string Description { get; set; }
    }

    // Note: Before using, ensure Ollama is running and the model is pulled:
    // $ ollama pull nomic-embed-text
}
#endif
