using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.ComponentModel.Validation;

///<summary>
///Validates objects using <see cref="System.ComponentModel.DataAnnotations"/> attributes and exposes results through
public sealed class DataAnnotationValidator : INotifyDataErrorInfo
{
    #region Fields
    private readonly ValidationContextFactory _contextFactory;
    private readonly ErrorDictionary _errors = new();
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="DataAnnotationValidator"/>.
    ///</summary>
    ///<param name="contextFactory">
    ///An optional factory for creating <see cref="ValidationContext"/> instances. If <c>null</c>, a default factory is
    ///used.
    ///</param>
    public DataAnnotationValidator(ValidationContextFactory? contextFactory = null)
    {
        _contextFactory = contextFactory ?? new ValidationContextFactory();
        _errors.ErrorsChanged += (_, e) => ErrorsChanged?.Invoke(this, e);
    }
    #endregion

    #region Events
    ///<inheritdoc/>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    #endregion

    #region Public methods
    ///<summary>
    ///Clears all validation errors.
    ///</summary>
    public void ClearAll() => _errors.ClearAll();

    ///<inheritdoc/>
    public IEnumerable GetErrors(string? propertyName) => _errors.GetErrors(propertyName);

    ///<summary>
    ///Validates all annotated properties on the specified object and populates the <see cref="Errors"/> dictionary with
    ///results.
    ///</summary>
    ///<param name="instance">The object to validate.</param>
    ///<returns><c>true</c> if the object is valid; otherwise, <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException">
    public bool ValidateObject(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        _errors.ClearAll();

        ValidationContext context = _contextFactory.CreateContext(instance);
        List<ValidationResult> results = [];

        bool isValid = Validator.TryValidateObject(instance, context, results, validateAllProperties: true);

        foreach(ValidationResult result in results)
        {
            List<string> members = [ .. result.MemberNames ];

            if(members.Count == 0)
            {
                _errors.AddError(string.Empty, new ErrorInfo(result.ErrorMessage ?? "Validation failed."));
            } else
            {
                foreach(string member in members)
                {
                    _errors.AddError(member, new ErrorInfo(result.ErrorMessage ?? "Validation failed.", PropertyName: member));
                }
            }
        }

        return isValid;
    }

    ///<summary>
    ///Validates a single property on the specified object and updates the <see cref="Errors"/> dictionary for that
    ///property.
    ///</summary>
    ///<param name="instance">The object that owns the property.</param>
    ///<param name="propertyName">The property name.</param>
    ///<param name="value">The property value to validate.</param>
    ///<returns><c>true</c> if the property is valid; otherwise, <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException">
    public bool ValidateProperty(object instance, string propertyName, object? value)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(propertyName);

        _errors.ClearErrors(propertyName);

        ValidationContext context = _contextFactory.CreatePropertyContext(instance, propertyName);
        List<ValidationResult> results = [];

        bool isValid = Validator.TryValidateProperty(value, context, results);

        foreach(ValidationResult result in results)
        {
            _errors.AddError(propertyName, new ErrorInfo(result.ErrorMessage ?? "Validation failed.", PropertyName: propertyName));
        }

        return isValid;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the underlying error dictionary.
    ///</summary>
    public ErrorDictionary Errors => _errors;

    ///<inheritdoc/>
    public bool HasErrors => _errors.HasErrors;
    #endregion
}
