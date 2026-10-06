using Catharsis.Common;
using System.Collections.Concurrent;

namespace Catharsis.ComponentModel;

///<summary>
///A lightweight, type-keyed publish/subscribe hub scoped to a component graph. This is intentionally simpler than a
///full messenger (no per-recipient tokens, channels, or weak references): it exists for wiring up sibling
///components within a single container, not for cross-cutting application-wide messaging.
///</summary>
public sealed class ComponentEventAggregator
{
    #region Fields
    readonly Lock _gate = new();
    readonly ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();
    #endregion

    #region Private methods
    void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        lock(_gate)
        {
            if(_subscribers.TryGetValue(typeof(TEvent), out List<Delegate>? handlers))
            {
                handlers.Remove(handler);
            }
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Publishes an event to every handler currently subscribed to <typeparamref name="TEvent"/>, in subscription
    ///order.
    ///</summary>
    ///<typeparam name="TEvent">The type of the event.</typeparam>
    ///<param name="event">The event instance to publish.</param>
    ///<exception cref="ArgumentNullException"><paramref name="event"/> is <c>null</c>.</exception>
    public void Publish<TEvent>(TEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        List<Action<TEvent>> snapshot;

        lock(_gate)
        {
            if(!_subscribers.TryGetValue(typeof(TEvent), out List<Delegate>? handlers))
            {
                return;
            }

            snapshot = [.. handlers.Cast<Action<TEvent>>()];
        }

        foreach(Action<TEvent> handler in snapshot)
        {
            handler(@event);
        }
    }

    ///<summary>
    ///Subscribes a handler to events of type <typeparamref name="TEvent"/>.
    ///</summary>
    ///<typeparam name="TEvent">The type of the event to subscribe to.</typeparam>
    ///<param name="handler">The handler to invoke for each published event.</param>
    ///<returns>An <see cref="IDisposable"/> that unsubscribes <paramref name="handler"/> when disposed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock(_gate)
        {
            List<Delegate> handlers = _subscribers.GetOrAdd(typeof(TEvent), static _ => []);
            handlers.Add(handler);
        }

        return Disposable.Create(() => Unsubscribe(handler));
    }
    #endregion
}
