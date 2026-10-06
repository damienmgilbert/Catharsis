namespace Catharsis.Linq;

///<summary>
///Provides a fluent builder for constructing group joins between <see cref="IEnumerable{T}"/> sequences. The builder
///supports configuring outer/inner sequences, key selectors, result projections, equality comparers, and left-outer
///join semantics in a step-by-step fashion.
///</summary>
public static class GroupJoinBuilder
{
    #region Public methods
    ///<summary>
    ///Begins a group join builder starting from the specified outer sequence.
    ///</summary>
    ///<typeparam name="TOuter">The outer element type.</typeparam>
    ///<param name="outer">The outer sequence.</param>
    ///<returns>A builder awaiting the inner sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="outer"/> is <c>null</c>.</exception>
    public static GroupJoinBuilderInner<TOuter> GroupJoin<TOuter>(this IEnumerable<TOuter> outer)
    {
        ArgumentNullException.ThrowIfNull(outer, nameof(outer));
        return new GroupJoinBuilderInner<TOuter>(outer);
    }
    #endregion
}
