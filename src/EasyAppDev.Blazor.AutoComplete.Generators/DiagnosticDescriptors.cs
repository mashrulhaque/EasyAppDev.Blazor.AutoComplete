using Microsoft.CodeAnalysis;

namespace EasyAppDev.Blazor.AutoComplete.Generators;

internal static class DiagnosticDescriptors
{
    private const string HelpLinkUri = "https://github.com/easyappdev/blazor-autocomplete/blob/main/docs/diagnostics/{0}.md";

    public static readonly DiagnosticDescriptor InvalidTextFieldExpression = new(
        id: "EBDAC001",
        title: "Invalid TextField Expression",
        messageFormat: "TextField expression must be a simple property access (e.g., item => item.Name)",
        category: "EasyAppDev.Blazor.AutoComplete",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "TextField parameter must use a simple property access expression to maintain AOT compatibility. Complex expressions cannot be compiled ahead-of-time.",
        helpLinkUri: string.Format(HelpLinkUri, "EBDAC001"));

    public static readonly DiagnosticDescriptor InvalidValueFieldExpression = new(
        id: "EBDAC002",
        title: "Invalid ValueField Expression",
        messageFormat: "ValueField expression must be a simple property access (e.g., item => item.Id)",
        category: "EasyAppDev.Blazor.AutoComplete",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ValueField parameter must use a simple property access expression to maintain AOT compatibility. Complex expressions cannot be compiled ahead-of-time.",
        helpLinkUri: string.Format(HelpLinkUri, "EBDAC002"));

    public static readonly DiagnosticDescriptor UnsupportedExpressionType = new(
        id: "EBDAC003",
        title: "Unsupported Expression Type",
        messageFormat: "Expression type '{0}' is not supported for trimming. Use simple property access instead.",
        category: "EasyAppDev.Blazor.AutoComplete",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The expression type is not supported for AOT compilation and trimming. Only simple property access expressions (MemberExpression) are supported.",
        helpLinkUri: string.Format(HelpLinkUri, "EBDAC003"));
}
