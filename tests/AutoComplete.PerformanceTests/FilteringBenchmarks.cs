using BenchmarkDotNet.Attributes;
using EasyAppDev.Blazor.AutoComplete.Filtering;

namespace EasyAppDev.Blazor.AutoComplete.PerformanceTests;

/// <summary>
/// Performance benchmarks for filtering operations.
/// Tests filtering performance with various dataset sizes.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class FilteringBenchmarks
{
    private List<Product> _products = null!;
    private StartsWithFilter<Product> _startsWithFilter = null!;
    private ContainsFilter<Product> _containsFilter = null!;
    private FuzzyFilter<Product> _fuzzyFilter = null!;

    [Params(100, 1_000, 10_000, 100_000)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _products = Enumerable.Range(1, ItemCount)
            .Select(i => new Product { Id = i, Name = $"Product {i}" })
            .ToList();

        _startsWithFilter = new StartsWithFilter<Product>();
        _containsFilter = new ContainsFilter<Product>();
        _fuzzyFilter = new FuzzyFilter<Product>();
    }

    [Benchmark(Baseline = true)]
    public void StartsWith_Filter()
    {
        var result = _startsWithFilter.Filter(_products, "Product 1", p => p.Name).ToList();
    }

    [Benchmark]
    public void Contains_Filter()
    {
        var result = _containsFilter.Filter(_products, "Product 1", p => p.Name).ToList();
    }

    [Benchmark]
    public void Fuzzy_Filter()
    {
        var result = _fuzzyFilter.Filter(_products, "Prodct 1", p => p.Name).ToList();
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
