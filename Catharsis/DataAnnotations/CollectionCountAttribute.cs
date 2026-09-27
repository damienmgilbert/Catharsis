using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Catharsis.DataAnnotations;

///<summary>
///Validates that the number of elements in an <see cref="IEnumerable"/> falls within a specified range, analogous to
///<see cref="RangeAttribute"/> but operating on collection counts instead of scalar values.
///</summary>
///<remarks>
///A <c>null</c> value is considered valid (combine with <see cref="RequiredAttribute"/> to disallow nulls). If the
///value does not implement <see cref="IEnumerable"/>, validation fails.
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class CollectionCountAttribute : ValidationAttribute
{
    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="CollectionCountAttribute"/> with the specified minimum and maximum
    ///element counts.
    ///</summary>
    ///<param name="minimum">The minimum number of elements (inclusive).</param>
    ///<param name="maximum">The maximum number of elements (inclusive).</param>
    ///<exception cref="ArgumentOutOfRangeException">
    ///<paramref name="minimum"/> is negative, or <paramref name="maximum"/> is less than <paramref name="minimum"/>.
    ///</exception>
    public CollectionCountAttribute(int minimum, int maximum = int.MaxValue) : base("The field {0} must contain between {1} and {2} elements.")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimum);
        ArgumentOutOfRangeException.ThrowIfLessThan(maximum, minimum);

        Minimum = minimum;
        Maximum = maximum;
    }
    #endregion

    #region Private methods
    static int CountElements(IEnumerable enumerable)
    {
        if(enumerable is ICollection collection)
        {
            return collection.Count;
        }

        int count = 0;
        IEnumerator enumerator = enumerable.GetEnumerator();
        try
        {
            while(enumerator.MoveNext())
            {
                count++;
            }
        } finally
        {
            (enumerator as IDisposable)?.Dispose();
        }

        return count;
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if(value is null)
        {
            return ValidationResult.Success;
        }

        if(value is not IEnumerable enumerable)
        {
            return new ValidationResult($"The field {validationContext.DisplayName} must be a collection.", (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
        }

        int count = CountElements(enumerable);

        if((count < Minimum) || (count > Maximum))
        {
            string message = (Maximum == int.MaxValue) ? ($"The field {validationContext.DisplayName} must contain at least {Minimum} element(s).") : ($"The field {validationContext.DisplayName} must contain between {Minimum} and {Maximum} element(s).");

            return new ValidationResult(message, (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
        }

        return ValidationResult.Success;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override string FormatErrorMessage(string name) { return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, Minimum, Maximum); }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the maximum number of elements allowed (inclusive). A value of <see cref="int.MaxValue"/> means no upper
    ///limit.
    ///</summary>
    public int Maximum { get; }

    ///<summary>
    ///Gets the minimum number of elements required (inclusive).
    ///</summary>
    public int Minimum { get; }
    #endregion
}
