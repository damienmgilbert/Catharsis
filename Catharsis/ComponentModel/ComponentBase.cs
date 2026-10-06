using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///Abstract base class for components that implement <see cref="IComponent"/> with deterministic disposal, site
///management, and service resolution support.
///</summary>
///<remarks>
///Derived classes should override <see cref="Dispose(bool)"/> to release managed and unmanaged resources. The <see
///cref="Disposed"/> event is raised after disposal completes.
///</remarks>
public abstract class ComponentBase : IComponent, IServiceProvider
{
    #region Fields
    EventHandler? _disposed;
    ISite? _site;
    #endregion

    #region Events
    ///<inheritdoc/>
    public event EventHandler? Disposed { add => _disposed += value; remove => _disposed -= value; }
    #endregion

    #region Protected methods
    ///<summary>
    ///Releases resources used by this component.
    ///</summary>
    ///<param name="disposing">
    ///<c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
    ///</param>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            _site?.Container?.Remove(this);
            _site = null;
            _disposed?.Invoke(this, EventArgs.Empty);
            _disposed = null;
        }

        IsDisposed = true;
    }

    ///<summary>
    ///Throws <see cref="ObjectDisposedException"/> if this component has been disposed.
    ///</summary>
    protected void ThrowIfDisposed() { ObjectDisposedException.ThrowIf(IsDisposed, this); }
    #endregion

    #region Protected properties
    ///<summary>
    ///Gets the container that hosts this component, or <c>null</c> if the component is not sited.
    ///</summary>
    protected IContainer? Container => _site?.Container;

    ///<summary>
    ///Gets a value indicating whether the component is in design mode.
    ///</summary>
    protected bool DesignMode => _site?.DesignMode ?? false;

    ///<summary>
    ///Gets a value indicating whether this component has been disposed.
    ///</summary>
    protected bool IsDisposed { get; private set; }
    #endregion

    #region Public methods
    ///<summary>
    ///Releases all resources used by this component.
    ///</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ///<summary>
    ///Returns a service of the specified type from the component's site.
    ///</summary>
    ///<param name="serviceType">The type of service to retrieve.</param>
    ///<returns>
    ///An object implementing the requested service, or <c>null</c> if the service is not available.
    ///</returns>
    public virtual object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        if (serviceType == typeof(IComponent))
        {
            return this;
        }

        if (serviceType == typeof(ISite))
        {
            return _site;
        }

        if (serviceType == typeof(IContainer))
        {
            return _site?.Container;
        }

        return _site?.GetService(serviceType);
    }

    ///<inheritdoc/>
    public override string ToString()
    {
        string? name = _site?.Name;
        string typeName = GetType().Name;
        return (name is not null) ? ($"{typeName} [{name}]") : typeName;
    }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public ISite? Site { get => _site; set => _site = value; }
    #endregion
}
