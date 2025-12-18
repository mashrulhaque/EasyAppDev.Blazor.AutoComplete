using EasyAppDev.Blazor.AutoComplete.Accessibility;
using EasyAppDev.Blazor.AutoComplete.Input;
using EasyAppDev.Blazor.AutoComplete.Utilities;

namespace EasyAppDev.Blazor.AutoComplete.Services;

/// <summary>
/// Default implementation of the AutoComplete service factory.
/// Creates standard service instances for component operation.
/// </summary>
public class AutoCompleteServiceFactory : IAutoCompleteServiceFactory
{
    /// <inheritdoc />
    public IInputHandler<TItem> CreateInputHandler<TItem>(int maxSearchLength)
    {
        return new InputHandler<TItem>(maxSearchLength);
    }

    /// <inheritdoc />
    public KeyboardNavigationHandler<TItem> CreateKeyboardHandler<TItem>(List<TItem> items)
    {
        return new KeyboardNavigationHandler<TItem>(items);
    }

    /// <inheritdoc />
    public IDebouncer? CreateDebouncer(int intervalMs)
    {
        if (intervalMs <= 0)
        {
            return null;
        }

        return new AsyncDebouncer(intervalMs);
    }
}
