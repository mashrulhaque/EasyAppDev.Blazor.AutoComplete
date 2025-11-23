// This file contains example code for using SemanticSearchDataSource with OpenAI.
// Add the Azure.AI.OpenAI NuGet package to use this example.

#if EXAMPLE_CODE
using Azure.AI.OpenAI;
using EasyAppDev.Blazor.AutoComplete.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace AutoComplete.Samples.AIExamples;

/// <summary>
/// Example showing how to configure semantic search with OpenAI.
/// </summary>
public class OpenAIExample
{
    public static void ConfigureServices(IServiceCollection services, string apiKey)
    {
        // Register OpenAI embedding generator
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
            new OpenAIClient(apiKey)
                .AsEmbeddingGenerator("text-embedding-3-small"));

        // Or use AddSemanticAutoComplete extension
        services.AddSemanticAutoComplete(options =>
        {
            options.EmbeddingGenerator = new OpenAIClient(apiKey)
                .AsEmbeddingGenerator("text-embedding-3-small");
            options.CacheDuration = TimeSpan.FromHours(2);
            options.MinSimilarityScore = 0.75f;
        });
    }

    // Example Blazor component usage
    public class ExampleComponent
    {
        [Inject]
        public required IEmbeddingGenerator<string, Embedding<float>> EmbeddingGenerator { get; set; }

        private List<Product> products = new()
        {
            new Product { Id = 1, Name = "Laptop", Description = "High-performance computing device" },
            new Product { Id = 2, Name = "Smartphone", Description = "Mobile communication device" },
            new Product { Id = 3, Name = "Tablet", Description = "Portable touchscreen computer" }
        };

        private SemanticSearchDataSource<Product>? semanticDataSource;

        protected async Task OnInitializedAsync()
        {
            semanticDataSource = new SemanticSearchDataSource<Product>(
                items: products,
                textSelector: p => $"{p.Name} - {p.Description}",
                embeddingGenerator: EmbeddingGenerator,
                similarityThreshold: 0.7f,
                cacheDuration: TimeSpan.FromHours(1));

            // Optional: Pre-compute embeddings
            await semanticDataSource.InitializeAsync();
        }

        // Use in AutoComplete:
        // <AutoComplete TItem="Product"
        //               DataSource="@semanticDataSource"
        //               TextField="@(p => p.Name)" />
    }

    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}
#endif
