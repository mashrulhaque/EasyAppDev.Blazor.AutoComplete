// This file contains example code for using SemanticSearchDataSource with Azure AI.
// Add the Azure.AI.Inference NuGet package to use this example.

#if EXAMPLE_CODE
using Azure;
using Azure.AI.Inference;
using EasyAppDev.Blazor.AutoComplete.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace AutoComplete.Samples.AIExamples;

/// <summary>
/// Example showing how to configure semantic search with Azure AI.
/// </summary>
public class AzureAIExample
{
    public static void ConfigureServices(
        IServiceCollection services,
        Uri endpoint,
        AzureKeyCredential credential)
    {
        // Register Azure AI embedding generator
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
            new EmbeddingsClient(endpoint, credential)
                .AsEmbeddingGenerator("text-embedding-ada-002"));

        // Or use AddSemanticAutoComplete extension
        services.AddSemanticAutoComplete(options =>
        {
            options.EmbeddingGenerator = new EmbeddingsClient(endpoint, credential)
                .AsEmbeddingGenerator("text-embedding-ada-002");
            options.CacheDuration = TimeSpan.FromHours(24); // Longer cache for Azure
            options.MinSimilarityScore = 0.75f;
        });
    }

    // Example Blazor component usage
    public class ExampleComponent
    {
        [Inject]
        public required IEmbeddingGenerator<string, Embedding<float>> EmbeddingGenerator { get; set; }

        private List<Document> documents = new()
        {
            new Document { Id = 1, Title = "Introduction to AI", Content = "Artificial intelligence overview" },
            new Document { Id = 2, Title = "Machine Learning Basics", Content = "Fundamental ML concepts" },
            new Document { Id = 3, Title = "Neural Networks", Content = "Deep learning and neural architectures" }
        };

        private SemanticSearchDataSource<Document>? semanticDataSource;

        protected async Task OnInitializedAsync()
        {
            semanticDataSource = new SemanticSearchDataSource<Document>(
                items: documents,
                textSelector: d => $"{d.Title}: {d.Content}",
                embeddingGenerator: EmbeddingGenerator,
                similarityThreshold: 0.75f,
                cacheDuration: TimeSpan.FromHours(24));

            // Optional: Pre-compute embeddings
            await semanticDataSource.InitializeAsync();
        }

        // Use in AutoComplete:
        // <AutoComplete TItem="Document"
        //               DataSource="@semanticDataSource"
        //               TextField="@(d => d.Title)" />
    }

    public class Document
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
    }
}
#endif
