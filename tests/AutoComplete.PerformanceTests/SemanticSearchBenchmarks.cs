using BenchmarkDotNet.Attributes;
using System.Numerics.Tensors;

namespace EasyAppDev.Blazor.AutoComplete.PerformanceTests;

/// <summary>
/// Benchmarks for AI semantic search operations.
/// Tests embedding generation, similarity calculation, and caching.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class SemanticSearchBenchmarks
{
    private ReadOnlyMemory<float> _queryEmbedding;
    private List<ReadOnlyMemory<float>> _documentEmbeddings = null!;

    [Params(100, 1_000, 10_000)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Generate random embeddings (simulating OpenAI text-embedding-3-small dimension)
        _queryEmbedding = GenerateRandomEmbedding(1536);
        _documentEmbeddings = Enumerable.Range(0, ItemCount)
            .Select(_ => GenerateRandomEmbedding(1536))
            .ToList();
    }

    [Benchmark(Baseline = true)]
    public void SIMD_CosineSimilarity()
    {
        // Benchmark SIMD-accelerated similarity calculation
        float maxSimilarity = 0f;
        foreach (var docEmbedding in _documentEmbeddings)
        {
            var similarity = TensorPrimitives.CosineSimilarity(
                _queryEmbedding.Span,
                docEmbedding.Span);

            if (similarity > maxSimilarity)
            {
                maxSimilarity = similarity;
            }
        }
    }

    [Benchmark]
    public void Manual_CosineSimilarity()
    {
        // Benchmark manual calculation for comparison
        float maxSimilarity = 0f;
        foreach (var docEmbedding in _documentEmbeddings)
        {
            var similarity = CalculateCosineSimilarityManual(
                _queryEmbedding.Span,
                docEmbedding.Span);

            if (similarity > maxSimilarity)
            {
                maxSimilarity = similarity;
            }
        }
    }

    [Benchmark]
    public void TopK_Search()
    {
        // Benchmark finding top K most similar items
        const int k = 10;
        var results = new List<(int index, float similarity)>();

        for (int i = 0; i < _documentEmbeddings.Count; i++)
        {
            var similarity = TensorPrimitives.CosineSimilarity(
                _queryEmbedding.Span,
                _documentEmbeddings[i].Span);

            results.Add((i, similarity));
        }

        // Get top K results
        var topK = results.OrderByDescending(r => r.similarity).Take(k).ToList();
    }

    private static ReadOnlyMemory<float> GenerateRandomEmbedding(int dimensions)
    {
        var random = new Random(42); // Fixed seed for consistency
        var embedding = new float[dimensions];

        // Generate random normalized vector
        float sumOfSquares = 0f;
        for (int i = 0; i < dimensions; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2 - 1); // Range: [-1, 1]
            sumOfSquares += embedding[i] * embedding[i];
        }

        // Normalize to unit vector
        float norm = MathF.Sqrt(sumOfSquares);
        for (int i = 0; i < dimensions; i++)
        {
            embedding[i] /= norm;
        }

        return new ReadOnlyMemory<float>(embedding);
    }

    private static float CalculateCosineSimilarityManual(
        ReadOnlySpan<float> a,
        ReadOnlySpan<float> b)
    {
        float dotProduct = 0f;
        float normA = 0f;
        float normB = 0f;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        return dotProduct / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
    }
}
