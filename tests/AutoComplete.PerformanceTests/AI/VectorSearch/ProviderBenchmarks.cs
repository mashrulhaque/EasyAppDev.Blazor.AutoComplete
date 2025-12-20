using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.Models;

namespace EasyAppDev.Blazor.AutoComplete.PerformanceTests.AI.VectorSearch;

/// <summary>
/// Performance benchmarks for vector search providers.
/// These benchmarks measure search latency and memory allocation for different providers.
///
/// Note: Integration benchmarks require running databases:
/// - PostgreSQL: docker run -d -p 5432:5432 pgvector/pgvector:pg16
/// - Qdrant: docker run -d -p 6333:6333 -p 6334:6334 qdrant/qdrant
///
/// Cloud providers (Azure AI Search, Pinecone, CosmosDB) require configuration.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[RankColumn]
public class ProviderBenchmarks
{
    /// <summary>
    /// Sample product model for benchmarking.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    private ReadOnlyMemory<float> _queryEmbedding;
    private VectorSearchOptions _options = null!;

    // Mock providers for unit benchmark testing (no external dependencies)
    private MockVectorSearchProvider<Product> _mockProvider = null!;

    [Params(100, 1000, 10000)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Create a realistic embedding vector (1536 dimensions for text-embedding-3-small)
        var embedding = new float[1536];
        var random = new Random(42); // Deterministic for reproducibility
        for (var i = 0; i < embedding.Length; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2 - 1);
        }
        // Normalize to unit length (like real embeddings)
        var magnitude = MathF.Sqrt(embedding.Sum(x => x * x));
        for (var i = 0; i < embedding.Length; i++)
        {
            embedding[i] /= magnitude;
        }
        _queryEmbedding = embedding;

        _options = new VectorSearchOptions
        {
            MaxResults = 10,
            MinScore = 0.15f
        };

        // Set up mock provider with pre-generated items
        _mockProvider = new MockVectorSearchProvider<Product>(ItemCount, _queryEmbedding);
    }

    [Benchmark(Baseline = true, Description = "Mock Provider (Baseline)")]
    public async Task<List<VectorSearchResult<Product>>> MockProvider_Search()
    {
        var results = await _mockProvider.SearchAsync(_queryEmbedding, _options);
        return results.ToList();
    }

    /// <summary>
    /// Benchmark for in-memory similarity calculation.
    /// This measures the theoretical minimum latency for similarity search.
    /// </summary>
    [Benchmark(Description = "SIMD Similarity Only")]
    public List<float> SimilarityCalculation_SIMD()
    {
        var results = new List<float>(ItemCount);
        for (var i = 0; i < ItemCount; i++)
        {
            // Simulate SIMD cosine similarity
            var similarity = CalculateCosineSimilarity(_queryEmbedding.Span, _mockProvider.GetItemEmbedding(i).Span);
            if (similarity >= _options.MinScore)
            {
                results.Add(similarity);
            }
        }
        return results;
    }

    private static float CalculateCosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        // Use System.Numerics.Tensors for SIMD-accelerated similarity
        return System.Numerics.Tensors.TensorPrimitives.CosineSimilarity(a, b);
    }
}

/// <summary>
/// Mock vector search provider for baseline benchmarking without external dependencies.
/// </summary>
internal sealed class MockVectorSearchProvider<TItem> : IVectorSearchProvider<TItem>
{
    private readonly List<(TItem Item, ReadOnlyMemory<float> Embedding)> _items;
    private readonly ReadOnlyMemory<float>[] _embeddings;

    public MockVectorSearchProvider(int itemCount, ReadOnlyMemory<float> queryEmbedding)
    {
        var random = new Random(42);
        _items = new List<(TItem, ReadOnlyMemory<float>)>(itemCount);
        _embeddings = new ReadOnlyMemory<float>[itemCount];

        for (var i = 0; i < itemCount; i++)
        {
            // Create item
            var category = (i % 5) switch
            {
                0 => "Electronics",
                1 => "Clothing",
                2 => "Home",
                3 => "Sports",
                _ => "Books"
            };
            var item = (TItem)(object)new ProviderBenchmarks.Product
            {
                Id = i,
                Name = $"Product {i}",
                Description = $"Description for product {i}",
                Category = category
            };

            // Create random embedding
            var embedding = new float[1536];
            for (var j = 0; j < embedding.Length; j++)
            {
                embedding[j] = (float)(random.NextDouble() * 2 - 1);
            }
            // Normalize
            var magnitude = MathF.Sqrt(embedding.Sum(x => x * x));
            for (var j = 0; j < embedding.Length; j++)
            {
                embedding[j] /= magnitude;
            }

            _items.Add((item, embedding));
            _embeddings[i] = embedding;
        }
    }

    public ReadOnlyMemory<float> GetItemEmbedding(int index) => _embeddings[index];

    public Task<IEnumerable<VectorSearchResult<TItem>>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding,
        VectorSearchOptions options,
        CancellationToken cancellationToken = default)
    {

        // Simulate vector search with SIMD similarity
        var results = _items
            .Select(item =>
            {
                var similarity = System.Numerics.Tensors.TensorPrimitives.CosineSimilarity(
                    queryEmbedding.Span, item.Embedding.Span);
                return new VectorSearchResult<TItem> { Item = item.Item, Score = similarity };
            })
            .Where(r => r.Score >= options.MinScore)
            .OrderByDescending(r => r.Score)
            .Take(options.MaxResults)
            .ToList();

        return Task.FromResult<IEnumerable<VectorSearchResult<TItem>>>(results);
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<long> GetItemCountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((long)_items.Count);
}
