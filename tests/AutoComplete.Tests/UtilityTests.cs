using EasyAppDev.Blazor.AutoComplete.Utilities;
using System.Linq.Expressions;

namespace AutoComplete.Tests;

/// <summary>
/// Unit tests for utility classes (ExpressionCompiler and FieldAccessor)
/// </summary>
public class UtilityTests
{
    #region ExpressionCompiler Tests

    [Fact]
    public void ExpressionCompiler_CompileOrNull_WithNullExpression_ReturnsNull()
    {
        // Arrange
        Expression<Func<Product, string>>? expr = null;

        // Act
        var result = ExpressionCompiler.CompileOrNull(expr);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExpressionCompiler_CompileOrNull_WithValidExpression_ReturnsCompiledFunc()
    {
        // Arrange
        Expression<Func<Product, string>> expr = p => p.Name;

        // Act
        var result = ExpressionCompiler.CompileOrNull(expr);

        // Assert
        result.Should().NotBeNull();
        result!(new Product { Name = "Test Product" }).Should().Be("Test Product");
    }

    [Fact]
    public void ExpressionCompiler_CompileFieldsOrNull_WithNullExpression_ReturnsNull()
    {
        // Arrange
        Expression<Func<Product, string[]>>? expr = null;

        // Act
        var result = ExpressionCompiler.CompileFieldsOrNull(expr);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExpressionCompiler_CompileFieldsOrNull_WithValidExpression_ReturnsCompiledFunc()
    {
        // Arrange
        Expression<Func<Product, string[]>> expr = p => new[] { p.Name, p.Category };

        // Act
        var result = ExpressionCompiler.CompileFieldsOrNull(expr);

        // Assert
        result.Should().NotBeNull();
        var product = new Product { Name = "iPhone", Category = "Electronics" };
        var fields = result!(product);
        fields.Should().HaveCount(2);
        fields[0].Should().Be("iPhone");
        fields[1].Should().Be("Electronics");
    }

    [Fact]
    public void ExpressionCompiler_BuildTextSelector_PrefersSearchFields()
    {
        // Arrange
        Expression<Func<Product, string[]>> searchFields = p => new[] { p.Name, p.Category };
        Expression<Func<Product, string>> textField = p => p.Name;

        // Act
        var selector = ExpressionCompiler.BuildTextSelector(searchFields, textField);
        var product = new Product { Name = "Apple iPhone", Category = "Electronics" };
        var result = selector(product);

        // Assert
        result.Should().Contain("Apple iPhone");
        result.Should().Contain("Electronics");
    }

    [Fact]
    public void ExpressionCompiler_BuildTextSelector_FallsBackToTextField()
    {
        // Arrange
        Expression<Func<Product, string[]>>? searchFields = null;
        Expression<Func<Product, string>> textField = p => p.Name;

        // Act
        var selector = ExpressionCompiler.BuildTextSelector(searchFields, textField);
        var product = new Product { Name = "Samsung Galaxy" };
        var result = selector(product);

        // Assert
        result.Should().Be("Samsung Galaxy");
    }

    [Fact]
    public void ExpressionCompiler_BuildTextSelector_FallsBackToToString()
    {
        // Arrange
        Expression<Func<Product, string[]>>? searchFields = null;
        Expression<Func<Product, string>>? textField = null;

        // Act
        var selector = ExpressionCompiler.BuildTextSelector(searchFields, textField);
        var product = new Product { Id = 123, Name = "Test" };
        var result = selector(product);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ExpressionCompiler_BuildTextSelector_SkipsNullOrEmptyFields()
    {
        // Arrange
        Expression<Func<Product, string[]>> searchFields = p => new[] { p.Name, p.Category, "" };

        // Act
        var selector = ExpressionCompiler.BuildTextSelector(searchFields, null);
        var product = new Product { Name = "Apple", Category = null! };
        var result = selector(product);

        // Assert
        result.Should().Be("Apple");
    }

    #endregion

    #region FieldAccessor Tests

    [Fact]
    public void FieldAccessor_GetText_WithNullItem_ReturnsDefault()
    {
        // Arrange
        Product? product = null;

        // Act
        var result = FieldAccessor.GetText(product, p => p.Name, "N/A");

        // Assert
        result.Should().Be("N/A");
    }

    [Fact]
    public void FieldAccessor_GetText_WithNullAccessor_ReturnsDefault()
    {
        // Arrange
        var product = new Product { Name = "Test" };

        // Act
        var result = FieldAccessor.GetText(product, (Func<Product, string>?)null, "Default");

        // Assert
        result.Should().Be("Default");
    }

    [Fact]
    public void FieldAccessor_GetText_WithValidItemAndAccessor_ReturnsValue()
    {
        // Arrange
        var product = new Product { Name = "iPhone" };

        // Act
        var result = FieldAccessor.GetText(product, p => p.Name);

        // Assert
        result.Should().Be("iPhone");
    }

    [Fact]
    public void FieldAccessor_GetText_WithAccessorReturningNull_ReturnsDefault()
    {
        // Arrange
        var product = new Product { Name = "Test", Category = null! };

        // Act
        var result = FieldAccessor.GetText(product, p => p.Category, "No Category");

        // Assert
        result.Should().Be("No Category");
    }

    [Fact]
    public void FieldAccessor_GetOptionalText_WithNullItem_ReturnsNull()
    {
        // Arrange
        Product? product = null;

        // Act
        var result = FieldAccessor.GetOptionalText(product, p => p.Name);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FieldAccessor_GetOptionalText_WithNullAccessor_ReturnsNull()
    {
        // Arrange
        var product = new Product { Name = "Test" };

        // Act
        var result = FieldAccessor.GetOptionalText(product, (Func<Product, string>?)null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FieldAccessor_GetOptionalText_WithValidItemAndAccessor_ReturnsValue()
    {
        // Arrange
        var product = new Product { Category = "Electronics" };

        // Act
        var result = FieldAccessor.GetOptionalText(product, p => p.Category);

        // Assert
        result.Should().Be("Electronics");
    }

    [Fact]
    public void FieldAccessor_GetTextWithToStringFallback_WithNullItem_ReturnsEmpty()
    {
        // Arrange
        Product? product = null;

        // Act
        var result = FieldAccessor.GetTextWithToStringFallback(product, p => p.Name);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FieldAccessor_GetTextWithToStringFallback_WithNullAccessor_UsesToString()
    {
        // Arrange
        var product = new Product { Id = 42, Name = "Test" };

        // Act
        var result = FieldAccessor.GetTextWithToStringFallback(product, null);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FieldAccessor_GetTextWithToStringFallback_WithValidAccessor_ReturnsValue()
    {
        // Arrange
        var product = new Product { Name = "MacBook Pro" };

        // Act
        var result = FieldAccessor.GetTextWithToStringFallback(product, p => p.Name);

        // Assert
        result.Should().Be("MacBook Pro");
    }

    [Fact]
    public void FieldAccessor_GetTextWithToStringFallback_WithAccessorReturningNull_ReturnsEmpty()
    {
        // Arrange
        var product = new Product { Name = "Test", Category = null! };

        // Act
        var result = FieldAccessor.GetTextWithToStringFallback(product, p => p.Category);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
