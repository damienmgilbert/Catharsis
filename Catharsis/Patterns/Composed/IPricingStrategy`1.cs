using Catharsis.Operators;

namespace Catharsis.Patterns.Composed;

///<summary>
///The strategy interface for pricing an item: each implementation is one interchangeable way of arriving at a ///<see
///cref="Money"/> amount, chosen at runtime (see <see cref="PricingContext{TItem}"/>).
///</summary>
///<typeparam name="TItem">The kind of item being priced.</typeparam>
public interface IPricingStrategy<in TItem>
{
    #region Public methods

    ///<summary>
    ///Computes the price of <paramref name="item"/>.
    ///</summary>
    ///<param name="item">The item to price.</param>
    Money Price(TItem item);
    #endregion
}
