namespace Catharsis.Scheduling;

///<summary>
///Invokes an async callback on a fixed period using <see cref="PeriodicTimer"/>, running the callback loop on a
///background task until disposed. If the callback throws, the loop stops; observe <see cref="Completion"/> to be
///notified of that failure without waiting for disposal.
///</summary>
public sealed class RecurringTimer : IAsyncDisposable
{
    #region Fields
    private bool _disposed;
    private readonly Task _loop;
    private readonly CancellationTokenSource _stop = new();
    private readonly PeriodicTimer _timer;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates and starts a recurring timer with the specified period and callback.
    ///</summary>
    ///<param name="period">The interval between callback invocations. Must be greater than zero.</param>
    ///<param name="callback">The async callback invoked on each tick. Receives a <see cref="CancellationToken"/> canceled on disposal.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="period"/> is not greater than zero.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="callback"/> is <c>null</c>.</exception>
    public RecurringTimer(TimeSpan period, Func<CancellationToken, Task> callback)
    {
        if(period <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(period), "Period must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(callback);

        _timer = new PeriodicTimer(period);
        _loop = RunAsync(callback, _stop.Token);
    }
    #endregion

    #region Private methods
    private async Task RunAsync(Func<CancellationToken, Task> callback, CancellationToken cancellationToken)
    {
        while(await _timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            await callback(cancellationToken).ConfigureAwait(false);
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Stops the timer, cancels the running callback (if any), and waits for the background loop to complete. Disposal
    ///never throws; a callback failure remains observable via <see cref="Completion"/>.
    ///</summary>
    public async ValueTask DisposeAsync()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;

        await _stop.CancelAsync().ConfigureAwait(false);
        _timer.Dispose();

        try
        {
            await _loop.ConfigureAwait(false);
        } catch
        {
            // A canceled or failed callback is observable via Completion; disposal itself must not throw.
        }

        _stop.Dispose();
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a task that completes when the callback loop stops, either because it was disposed or because the callback
    ///threw. Awaiting this surfaces a callback failure without needing to dispose first.
    ///</summary>
    public Task Completion => _loop;
    #endregion
}
