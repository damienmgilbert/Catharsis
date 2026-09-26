using System.ComponentModel;
using System.Reflection;

namespace Catharsis.ComponentModel;

///<summary>
///An <see cref="ObservableComponent"/> that implements <see cref="IEditableObject"/> with automatic snapshot-based
///state management for transactional editing.
///</summary>
///<remarks>
///<para> Call <see cref="BeginEdit"/> to capture a snapshot of all editable property values.<see cref="CancelEdit"/>
///restores the snapshot; <see cref="EndEdit"/> discards it and accepts the current values.</para> <para> Override <see
///cref="GetEditableProperties"/> to control which properties participate in edit transactions.</para>
///</remarks>
public abstract class EditableComponent : ObservableComponent, IEditableObject
{
    #region Fields
    Dictionary<string, object?>? _snapshot;
    #endregion

    #region Private methods
    Dictionary<string, object?> CaptureSnapshot()
    {
        Dictionary<string, object?> snapshot = [with(StringComparer.Ordinal)];

        foreach(PropertyInfo property in GetEditableProperties())
        {
            snapshot[property.Name] = property.GetValue(this);
        }

        return snapshot;
    }

    void RestoreSnapshot(Dictionary<string, object?> snapshot)
    {
        foreach(PropertyInfo property in GetEditableProperties())
        {
            if(snapshot.TryGetValue(property.Name, out object? value))
            {
                object? current = property.GetValue(this);

                if(!Equals(current, value))
                {
                    property.SetValue(this, value);
                    OnPropertyChanged(property.Name);
                }
            }
        }
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if(disposing)
        {
            _snapshot = null;
            IsEditing = false;
        }

        base.Dispose(disposing);
    }

    ///<summary>
    ///Returns the bindable properties of this object used for snapshotting. Override to customise which properties
    ///participate in edit transactions.
    ///</summary>
    ///<returns>
    ///An enumerable of <see cref="PropertyInfo"/> instances representing the properties to include in snapshots.
    ///</returns>
    protected virtual IEnumerable<PropertyInfo> GetEditableProperties() { return GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(static p => p.CanRead && p.CanWrite && (p.GetIndexParameters().Length == 0) && (p.Name != nameof(Site)) && (p.Name != nameof(IsEditing))); }
    #endregion

    #region Public methods
    ///<summary>
    ///Begins an edit on the object, capturing a snapshot of the current property values. Nested calls are ignored.
    ///</summary>
    public void BeginEdit()
    {
        ThrowIfDisposed();

        if(IsEditing)
        {
            return;
        }

        IsEditing = true;
        _snapshot = CaptureSnapshot();
    }

    ///<summary>
    ///Discards changes since the last <see cref="BeginEdit"/> call, restoring the captured snapshot.
    ///</summary>
    public void CancelEdit()
    {
        ThrowIfDisposed();

        if(!IsEditing)
        {
            return;
        }

        if(_snapshot is not null)
        {
            RestoreSnapshot(_snapshot);
            _snapshot = null;
        }

        IsEditing = false;
    }

    ///<summary>
    ///Pushes changes since the last <see cref="BeginEdit"/> call. The snapshot is discarded.
    ///</summary>
    public void EndEdit()
    {
        ThrowIfDisposed();

        if(!IsEditing)
        {
            return;
        }

        _snapshot = null;
        IsEditing = false;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a value indicating whether the object is currently in edit mode.
    ///</summary>
    public bool IsEditing { get; private set; }
    #endregion
}
