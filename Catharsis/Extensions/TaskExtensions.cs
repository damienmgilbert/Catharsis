namespace Catharsis.Extensions;

///<summary>
///Provides extension methods for <see cref="Task"/> that fill small gaps left by the base class library.
///</summary>
public static class TaskExtensions
{
    #region Private methods

    ///<remarks>
    ///Validation lives in the synchronous <see cref="FireAndForget"/> wrapper because an <c>async void</c> method
    ///cannot throw synchronously to its caller: an exception thrown here would instead surface as an unhandled
    ///exception on the thread pool.
    ///</remarks>
    private static async void FireAndForgetCore(Task task, Action<Exception>? onException)
    {
        try
        {
            await task.ConfigureAwait(false);
        } catch(Exception ex) when(onException is not null)
        {
            onException(ex);
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Observes the task's outcome without awaiting it, invoking <paramref name="onException"/> if it faults instead of
    ///leaving the exception unobserved. If <paramref name="onException"/> is <c>null</c>, a fault propagates as an
    ///unhandled exception rather than being silently swallowed.
    ///</summary>
    ///<remarks>
    ///This is one of the few legitimate uses of <c>async void</c>: the point of "fire and forget" is that the caller
    ///has no <see cref="Task"/> to await or observe, so returning one here would just reintroduce the unobserved-
    ///exception problem this method exists to avoid.
    ///</remarks>
    ///<param name="task">The task to run to completion in the background.</param>
    ///<param name="onException">An optional callback invoked with the exception if the task faults.</param>
    ///<exception cref="ArgumentNullException"><paramref name="task"/> is <c>null</c>.</exception>
    public static void FireAndForget(this Task task, Action<Exception>? onException = null)
    {
        if(task is null)
        {
            throw new ArgumentNullException(nameof(task), "Task must not be null.");
        }

        FireAndForgetCore(task, onException);
    }

    ///<summary>
    ///Waits for every task to complete, throwing the first exception encountered as soon as any task faults instead of
    ///waiting for all of them and wrapping the failures in an <see cref="AggregateException"/>.
    ///</summary>
    ///<param name="tasks">The tasks to wait for.</param>
    ///<returns>A task that completes when every input task completes, or faults with the first observed exception.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="tasks"/> is <c>null</c>.</exception>
    public static async Task WhenAllOrFirstException(this IEnumerable<Task> tasks)
    {
        if(tasks is null)
        {
            throw new ArgumentNullException(nameof(tasks), "Tasks must not be null.");
        }

        List<Task> remaining = [ .. tasks ];

        while(remaining.Count > 0)
        {
            Task completed = await Task.WhenAny(remaining).ConfigureAwait(false);

            if(completed.IsFaulted)
            {
                await completed.ConfigureAwait(false);
            }

            remaining.Remove(completed);
        }
    }
    #endregion
}
