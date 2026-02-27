using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Catharsis.DataAnnotations;

///<summary>
///Delegates validation to a named static method on a specified type, similar in purpose to <see
///cref="CustomValidationAttribute"/> but with a simplified contract: the method receives the value and returns a <see
///cref="bool"/>.
///</summary>
///<remarks>
///<para> The referenced method must be <c>public static</c> and have one of these signatures:</para> <list
///type="bullet"><item><c>static bool MethodName(object? value)</c></item><item><c>static bool MethodName(T value)</c>
///where T matches the property type</item></list> <para>A <c>null</c> value is passed through to the predicate.</para>
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class PredicateValidationAttribute : ValidationAttribute
{
    #region Constructors
    ///<summary>
    ///Initializes a FileName instance of <see cref="PredicateValidationAttribute"/>.
    ///</summary>
    ///<param name="validatorType">The type containing the predicate method.</param>
    ///<param name="methodName">The name of the <c>public static bool</c> method.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="validatorType"/> or <paramref name="methodName"/> is <c>null</c>.
    ///</exception>
    public PredicateValidationAttribute(Type validatorType, string methodName) : base("The field {0} failed predicate validation ({1}.{2}).")
    {
        ArgumentNullException.ThrowIfNull(validatorType);
        ArgumentNullException.ThrowIfNull(methodName);

        ValidatorType = validatorType;
        MethodName = methodName;
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        MethodInfo? method = ValidatorType.GetMethod(MethodName, BindingFlags.Public | BindingFlags.Static);

        if((method is null) || (method.ReturnType != typeof(bool)))
        {
            return new ValidationResult($"Predicate method '{ValidatorType.Name}.{MethodName}' was not found or does not return bool.");
        }

        ParameterInfo[] parameters = method.GetParameters();
        if(parameters.Length != 1)
        {
            return new ValidationResult($"Predicate method '{ValidatorType.Name}.{MethodName}' must accept exactly one parameter.");
        }

        bool result;
        try
        {
            object? invokeResult = method.Invoke(null, [ value ]);
            result = invokeResult is true;
        } catch(TargetInvocationException ex)
        {
            return new ValidationResult($"Predicate method '{ValidatorType.Name}.{MethodName}' threw an exception: {ex.InnerException?.Message ?? ex.Message}.");
        }

        if(!result)
        {
            return new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName, ValidatorType.Name, MethodName), (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
        }

        return ValidationResult.Success;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override string FormatErrorMessage(string name) { return string.Format(ErrorMessageString, name, ValidatorType.Name, MethodName); }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the name of the static predicate method.
    ///</summary>
    public string MethodName { get; }

    ///<summary>
    ///Gets the type that contains the predicate method.
    ///</summary>
    public Type ValidatorType { get; }
    #endregion
}
