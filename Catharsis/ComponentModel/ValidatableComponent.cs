using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Catharsis.ComponentModel;

///<summary>
///An <see cref="ObservableComponent"/> that implements <see cref="INotifyDataErrorInfo"/> to provide per-property
///validation support with user-friendly error messages.
///</summary>
///<remarks>
public abstract class ValidatableComponent : ObservableComponent, INotifyDataErrorInfo
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

        if (!_errors.TryGetValue(propertyName, out List<string>? errors))
        {
            errors = [];
            _errors[propertyName] = errors;
        }

        if (!errors.Contains(error, StringComparer.Ordinal))
        {
            errors.Add(error);
            OnErrorsChanged(propertyName);
        }
    }

    ///<summary>
    ///Clears all validation errors for all properties.
    ///</summary>
    protected void ClearAllErrors()
    {
        List<string> propertyNames = [.. _errors.Keys];
        _errors.Clear();

        foreach (string name in propertyNames)
        {
            OnErrorsChanged(name);
        }
    }

    ///<summary>
    ///Clears all validation errors for the specified property.
    ///</summary>
    ///<param name="propertyName">
    ///The name of the property. Automatically provided by the compiler.
    ///</param>
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

    ///<inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _errors.Clear();
            ErrorsChanged = null;
        }

        base.Dispose(disposing);
    }

    ///<summary>
    ///Raises the <see cref="ErrorsChanged"/> event for the specified property.
    ///</summary>
    ///<param name="propertyName">The name of the property whose errors changed.</param>
    protected virtual void OnErrorsChanged(string? propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    ///<summary>
    ///Sets the validation errors for the specified property.
    ///</summary>
    ///<param name="errors">The error messages to associate with the property.</param>
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

        List<string> errorList = [.. errors.Where(static e => !string.IsNullOrWhiteSpace(e))];

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
    ///<param name="value">The FileName value.</param>
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
    ///Called to validate a property value. Override to provide custom validation logic. The default implementation does
    ///nothing.
    ///</summary>
    ///<param name="propertyName">The name of the property to validate.</param>
    ///<param name="value">The current value of the property.</param>
    protected virtual void ValidateProperty(string? propertyName, object? value)
    {
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
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public bool HasErrors => _errors.Count > 0;
    #endregion
}
