using Microsoft.AspNetCore.Components.Web;

namespace EasyAppDev.Blazor.AutoComplete.Accessibility;

/// <summary>
/// Handles keyboard navigation for the AutoComplete component.
/// Implements ARIA 1.2 combobox keyboard interaction patterns.
/// </summary>
/// <typeparam name="TItem">The type of items in the autocomplete list</typeparam>
public class KeyboardNavigationHandler<TItem>
{
    private readonly List<TItem> _items;
    private int _selectedIndex = -1;
    private bool _isOpen;

    /// <summary>
    /// Gets or sets the currently selected index in the list.
    /// </summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => _selectedIndex = value;
    }

    /// <summary>
    /// Gets or sets whether the dropdown is open.
    /// </summary>
    public bool IsOpen
    {
        get => _isOpen;
        set => _isOpen = value;
    }

    /// <summary>
    /// Gets the currently selected item, or default if no item is selected.
    /// </summary>
    public TItem? SelectedItem => _selectedIndex >= 0 && _selectedIndex < _items.Count
        ? _items[_selectedIndex]
        : default;

    /// <summary>
    /// Initializes a new instance of the KeyboardNavigationHandler.
    /// </summary>
    /// <param name="items">The list of items to navigate through</param>
    public KeyboardNavigationHandler(List<TItem> items)
    {
        _items = items;
    }

    /// <summary>
    /// Updates the items list (call when filtered items change).
    /// </summary>
    /// <param name="items">The new list of items</param>
    public void UpdateItems(List<TItem> items)
    {
        // Create a temporary copy to avoid clearing the source list if it's the same reference
        var itemsCopy = items.ToList();
        _items.Clear();
        _items.AddRange(itemsCopy);

        // Reset selection if it's out of bounds
        if (_selectedIndex >= _items.Count)
        {
            _selectedIndex = -1;
        }
    }

    /// <summary>
    /// Handles keyboard events and returns the appropriate navigation action.
    /// </summary>
    /// <param name="e">The keyboard event arguments</param>
    /// <returns>The navigation action to perform</returns>
    public KeyboardNavigationAction HandleKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowDown":
                return HandleArrowDown();

            case "ArrowUp":
                return HandleArrowUp();

            case "Enter":
                return HandleEnter();

            case "Escape":
                return HandleEscape();

            case "Home":
                return HandleHome();

            case "End":
                return HandleEnd();

            case "Tab":
                return HandleTab();

            default:
                return new KeyboardNavigationAction { Type = NavigationActionType.None };
        }
    }

    private KeyboardNavigationAction HandleArrowDown()
    {
        if (!_isOpen && _items.Any())
        {
            _isOpen = true;
            _selectedIndex = 0;
            return new KeyboardNavigationAction
            {
                Type = NavigationActionType.OpenAndSelect,
                SelectedIndex = _selectedIndex,
                PreventDefault = true
            };
        }

        if (_items.Any())
        {
            _selectedIndex = Math.Min(_selectedIndex + 1, _items.Count - 1);
            return new KeyboardNavigationAction
            {
                Type = NavigationActionType.SelectNext,
                SelectedIndex = _selectedIndex,
                PreventDefault = true
            };
        }

        return new KeyboardNavigationAction { Type = NavigationActionType.None };
    }

    private KeyboardNavigationAction HandleArrowUp()
    {
        if (!_isOpen)
        {
            return new KeyboardNavigationAction { Type = NavigationActionType.None };
        }

        if (_items.Any())
        {
            _selectedIndex = Math.Max(_selectedIndex - 1, 0);
            return new KeyboardNavigationAction
            {
                Type = NavigationActionType.SelectPrevious,
                SelectedIndex = _selectedIndex,
                PreventDefault = true
            };
        }

        return new KeyboardNavigationAction { Type = NavigationActionType.None };
    }

    private KeyboardNavigationAction HandleEnter()
    {
        if (_isOpen && _selectedIndex >= 0 && _selectedIndex < _items.Count)
        {
            return new KeyboardNavigationAction
            {
                Type = NavigationActionType.ConfirmSelection,
                SelectedIndex = _selectedIndex,
                SelectedItem = _items[_selectedIndex],
                PreventDefault = true
            };
        }

        return new KeyboardNavigationAction { Type = NavigationActionType.None };
    }

    private KeyboardNavigationAction HandleEscape()
    {
        if (_isOpen)
        {
            _selectedIndex = -1;
            _isOpen = false;
            return new KeyboardNavigationAction
            {
                Type = NavigationActionType.Close,
                PreventDefault = true
            };
        }

        return new KeyboardNavigationAction { Type = NavigationActionType.None };
    }

    private KeyboardNavigationAction HandleHome()
    {
        if (_isOpen && _items.Any())
        {
            _selectedIndex = 0;
            return new KeyboardNavigationAction
            {
                Type = NavigationActionType.SelectFirst,
                SelectedIndex = _selectedIndex,
                PreventDefault = true
            };
        }

        return new KeyboardNavigationAction { Type = NavigationActionType.None };
    }

    private KeyboardNavigationAction HandleEnd()
    {
        if (_isOpen && _items.Any())
        {
            _selectedIndex = _items.Count - 1;
            return new KeyboardNavigationAction
            {
                Type = NavigationActionType.SelectLast,
                SelectedIndex = _selectedIndex,
                PreventDefault = true
            };
        }

        return new KeyboardNavigationAction { Type = NavigationActionType.None };
    }

    private KeyboardNavigationAction HandleTab()
    {
        if (_isOpen)
        {
            _isOpen = false;
            return new KeyboardNavigationAction
            {
                Type = NavigationActionType.Close,
                PreventDefault = false // Allow default tab behavior
            };
        }

        return new KeyboardNavigationAction { Type = NavigationActionType.None };
    }

    /// <summary>
    /// Resets the navigation state.
    /// </summary>
    public void Reset()
    {
        _selectedIndex = -1;
        _isOpen = false;
    }
}

/// <summary>
/// Represents the type of navigation action to perform.
/// </summary>
public enum NavigationActionType
{
    None,
    OpenAndSelect,
    SelectNext,
    SelectPrevious,
    SelectFirst,
    SelectLast,
    ConfirmSelection,
    Close
}

/// <summary>
/// Represents a keyboard navigation action.
/// </summary>
public class KeyboardNavigationAction
{
    /// <summary>
    /// The type of navigation action.
    /// </summary>
    public NavigationActionType Type { get; set; }

    /// <summary>
    /// The selected index after the action.
    /// </summary>
    public int SelectedIndex { get; set; } = -1;

    /// <summary>
    /// The selected item after the action (for ConfirmSelection).
    /// </summary>
    public object? SelectedItem { get; set; }

    /// <summary>
    /// Whether to prevent the default browser behavior.
    /// </summary>
    public bool PreventDefault { get; set; }
}
