namespace EasyAppDev.Blazor.AutoComplete.Grouping;

/// <summary>
/// Represents a group of items with a common key.
/// </summary>
/// <typeparam name="TItem">The type of items in the group</typeparam>
/// <typeparam name="TKey">The type of the grouping key</typeparam>
public class GroupDescriptor<TItem, TKey>
{
    /// <summary>
    /// Gets or initializes the grouping key.
    /// </summary>
    public required TKey Key { get; init; }

    /// <summary>
    /// Gets or initializes the list of items in this group.
    /// </summary>
    public required List<TItem> Items { get; init; }

    /// <summary>
    /// Gets the number of items in this group.
    /// </summary>
    public int Count => Items.Count;
}
