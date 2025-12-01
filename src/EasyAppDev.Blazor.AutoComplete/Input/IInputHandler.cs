namespace EasyAppDev.Blazor.AutoComplete.Input;

/// <summary>
/// Interface for handling input operations in the AutoComplete component.
/// Manages search text changes, clearing, and input security.
/// </summary>
/// <typeparam name="TItem">The type of items in the autocomplete list.</typeparam>
public interface IInputHandler<TItem>
{
    /// <summary>
    /// Gets the current search text.
    /// </summary>
    string SearchText { get; }

    /// <summary>
    /// Gets or sets whether the dropdown is currently open.
    /// </summary>
    bool IsDropdownOpen { get; set; }

    /// <summary>
    /// Gets or sets whether a search operation is in progress.
    /// </summary>
    bool IsLoading { get; set; }

    /// <summary>
    /// Gets the maximum allowed search length after security enforcement.
    /// </summary>
    int EffectiveMaxSearchLength { get; }

    /// <summary>
    /// Updates the search text with security validation.
    /// Returns true if the text was modified (truncated for security).
    /// </summary>
    /// <param name="text">The new search text.</param>
    /// <returns>True if text was truncated, false otherwise.</returns>
    bool SetSearchText(string text);

    /// <summary>
    /// Clears the search text and resets input state.
    /// </summary>
    void Clear();

    /// <summary>
    /// Determines if search should be triggered based on minimum length requirements.
    /// </summary>
    /// <param name="minSearchLength">Minimum characters required to trigger search.</param>
    /// <returns>True if search should be triggered.</returns>
    bool ShouldTriggerSearch(int minSearchLength);

    /// <summary>
    /// Gets the display text for a selected item.
    /// </summary>
    /// <param name="item">The selected item.</param>
    /// <param name="textAccessor">Function to extract text from item.</param>
    /// <returns>Display text for the item.</returns>
    string GetDisplayText(TItem? item, Func<TItem, string> textAccessor);
}
