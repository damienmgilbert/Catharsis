using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.ComponentModel.DTO;

///<summary>
///A <see cref="BindableRecord{T}"/> that additionally implements <see cref="INotifyDataErrorInfo"/>, running
///DataAnnotations validation against the record value whenever it changes.
///</summary>
///<typeparam name="T">The record type to wrap and validate.</typeparam>
///<remarks>
public class ValidatedRecord<T> : BindableRecord<T>, INotifyDataErrorInfo where T : class
{
    #region Fields
    private readonly Dictionary<string, List<string>> _errors = [with(StringComparer.Ordinal)];
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="ValidatedRecord{T}"/>.
    ///</summary>
    ///<param name="value">The initial record value.</param>
    ///<exception cref="ArgumentNullException">
    public ValidatedRecord(T value) : base(value)
    {
    }
    #endregion

    #region Events
    ///<inheritdoc/>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    #endregion

    #region Private methods
    private void AddError(string propertyName, string error)
    {
        if (!_errors.TryGetValue(propertyName, out List<string>? list))
        {
            list = [];
            _errors[propertyName] = list;
        }

        list.Add(error);
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ClearAllErrors()
    {
        string[] keys = [.. _errors.Keys];
        _errors.Clear();

        foreach (string key in keys)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));
        }
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (string.Equals(propertyName, nameof(Value), StringComparison.Ordinal))
        {
            Validate();
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
    ///Performs validation against the current record value and returns <c>true</c> if valid.
    ///</summary>
    ///<returns><c>true</c> if the record passes validation; otherwise, <c>false</c>.</returns>
    public bool Validate()
    {
        ClearAllErrors();

        ValidationContext context = new(Value);
        List<ValidationResult> results = [];

        Validator.TryValidateObject(Value, context, results, validateAllProperties: true);

        foreach (ValidationResult result in results)
        {
            List<string> members = [.. result.MemberNames];

            if (members.Count == 0)
            {
                AddError(string.Empty, result.ErrorMessage ?? "Validation failed.");
            }
            else
            {
                foreach (string member in members)
                {
                    AddError(member, result.ErrorMessage ?? "Validation failed.");
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
    public IReadOnlyDictionary<string, IReadOnlyList<string>> CurrentErrors => _errors.ToDictionary(static kvp => kvp.Key, static kvp => (IReadOnlyList<string>)kvp.Value.AsReadOnly(), StringComparer.Ordinal);

    ///<inheritdoc/>
    public bool HasErrors => _errors.Count > 0;
    #endregion
}
