using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using QdrantDemo.Models;

namespace QdrantDemo.Services;

/// <summary>
/// Background service that indexes products on application startup.
/// </summary>
public class ProductIndexingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductIndexingService> _logger;

    public ProductIndexingService(
        IServiceProvider serviceProvider,
        ILogger<ProductIndexingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the application to fully start
        await Task.Delay(2000, stoppingToken);

        using var scope = _serviceProvider.CreateScope();

        try
        {
            var indexer = scope.ServiceProvider.GetRequiredService<IVectorIndexer<Product>>();

            _logger.LogInformation("Starting product indexing to Qdrant...");

            // Ensure the collection exists
            await indexer.EnsureCollectionExistsAsync(stoppingToken);

            // Check if already indexed
            var provider = scope.ServiceProvider.GetRequiredService<IVectorSearchProvider<Product>>();
            var count = await provider.GetItemCountAsync(stoppingToken);

            if (count > 0)
            {
                _logger.LogInformation("Products already indexed ({Count} items). Skipping indexing.", count);
                return;
            }

            // Get sample products
            var products = ProductDataService.GetSampleProducts();

            _logger.LogInformation("Indexing {Count} products to Qdrant...", products.Count);

            // Track progress
            indexer.ProgressChanged += (s, e) =>
                _logger.LogInformation("Indexed {Processed}/{Total} products ({Percent:P0})",
                    e.ProcessedItems, e.TotalItems, (double)e.ProcessedItems / e.TotalItems);

            // Index all products
            await indexer.IndexAsync(products, stoppingToken);

            _logger.LogInformation("Product indexing to Qdrant complete!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index products. Make sure Qdrant is running (docker-compose up -d).");
        }
    }
}
