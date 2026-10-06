using System.Collections.Concurrent;
using System.Collections.Immutable;
using Catharsis.Common;

namespace Catharsis.Events;

///<summary>
///A lightweight, in-process publish/subscribe bus for asynchronous handlers. This differs from ///<see
///cref="Catharsis.ComponentModel.ComponentEventAggregator"/>, which is synchronous and meant to be scoped to a single
///component graph: <see cref="EventBus"/> is meant to be shared more broadly (e.g. as a singleton service) and awaits
///each handler through <see cref="PublishAsync{TEvent}"/>.
///</summary>
public sealed class EventBus
{
    #region Fields
    private readonly ConcurrentDictionary<Type, ImmutableArray<Delegate>> _handlers = new();
    #endregion

    #region Private methods
    private void Unsubscribe(Type eventType, Delegate handler) => _handlers.AddOrUpdate(eventType, static _ => ImmutableArray<Delegate>.Empty, (_, existing) => existing.Remove(handler));
    #endregion

    #region Public methods
    ///<summary>
    ///Publishes an event to every handler currently subscribed to <typeparamref name="TEvent"/>, in subscription order,
    ///awaiting each one before invoking the next.
    ///</summary>
    ///<typeparam name="TEvent">The type of the event.</typeparam>
    ///<param name="event">The event instance to publish.</param>
    ///<param name="cancellationToken">A token that can abandon publishing before every handler has run.</param>
    ///<returns>A task that completes once every subscribed handler has run.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="event"/> is <c>null</c>.</exception>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if(!_handlers.TryGetValue(typeof(TEvent), out ImmutableArray<Delegate> handlers))
        {
            return;
        }

        foreach(Delegate handler in handlers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ((Func<TEvent, Task>)handler)(@event).ConfigureAwait(false);
        }
    }

    ///<summary>
    ///Subscribes an asynchronous handler to events of type <typeparamref name="TEvent"/>.
    ///</summary>
    ///<typeparam name="TEvent">The type of the event to subscribe to.</typeparam>
    ///<param name="handler">The asynchronous handler to invoke for each published event.</param>
    ///<returns>An <see cref="IDisposable"/> that unsubscribes <paramref name="handler"/> when disposed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
    public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        Type eventType = typeof(TEvent);
        _handlers.AddOrUpdate(eventType, _ => ImmutableArray.Create<Delegate>(handler), (_, existing) => existing.Add(handler));

        return Disposable.Create(() => Unsubscribe(eventType, handler));
    }
    #endregion
}
