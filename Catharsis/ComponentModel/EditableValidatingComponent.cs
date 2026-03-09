using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Catharsis.ComponentModel;

///<summary>
///An <see cref="EditableComponent"/> that additionally implements <see cref="INotifyDataErrorInfo"/>, combining
///snapshot-based transactional editing with per-property validation support.
///</summary>
///<remarks>
public abstract class EditableValidatingComponent : EditableComponent, INotifyDataErrorInfo
{
    #region Fields
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);
    #endregion

    #region Events
    ///<inheritdoc/>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    #endregion

    #region Protected methods
    ///<summary>
    ///Adds a single validation error for the specified property.
    ///</summary>
    ///<param name="error">The error message.</param>
    ///<param name="propertyName">
    ///The name of the property. Automatically provided by the compiler.
    ///</param>
    protected void AddError(string error, [CallerMemberName] string? propertyName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        if (!_errors.TryGetValue(propertyName, out List<string>? list))
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

    ///<summary>
    ///Clears all validation errors for all properties.
    ///</summary>
    protected void ClearAllErrors()
    {
        List<string> properties = [.. _errors.Keys];
        _errors.Clear();

        foreach (string property in properties)
        {
            OnErrorsChanged(property);
        }
    }

    ///<summary>
    ///Clears errors for the specified property.
    ///</summary>
    ///<param name="propertyName">The property name.</param>
    protected void ClearErrors([CallerMemberName] string? propertyName = null)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        if (_errors.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
        }
    }

    ///<summary>
    ///Raises the <see cref="ErrorsChanged"/> event.
    ///</summary>
    ///<param name="propertyName">The property name.</param>
    protected virtual void OnErrorsChanged(string? propertyName) { ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName)); }

    ///<summary>
    ///Sets the validation errors for the specified property.
    ///</summary>
    ///<param name="errors">The error messages.</param>
    ///<param name="propertyName">
    ///The name of the property. Automatically provided by the compiler.
    ///</param>
    protected void SetErrors(IEnumerable<string> errors, [CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        List<string> errorList = [.. errors.Where(e => !string.IsNullOrWhiteSpace(e))];

        if (errorList.Count == 0)
        {
            ClearErrors(propertyName);
            return;
        }

        _errors[propertyName] = errorList;
        OnErrorsChanged(propertyName);
    }

    ///<summary>
    ///Sets the backing field, raises change notifications, and validates the property in a single operation.
    ///</summary>
    ///<typeparam name="T">The type of the property.</typeparam>
    ///<param name="field">A reference to the backing field.</param>
    ///<param name="value">The new value.</param>
    ///<param name="propertyName">
    ///The name of the property. Automatically provided by the compiler.
    ///</param>
    ///<returns>
    protected bool SetPropertyAndValidate<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref field, value, propertyName))
        {
            return false;
        }

        ValidateProperty(propertyName, value);
        return true;
    }

    ///<summary>
    ///Called to validate a single property value. Override to provide custom validation logic. The default
    ///implementation runs DataAnnotations validation against the property.
    ///</summary>
    ///<param name="propertyName">The name of the property to validate.</param>
    ///<param name="value">The current value of the property.</param>
    protected virtual void ValidateProperty(string? propertyName, object? value)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        ClearErrors(propertyName);

        ValidationContext context = new ValidationContext(this) { MemberName = propertyName };
        List<ValidationResult> results = new List<ValidationResult>();

        Validator.TryValidateProperty(value, context, results);

        List<string> messages = [.. results
            .Where(r => !string.IsNullOrWhiteSpace(r.ErrorMessage))
            .Select(r => r.ErrorMessage!)];

        if (messages.Count > 0)
        {
            SetErrors(messages, propertyName);
        }
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.Values.SelectMany(static e => e);
        }

        return _errors.TryGetValue(propertyName, out List<string>? errors) ? errors : [];
    }

    ///<summary>
    ///Validates all public properties of this component using DataAnnotations.
    ///</summary>
    ///<returns><c>true</c> if all properties pass validation; otherwise, <c>false</c>.</returns>
    public bool ValidateAllProperties()
    {
        ClearAllErrors();

        ValidationContext context = new ValidationContext(this);
        List<ValidationResult> results = new List<ValidationResult>();

        Validator.TryValidateObject(this, context, results, validateAllProperties: true);

        foreach (ValidationResult result in results)
        {
            List<string> members = [.. result.MemberNames];
            string message = result.ErrorMessage ?? "Validation failed.";

            if (members.Count == 0)
            {
                AddError(message, string.Empty);
            }
            else
            {
                foreach (string member in members)
                {
                    AddError(message, member);
                }
            }
        }

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
    #endregion
}
