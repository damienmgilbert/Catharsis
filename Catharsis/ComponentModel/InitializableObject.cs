using System.ComponentModel;

namespace Catharsis.ComponentModel;

/// <summary>
/// Abstract base class implementing <see cref="ISupportInitialize"/> and
/// <see cref="ISupportInitializeNotification"/> with state tracking and
/// an <see cref="Initialized"/> event.
/// </summary>
public abstract class InitializableObject : ISupportInitializeNotification
{
    private bool _isInitializing;
    private bool _isInitialized;

    /// <inheritdoc />
    public bool IsInitialized => _isInitialized;

    /// <inheritdoc />
    public event EventHandler? Initialized;

    /// <summary>
    /// Gets a value indicating whether the object is currently being initialized
    /// (between <see cref="BeginInit"/> and <see cref="EndInit"/>).
    /// </summary>
    public bool IsInitializing => _isInitializing;

    /// <summary>
    /// Signals the object that initialization is starting.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="BeginInit"/> was called while already initializing.
    /// </exception>
    public void BeginInit()
    {
        if (_isInitializing)
            throw new InvalidOperationException("Initialization has already begun.");

        _isInitializing = true;
        _isInitialized = false;
        OnBeginInit();
    }

    /// <summary>
    /// Signals the object that initialization is complete.
    /// Raises the <see cref="Initialized"/> event.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="EndInit"/> was called without a matching <see cref="BeginInit"/>.
    /// </exception>
    public void EndInit()
    {
        if (!_isInitializing)
            throw new InvalidOperationException("BeginInit must be called before EndInit.");

        OnEndInit();
        _isInitializing = false;
        _isInitialized = true;
        Initialized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when initialization begins. Override to perform setup logic.
    /// </summary>
    protected virtual void OnBeginInit() { }

    /// <summary>
    /// Called when initialization ends, before the <see cref="Initialized"/> event is raised.
    /// Override to perform finalization logic such as validation.
    /// </summary>
    protected virtual void OnEndInit() { }
}
