using System.Linq.Expressions;

namespace EasyAppDev.Blazor.AutoComplete.Utilities;

/// <summary>
/// Utility for safely compiling LINQ expressions with null handling.
/// </summary>
public static class ExpressionCompiler
{
    /// <summary>
    /// Compiles an expression to a delegate if not null, otherwise returns null.
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="expression">The expression to compile, or null</param>
    /// <returns>The compiled delegate, or null if the expression was null</returns>
    public static Func<TSource, TResult>? CompileOrNull<TSource, TResult>(
        Expression<Func<TSource, TResult>>? expression)
    {
        return expression?.Compile();
    }

    /// <summary>
    /// Compiles multiple field expressions for multi-field search.
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <param name="fieldsExpression">The expression that returns an array of strings</param>
    /// <returns>The compiled delegate, or null if the expression was null</returns>
    public static Func<TSource, string[]>? CompileFieldsOrNull<TSource>(
        Expression<Func<TSource, string[]>>? fieldsExpression)
    {
        return fieldsExpression?.Compile();
    }

    /// <summary>
    /// Builds a text selector from either SearchFields or TextField.
    /// Returns a function that concatenates all search fields into a single string.
    /// </summary>
    /// <typeparam name="TItem">The item type</typeparam>
    /// <param name="searchFields">Expression for multiple search fields</param>
    /// <param name="textField">Expression for a single text field</param>
    /// <returns>A function that extracts searchable text from an item</returns>
    public static Func<TItem, string> BuildTextSelector<TItem>(
        Expression<Func<TItem, string[]>>? searchFields,
        Expression<Func<TItem, string>>? textField)
    {
        if (searchFields != null)
        {
            var compiled = searchFields.Compile();
            return item =>
            {
                var fields = compiled(item);
                return string.Join(" ", fields.Where(f => !string.IsNullOrWhiteSpace(f)));
            };
        }

        if (textField != null)
        {
            return textField.Compile();
        }

        // Fallback to ToString()
        return item => item?.ToString() ?? string.Empty;
    }
}
