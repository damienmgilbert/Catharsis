using System.ComponentModel;

namespace Catharsis.ComponentModel.Binding;

///<summary>
///A disposable scope that defers <see cref="INotifyPropertyChanged.PropertyChanged"/> notifications. Property names are
///collected during the scope and a single notification per unique property is raised when the scope is disposed.
///</summary>
///<remarks>
public sealed class PropertyChangeScope : IDisposable
{
    #region Fields
    private bool _disposed;
    private readonly HashSet<string> _pendingProperties = [ with(StringComparer.Ordinal) ];
    private readonly Action<string> _raisePropertyChanged;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="PropertyChangeScope"/>.
    ///</summary>
    ///<param name="raisePropertyChanged">
    ///A callback that raises <see cref="INotifyPropertyChanged.PropertyChanged"/> for the specified property name.
    ///</param>
    ///<exception cref="ArgumentNullException">
    public PropertyChangeScope(Action<string> raisePropertyChanged)
    {
        ArgumentNullException.ThrowIfNull(raisePropertyChanged);
        _raisePropertyChanged = raisePropertyChanged;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;

        foreach(string propertyName in _pendingProperties)
        {
            _raisePropertyChanged(propertyName);
        }

        _pendingProperties.Clear();
    }

    ///<summary>
    ///Records a property name for deferred notification.
    ///</summary>
    ///<param name="propertyName">The name of the changed property.</param>
    ///<exception cref="ArgumentNullException">
    public void RecordChange(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        ObjectDisposedException.ThrowIf(_disposed, this);
        _pendingProperties.Add(propertyName);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a value indicating whether this scope has been disposed.
    ///</summary>
    public bool IsDisposed => _disposed;

    ///<summary>
    ///Gets the number of unique property names pending notification.
    ///</summary>
    public int PendingCount => _pendingProperties.Count;
    #endregion
}
