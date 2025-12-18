using EasyAppDev.Blazor.AutoComplete.Accessibility;
using EasyAppDev.Blazor.AutoComplete.Input;
using EasyAppDev.Blazor.AutoComplete.Utilities;

namespace EasyAppDev.Blazor.AutoComplete.Services;

/// <summary>
/// Factory interface for creating internal AutoComplete services.
/// Enables dependency injection and mocking for unit testing.
/// </summary>
/// <remarks>
/// This factory creates short-lived services that are specific to each component instance.
/// For singleton services like IThemeManager, inject them directly.
/// </remarks>
public interface IAutoCompleteServiceFactory
{
    /// <summary>
    /// Creates an input handler for managing search text and input state.
    /// </summary>
    /// <typeparam name="TItem">The type of items in the autocomplete list.</typeparam>
    /// <param name="maxSearchLength">Maximum allowed search length.</param>
    /// <returns>A new input handler instance.</returns>
    IInputHandler<TItem> CreateInputHandler<TItem>(int maxSearchLength);

    /// <summary>
    /// Creates a keyboard navigation handler for managing selection state.
    /// </summary>
    /// <typeparam name="TItem">The type of items in the autocomplete list.</typeparam>
    /// <param name="items">The initial list of items to navigate.</param>
    /// <returns>A new keyboard navigation handler instance.</returns>
    KeyboardNavigationHandler<TItem> CreateKeyboardHandler<TItem>(List<TItem> items);

    /// <summary>
    /// Creates a debounce timer for throttling filter operations.
    /// </summary>
    /// <param name="intervalMs">The debounce interval in milliseconds.</param>
    /// <returns>A new debounce timer instance, or null if debouncing is disabled.</returns>
    IDebouncer? CreateDebouncer(int intervalMs);
}

/// <summary>
/// Interface for debouncing operations with async support.
/// </summary>
public interface IDebouncer : IDisposable
{
    /// <summary>
    /// Debounces the specified action.
    /// </summary>
    /// <param name="action">The action to debounce.</param>
    void Debounce(Action action);

    /// <summary>
    /// Debounces the specified async action.
    /// </summary>
    /// <param name="action">The async action to debounce.</param>
    void DebounceAsync(Func<Task> action);

    /// <summary>
    /// Cancels any pending debounced action.
    /// </summary>
    void Cancel();
}
