using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace EasyAppDev.Blazor.AutoComplete.Generators;

/// <summary>
/// Source generator that automatically generates the ApplyConfiguration method
/// for the AutoComplete component based on matching properties in AutoCompleteConfig.
/// This eliminates manual mapping and ensures all configuration properties are applied.
/// </summary>
[Generator]
public class ConfigurationApplierGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find AutoCompleteConfig class
        var configClassProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsConfigClass(node),
                transform: static (ctx, _) => GetConfigClassInfo(ctx))
            .Where(static info => info is not null);

        // Find AutoComplete component class
        var componentClassProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsAutoCompleteComponent(node),
                transform: static (ctx, _) => GetComponentClassInfo(ctx))
            .Where(static info => info is not null);

        // Combine both providers
        var combined = configClassProvider.Collect().Combine(componentClassProvider.Collect());

        // Generate ApplyConfiguration method
        context.RegisterSourceOutput(combined, GenerateApplyConfiguration!);
    }

    private static bool IsConfigClass(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDecl)
        {
            return false;
        }

        return classDecl.Identifier.Text == "AutoCompleteConfig";
    }

    private static bool IsAutoCompleteComponent(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDecl)
        {
            return false;
        }

        return classDecl.Identifier.Text == "AutoComplete";
    }

    private static ConfigClassInfo? GetConfigClassInfo(GeneratorSyntaxContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

        if (symbol is null)
        {
            return null;
        }

        // Get all properties with internal setters
        var properties = symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.SetMethod?.DeclaredAccessibility == Accessibility.Internal)
            .Select(p => new PropertyInfo
            {
                Name = p.Name,
                Type = p.Type.ToDisplayString()
            })
            .ToList();

        return new ConfigClassInfo
        {
            Properties = properties,
            TypeParameter = classDecl.TypeParameterList?.Parameters.FirstOrDefault()?.Identifier.Text ?? "TItem"
        };
    }

    private static ComponentClassInfo? GetComponentClassInfo(GeneratorSyntaxContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

        if (symbol is null)
        {
            return null;
        }

        // Get all properties with [Parameter] attribute
        var properties = symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.Name == "ParameterAttribute"))
            .Where(p => p.Name != "Config") // Exclude the Config parameter itself
            .Select(p => new PropertyInfo
            {
                Name = p.Name,
                Type = p.Type.ToDisplayString()
            })
            .ToList();

        return new ComponentClassInfo
        {
            Properties = properties,
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            TypeParameter = classDecl.TypeParameterList?.Parameters.FirstOrDefault()?.Identifier.Text ?? "TItem"
        };
    }

    private static void GenerateApplyConfiguration(
        SourceProductionContext context,
        (ImmutableArray<ConfigClassInfo> ConfigClasses, ImmutableArray<ComponentClassInfo> ComponentClasses) data)
    {
        var (configClasses, componentClasses) = data;

        if (configClasses.IsEmpty || componentClasses.IsEmpty)
        {
            return;
        }

        var configClass = configClasses[0];
        var componentClass = componentClasses[0];

        // Find matching properties between config and component
        var matchingProperties = configClass.Properties
            .Where(configProp => componentClass.Properties.Any(compProp => compProp.Name == configProp.Name))
            .ToList();

        if (!matchingProperties.Any())
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using EasyAppDev.Blazor.AutoComplete.Configuration;");
        sb.AppendLine();
        sb.AppendLine($"namespace {componentClass.Namespace}");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Auto-generated partial class for AutoComplete component");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public partial class AutoComplete<{componentClass.TypeParameter}>");
        sb.AppendLine("    {");
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Applies configuration from AutoCompleteConfig to component parameters.");
        sb.AppendLine("        /// This method is auto-generated to ensure all configuration properties are applied.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine($"        /// <param name=\"config\">The configuration to apply</param>");
        sb.AppendLine($"        [global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{nameof(ConfigurationApplierGenerator)}\", \"1.0.0\")]");
        sb.AppendLine($"        partial void ApplyConfigurationGenerated(AutoCompleteConfig<{componentClass.TypeParameter}> config)");
        sb.AppendLine("        {");

        // Group properties by category based on naming patterns
        var dataProps = matchingProperties.Where(p => new[] { "Items", "DataSource", "Value", "ValueChanged" }.Contains(p.Name)).ToList();
        var displayProps = matchingProperties.Where(p => new[] { "TextField", "SearchFields", "Placeholder", "Theme", "BootstrapTheme", "ThemePreset", "ThemeOverrides", "Size", "EnableThemeTransitions", "RightToLeft", "PrimaryColor", "BackgroundColor", "TextColor", "BorderColor", "HoverColor", "SelectedColor", "BorderRadius", "FontFamily", "FontSize", "DropdownShadow" }.Contains(p.Name)).ToList();
        var behaviorProps = matchingProperties.Where(p => new[] { "MinSearchLength", "MaxDisplayedItems", "DebounceMs", "AllowClear", "Disabled", "CloseOnSelect" }.Contains(p.Name)).ToList();
        var filteringProps = matchingProperties.Where(p => new[] { "FilterStrategy", "CustomFilter" }.Contains(p.Name)).ToList();
        var virtualizationProps = matchingProperties.Where(p => new[] { "Virtualize", "VirtualizationThreshold", "ItemHeight" }.Contains(p.Name)).ToList();
        var groupingProps = matchingProperties.Where(p => new[] { "GroupBy", "GroupTemplate" }.Contains(p.Name)).ToList();
        var displayModeProps = matchingProperties.Where(p => new[] { "DisplayMode", "DescriptionField", "BadgeField", "IconField", "SubtitleField", "BadgeClass" }.Contains(p.Name)).ToList();
        var templateProps = matchingProperties.Where(p => new[] { "ItemTemplate", "NoResultsTemplate", "LoadingTemplate", "HeaderTemplate", "FooterTemplate" }.Contains(p.Name)).ToList();
        var accessibilityProps = matchingProperties.Where(p => new[] { "AriaLabel" }.Contains(p.Name)).ToList();

        // Generate assignments by category
        if (dataProps.Any())
        {
            sb.AppendLine("            // Data properties");
            foreach (var prop in dataProps)
            {
                sb.AppendLine($"            {prop.Name} = config.{prop.Name};");
            }
            sb.AppendLine();
        }

        if (displayProps.Any())
        {
            sb.AppendLine("            // Display properties");
            foreach (var prop in displayProps)
            {
                sb.AppendLine($"            {prop.Name} = config.{prop.Name};");
            }
            sb.AppendLine();
        }

        if (behaviorProps.Any())
        {
            sb.AppendLine("            // Behavior properties");
            foreach (var prop in behaviorProps)
            {
                sb.AppendLine($"            {prop.Name} = config.{prop.Name};");
            }
            sb.AppendLine();
        }

        if (filteringProps.Any())
        {
            sb.AppendLine("            // Filtering properties");
            foreach (var prop in filteringProps)
            {
                sb.AppendLine($"            {prop.Name} = config.{prop.Name};");
            }
            sb.AppendLine();
        }

        if (virtualizationProps.Any())
        {
            sb.AppendLine("            // Virtualization properties");
            foreach (var prop in virtualizationProps)
            {
                sb.AppendLine($"            {prop.Name} = config.{prop.Name};");
            }
            sb.AppendLine();
        }

        if (groupingProps.Any())
        {
            sb.AppendLine("            // Grouping properties");
            foreach (var prop in groupingProps)
            {
                sb.AppendLine($"            {prop.Name} = config.{prop.Name};");
            }
            sb.AppendLine();
        }

        if (displayModeProps.Any())
        {
            sb.AppendLine("            // Display mode properties");
            foreach (var prop in displayModeProps)
            {
                sb.AppendLine($"            {prop.Name} = config.{prop.Name};");
            }
            sb.AppendLine();
        }

        if (templateProps.Any())
        {
            sb.AppendLine("            // Template properties");
            foreach (var prop in templateProps)
            {
                sb.AppendLine($"            {prop.Name} = config.{prop.Name};");
            }
            sb.AppendLine();
        }

        if (accessibilityProps.Any())
        {
            sb.AppendLine("            // Accessibility properties");
            foreach (var prop in accessibilityProps)
            {
                sb.AppendLine($"            {prop.Name} = config.{prop.Name};");
            }
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("AutoComplete.ApplyConfiguration.g.cs", sb.ToString());
    }

    private class ConfigClassInfo
    {
        public List<PropertyInfo> Properties { get; set; } = new();
        public string TypeParameter { get; set; } = string.Empty;
    }

    private class ComponentClassInfo
    {
        public List<PropertyInfo> Properties { get; set; } = new();
        public string Namespace { get; set; } = string.Empty;
        public string TypeParameter { get; set; } = string.Empty;
    }

    private class PropertyInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
