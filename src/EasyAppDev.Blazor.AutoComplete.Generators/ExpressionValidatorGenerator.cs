using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAppDev.Blazor.AutoComplete.Generators;

[Generator]
public class ExpressionValidatorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var lambdaExpressions = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsAutoCompleteParameterExpression(node),
                transform: static (ctx, _) => ValidateExpression(ctx))
            .Where(static result => result.HasValue)
            .Select(static (result, _) => result!.Value);

        context.RegisterSourceOutput(lambdaExpressions, ReportDiagnostic);
    }

    private static bool IsAutoCompleteParameterExpression(SyntaxNode node)
    {
        // Check if this is a lambda expression in an AutoComplete parameter
        if (node is not SimpleLambdaExpressionSyntax lambda)
        {
            return false;
        }

        // Check if parent is an attribute argument for TextField or ValueField
        var parent = lambda.Parent;
        while (parent is not null)
        {
            if (parent is AttributeArgumentSyntax argument)
            {
                var name = argument.NameEquals?.Name.Identifier.Text;
                return name == "TextField" || name == "ValueField" || name == "GroupBy";
            }
            parent = parent.Parent;
        }

        return false;
    }

    private static ExpressionValidationResult? ValidateExpression(GeneratorSyntaxContext context)
    {
        var lambda = (SimpleLambdaExpressionSyntax)context.Node;

        // Validate that the lambda is a simple property access
        // Valid: item => item.Name
        // Invalid: item => item.Name.ToUpper()
        // Invalid: item => ComputeValue(item)

        if (lambda.Body is not MemberAccessExpressionSyntax memberAccess)
        {
            return new ExpressionValidationResult
            {
                Location = lambda.GetLocation(),
                IsValid = false,
                Message = "Expression must be a simple property access (e.g., item => item.Name)"
            };
        }

        // Ensure it's accessing a property on the parameter
        if (memberAccess.Expression is not IdentifierNameSyntax identifier ||
            identifier.Identifier.Text != lambda.Parameter.Identifier.Text)
        {
            return new ExpressionValidationResult
            {
                Location = lambda.GetLocation(),
                IsValid = false,
                Message = "Expression must access a property directly on the parameter"
            };
        }

        return null; // Valid expression
    }

    private static void ReportDiagnostic(SourceProductionContext context, ExpressionValidationResult result)
    {
        if (!result.IsValid)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.InvalidTextFieldExpression,
                result.Location,
                result.Message);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private struct ExpressionValidationResult
    {
        public Location Location { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
