using Catharsis.Common;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Catharsis.Events;

///<summary>
///An event that holds its instance-method subscribers weakly, so subscribing does not keep the subscriber alive. This
///avoids the classic "forgotten subscription" memory leak. Each subscription is a
///<see cref="WeakEventHandler{TEventArgs}"/>; static handlers are held normally.
///</summary>
///<remarks>
///Because the subscriber is only weakly referenced, a lambda that captures variables (whose closure object nothing
///else references) may be collected and silently stop being invoked. Subscribe a method on a long-lived object
///instead.
///</remarks>
///<typeparam name="TArgs">The type of the event data.</typeparam>
public sealed class WeakEvent<TArgs>
{
    #region Fields
    readonly Lock _gate = new();
    readonly List<Entry> _entries = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Subscribes a handler. Instance handlers are held weakly.
    ///</summary>
    ///<param name="handler">The handler to invoke when the event is raised.</param>
    ///<exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="handler"/> is a multicast delegate.</exception>
    public void Subscribe(EventHandler<TArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (handler.GetInvocationList().Length != 1)
        {
            throw new ArgumentException("Multicast delegates are not supported; subscribe each handler separately.", nameof(handler));
        }

        Entry entry = new(new WeakEventHandler<TArgs>(handler), handler.Method, handler.Target is null ? null : new WeakReference(handler.Target));

        lock (_gate)
        {
            _entries.Add(entry);
        }
    }

    ///<summary>
    ///Removes a previously subscribed handler.
    ///</summary>
    ///<param name="handler">The handler to remove.</param>
    ///<returns><c>true</c> if a matching subscription was found and removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
    public bool Unsubscribe(EventHandler<TArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_gate)
        {
            int index = _entries.FindIndex(entry => entry.Method == handler.Method && ReferenceEquals(entry.Target?.Target, handler.Target));

            if (index < 0)
            {
                return false;
            }

            _entries.RemoveAt(index);
            return true;
        }
    }

    ///<summary>
    ///Raises the event, invoking every handler whose subscriber is still alive and discarding those that are not. An
    ///exception thrown by a handler propagates unwrapped to the caller.
    ///</summary>
    ///<param name="sender">The source of the event.</param>
    ///<param name="args">The event data.</param>
    public void Invoke(object? sender, TArgs args)
    {
        Entry[] snapshot;

        lock (_gate)
        {
            snapshot = [.. _entries];
        }

        List<Entry> dead = [];

        foreach (Entry entry in snapshot)
        {
            try
            {
                if (!entry.Handler.Invoke(sender, args))
                {
                    dead.Add(entry);
                }
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }

        if (dead.Count > 0)
        {
            lock (_gate)
            {
                _entries.RemoveAll(dead.Contains);
            }
        }
    }
    #endregion

    #region Public properties
    ///<summary>Gets the number of subscriptions whose subscriber is still alive.</summary>
    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _entries.Count(static entry => entry.Handler.IsTargetAlive);
            }
        }
    }
    #endregion

    #region Nested types
    sealed record Entry(WeakEventHandler<TArgs> Handler, MethodInfo Method, WeakReference? Target);
    #endregion
}
