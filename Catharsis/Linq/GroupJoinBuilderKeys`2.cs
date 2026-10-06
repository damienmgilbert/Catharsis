namespace Catharsis.Linq;

///<summary>
///Builder stage awaiting the key selectors.
///</summary>
///<typeparam name="TOuter">The outer element type.</typeparam>
///<typeparam name="TInner">The inner element type.</typeparam>
public sealed class GroupJoinBuilderKeys<TOuter, TInner>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner)
{
    #region Public methods

    ///<summary>
    ///Specifies the key selectors for both outer and inner sequences.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="outerKeySelector">A function that extracts the key from an outer element.</param>
    ///<param name="innerKeySelector">A function that extracts the key from an inner element.</param>
    ///<returns>A builder awaiting the result selector.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="outerKeySelector"/> or <paramref name="innerKeySelector"/> is <c>null</c>.</exception>
    public GroupJoinBuilderResult<TOuter, TInner, TKey> On<TKey>(Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector)
    {
        ArgumentNullException.ThrowIfNull(outerKeySelector, nameof(outerKeySelector));
        ArgumentNullException.ThrowIfNull(innerKeySelector, nameof(innerKeySelector));
        return new GroupJoinBuilderResult<TOuter, TInner, TKey>(outer, inner, outerKeySelector, innerKeySelector);
    }
    #endregion
}
