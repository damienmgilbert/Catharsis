using Catharsis.Operators;

namespace Catharsis.Patterns.Composed;

///<summary>
///Prices every item at the same fixed amount.
///</summary>
///<typeparam name="TItem">The kind of item being priced.</typeparam>
///<param name="price">The price charged for any item.</param>
public sealed class FlatPricingStrategy<TItem>(Money price) : IPricingStrategy<TItem>
{
    #region Public methods

    ///<inheritdoc/>
    public Money Price(TItem item) => price;
    #endregion
}
