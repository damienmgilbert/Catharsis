namespace Catharsis.Linq;

///<summary>
///Specifies comparison operators for <see cref="QueryableFilterBuilder{T}.WhereCompare{TProperty}"/>.
///</summary>
public enum FilterComparison
{
    ///<summary>
    ///Equal (<c>==</c>).
    ///</summary>
    Equal,

    ///<summary>
    ///Not equal (<c>!=</c>).
    ///</summary>
    NotEqual,

    ///<summary>
    ///Greater than (<c>&gt;</c>).
    ///</summary>
    GreaterThan,

    ///<summary>
    ///Greater than or equal (<c>&gt;=</c>).
    ///</summary>
    GreaterThanOrEqual,

    ///<summary>
    ///Less than (<c>&lt;</c>).
    ///</summary>
    LessThan,

    ///<summary>
    ///Less than or equal (<c>&lt;=</c>).
    ///</summary>
    LessThanOrEqual
}
