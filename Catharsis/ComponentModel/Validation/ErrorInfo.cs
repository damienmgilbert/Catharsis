namespace Catharsis.ComponentModel.Validation;

///<summary>
///A structured validation error that captures the error message, severity, and originating property name.
///</summary>
///<param name="Message">The human-readable error message.</param>
///<param name="Severity">The severity of the error.</param>
///<param name="PropertyName">
///The name of the property that produced the error, or <c>null</c> for object-level errors.
///</param>
public sealed record ErrorInfo(string Message, ValidationSeverity Severity = ValidationSeverity.Error, string? PropertyName = null)
{
    #region Public methods

    ///<inheritdoc/>
    public override string ToString() => (PropertyName is not null) ? ($"[{Severity}] {PropertyName}: {Message}") : ($"[{Severity}] {Message}");
    #endregion
}
