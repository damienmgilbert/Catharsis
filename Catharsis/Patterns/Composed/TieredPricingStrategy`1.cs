using Catharsis.Operators;

namespace Catharsis.Patterns.Composed;

///<summary>
///Prices an item by quantity using volume tiers: the tier with the largest minimum quantity that the item's quantity
///reaches supplies the unit price for <em>all</em> units (a volume discount, not a graduated one).
///</summary>
///<typeparam name="TItem">The kind of item being priced.</typeparam>
public sealed class TieredPricingStrategy<TItem> : IPricingStrategy<TItem>
{
    #region Fields
    readonly Func<TItem, int> _quantity;
    readonly string _currency;
    readonly (int MinQuantity, decimal UnitPrice)[] _tiers;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="TieredPricingStrategy{TItem}"/>.
    ///</summary>
    ///<param name="quantity">Reads the quantity from an item.</param>
    ///<param name="currency">The currency code of every price.</param>
    ///<param name="tiers">The tiers as minimum quantity and unit price. The lowest minimum must be 0 or 1.</param>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="tiers"/> is empty, has duplicate minimums, or its lowest minimum is above 1.</exception>
    public TieredPricingStrategy(Func<TItem, int> quantity, string currency, IEnumerable<(int MinQuantity, decimal UnitPrice)> tiers)
    {
        ArgumentNullException.ThrowIfNull(quantity);
        ArgumentNullException.ThrowIfNull(currency);
        ArgumentNullException.ThrowIfNull(tiers);

        _tiers = [.. tiers.OrderBy(static t => t.MinQuantity)];

        if(_tiers.Length == 0 || _tiers[0].MinQuantity > 1)
        {
            throw new ArgumentException("Tiers must be non-empty and the lowest minimum quantity must be 0 or 1.", nameof(tiers));
        }

        if(_tiers.Select(static t => t.MinQuantity).Distinct().Count() != _tiers.Length)
        {
            throw new ArgumentException("Tiers must not share a minimum quantity.", nameof(tiers));
        }

        _quantity = quantity;
        _currency = new Money(0m, currency).Currency;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    ///<exception cref="ArgumentOutOfRangeException">The item's quantity is negative.</exception>
    public Money Price(TItem item)
    {
        int quantity = _quantity(item);
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        // Tiers are sorted ascending and the lowest minimum is 0 or 1, so only a quantity of 0 can miss every tier;
        // that case prices to zero whichever tier is used.
        int index = Array.FindLastIndex(_tiers, t => t.MinQuantity <= quantity);

        return new Money(_tiers[Math.Max(index, 0)].UnitPrice * quantity, _currency);
    }
    #endregion
}
