using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Catharsis.ComponentModel;

/// <summary>
/// A component that combines <see cref="ObservableComponent"/> with
/// <see cref="INotifyDataErrorInfo"/> and <see cref="IEditableObject"/>,
/// providing observable property notifications, per-property validation,
/// and snapshot-based transactional editing in a single base class.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="SetPropertyValidated{T}"/> in property setters to set,
/// notify, and validate in one call. Override <see cref="ValidateProperty"/>
/// or <see cref="ValidateAllProperties"/> for custom validation logic.
/// </para>
/// <para>
/// Call <see cref="BeginEdit"/> to snapshot current state, <see cref="CancelEdit"/>
/// to revert, or <see cref="EndEdit"/> to commit. Validation errors are
/// exposed via <see cref="INotifyDataErrorInfo"/>.
/// </para>
/// </remarks>
public abstract class ValidatingObservableComponent : ObservableComponent, INotifyDataErrorInfo, IEditableObject
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);
    private Dictionary<string, object?>? _snapshot;

    /// <summary>
    /// Gets a value indicating whether the component is currently in edit mode.
    /// </summary>
    public bool IsEditing { get; private set; }

    /// <inheritdoc />
    public bool HasErrors => _errors.Count > 0;

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
    /// Sets the backing field, raises change notifications, and validates
    /// the property in a single operation.
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
    protected bool SetPropertyValidated<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref field, value, propertyName))
            return false;

        ValidateProperty(propertyName, value);
        return true;
    }

    /// <summary>
    /// Called to validate a single property value. Override to provide custom
    /// validation logic. The default implementation runs DataAnnotations
    /// validation against the property.
    /// </summary>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <param name="value">The current value of the property.</param>
    protected virtual void ValidateProperty(string? propertyName, object? value)
    {
        if (string.IsNullOrEmpty(propertyName))
            return;

        ClearErrors(propertyName);

        var context = new ValidationContext(this) { MemberName = propertyName };
        var results = new List<ValidationResult>();

        Validator.TryValidateProperty(value, context, results);

        var messages = results
            .Where(r => !string.IsNullOrWhiteSpace(r.ErrorMessage))
            .Select(r => r.ErrorMessage!)
            .ToList();

        if (messages.Count > 0)
            SetErrors(messages, propertyName);
    }

    /// <summary>
    /// Validates all public properties of this component using DataAnnotations.
    /// </summary>
    /// <returns><c>true</c> if all properties pass validation; otherwise, <c>false</c>.</returns>
    public bool ValidateAllProperties()
    {
        ClearAllErrors();

        var context = new ValidationContext(this);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(this, context, results, validateAllProperties: true);

        foreach (var result in results)
        {
            var members = result.MemberNames.ToList();
            var message = result.ErrorMessage ?? "Validation failed.";

            if (members.Count == 0)
            {
                AddError(message, string.Empty);
            }
            else
            {
                foreach (var member in members)
                    AddError(message, member);
            }
        }

        return !HasErrors;
    }

    /// <summary>
    /// Sets the validation errors for the specified property.
    /// </summary>
    /// <param name="errors">The error messages.</param>
    /// <param name="propertyName">
    /// The name of the property. Automatically provided by the compiler.
    /// </param>
    protected void SetErrors(
        IEnumerable<string> errors,
        [CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (string.IsNullOrEmpty(propertyName))
            return;

        var errorList = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToList();

        if (errorList.Count == 0)
        {
            ClearErrors(propertyName);
            return;
        }

        _errors[propertyName] = errorList;
        OnErrorsChanged(propertyName);
    }

    /// <summary>
    /// Adds a single validation error for the specified property.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="propertyName">
    /// The name of the property. Automatically provided by the compiler.
    /// </param>
    protected void AddError(
        string error,
        [CallerMemberName] string? propertyName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        if (string.IsNullOrEmpty(propertyName))
            return;

        if (!_errors.TryGetValue(propertyName, out var list))
        {
            list = [];
            _errors[propertyName] = list;
        }

        if (!list.Contains(error))
        {
            list.Add(error);
            OnErrorsChanged(propertyName);
        }
    }

    /// <summary>
    /// Clears errors for the specified property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected void ClearErrors([CallerMemberName] string? propertyName = null)
    {
        if (string.IsNullOrEmpty(propertyName))
            return;

        if (_errors.Remove(propertyName))
            OnErrorsChanged(propertyName);
    }

    /// <summary>
    /// Clears all validation errors for all properties.
    /// </summary>
    protected void ClearAllErrors()
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

    /// <inheritdoc />
    public void BeginEdit()
    {
        ThrowIfDisposed();

        if (IsEditing)
            return;

        IsEditing = true;
        _snapshot = CaptureSnapshot();
    }

    /// <inheritdoc />
    public void CancelEdit()
    {
        ThrowIfDisposed();

        if (!IsEditing)
            return;

        if (_snapshot is not null)
        {
            RestoreSnapshot(_snapshot);
            _snapshot = null;
        }

        IsEditing = false;
    }

    /// <inheritdoc />
    public void EndEdit()
    {
        ThrowIfDisposed();

        if (!IsEditing)
            return;

        _snapshot = null;
        IsEditing = false;
    }

    /// <summary>
    /// Returns the properties that participate in edit transactions.
    /// Override to customize.
    /// </summary>
    protected virtual IEnumerable<PropertyInfo> GetEditableProperties()
    {
        return GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead
                        && p.CanWrite
                        && p.GetIndexParameters().Length == 0
                        && p.Name != nameof(Site)
                        && p.Name != nameof(IsEditing));
    }

    private Dictionary<string, object?> CaptureSnapshot()
    {
        var snapshot = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var property in GetEditableProperties())
            snapshot[property.Name] = property.GetValue(this);

        return snapshot;
    }

    private void RestoreSnapshot(Dictionary<string, object?> snapshot)
    {
        foreach (var property in GetEditableProperties())
        {
            if (snapshot.TryGetValue(property.Name, out var value))
                property.SetValue(this, value);
        }
    }
}
