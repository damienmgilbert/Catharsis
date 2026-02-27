using System.Collections;
using System.ComponentModel;

namespace Catharsis.ComponentModel.Validation;

/// <summary>
/// A thread-safe dictionary that stores per-property <see cref="ErrorInfo"/>
/// collections and raises <see cref="ErrorsChanged"/> when the error state
/// of any property changes, compatible with <see cref="INotifyDataErrorInfo"/>.
/// </summary>
public sealed class ErrorDictionary : INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<ErrorInfo>> _errors = new(StringComparer.Ordinal);
    private readonly object _lock = new();

    /// <inheritdoc />
    public bool HasErrors
    {
        get
        {
            lock (_lock)
                return _errors.Count > 0;
        }
    }

    /// <inheritdoc />
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <inheritdoc />
    public IEnumerable GetErrors(string? propertyName)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.Values.SelectMany(e => e).ToList();

            return _errors.TryGetValue(propertyName, out var errors)
                ? errors.ToList()
                : [];
        }
    }

    /// <summary>
    /// Gets all <see cref="ErrorInfo"/> entries for the specified property.
    /// </summary>
    /// <param name="propertyName">
    /// The property name, or <c>null</c> to get all errors.
    /// </param>
    /// <returns>A read-only list of error entries.</returns>
    public IReadOnlyList<ErrorInfo> GetErrorInfos(string? propertyName)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.Values.SelectMany(e => e).ToList();

            return _errors.TryGetValue(propertyName, out var errors)
                ? errors.ToList()
                : [];
        }
    }

    /// <summary>
    /// Sets the errors for the specified property, replacing any existing errors.
    /// Raises <see cref="ErrorsChanged"/> if the error state changed.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="errors">
    /// The errors to set. If empty or <c>null</c>, existing errors are cleared.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is <c>null</c>.
    /// </exception>
    public void SetErrors(string propertyName, IEnumerable<ErrorInfo>? errors)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        bool changed;

        lock (_lock)
        {
            var errorList = errors?.ToList() ?? [];

            if (errorList.Count == 0)
            {
                changed = _errors.Remove(propertyName);
            }
            else
            {
                _errors[propertyName] = errorList;
                changed = true;
            }
        }

        if (changed)
            OnErrorsChanged(propertyName);
    }

    /// <summary>
    /// Adds a single error for the specified property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="error">The error to add.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> or <paramref name="error"/> is <c>null</c>.
    /// </exception>
    public void AddError(string propertyName, ErrorInfo error)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        ArgumentNullException.ThrowIfNull(error);

        lock (_lock)
        {
            if (!_errors.TryGetValue(propertyName, out var list))
            {
                list = [];
                _errors[propertyName] = list;
            }

            list.Add(error);
        }

        OnErrorsChanged(propertyName);
    }

    /// <summary>
    /// Clears all errors for the specified property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    public void ClearErrors(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        bool removed;

        lock (_lock)
        {
            removed = _errors.Remove(propertyName);
        }

        if (removed)
            OnErrorsChanged(propertyName);
    }

    /// <summary>
    /// Clears all errors for all properties.
    /// </summary>
    public void ClearAll()
    {
        string[] keys;

        lock (_lock)
        {
            keys = [.. _errors.Keys];
            _errors.Clear();
        }

        foreach (var key in keys)
            OnErrorsChanged(key);
    }

    /// <summary>
    /// Gets the total number of properties that currently have errors.
    /// </summary>
    public int PropertyErrorCount
    {
        get
        {
            lock (_lock)
                return _errors.Count;
        }
    }

    /// <summary>
    /// Gets the total number of individual error entries across all properties.
    /// </summary>
    public int TotalErrorCount
    {
        get
        {
            lock (_lock)
                return _errors.Values.Sum(e => e.Count);
        }
    }

    private void OnErrorsChanged(string propertyName) =>
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
}
