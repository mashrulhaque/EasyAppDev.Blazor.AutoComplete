using System.Linq.Expressions;

namespace EasyAppDev.Blazor.AutoComplete.Grouping;

/// <summary>
/// Provides grouping functionality for autocomplete items.
/// </summary>
/// <typeparam name="TItem">The type of items to group</typeparam>
/// <typeparam name="TKey">The type of the grouping key</typeparam>
public class GroupingEngine<TItem, TKey> where TKey : notnull
{
    private readonly Func<TItem, TKey> _keySelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupingEngine{TItem, TKey}"/> class.
    /// </summary>
    /// <param name="groupByExpression">Expression to extract the grouping key from each item</param>
    public GroupingEngine(Expression<Func<TItem, TKey>> groupByExpression)
    {
        _keySelector = groupByExpression.Compile();
    }

    /// <summary>
    /// Groups the items by the specified key.
    /// </summary>
    /// <param name="items">The items to group</param>
    /// <returns>List of grouped items</returns>
    public List<GroupDescriptor<TItem, TKey>> GroupItems(IEnumerable<TItem> items)
    {
        return items
            .GroupBy(_keySelector)
            .Select(g => new GroupDescriptor<TItem, TKey>
            {
                Key = g.Key,
                Items = g.ToList()
            })
            .ToList();
    }
}
