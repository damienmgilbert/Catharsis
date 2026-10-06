namespace Catharsis.Linq;

///<summary>
///Builder stage awaiting the inner sequence.
///</summary>
///<typeparam name="TOuter">The outer element type.</typeparam>
public sealed class GroupJoinBuilderInner<TOuter>(IEnumerable<TOuter> outer)
{
    #region Public methods
    ///<summary>
    ///Specifies the inner sequence to join against.
    ///</summary>
    ///<typeparam name="TInner">The inner element type.</typeparam>
    ///<param name="inner">The inner sequence.</param>
    ///<returns>A builder awaiting the key selectors.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="inner"/> is <c>null</c>.</exception>
    public GroupJoinBuilderKeys<TOuter, TInner> With<TInner>(IEnumerable<TInner> inner)
    {
        ArgumentNullException.ThrowIfNull(inner, nameof(inner));
        return new GroupJoinBuilderKeys<TOuter, TInner>(outer, inner);
    }
    #endregion
}
