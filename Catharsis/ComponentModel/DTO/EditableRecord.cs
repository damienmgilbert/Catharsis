using System.ComponentModel;

namespace Catharsis.ComponentModel.DTO;

///<summary>
///A <see cref="BindableRecord{T}"/> that implements <see cref="IEditableObject"/> with snapshot-based transactional
///editing and <see cref="IRevertibleChangeTracking"/>.
///</summary>
///<typeparam name="T">The record type to wrap.</typeparam>
///<remarks>
///<para> Call <see cref="BeginEdit"/> to capture a snapshot of the current value,<see cref="CancelEdit"/> to revert to
///the snapshot, or <see cref="EndEdit"/> to commit the changes. <see cref="AcceptChanges"/> and<see
///cref="RejectChanges"/> provide <see cref="IRevertibleChangeTracking"/> semantics.</para>
///</remarks>
///<remarks>
///Initializes a new instance of <see cref="EditableRecord{T}"/>.
///</remarks>
///<param name="value">The initial record value.</param>
///<exception cref="ArgumentNullException">
///<paramref name="value"/> is <c>null</c>.
///</exception>
public class EditableRecord<T>(T value) : BindableRecord<T>(value), IEditableObject, IRevertibleChangeTracking where T : class
{
    #region Fields
    T? _acceptedValue = value;
    bool _isEditing;
    T? _snapshot;

    #endregion
    #region Constructors
    #endregion

    #region Public methods
    ///<summary>
    ///Accepts the current value as the new baseline, resetting <see cref="IsChanged"/> to <c>false</c>.
    ///</summary>
    public void AcceptChanges()
    {
        _acceptedValue = Value;
        OnPropertyChanged(nameof(IsChanged));
    }

    ///<summary>
    ///Begins an edit on the record, capturing a snapshot of the current value.
    ///</summary>
    public void BeginEdit()
    {
        if (_isEditing)
        {
            return;
        }

        _isEditing = true;
        _snapshot = Value;
    }

    ///<summary>
    ///Discards changes since the last <see cref="BeginEdit"/> call, restoring the snapshot.
    ///</summary>
    public void CancelEdit()
    {
        if (!_isEditing)
        {
            return;
        }

        if (_snapshot is not null)
        {
            Value = _snapshot;
        }

        _snapshot = null;
        _isEditing = false;
    }

    ///<summary>
    ///Commits changes since the last <see cref="BeginEdit"/> call. The snapshot is discarded.
    ///</summary>
    public void EndEdit()
    {
        if (!_isEditing)
        {
            return;
        }

        _snapshot = null;
        _isEditing = false;
    }

    ///<summary>
    ///Reverts the current value to the last accepted baseline.
    ///</summary>
    public void RejectChanges()
    {
        if (_acceptedValue is not null)
        {
            Value = _acceptedValue;
        }

        OnPropertyChanged(nameof(IsChanged));
    }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public bool IsChanged => !ReferenceEquals(Value, _acceptedValue) && !Equals(Value, _acceptedValue);

    ///<summary>
    ///Gets a value indicating whether the object is currently in edit mode.
    ///</summary>
    public bool IsEditing => _isEditing;
    #endregion
}
