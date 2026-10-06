using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///Abstract base class implementing <see cref="ISupportInitialize"/> and <see cref="ISupportInitializeNotification"/>
///with state tracking and an <see cref="Initialized"/> event.
///</summary>
public abstract class InitializableObject : ISupportInitializeNotification
{
    #region Fields
    private bool _isInitialized;
    private bool _isInitializing;
    #endregion

    #region Events
    ///<inheritdoc/>
    public event EventHandler? Initialized;
    #endregion

    #region Protected methods
    ///<summary>
    ///Called when initialization begins. Override to perform setup logic.
    ///</summary>
    protected virtual void OnBeginInit()
    {
    }
    ///<summary>
    ///Called when initialization ends, before the <see cref="Initialized"/> event is raised. Override to perform
    ///finalization logic such as validation.
    ///</summary>
    protected virtual void OnEndInit()
    {
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Signals the object that initialization is starting.
    ///</summary>
    ///<exception cref="InvalidOperationException">
    public void BeginInit()
    {
        if(_isInitializing)
        {
            throw new InvalidOperationException("Initialization has already begun.");
        }

        _isInitializing = true;
        _isInitialized = false;
        OnBeginInit();
    }

    ///<summary>
    ///Signals the object that initialization is complete. Raises the <see cref="Initialized"/> event.
    ///</summary>
    ///<exception cref="InvalidOperationException">
    public void EndInit()
    {
        if(!_isInitializing)
        {
            throw new InvalidOperationException("BeginInit must be called before EndInit.");
        }

        OnEndInit();
        _isInitializing = false;
        _isInitialized = true;
        Initialized?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public bool IsInitialized => _isInitialized;

    ///<summary>
    ///Gets a value indicating whether the object is currently being initialized (between <see cref="BeginInit"/> and
    public bool IsInitializing => _isInitializing;
    #endregion
}
