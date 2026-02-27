using System.ComponentModel;

namespace Catharsis.ComponentModel.Binding;

/// <summary>
/// A disposable scope that defers <see cref="INotifyPropertyChanged.PropertyChanged"/>
/// notifications. Property names are collected during the scope and a single
/// notification per unique property is raised when the scope is disposed.
/// </summary>
/// <remarks>
/// <para>
/// Wrap code that modifies multiple properties in a <c>using</c> block
/// with <see cref="PropertyChangeScope"/> to batch the resulting notifications.
/// </para>
/// <para>
/// This class is designed to work with any <see cref="INotifyPropertyChanged"/>
/// source by accepting a notification callback at construction time.
/// </para>
/// </remarks>
public sealed class PropertyChangeScope : IDisposable
{
    private readonly Action<string> _raisePropertyChanged;
    private readonly HashSet<string> _pendingProperties = new(StringComparer.Ordinal);
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="PropertyChangeScope"/>.
    /// </summary>
    /// <param name="raisePropertyChanged">
    /// A callback that raises <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// for the specified property name.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="raisePropertyChanged"/> is <c>null</c>.
    /// </exception>
    public PropertyChangeScope(Action<string> raisePropertyChanged)
    {
        ArgumentNullException.ThrowIfNull(raisePropertyChanged);
        _raisePropertyChanged = raisePropertyChanged;
    }

    /// <summary>
    /// Gets the number of unique property names pending notification.
    /// </summary>
    public int PendingCount => _pendingProperties.Count;

    /// <summary>
    /// Gets a value indicating whether this scope has been disposed.
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Records a property name for deferred notification.
    /// </summary>
    /// <param name="propertyName">The name of the changed property.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// The scope has already been disposed.
    /// </exception>
    public void RecordChange(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        ObjectDisposedException.ThrowIf(_disposed, this);
        _pendingProperties.Add(propertyName);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (var propertyName in _pendingProperties)
            _raisePropertyChanged(propertyName);

        _pendingProperties.Clear();
    }
}
