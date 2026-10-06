namespace Catharsis.Concurrency;

///<summary>
///An async-friendly auto-reset event, analogous to <see cref="AutoResetEvent"/> but awaitable without blocking a
///thread. A single <see cref="Set"/> call releases exactly one waiter, or leaves the event signaled for the next caller
///if none are currently waiting.
///</summary>
///<param name="initialState"><c>true</c> to start signaled; otherwise <c>false</c>.</param>
public sealed class AsyncAutoResetEvent(bool initialState = false)
{
    #region Fields
    private readonly Lock _gate = new();
    private bool _signaled = initialState;
    private readonly Queue<TaskCompletionSource> _waiters = new();
    #endregion

    #region Public methods
    ///<summary>
    ///Sets the event, releasing exactly one waiter. If no waiters are queued, the signal is retained for the next
    ///caller of <see cref="WaitAsync"/>.
    ///</summary>
    public void Set()
    {
        while(true)
        {
            TaskCompletionSource? toRelease = null;

            lock(_gate)
            {
                if(_waiters.Count > 0)
                {
                    toRelease = _waiters.Dequeue();
                } else
                {
                    _signaled = true;
                    return;
                }
            }

            if(toRelease.TrySetResult())
            {
                return;
            }
        }
    }

        ///<summary>
///Asynchronously waits for the event to be set, consuming exactly one signal.
///</summary>
    ///<param name="cancellationToken">A cancellation token that can abandon the wait.</param>
    ///<returns>A task that completes once a signal is consumed.</returns>
    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock(_gate)
        {
            if(_signaled)
            {
                _signaled = false;
                return Task.CompletedTask;
            }

            TaskCompletionSource tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _waiters.Enqueue(tcs);

            if(cancellationToken.CanBeCanceled)
            {
                CancellationTokenRegistration registration = cancellationToken.Register(static state => ((TaskCompletionSource)state!).TrySetCanceled(), tcs);
                tcs.Task.ContinueWith(static(_, state) => ((CancellationTokenRegistration)state!).Dispose(), registration, TaskScheduler.Default);
            }

            return tcs.Task;
        }
    }
    #endregion
}
