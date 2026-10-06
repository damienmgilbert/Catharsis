using System.ComponentModel;

namespace Catharsis.ComponentModel.Licensing;

///<summary>
///Abstract base class for component designers that provides a simplified design-time support model without depending on
///Windows Forms designer infrastructure.
///</summary>
///<remarks>
///<para> Derived classes override <see cref="OnInitialize"/> to perform design-time setup, <see
///cref="CreateActionList"/> to provide verbs, and <see cref="OnComponentChanged"/> to react to property changes.</para>
///<para> Call <see cref="Initialize"/> with a component to begin the design session, and <see cref="Dispose()"/> when
///the designer is no longer needed.</para>
///</remarks>
public abstract class ComponentDesignerBase : IDisposable
{
    #region Fields
    bool _disposed;
    #endregion

    #region Protected methods
    ///<summary>
    ///Creates the action list for this designer. Override to provide custom design-time verbs.
    ///</summary>
    ///<param name="context">The design context.</param>
    ///<returns>
    ///A <see cref="ComponentActionList"/>, or <c>null</c> if no actions are needed.
    ///</returns>
    protected virtual ComponentActionList? CreateActionList(ComponentDesignContext context) { return null; }

    ///<summary>
    ///Releases resources used by the designer.
    ///</summary>
    ///<param name="disposing">
    ///<c>true</c> to release managed resources; <c>false</c> for unmanaged resources only.
    ///</param>
    protected virtual void Dispose(bool disposing)
    {
        if(_disposed)
        {
            return;
        }

        if(disposing)
        {
            Context = null;
            ActionList = null;
        }

        _disposed = true;
    }

    ///<summary>
    ///Called when the component's properties change at design time. Override to react to property modifications.
    ///</summary>
    ///<param name="propertyName">The name of the changed property.</param>
    protected virtual void OnComponentChanged(string propertyName)
    {
    }
    ///<summary>
    ///Called after <see cref="Initialize"/> to allow derived classes to perform design-time setup.
    ///</summary>
    ///<param name="context">The design context.</param>
    protected virtual void OnInitialize(ComponentDesignContext context)
    {
    }
    #endregion

    #region Protected properties
    ///<summary>
    ///Gets the design context for the current component, or <c>null</c> if <see cref="Initialize"/> has not been
    ///called.
    ///</summary>
    protected ComponentDesignContext? Context { get; private set; }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ///<summary>
    ///Initializes the designer for the specified component.
    ///</summary>
    ///<param name="component">The component to design.</param>
    ///<param name="container">
    ///The container, or <c>null</c> to use the component's site container.
    ///</param>
    ///<param name="serviceProvider">
    ///An optional service provider for design-time service resolution.
    ///</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="component"/> is <c>null</c>.
    ///</exception>
    ///<exception cref="InvalidOperationException">
    ///The designer has already been initialized.
    ///</exception>
    public void Initialize(IComponent component, IContainer? container = null, IServiceProvider? serviceProvider = null)
    {
        ArgumentNullException.ThrowIfNull(component);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if(Context is not null)
        {
            throw new InvalidOperationException("The designer has already been initialized.");
        }

        Context = new ComponentDesignContext(component, container, serviceProvider);
        ActionList = CreateActionList(Context);
        OnInitialize(Context);
    }

    ///<summary>
    ///Raises a component-changed notification for the specified property.
    ///</summary>
    ///<param name="propertyName">The property name.</param>
    public void NotifyComponentChanged(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        OnComponentChanged(propertyName);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the action list provided by <see cref="CreateActionList"/>, or <c>null</c> if no actions are defined.
    ///</summary>
    public ComponentActionList? ActionList { get; private set; }

    ///<summary>
    ///Gets the component being designed, or <c>null</c> if not initialized.
    ///</summary>
    public IComponent? Component => Context?.Component;
    #endregion
}
