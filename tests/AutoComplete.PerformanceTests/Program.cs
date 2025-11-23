using BenchmarkDotNet.Running;
using EasyAppDev.Blazor.AutoComplete.PerformanceTests;

// Run all benchmarks or specific benchmark based on args
if (args.Length > 0)
{
    // Run specific benchmark if specified
    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
else
{
    // Run all benchmarks
    Console.WriteLine("Running all benchmarks...");
    Console.WriteLine();

    Console.WriteLine("=== Filtering Benchmarks ===");
    BenchmarkRunner.Run<FilteringBenchmarks>();
    Console.WriteLine();

    Console.WriteLine("=== Semantic Search Benchmarks ===");
    BenchmarkRunner.Run<SemanticSearchBenchmarks>();
    Console.WriteLine();

    Console.WriteLine("=== Theming Benchmarks ===");
    BenchmarkRunner.Run<ThemingBenchmarks>();
    Console.WriteLine();

    Console.WriteLine("All benchmarks completed!");
}
