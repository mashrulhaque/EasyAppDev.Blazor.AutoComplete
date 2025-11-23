namespace EasyAppDev.Blazor.AutoComplete.DataSources;

/// <summary>
/// A remote data source implementation that uses a custom async function to fetch data.
/// Useful for fetching data from APIs, databases, or other remote sources.
/// </summary>
/// <typeparam name="TItem">The type of items to retrieve</typeparam>
public class RemoteDataSource<TItem> : IAutoCompleteDataSource<TItem>
{
    private readonly Func<string, CancellationToken, Task<IEnumerable<TItem>>> _searchFunc;

    /// <summary>
    /// Initializes a new instance of the RemoteDataSource.
    /// </summary>
    /// <param name="searchFunc">The async function to execute when searching</param>
    /// <exception cref="ArgumentNullException">Thrown when searchFunc is null</exception>
    public RemoteDataSource(Func<string, CancellationToken, Task<IEnumerable<TItem>>> searchFunc)
    {
        _searchFunc = searchFunc ?? throw new ArgumentNullException(nameof(searchFunc));
    }

    /// <inheritdoc />
    public Task<IEnumerable<TItem>> SearchAsync(string searchText, CancellationToken cancellationToken)
        => _searchFunc(searchText, cancellationToken);
}

/// <summary>
/// A local data source implementation that filters data in-memory.
/// Useful for small to medium datasets that can be loaded into memory.
/// </summary>
/// <typeparam name="TItem">The type of items to retrieve</typeparam>
public class LocalDataSource<TItem> : IAutoCompleteDataSource<TItem>
{
    private readonly IEnumerable<TItem> _items;
    private readonly Func<TItem, string, bool> _filterPredicate;

    /// <summary>
    /// Initializes a new instance of the LocalDataSource.
    /// </summary>
    /// <param name="items">The items to search through</param>
    /// <param name="filterPredicate">The predicate to filter items (item, searchText) => bool</param>
    /// <exception cref="ArgumentNullException">Thrown when items or filterPredicate is null</exception>
    public LocalDataSource(
        IEnumerable<TItem> items,
        Func<TItem, string, bool> filterPredicate)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
        _filterPredicate = filterPredicate ?? throw new ArgumentNullException(nameof(filterPredicate));
    }

    /// <inheritdoc />
    public Task<IEnumerable<TItem>> SearchAsync(string searchText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            return Task.FromResult(_items);
        }

        var filtered = _items.Where(item => _filterPredicate(item, searchText));
        return Task.FromResult(filtered);
    }
}
