using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Catharsis.ComponentModel.DTO;

///<summary>
///A record wrapper that combines <see cref="BindableRecord{T}"/> functionality with <see cref="INotifyDataErrorInfo"/>
///validation, <see cref="IEditableObject"/> transactional editing, and <see cref="IRevertibleChangeTracking"/> support.
///
///</summary>
///<typeparam name="T">The record type to wrap, validate, and edit.</typeparam>
///<remarks>
public class BindableValidatedRecord<T> : BindableRecord<T>, INotifyDataErrorInfo, IEditableObject, IRevertibleChangeTracking where T : class
{
    #region Fields
    private T? _acceptedValue;
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);
    private bool _isEditing;
    private T? _snapshot;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="BindableValidatedRecord{T}"/>.
    ///</summary>
    ///<param name="value">The initial record value.</param>
    ///<exception cref="ArgumentNullException">
    public BindableValidatedRecord(T value) : base(value) { _acceptedValue = value; }
    #endregion

    #region Events
    ///<inheritdoc/>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    #endregion

    #region Private methods
    private void AddError(string propertyName, string message)
    {
        if (!_errors.TryGetValue(propertyName, out List<string>? list))
        {
            list = [];
            _errors[propertyName] = list;
        }

        if (!list.Contains(message))
        {
            list.Add(message);
            OnErrorsChanged(propertyName);
        }
    }

    private void ClearAllErrors()
    {
        List<string> properties = [.. _errors.Keys];
        _errors.Clear();

        foreach (string property in properties)
        {
            OnErrorsChanged(property);
        }
    }
    #endregion

    #region Protected methods
    ///<summary>
    ///Raises the <see cref="ErrorsChanged"/> event.
    ///</summary>
    ///<param name="propertyName">The property name.</param>
    protected virtual void OnErrorsChanged(string? propertyName) { ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName)); }

    ///<inheritdoc/>
    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (string.Equals(propertyName, nameof(Value), StringComparison.Ordinal))
        {
            Validate();
        }
    }

    ///<summary>
    ///Called to validate the record value. Override for custom validation. The default implementation runs
    ///DataAnnotations validation.
    ///</summary>
    ///<param name="value">The value to validate.</param>
    protected virtual void ValidateValue(T value)
    {
        ValidationContext context = new ValidationContext(value);
        List<ValidationResult> results = new List<ValidationResult>();

        Validator.TryValidateObject(value, context, results, validateAllProperties: true);

        foreach (ValidationResult result in results)
        {
            List<string> members = [.. result.MemberNames];
            string message = result.ErrorMessage ?? "Validation failed.";

            if (members.Count == 0)
            {
                AddError(string.Empty, message);
            }
            else
            {
                foreach (string member in members)
                {
                    AddError(member, message);
                }
            }
        }
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void AcceptChanges()
    {
        _acceptedValue = Value;
        OnPropertyChanged(nameof(IsChanged));
    }

    ///<summary>
    ///Begins an edit, capturing a snapshot of the current value.
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
    ///Discards changes since the last <see cref="BeginEdit"/> call.
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
    ///Commits changes since the last <see cref="BeginEdit"/> call.
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

    ///<inheritdoc/>
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.Values.SelectMany(static e => e);
        }

        return _errors.TryGetValue(propertyName, out List<string>? errors) ? errors : [];
    }

    ///<inheritdoc/>
    public void RejectChanges()
    {
        if (_acceptedValue is not null)
        {
            Value = _acceptedValue;
        }

        OnPropertyChanged(nameof(IsChanged));
    }

    ///<summary>
    ///Validates the current record value using DataAnnotations. Override <see cref="ValidateValue"/> for custom logic.
    ///</summary>
    ///<returns><c>true</c> if the record passes validation; otherwise, <c>false</c>.</returns>
    public bool Validate()
    {
        ClearAllErrors();
        ValidateValue(Value);
        return !HasErrors;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the current validation errors as a read-only dictionary.
    ///</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> CurrentErrors => _errors.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<string>)kvp.Value.AsReadOnly(), StringComparer.Ordinal);

    ///<inheritdoc/>
    public bool HasErrors => _errors.Count > 0;

    ///<inheritdoc/>
    public bool IsChanged => !ReferenceEquals(Value, _acceptedValue) && !Equals(Value, _acceptedValue);

    ///<summary>
    ///Gets a value indicating whether the record is currently in edit mode.
    ///</summary>
    public bool IsEditing => _isEditing;
    #endregion
}
