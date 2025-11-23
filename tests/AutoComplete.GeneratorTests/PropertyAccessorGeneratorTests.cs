using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using EasyAppDev.Blazor.AutoComplete.Generators;

namespace AutoComplete.GeneratorTests;

public class PropertyAccessorGeneratorTests
{
    [Fact]
    public void Generator_CreatesAccessors_ForAutoCompleteUsage()
    {
        // Arrange
        var source = """
            using EasyAppDev.Blazor.AutoComplete;

            public class TestComponent
            {
                public void Test()
                {
                    var component = new AutoComplete<MyModel>();
                }
            }

            public class MyModel
            {
                public string Name { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("AutoCompleteAccessors_", output);
    }

    [Fact]
    public void Generator_HandlesMultipleTypes()
    {
        // Arrange
        var source = """
            using EasyAppDev.Blazor.AutoComplete;

            public class TestComponent
            {
                public void Test()
                {
                    var component1 = new AutoComplete<Model1>();
                    var component2 = new AutoComplete<Model2>();
                }
            }

            public class Model1
            {
                public string Name { get; set; }
            }

            public class Model2
            {
                public int Id { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        // Should generate accessors for both types
        Assert.Contains("AutoCompleteAccessors_", output);
    }

    private static (ImmutableArray<Diagnostic>, string) GetGeneratedOutput(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new PropertyAccessorGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var output = string.Join("\n", outputCompilation.SyntaxTrees.Skip(1).Select(t => t.ToString()));
        return (diagnostics, output);
    }
}
