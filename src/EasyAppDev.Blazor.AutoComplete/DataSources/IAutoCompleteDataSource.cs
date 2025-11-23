namespace EasyAppDev.Blazor.AutoComplete.DataSources;

/// <summary>
/// Represents an asynchronous data source for the AutoComplete component.
/// Implement this interface to provide remote or async data fetching.
/// </summary>
/// <typeparam name="TItem">The type of items to retrieve</typeparam>
public interface IAutoCompleteDataSource<TItem>
{
    /// <summary>
    /// Asynchronously searches for items matching the search text.
    /// </summary>
    /// <param name="searchText">The text to search for</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation, containing the matching items</returns>
    Task<IEnumerable<TItem>> SearchAsync(string searchText, CancellationToken cancellationToken = default);
}
