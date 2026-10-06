using System.Runtime.CompilerServices;

namespace Catharsis.Concurrency;

///<summary>
///Provides async-safe lazy initialization: the value factory runs at most once, even when
///<see cref="Value"/> is accessed concurrently from multiple callers, and the type is directly awaitable.
///</summary>
///<typeparam name="T">The type of the lazily produced value.</typeparam>
///<example>
///<code>
///AsyncLazy&lt;Config&gt; config = new(async () => await LoadConfigAsync());
///Config value = await config;
///</code>
///</example>
public sealed class AsyncLazy<T>
{
    #region Fields
    readonly Lazy<Task<T>> _instance;
    #endregion

    #region Public methods
    ///<summary>
    ///Creates an instance backed by a synchronous value factory, which runs on the thread pool.
    ///</summary>
    ///<param name="valueFactory">The factory that produces the value.</param>
    ///<exception cref="ArgumentNullException"><paramref name="valueFactory"/> is <c>null</c>.</exception>
    public AsyncLazy(Func<T> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(valueFactory);
        _instance = new Lazy<Task<T>>(() => Task.Run(valueFactory));
    }

    ///<summary>
    ///Creates an instance backed by an asynchronous value factory.
    ///</summary>
    ///<param name="taskFactory">The factory that produces the value.</param>
    ///<exception cref="ArgumentNullException"><paramref name="taskFactory"/> is <c>null</c>.</exception>
    public AsyncLazy(Func<Task<T>> taskFactory)
    {
        ArgumentNullException.ThrowIfNull(taskFactory);
        _instance = new Lazy<Task<T>>(() => Task.Run(taskFactory));
    }

    ///<summary>
    ///Gets an awaiter for the lazily produced value, so instances can be awaited directly.
    ///</summary>
    public TaskAwaiter<T> GetAwaiter() => Value.GetAwaiter();
    #endregion

    #region Public properties
    ///<summary>
    ///Gets whether the value factory has already started running.
    ///</summary>
    public bool IsValueCreated => _instance.IsValueCreated;

    ///<summary>
    ///Gets the task representing the lazily produced value, starting the factory on first access.
    ///</summary>
    public Task<T> Value => _instance.Value;
    #endregion
}
