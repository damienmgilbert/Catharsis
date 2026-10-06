using System.Reflection;

namespace Catharsis.Common;

///<summary>
///Wraps an <see cref="EventHandler{TEventArgs}"/> using a <see cref="WeakReference"/> to its target, so subscribing to
///a long-lived publisher does not keep a short-lived subscriber alive. This is the standard fix for the memory leak
///that arises when an object subscribes to an event on something that outlives it but never unsubscribes.
///</summary>
///<typeparam name="TEventArgs">The type of the event arguments.</typeparam>
///<remarks>
///Invocation goes through reflection since the handler's target is only known as a weak <see cref="object"/> reference.
///This trades a small per-invocation cost for not keeping the subscriber alive; it is not intended for hot-path events
///raised at high frequency.
///</remarks>
public sealed class WeakEventHandler<TEventArgs>
{
    #region Fields
    private readonly MethodInfo _method;
    private readonly WeakReference? _targetReference;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="WeakEventHandler{TEventArgs}"/> wrapping the specified handler.
    ///</summary>
    ///<param name="handler">The handler to wrap. Its target, if any, is held only weakly.</param>
    ///<exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
    public WeakEventHandler(EventHandler<TEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _targetReference = handler.Target is not null ? new WeakReference(handler.Target) : null;
        _method = handler.Method;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Invokes the wrapped handler if its target is still alive (or if it was a static method).
    ///</summary>
    ///<param name="sender">The event source.</param>
    ///<param name="args">The event arguments.</param>
    ///<returns><c>true</c> if the handler was invoked; <c>false</c> if the subscriber has been garbage collected.</returns>
    public bool Invoke(object? sender, TEventArgs args)
    {
        if(_targetReference is null)
        {
            _method.Invoke(null, [ sender, args ]);
            return true;
        }

        object? target = _targetReference.Target;

        if(target is null)
        {
            return false;
        }

        _method.Invoke(target, [ sender, args ]);
        return true;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets whether the wrapped handler's target has been garbage collected. Always <c>false</c> for a static method
    ///handler.
    ///</summary>
    public bool IsTargetAlive => (_targetReference is null) || _targetReference.IsAlive;
    #endregion
}
