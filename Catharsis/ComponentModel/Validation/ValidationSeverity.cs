namespace Catharsis.ComponentModel.Validation;

///<summary>
///Specifies the severity level of a validation error.
///</summary>
public enum ValidationSeverity
{
    ///<summary>
    ///An informational message that does not prevent the operation.
    ///</summary>
    Info = 0,

    ///<summary>
    ///A warning that should be reviewed but does not prevent the operation.
    ///</summary>
    Warning = 1,

    ///<summary>
    ///An error that prevents the operation from completing.
    ///</summary>
    Error = 2
}
