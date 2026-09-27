using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///Defers a component's construction until <see cref="Value"/> is first accessed, and disposes it on
///<see cref="Dispose"/> only if it was actually created — unlike disposing a plain <see cref="Lazy{T}"/> wrapper
///directly, which would force creation of a component that was never used just to immediately dispose it.
///</summary>
///<typeparam name="T">The type of component to create lazily.</typeparam>
///<param name="factory">The delegate that creates the component on first access.</param>
public sealed class LazyComponent<T>(Func<T> factory) : IDisposable where T : IComponent
{
    #region Fields
    readonly Lazy<T> _lazy = new(factory ?? throw new ArgumentNullException(nameof(factory), "Factory must not be null."), isThreadSafe: true);
    #endregion

    #region Public methods
    ///<summary>
    ///Disposes the wrapped component if it has been created. Does nothing if <see cref="Value"/> was never accessed.
    ///</summary>
    public void Dispose()
    {
        if(_lazy.IsValueCreated)
        {
            _lazy.Value.Dispose();
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets whether the component has been created yet.
    ///</summary>
    public bool IsValueCreated => _lazy.IsValueCreated;

    ///<summary>
    ///Gets the component, creating it via the configured factory on first access.
    ///</summary>
    public T Value => _lazy.Value;
    #endregion
}
