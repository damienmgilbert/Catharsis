using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Catharsis.ComponentModel.Observability;

/// <summary>
/// An observable component that implements <see cref="IChangeTrackable"/>,
/// providing per-property change tracking with undo/redo support and
/// <see cref="INotifyPropertyChanged"/> notifications.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="SetTrackedProperty{T}"/> in property setters to automatically
/// record changes. The <see cref="Changes"/> property exposes the full
/// <see cref="ChangeSet"/> for querying, and <see cref="Undo"/>/<see cref="Redo"/>
/// revert or reapply individual property changes.
/// </para>
/// <para>
/// Call <see cref="AcceptChanges"/> to commit all pending changes (clearing
/// the undo/redo history), or <see cref="RejectChanges"/> to revert all
/// pending changes in reverse order.
/// </para>
/// </remarks>
public abstract class ChangeTrackingComponent : IChangeTrackable, INotifyPropertyChanged, INotifyPropertyChanging
{
    private readonly ChangeSet _changes = new();
    private bool _isTrackingSuspended;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <inheritdoc />
    public ChangeSet Changes => _changes;

    /// <inheritdoc />
    public bool IsChanged => _changes.HasChanges;

    /// <inheritdoc />
    public bool CanUndo => _changes.CanUndo;

    /// <inheritdoc />
    public bool CanRedo => _changes.CanRedo;

    /// <summary>
    /// Gets a value indicating whether change tracking is currently suspended.
    /// </summary>
    protected bool IsTrackingSuspended => _isTrackingSuspended;

    /// <summary>
    /// Sets the backing field to the specified value, records the change,
    /// and raises property change notifications.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">A reference to the backing field.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">
    /// The name of the property. Automatically provided by the compiler.
    /// </param>
    /// <returns>
    /// <c>true</c> if the value changed; <c>false</c> if the existing
    /// value matched the new value.
    /// </returns>
    protected bool SetTrackedProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        OnPropertyChanging(propertyName);

        var oldValue = field;
        field = value;

        if (!_isTrackingSuspended && propertyName is not null)
            _changes.Record(propertyName, oldValue, value);

        OnPropertyChanged(propertyName);
        return true;
    }

    /// <inheritdoc />
    public ChangeEntry? Undo()
    {
        var entry = _changes.Undo();

        if (entry is null)
            return null;

        ApplyPropertyValue(entry.PropertyName, entry.OldValue);
        return entry;
    }

    /// <inheritdoc />
    public ChangeEntry? Redo()
    {
        var entry = _changes.Redo();

        if (entry is null)
            return null;

        ApplyPropertyValue(entry.PropertyName, entry.NewValue);
        return entry;
    }

    /// <inheritdoc />
    public void AcceptChanges() => _changes.AcceptAll();

    /// <summary>
    /// Reverts all pending changes in reverse order, restoring each property
    /// to its original value, then clears the change history.
    /// </summary>
    public void RejectChanges()
    {
        _isTrackingSuspended = true;

        try
        {
            var entries = _changes.GetAll();

            for (int i = entries.Count - 1; i >= 0; i--)
                ApplyPropertyValue(entries[i].PropertyName, entries[i].OldValue);
        }
        finally
        {
            _isTrackingSuspended = false;
            _changes.Clear();
        }
    }

    /// <summary>
    /// Suspends change tracking until the returned scope is disposed.
    /// Changes made during the scope are not recorded.
    /// </summary>
    /// <returns>A disposable that resumes tracking on disposal.</returns>
    protected IDisposable SuspendTracking() =>
        new TrackingSuspensionScope(this);

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Raises the <see cref="PropertyChanging"/> event.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected virtual void OnPropertyChanging([CallerMemberName] string? propertyName = null) =>
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

    private void ApplyPropertyValue(string propertyName, object? value)
    {
        var prop = GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (prop is null || !prop.CanWrite)
            return;

        _isTrackingSuspended = true;

        try
        {
            prop.SetValue(this, value);
        }
        finally
        {
            _isTrackingSuspended = false;
        }
    }

    private sealed class TrackingSuspensionScope : IDisposable
    {
        private readonly ChangeTrackingComponent _owner;
        private readonly bool _previousState;
        private bool _disposed;

        public TrackingSuspensionScope(ChangeTrackingComponent owner)
        {
            _owner = owner;
            _previousState = owner._isTrackingSuspended;
            owner._isTrackingSuspended = true;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _owner._isTrackingSuspended = _previousState;
        }
    }
}
