using Catharsis.Concurrency;

namespace Catharsis.Events;

///<summary>
///Coalesces a burst of values into one call: each <see cref="Post"/> restarts a quiet-period timer, and once the timer
///elapses without a further post the action runs once with the <em>latest</em> value. It is the value-carrying form of
public sealed class EventDebouncer<T> : IDisposable
{
    #region Fields
    private readonly Func<T, CancellationToken, Task> _action;
    private readonly TaskDebouncer _debouncer;
    private readonly Lock _gate = new();
    private T? _latest;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a debouncer with an asynchronous action.
    ///</summary>
    ///<param name="delay">The quiet period that must pass after the last post before the action runs.</param>
    ///<param name="action">The action to run with the latest posted value.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> is negative.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public EventDebouncer(TimeSpan delay, Func<T, CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        _action = action;
        _debouncer = new TaskDebouncer(delay, RunAsync);
    }

    ///<summary>
    ///Creates a debouncer with a synchronous action.
    ///</summary>
    ///<param name="delay">The quiet period that must pass after the last post before the action runs.</param>
    ///<param name="action">The action to run with the latest posted value.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> is negative.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public EventDebouncer(TimeSpan delay, Action<T> action) : this(delay, WrapAction(action))
    {
    }
    #endregion

    #region Private methods
    private Task RunAsync(CancellationToken cancellationToken)
    {
        T value;

        lock(_gate)
        {
            value = _latest!;
        }

        return _action(value, cancellationToken);
    }

    private static Func<T, CancellationToken, Task> WrapAction(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return(value, _) =>
        {
            action(value);
            return Task.CompletedTask;
        };
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Cancels any pending run and stops accepting posts.
    ///</summary>
    public void Dispose() => _debouncer.Dispose();

        ///<summary>
///Records <paramref name="value"/> as the latest and restarts the quiet period.
///</summary>
    ///<param name="value">The value to deliver once posting goes quiet.</param>
    ///<exception cref="ObjectDisposedException">The debouncer has been disposed.</exception>
    public void Post(T value)
    {
        lock(_gate)
        {
            _latest = value;
        }

        _debouncer.Trigger();
    }
    #endregion
}
