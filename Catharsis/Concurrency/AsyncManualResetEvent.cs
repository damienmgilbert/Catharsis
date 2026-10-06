namespace Catharsis.Concurrency;

///<summary>
///An async-friendly manual-reset event, analogous to <see cref="ManualResetEventSlim"/> but awaitable without blocking
///a thread. Once <see cref="Set"/> is called, every current and future waiter is released until ///<see cref="Reset"/>
///is called.
///</summary>
public sealed class AsyncManualResetEvent
{
    #region Fields
    private volatile TaskCompletionSource _tcs;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates an event in the specified initial state.
    ///</summary>
    ///<param name="initialState"><c>true</c> to start signaled; otherwise <c>false</c>.</param>
    public AsyncManualResetEvent(bool initialState = false)
    {
        _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        if(initialState)
        {
            _tcs.TrySetResult();
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Resets the event so that subsequent waiters block until <see cref="Set"/> is called again.
    ///</summary>
    public void Reset()
    {
        while(true)
        {
            TaskCompletionSource current = _tcs;

            if(!current.Task.IsCompleted)
            {
                return;
            }

            if(Interlocked.CompareExchange(ref _tcs, new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously), current) == current)
            {
                return;
            }
        }
    }

    ///<summary>
    ///Sets the event, releasing all current and future waiters until the event is reset.
    ///</summary>
    public void Set() => _tcs.TrySetResult();

    ///<summary>
    ///Synchronously blocks until the event is set.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token that can abandon the wait.</param>
    public void Wait(CancellationToken cancellationToken = default) => _tcs.Task.Wait(cancellationToken);

    ///<summary>
    ///Asynchronously waits until the event is set.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token that can abandon the wait.</param>
    ///<returns>A task that completes once the event is set.</returns>
    public Task WaitAsync(CancellationToken cancellationToken = default) => _tcs.Task.WaitAsync(cancellationToken);
    #endregion

    #region Public properties
    ///<summary>
    ///Gets whether the event is currently set.
    ///</summary>
    public bool IsSet => _tcs.Task.IsCompleted;
    #endregion
}
