using System.Collections.Concurrent;

namespace Catharsis.Events;

///<summary>
///Minimal scaffolding for dispatching <see cref="DomainEvent"/> instances raised by an aggregate to whichever
///handlers are subscribed to that specific event's runtime type.
///</summary>
public sealed class DomainEventDispatcher
{
    #region Fields
    readonly Lock _gate = new();
    readonly ConcurrentDictionary<Type, List<Func<DomainEvent, Task>>> _handlers = new();
    #endregion

    #region Public methods
    ///<summary>
    ///Dispatches a domain event to every handler subscribed to its exact runtime type, in subscription order,
    ///awaiting each one before invoking the next.
    ///</summary>
    ///<param name="domainEvent">The event to dispatch.</param>
    ///<param name="cancellationToken">A token that can abandon dispatch before every handler has run.</param>
    ///<returns>A task that completes once every subscribed handler has run.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="domainEvent"/> is <c>null</c>.</exception>
    public async Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        List<Func<DomainEvent, Task>> snapshot;

        lock(_gate)
        {
            if(!_handlers.TryGetValue(domainEvent.GetType(), out List<Func<DomainEvent, Task>>? handlers))
            {
                return;
            }

            snapshot = [.. handlers];
        }

        foreach(Func<DomainEvent, Task> handler in snapshot)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await handler(domainEvent).ConfigureAwait(false);
        }
    }

    ///<summary>
    ///Subscribes a handler to events of the exact type <typeparamref name="TEvent"/>.
    ///</summary>
    ///<typeparam name="TEvent">The domain event type to subscribe to.</typeparam>
    ///<param name="handler">The asynchronous handler to invoke for each dispatched event of this type.</param>
    ///<exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock(_gate)
        {
            List<Func<DomainEvent, Task>> handlers = _handlers.GetOrAdd(typeof(TEvent), static _ => []);
            handlers.Add(domainEvent => handler((TEvent)domainEvent));
        }
    }
    #endregion
}
