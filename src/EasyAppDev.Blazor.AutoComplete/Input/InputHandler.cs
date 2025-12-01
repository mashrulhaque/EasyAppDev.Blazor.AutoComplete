namespace EasyAppDev.Blazor.AutoComplete.Input;

/// <summary>
/// Handles input operations for the AutoComplete component.
/// Manages search text changes, clearing, and input security.
/// </summary>
/// <typeparam name="TItem">The type of items in the autocomplete list.</typeparam>
public class InputHandler<TItem> : IInputHandler<TItem>
{
    private const int AbsoluteMaxSearchLength = 2000;

    private readonly int _maxSearchLength;
    private string _searchText = string.Empty;

    /// <summary>
    /// Initializes a new instance of InputHandler with the specified max search length.
    /// </summary>
    /// <param name="maxSearchLength">Maximum allowed search length (will be capped at 2000).</param>
    public InputHandler(int maxSearchLength = 500)
    {
        _maxSearchLength = Math.Min(maxSearchLength, AbsoluteMaxSearchLength);
    }

    /// <inheritdoc />
    public string SearchText => _searchText;

    /// <inheritdoc />
    public bool IsDropdownOpen { get; set; }

    /// <inheritdoc />
    public bool IsLoading { get; set; }

    /// <inheritdoc />
    public int EffectiveMaxSearchLength => _maxSearchLength;

    /// <inheritdoc />
    public bool SetSearchText(string text)
    {
        var wasTruncated = false;

        if (text.Length > _maxSearchLength)
        {
            text = text.Substring(0, _maxSearchLength);
            wasTruncated = true;
        }

        _searchText = text;
        return wasTruncated;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _searchText = string.Empty;
        IsDropdownOpen = false;
        IsLoading = false;
    }

    /// <inheritdoc />
    public bool ShouldTriggerSearch(int minSearchLength)
    {
        return _searchText.Length >= minSearchLength;
    }

    /// <inheritdoc />
    public string GetDisplayText(TItem? item, Func<TItem, string> textAccessor)
    {
        if (item == null)
        {
            return string.Empty;
        }

        return textAccessor(item) ?? string.Empty;
    }

    /// <summary>
    /// Updates the max search length. Used when configuration changes.
    /// </summary>
    /// <param name="maxSearchLength">New maximum search length.</param>
    /// <returns>A new InputHandler with the updated setting.</returns>
    public InputHandler<TItem> WithMaxSearchLength(int maxSearchLength)
    {
        return new InputHandler<TItem>(maxSearchLength)
        {
            _searchText = this._searchText,
            IsDropdownOpen = this.IsDropdownOpen,
            IsLoading = this.IsLoading
        };
    }
}
