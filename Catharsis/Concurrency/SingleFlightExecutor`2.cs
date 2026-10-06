namespace Catharsis.Concurrency;

///<summary>
///Deduplicates concurrent calls for the same key so that only one underlying operation executes at a time per key.
///Concurrent callers for the same key await the same in-flight <see cref="Task{TResult}"/> instead of each starting
///their own; once the operation completes, the next call for that key starts a fresh execution.
///</summary>
///<typeparam name="TKey">The key type identifying duplicate work.</typeparam>
///<typeparam name="TResult">The result type produced by the deduplicated operation.</typeparam>
///<param name="comparer">The equality comparer used to match keys, or <c>null</c> to use the default comparer.</param>
///<example>
///<code>
///SingleFlightExecutor&lt;string, User&gt; executor = new();
///
///// If several callers request the same userId concurrently, only one database call is made.
///User user = await executor.ExecuteAsync(userId, () => database.LoadUserAsync(userId));
///</code>
///</example>
public sealed class SingleFlightExecutor<TKey, TResult>(IEqualityComparer<TKey>? comparer = null) where TKey : notnull
{
    #region Fields
    readonly Dictionary<TKey, Task<TResult>> _inFlight = new(comparer);
    readonly Lock _gate = new();
    #endregion

    #region Public methods
    ///<summary>
    ///Executes the operation for the specified key, or returns the already in-flight task for that key if one
    ///exists.
    ///</summary>
    ///<param name="key">The key identifying the operation.</param>
    ///<param name="operation">The operation to execute if none is currently in flight for this key.</param>
    ///<returns>The result of the (possibly shared) execution.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="operation"/> is <c>null</c>.</exception>
    public Task<TResult> ExecuteAsync(TKey key, Func<Task<TResult>> operation)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(operation);

        TaskCompletionSource<TResult> tcs;

        lock(_gate)
        {
            if(_inFlight.TryGetValue(key, out Task<TResult>? existing))
            {
                return existing;
            }

            tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _inFlight[key] = tcs.Task;
        }

        _ = RunAsync(key, operation, tcs);
        return tcs.Task;
    }

    async Task RunAsync(TKey key, Func<Task<TResult>> operation, TaskCompletionSource<TResult> tcs)
    {
        try
        {
            TResult result = await operation().ConfigureAwait(false);
            tcs.TrySetResult(result);
        }
        catch(OperationCanceledException)
        {
            tcs.TrySetCanceled();
        }
        catch(Exception ex)
        {
            tcs.TrySetException(ex);
        }
        finally
        {
            lock(_gate)
            {
                if(_inFlight.TryGetValue(key, out Task<TResult>? current) && current == tcs.Task)
                {
                    _inFlight.Remove(key);
                }
            }
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of operations currently in flight.
    ///</summary>
    public int InFlightCount
    {
        get
        {
            lock(_gate)
            {
                return _inFlight.Count;
            }
        }
    }
    #endregion
}
