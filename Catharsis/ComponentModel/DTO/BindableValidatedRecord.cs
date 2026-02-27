using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Catharsis.ComponentModel.DTO;

/// <summary>
/// A record wrapper that combines <see cref="BindableRecord{T}"/> functionality
/// with <see cref="INotifyDataErrorInfo"/> validation, <see cref="IEditableObject"/>
/// transactional editing, and <see cref="IRevertibleChangeTracking"/> support.
/// </summary>
/// <typeparam name="T">The record type to wrap, validate, and edit.</typeparam>
/// <remarks>
/// <para>
/// Setting <see cref="BindableRecord{T}.Value"/> automatically triggers
/// DataAnnotations validation. Call <see cref="BeginEdit"/> to snapshot the
/// current value, <see cref="CancelEdit"/> to revert, or <see cref="EndEdit"/>
/// to commit. Override <see cref="ValidateValue"/> for custom validation logic.
/// </para>
/// </remarks>
public class BindableValidatedRecord<T> : BindableRecord<T>, INotifyDataErrorInfo, IEditableObject, IRevertibleChangeTracking
    where T : class
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);
    private T? _snapshot;
    private T? _acceptedValue;
    private bool _isEditing;

    /// <summary>
    /// Initializes a new instance of <see cref="BindableValidatedRecord{T}"/>.
    /// </summary>
    /// <param name="value">The initial record value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <c>null</c>.
    /// </exception>
    public BindableValidatedRecord(T value) : base(value)
    {
        _acceptedValue = value;
    }

    /// <inheritdoc />
    public bool HasErrors => _errors.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the record is currently in edit mode.
    /// </summary>
    public bool IsEditing => _isEditing;

    /// <inheritdoc />
    public bool IsChanged => !ReferenceEquals(Value, _acceptedValue)
                             && !Equals(Value, _acceptedValue);

    /// <inheritdoc />
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <inheritdoc />
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.Values.SelectMany(static e => e);

        return _errors.TryGetValue(propertyName, out var errors)
            ? errors
            : [];
    }

    /// <summary>
    /// Gets the current validation errors as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> CurrentErrors =>
        _errors.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<string>)kvp.Value.AsReadOnly(),
            StringComparer.Ordinal);

    /// <summary>
    /// Validates the current record value using DataAnnotations.
    /// Override <see cref="ValidateValue"/> for custom logic.
    /// </summary>
    /// <returns><c>true</c> if the record passes validation; otherwise, <c>false</c>.</returns>
    public bool Validate()
    {
        ClearAllErrors();
        ValidateValue(Value);
        return !HasErrors;
    }

    /// <summary>
    /// Called to validate the record value. Override for custom validation.
    /// The default implementation runs DataAnnotations validation.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    protected virtual void ValidateValue(T value)
    {
        var context = new ValidationContext(value);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(value, context, results, validateAllProperties: true);

        foreach (var result in results)
        {
            var members = result.MemberNames.ToList();
            var message = result.ErrorMessage ?? "Validation failed.";

            if (members.Count == 0)
            {
                AddError(string.Empty, message);
            }
            else
            {
                foreach (var member in members)
                    AddError(member, message);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (string.Equals(propertyName, nameof(Value), StringComparison.Ordinal))
            Validate();
    }

    /// <summary>
    /// Begins an edit, capturing a snapshot of the current value.
    /// </summary>
    public void BeginEdit()
    {
        if (_isEditing)
            return;

        _isEditing = true;
        _snapshot = Value;
    }

    /// <summary>
    /// Discards changes since the last <see cref="BeginEdit"/> call.
    /// </summary>
    public void CancelEdit()
    {
        if (!_isEditing)
            return;

        if (_snapshot is not null)
            Value = _snapshot;

        _snapshot = null;
        _isEditing = false;
    }

    /// <summary>
    /// Commits changes since the last <see cref="BeginEdit"/> call.
    /// </summary>
    public void EndEdit()
    {
        if (!_isEditing)
            return;

        _snapshot = null;
        _isEditing = false;
    }

    /// <inheritdoc />
    public void AcceptChanges()
    {
        _acceptedValue = Value;
        OnPropertyChanged(nameof(IsChanged));
    }

    /// <inheritdoc />
    public void RejectChanges()
    {
        if (_acceptedValue is not null)
            Value = _acceptedValue;

        OnPropertyChanged(nameof(IsChanged));
    }

    private void AddError(string propertyName, string message)
    {
        if (!_errors.TryGetValue(propertyName, out var list))
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
        var properties = _errors.Keys.ToList();
        _errors.Clear();

        foreach (var property in properties)
            OnErrorsChanged(property);
    }

    /// <summary>
    /// Raises the <see cref="ErrorsChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected virtual void OnErrorsChanged(string? propertyName) =>
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
}
