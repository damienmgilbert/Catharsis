using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.ComponentModel.DTO;

/// <summary>
/// A <see cref="BindableRecord{T}"/> that additionally implements
/// <see cref="INotifyDataErrorInfo"/>, running DataAnnotations validation
/// against the record value whenever it changes.
/// </summary>
/// <typeparam name="T">The record type to wrap and validate.</typeparam>
/// <remarks>
/// <para>
/// Validation is performed automatically when <see cref="BindableRecord{T}.Value"/>
/// is set. The <see cref="HasErrors"/> property and <see cref="ErrorsChanged"/>
/// event integrate with standard data-binding error display.
/// </para>
/// </remarks>
public class ValidatedRecord<T> : BindableRecord<T>, INotifyDataErrorInfo
    where T : class
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of <see cref="ValidatedRecord{T}"/>.
    /// </summary>
    /// <param name="value">The initial record value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <c>null</c>.
    /// </exception>
    public ValidatedRecord(T value) : base(value) { }

    /// <inheritdoc />
    public bool HasErrors => _errors.Count > 0;

    /// <inheritdoc />
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <inheritdoc />
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.Values.SelectMany(e => e);

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
    /// Performs validation against the current record value and returns
    /// <c>true</c> if valid.
    /// </summary>
    /// <returns><c>true</c> if the record passes validation; otherwise, <c>false</c>.</returns>
    public bool Validate()
    {
        ClearAllErrors();

        var context = new ValidationContext(Value);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(Value, context, results, validateAllProperties: true);

        foreach (var result in results)
        {
            var members = result.MemberNames.ToList();

            if (members.Count == 0)
            {
                AddError(string.Empty, result.ErrorMessage ?? "Validation failed.");
            }
            else
            {
                foreach (var member in members)
                    AddError(member, result.ErrorMessage ?? "Validation failed.");
            }
        }

        return !HasErrors;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (string.Equals(propertyName, nameof(Value), StringComparison.Ordinal))
            Validate();
    }

    private void AddError(string propertyName, string error)
    {
        if (!_errors.TryGetValue(propertyName, out var list))
        {
            list = [];
            _errors[propertyName] = list;
        }

        list.Add(error);
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ClearAllErrors()
    {
        var keys = _errors.Keys.ToArray();
        _errors.Clear();

        foreach (var key in keys)
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));
    }
}
