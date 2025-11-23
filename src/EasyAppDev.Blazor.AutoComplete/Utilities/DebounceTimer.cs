using System.Timers;

namespace EasyAppDev.Blazor.AutoComplete.Utilities;

/// <summary>
/// Provides debouncing functionality for search input.
/// Delays execution of an action until a specified time has passed without new calls.
/// </summary>
public sealed class DebounceTimer : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private Action? _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebounceTimer"/> class.
    /// </summary>
    /// <param name="intervalMs">The debounce interval in milliseconds. Default is 300ms.</param>
    public DebounceTimer(int intervalMs = 300)
    {
        _timer = new System.Timers.Timer(intervalMs);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = false;
    }

    /// <summary>
    /// Debounces the specified action. The action will only execute after the interval
    /// has passed without any new calls to Debounce.
    /// </summary>
    /// <param name="action">The action to debounce.</param>
    public void Debounce(Action action)
    {
        _action = action;
        _timer.Stop();
        _timer.Start();
    }

    /// <summary>
    /// Cancels any pending debounced action.
    /// </summary>
    public void Cancel()
    {
        _timer.Stop();
        _action = null;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _action?.Invoke();
    }

    /// <summary>
    /// Disposes the timer and releases resources.
    /// </summary>
    public void Dispose()
    {
        _timer.Dispose();
    }
}
