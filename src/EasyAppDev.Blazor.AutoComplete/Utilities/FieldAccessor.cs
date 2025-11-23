namespace EasyAppDev.Blazor.AutoComplete.Utilities;

/// <summary>
/// Helper for safely accessing item fields with null handling.
/// </summary>
public static class FieldAccessor
{
    /// <summary>
    /// Gets text from item using accessor, with null safety and fallback.
    /// </summary>
    /// <typeparam name="TItem">The item type</typeparam>
    /// <param name="item">The item to extract text from</param>
    /// <param name="accessor">The function to extract text from the item</param>
    /// <param name="defaultValue">The default value to return if item or accessor is null</param>
    /// <returns>The extracted text, or the default value if item or accessor is null</returns>
    public static string GetText<TItem>(
        TItem? item,
        Func<TItem, string>? accessor,
        string defaultValue = "")
    {
        if (item == null || accessor == null)
        {
            return defaultValue;
        }

        return accessor(item) ?? defaultValue;
    }

    /// <summary>
    /// Gets optional text from item, returns null if item or accessor is null.
    /// </summary>
    /// <typeparam name="TItem">The item type</typeparam>
    /// <param name="item">The item to extract text from</param>
    /// <param name="accessor">The function to extract text from the item</param>
    /// <returns>The extracted text, or null if item or accessor is null</returns>
    public static string? GetOptionalText<TItem>(
        TItem? item,
        Func<TItem, string>? accessor)
    {
        if (item == null || accessor == null)
        {
            return null;
        }

        return accessor(item);
    }

    /// <summary>
    /// Gets text from item, with fallback to ToString() if accessor is null.
    /// </summary>
    /// <typeparam name="TItem">The item type</typeparam>
    /// <param name="item">The item to extract text from</param>
    /// <param name="accessor">The function to extract text from the item</param>
    /// <returns>The extracted text, or the result of ToString() if accessor is null, or empty string if item is null</returns>
    public static string GetTextWithToStringFallback<TItem>(
        TItem? item,
        Func<TItem, string>? accessor)
    {
        if (item == null)
        {
            return string.Empty;
        }

        if (accessor != null)
        {
            return accessor(item) ?? string.Empty;
        }

        return item.ToString() ?? string.Empty;
    }
}
