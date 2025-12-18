using EasyAppDev.Blazor.AutoComplete.Services;

namespace EasyAppDev.Blazor.AutoComplete.Utilities;

/// <summary>
/// Async-friendly debouncer using Task.Delay for better Blazor compatibility.
/// Replaces System.Timers.Timer-based approach with cancellation token pattern.
/// </summary>
public sealed class AsyncDebouncer : IDebouncer
{
    private readonly int _intervalMs;
    private CancellationTokenSource? _cts;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncDebouncer"/> class.
    /// </summary>
    /// <param name="intervalMs">The debounce interval in milliseconds.</param>
    public AsyncDebouncer(int intervalMs = AutoCompleteConstants.DefaultDebounceMs)
    {
        _intervalMs = intervalMs;
    }

    /// <summary>
    /// Debounces the specified synchronous action.
    /// </summary>
    /// <param name="action">The action to debounce.</param>
    public void Debounce(Action action)
    {
        DebounceAsync(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Debounces the specified async action.
    /// </summary>
    /// <param name="action">The async action to debounce.</param>
    public void DebounceAsync(Func<Task> action)
    {
        CancellationToken token;

        lock (_lock)
        {
            // Cancel any pending operation
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            token = _cts.Token;
        }

        // Fire and forget the debounced operation
        _ = ExecuteAfterDelayAsync(action, token);
    }

    /// <summary>
    /// Cancels any pending debounced action.
    /// </summary>
    public void Cancel()
    {
        lock (_lock)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// Executes the action after the debounce delay.
    /// </summary>
    private async Task ExecuteAfterDelayAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(_intervalMs, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                await action();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelled - silently ignore
        }
        catch (ObjectDisposedException)
        {
            // CTS was disposed - silently ignore
        }
    }

    /// <summary>
    /// Disposes the debouncer and cancels any pending operations.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}
