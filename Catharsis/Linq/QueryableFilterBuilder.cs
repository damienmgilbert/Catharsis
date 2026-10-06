namespace Catharsis.Linq;

///<summary>Specifies how predicates in a <see cref="QueryableFilterBuilder{T}"/> are combined.</summary>
public enum FilterCombineMode
{
    ///<summary>Predicates are combined with logical AND.</summary>
    And,

    ///<summary>Predicates are combined with logical OR.</summary>
    Or
}
