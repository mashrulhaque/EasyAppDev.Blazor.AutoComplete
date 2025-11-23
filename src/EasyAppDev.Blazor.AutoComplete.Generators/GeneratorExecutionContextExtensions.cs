using Microsoft.CodeAnalysis;
using System.Text;

namespace EasyAppDev.Blazor.AutoComplete.Generators;

internal static class GeneratorExecutionContextExtensions
{
    public static void AddSource(
        this SourceProductionContext context,
        string hintName,
        string source)
    {
        context.AddSource(hintName, Microsoft.CodeAnalysis.Text.SourceText.From(source, Encoding.UTF8));
    }
}
