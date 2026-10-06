using Catharsis.Operators;

namespace Catharsis.Patterns.Composed;

///<summary>
///A decorator over another <see cref="IPricingStrategy{TItem}"/> that takes a percentage off whatever the inner
///strategy charges. Because it is itself a strategy, discounts can be stacked.
///</summary>
///<typeparam name="TItem">The kind of item being priced.</typeparam>
public sealed class DiscountPricingStrategy<TItem> : IPricingStrategy<TItem>
{
    #region Fields
    readonly IPricingStrategy<TItem> _inner;
    readonly Percentage _remaining;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="DiscountPricingStrategy{TItem}"/>.
    ///</summary>
    ///<param name="inner">The strategy to discount.</param>
    ///<param name="discount">The share to take off, from 0% to 100%.</param>
    ///<exception cref="ArgumentNullException"><paramref name="inner"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="discount"/> is outside 0% to 100%.</exception>
    public DiscountPricingStrategy(IPricingStrategy<TItem> inner, Percentage discount)
    {
        ArgumentNullException.ThrowIfNull(inner);

        if (discount < Percentage.FromFraction(0m) || discount > Percentage.FromFraction(1m))
        {
            throw new ArgumentOutOfRangeException(nameof(discount), discount, "Discount must be between 0% and 100%.");
        }

        _inner = inner;
        _remaining = Percentage.FromFraction(1m) - discount;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public Money Price(TItem item) => (_inner.Price(item) * _remaining).Round();
    #endregion
}
