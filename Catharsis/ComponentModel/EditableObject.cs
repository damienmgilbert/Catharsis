using System.ComponentModel;
using System.Reflection;

namespace Catharsis.ComponentModel;

/// <summary>
/// Abstract base class implementing <see cref="IEditableObject"/> with
/// automatic snapshot-based state management for transactional editing.
/// Captures and restores all public instance properties that have both
/// a getter and a setter.
/// </summary>
public abstract class EditableObject : IEditableObject
{
    private Dictionary<string, object?>? _snapshot;
    private bool _isEditing;

    /// <summary>
    /// Gets a value indicating whether the object is currently in edit mode.
    /// </summary>
    public bool IsEditing => _isEditing;

    /// <summary>
    /// Begins an edit on the object, capturing a snapshot of the current property values.
    /// </summary>
    public void BeginEdit()
    {
        if (_isEditing)
            return;

        _isEditing = true;
        _snapshot = CaptureSnapshot();
    }

    /// <summary>
    /// Discards changes since the last <see cref="BeginEdit"/> call,
    /// restoring the captured snapshot.
    /// </summary>
    public void CancelEdit()
    {
        if (!_isEditing)
            return;

        if (_snapshot is not null)
        {
            RestoreSnapshot(_snapshot);
            _snapshot = null;
        }

        _isEditing = false;
    }

    /// <summary>
    /// Pushes changes since the last <see cref="BeginEdit"/> call.
    /// The snapshot is discarded.
    /// </summary>
    public void EndEdit()
    {
        if (!_isEditing)
            return;

        _snapshot = null;
        _isEditing = false;
    }

    /// <summary>
    /// Returns the bindable properties of this object used for snapshotting.
    /// Override to customise which properties participate in edit transactions.
    /// </summary>
    protected virtual IEnumerable<PropertyInfo> GetEditableProperties()
    {
        return GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0);
    }

    private Dictionary<string, object?> CaptureSnapshot()
    {
        var snapshot = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var property in GetEditableProperties())
        {
            snapshot[property.Name] = property.GetValue(this);
        }

        return snapshot;
    }

    private void RestoreSnapshot(Dictionary<string, object?> snapshot)
    {
        foreach (var property in GetEditableProperties())
        {
            if (snapshot.TryGetValue(property.Name, out var value))
            {
                property.SetValue(this, value);
            }
        }
    }
}
