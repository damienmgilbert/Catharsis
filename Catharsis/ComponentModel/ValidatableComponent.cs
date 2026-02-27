using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Catharsis.ComponentModel;

/// <summary>
/// An <see cref="ObservableComponent"/> that implements <see cref="INotifyDataErrorInfo"/>
/// to provide per-property validation support with user-friendly error messages.
/// </summary>
/// <remarks>
/// <para>
/// Derived classes call <see cref="SetErrors"/> or <see cref="ClearErrors"/>
/// to manage validation errors. The <see cref="ValidateProperty"/> method
/// can be overridden to implement custom validation logic that runs
/// automatically when properties change via <see cref="ObservableComponent.SetProperty{T}"/>.
/// </para>
/// <para>
/// Use <see cref="SetPropertyAndValidate{T}"/> to combine property setting
/// with automatic validation in a single call.
/// </para>
/// </remarks>
public abstract class ValidatableComponent : ObservableComponent, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public bool HasErrors => _errors.Count > 0;

    /// <inheritdoc />
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <inheritdoc />
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.Values.SelectMany(e => e);
        }

        return _errors.TryGetValue(propertyName, out var errors)
            ? errors
            : [];
    }

    /// <summary>
    /// Sets the backing field, raises change notifications, and validates
    /// the property in a single operation.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">A reference to the backing field.</param>
    /// <param name="value">The FileName value.</param>
    /// <param name="propertyName">
    /// The name of the property. Automatically provided by the compiler.
    /// </param>
    /// <returns>
    /// <c>true</c> if the value changed; <c>false</c> if the existing
    /// value matched the FileName value.
    /// </returns>
    protected bool SetPropertyAndValidate<T>(
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
    /// Called to validate a property value. Override to provide custom
    /// validation logic. The default implementation does nothing.
    /// </summary>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <param name="value">The current value of the property.</param>
    protected virtual void ValidateProperty(string? propertyName, object? value) { }

    /// <summary>
    /// Sets the validation errors for the specified property.
    /// </summary>
    /// <param name="errors">The error messages to associate with the property.</param>
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

        if (!_errors.TryGetValue(propertyName, out var errors))
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

    /// <summary>
    /// Clears all validation errors for the specified property.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property. Automatically provided by the compiler.
    /// </param>
    protected void ClearErrors([CallerMemberName] string? propertyName = null)
    {
        if (string.IsNullOrEmpty(propertyName))
            return;

        if (_errors.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
        }
    }

    /// <summary>
    /// Clears all validation errors for all properties.
    /// </summary>
    protected void ClearAllErrors()
    {
        var propertyNames = _errors.Keys.ToList();
        _errors.Clear();

        foreach (var name in propertyNames)
        {
            OnErrorsChanged(name);
        }
    }

    /// <summary>
    /// Raises the <see cref="ErrorsChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property whose errors changed.</param>
    protected virtual void OnErrorsChanged(string? propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _errors.Clear();
            ErrorsChanged = null;
        }

        base.Dispose(disposing);
    }
}
