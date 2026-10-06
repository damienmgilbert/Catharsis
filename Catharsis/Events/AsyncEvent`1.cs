using System.Collections.Immutable;

namespace Catharsis.Events;

///<summary>
///A multicast event whose handlers are asynchronous. Handlers are added with <see
///cref="Subscribe(AsyncEventHandler{TArgs})"/> (which returns a disposable subscription) or the <c>+</c>/<c>-</c>
///operators, and are raised with ///<see cref="InvokeAsync"/>, either one after another or all at once.
///</summary>
///<typeparam name="TArgs">The type of the event data.</typeparam>
public sealed class AsyncEvent<TArgs>
{
    #region Fields
    private readonly Lock _gate = new();
    private ImmutableArray<AsyncEventHandler<TArgs>> _handlers = [];
    #endregion

    #region Operators
    ///<summary>
    ///Removes a handler, so <c>myEvent -= handler</c> works.
    ///</summary>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static AsyncEvent<TArgs> operator -(AsyncEvent<TArgs> source, AsyncEventHandler<TArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(handler);

        source.Remove(handler);
        return source;
    }
    public static AsyncEvent<TArgs> operator +(AsyncEvent<TArgs> source, AsyncEventHandler<TArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(handler);

        source.Add(handler);
        return source;
    }
    #endregion

    #region Private methods
    private void Add(AsyncEventHandler<TArgs> handler)
    {
        lock(_gate)
        {
            _handlers = _handlers.Add(handler);
        }
    }

    private void Remove(AsyncEventHandler<TArgs> handler)
    {
        lock(_gate)
        {
            _handlers = _handlers.Remove(handler);
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Raises the event.
    ///</summary>
    ///<param name="sender">The source of the event.</param>
    ///<param name="args">The event data.</param>
    ///<param name="parallel">
    public async Task InvokeAsync(object? sender, TArgs args, bool parallel = false, CancellationToken cancellationToken = default)
    {
        ImmutableArray<AsyncEventHandler<TArgs>> snapshot = _handlers;

        if(parallel)
        {
            Task[] running = new Task[snapshot.Length];

            for(int i = 0; i < snapshot.Length; i++)
            {
                running[i] = snapshot[i](sender, args, cancellationToken);
            }

            await Task.WhenAll(running).ConfigureAwait(false);
            return;
        }

        foreach(AsyncEventHandler<TArgs> handler in snapshot)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await handler(sender, args, cancellationToken).ConfigureAwait(false);
        }
    }

    ///<summary>
    ///Raises the event, returning a <see cref="ValueTask"/>.
    ///</summary>
    ///<inheritdoc cref="InvokeAsync"/>
    public ValueTask InvokeValueAsync(object? sender, TArgs args, bool parallel = false, CancellationToken cancellationToken = default) { return _handlers.IsEmpty ? ValueTask.CompletedTask : new ValueTask(InvokeAsync(sender, args, parallel, cancellationToken)); }

        ///<summary>
///Subscribes a handler.
///</summary>
    ///<param name="handler">The handler to invoke when the event is raised.</param>
    ///<returns>An <see cref="IDisposable"/> that removes the handler when disposed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
    public IDisposable Subscribe(AsyncEventHandler<TArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        Add(handler);
        return new Subscription(this, handler);
    }

    ///<summary>
    ///Subscribes a handler that only receives events accepted by <paramref name="filter"/>.
    ///</summary>
    ///<param name="handler">The handler to invoke for accepted events.</param>
    ///<param name="filter">Decides which events reach <paramref name="handler"/>.</param>
    ///<returns>An <see cref="IDisposable"/> that removes the handler when disposed.</returns>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public IDisposable Subscribe(AsyncEventHandler<TArgs> handler, EventFilter<TArgs> filter)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(filter);

        AsyncEventHandler<TArgs> filtered = (sender, args, ct) => filter(args) ? handler(sender, args, ct) : Task.CompletedTask;

        Add(filtered);
        return new Subscription(this, filtered);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of subscribed handlers.
    ///</summary>
    public int Count => _handlers.Length;
    #endregion

    private sealed class Subscription(AsyncEvent<TArgs> owner, AsyncEventHandler<TArgs> handler) : IDisposable
    {
        #region Fields
        private AsyncEvent<TArgs>? _owner = owner;
        #endregion

        #region Public methods
        public void Dispose() => Interlocked.Exchange(ref _owner, null)?.Remove(handler);
        #endregion
    }
}
