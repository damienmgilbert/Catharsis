using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.DataAnnotations;

/// <summary>
/// Specifies the sort direction for <see cref="SortedAttribute"/> validation.
/// </summary>
public enum SortDirection
{
    /// <summary>Elements must be in ascending order.</summary>
    Ascending,

    /// <summary>Elements must be in descending order.</summary>
    Descending
}

/// <summary>
/// Validates that the elements in an <see cref="IEnumerable"/> of
/// <see cref="IComparable"/> values are sorted in the specified direction.
/// </summary>
/// <remarks>
/// A <c>null</c> value is considered valid (combine with
/// <see cref="RequiredAttribute"/> to disallow nulls).
/// Collections with zero or one element are always valid.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
public sealed class SortedAttribute : ValidationAttribute
{
    /// <summary>
    /// Gets the required sort direction.
    /// </summary>
    public SortDirection Direction { get; }

    /// <summary>
    /// Gets or sets a value indicating whether adjacent equal elements are
    /// permitted. Defaults to <c>true</c>.
    /// </summary>
    public bool AllowDuplicates { get; set; } = true;

    /// <summary>
    /// Initializes a FileName instance of <see cref="SortedAttribute"/>
    /// with the specified sort direction.
    /// </summary>
    /// <param name="direction">The required sort direction.</param>
    public SortedAttribute(SortDirection direction = SortDirection.Ascending)
        : base("The field {0} must be sorted in {1} order.")
    {
        Direction = direction;
    }

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not IEnumerable enumerable)
        {
            return new ValidationResult(
                $"The field {validationContext.DisplayName} must be a collection.",
                validationContext.MemberName is not null
                    ? [validationContext.MemberName]
                    : null);
        }

        IComparable? previous = null;
        bool first = true;
        int index = 0;

        foreach (object? element in enumerable)
        {
            if (element is not IComparable current)
            {
                return new ValidationResult(
                    $"The field {validationContext.DisplayName} contains a non-comparable element at index {index}.",
                    validationContext.MemberName is not null
                        ? [validationContext.MemberName]
                        : null);
            }

            if (!first)
            {
                int comparison = previous!.CompareTo(current);

                bool invalid = Direction == SortDirection.Ascending
                    ? (AllowDuplicates ? comparison > 0 : comparison >= 0)
                    : (AllowDuplicates ? comparison < 0 : comparison <= 0);

                if (invalid)
                {
                    string directionText = Direction == SortDirection.Ascending
                        ? "ascending"
                        : "descending";

                    return new ValidationResult(
                        $"The field {validationContext.DisplayName} is not sorted in {directionText} order at index {index}.",
                        validationContext.MemberName is not null
                            ? [validationContext.MemberName]
                            : null);
                }
            }

            previous = current;
            first = false;
            index++;
        }

        return ValidationResult.Success;
    }

    /// <inheritdoc />
    public override string FormatErrorMessage(string name) =>
        string.Format(ErrorMessageString, name,
            Direction == SortDirection.Ascending ? "ascending" : "descending");
}
