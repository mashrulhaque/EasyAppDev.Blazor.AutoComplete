using Microsoft.CodeAnalysis;

namespace EasyAppDev.Blazor.AutoComplete.Generators;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor InvalidTextFieldExpression = new(
        id: "EBDAC001",
        title: "Invalid TextField Expression",
        messageFormat: "TextField expression must be a simple property access (e.g., item => item.Name)",
        category: "EasyAppDev.Blazor.AutoComplete",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidValueFieldExpression = new(
        id: "EBDAC002",
        title: "Invalid ValueField Expression",
        messageFormat: "ValueField expression must be a simple property access (e.g., item => item.Id)",
        category: "EasyAppDev.Blazor.AutoComplete",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedExpressionType = new(
        id: "EBDAC003",
        title: "Unsupported Expression Type",
        messageFormat: "Expression type '{0}' is not supported for trimming. Use simple property access instead.",
        category: "EasyAppDev.Blazor.AutoComplete",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
