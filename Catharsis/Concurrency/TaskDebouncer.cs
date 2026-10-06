namespace Catharsis.Concurrency;

///<summary>
///Coalesces rapid, repeated trigger calls into a single execution of an operation: each call to ///<see
///cref="Trigger"/> restarts a delay window, and the operation only runs once the window elapses without a further
///trigger. Useful for search-as-you-type, save-on-idle, and similar debounced-action scenarios.
///</summary>
public sealed class TaskDebouncer : IDisposable
{
    #region Fields
    private readonly Func<CancellationToken, Task> _action;
    private readonly TimeSpan _delay;
    private bool _disposed;
    private readonly Lock _gate = new();
    private CancellationTokenSource? _pending;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a debouncer with the specified delay and action.
    ///</summary>
    ///<param name="delay">How long to wait after the last trigger before running the action.</param>
    ///<param name="action">The action to run once the delay elapses without a further trigger.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> is negative.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public TaskDebouncer(TimeSpan delay, Func<CancellationToken, Task> action)
    {
        if(delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), "Delay must not be negative.");
        }

        ArgumentNullException.ThrowIfNull(action);

        _delay = delay;
        _action = action;
    }
    #endregion

    #region Private methods
    private async Task RunAfterDelayAsync(CancellationTokenSource cts)
    {
        try
        {
            await Task.Delay(_delay, cts.Token).ConfigureAwait(false);
            await _action(cts.Token).ConfigureAwait(false);
        } catch(OperationCanceledException)
        {
            // Superseded by a later trigger; nothing to do.
        } finally
        {
            lock(_gate)
            {
                if(_pending == cts)
                {
                    _pending = null;
                }
            }

            cts.Dispose();
        }
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;

        CancellationTokenSource? pending;

        lock(_gate)
        {
            pending = _pending;
            _pending = null;
        }

        pending?.Cancel();
        pending?.Dispose();
    }

    ///<summary>
    ///Restarts the delay window, canceling any run scheduled by a previous trigger that has not yet started.
    ///</summary>
    ///<exception cref="ObjectDisposedException">The debouncer has already been disposed.</exception>
    public void Trigger()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        CancellationTokenSource cts = new();

        lock(_gate)
        {
            _pending?.Cancel();
            _pending?.Dispose();
            _pending = cts;
        }

        _ = RunAfterDelayAsync(cts);
    }
    #endregion
}
