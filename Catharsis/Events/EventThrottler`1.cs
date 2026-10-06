namespace Catharsis.Events;

///<summary>
///Limits how often an action runs: the first call in a window runs immediately and further calls inside the window are
///dropped. This complements <see cref="Catharsis.Concurrency.TaskDebouncer"/>, which instead waits for calls to go
///quiet before running once.
///</summary>
///<typeparam name="T">The type of the value passed to the action.</typeparam>
public sealed class EventThrottler<T>
{
    #region Fields
    readonly TimeSpan _interval;
    readonly Action<T> _action;
    readonly TimeProvider _timeProvider;
    readonly Lock _gate = new();
    long? _lastRun;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a throttler.
    ///</summary>
    ///<param name="interval">The minimum time between two runs of <paramref name="action"/>.</param>
    ///<param name="action">The action to run.</param>
    ///<param name="timeProvider">The clock to measure with. Defaults to <see cref="TimeProvider.System"/>.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="interval"/> is negative.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public EventThrottler(TimeSpan interval, Action<T> action, TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(interval, TimeSpan.Zero);
        ArgumentNullException.ThrowIfNull(action);

        _interval = interval;
        _action = action;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Runs the action with <paramref name="value"/> unless it already ran within the interval.
    ///</summary>
    ///<param name="value">The value to pass to the action.</param>
    ///<returns><c>true</c> if the action ran; <c>false</c> if the call was throttled.</returns>
    public bool TryInvoke(T value)
    {
        lock (_gate)
        {
            long now = _timeProvider.GetTimestamp();

            if (_lastRun is long last && _timeProvider.GetElapsedTime(last, now) < _interval)
            {
                return false;
            }

            _lastRun = now;
        }

        _action(value);
        return true;
    }

    ///<summary>
    ///Forgets the last run so the next call runs immediately.
    ///</summary>
    public void Reset()
    {
        lock (_gate)
        {
            _lastRun = null;
        }
    }
    #endregion
}
