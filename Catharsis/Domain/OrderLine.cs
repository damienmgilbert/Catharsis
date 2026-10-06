using Catharsis.Operators;

namespace Catharsis.Domain;

///<summary>
///One line of an <see cref="Order"/>: a quantity of a product at a unit price. It is a value object, so two lines with
///the same SKU, quantity and price are interchangeable.
///</summary>
public sealed class OrderLine : ValueObject
{
    #region Constructors
    ///<summary>Initializes a new <see cref="OrderLine"/>.</summary>
    ///<param name="sku">The product code. Must not be blank.</param>
    ///<param name="quantity">How many units. Must be positive.</param>
    ///<param name="unitPrice">The price of one unit. Must not be negative.</param>
    ///<exception cref="ArgumentException"><paramref name="sku"/> is blank.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="quantity"/> is not positive, or <paramref name="unitPrice"/> is negative.</exception>
    public OrderLine(string sku, int quantity, Money unitPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        if (unitPrice.Amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), unitPrice, "Unit price must not be negative.");
        }

        Sku = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
    #endregion

    #region Public properties
    ///<summary>Gets the product code.</summary>
    public string Sku { get; }

    ///<summary>Gets the number of units.</summary>
    public int Quantity { get; }

    ///<summary>Gets the price of one unit.</summary>
    public Money UnitPrice { get; }

    ///<summary>Gets the price of the whole line.</summary>
    public Money Total => UnitPrice * Quantity;
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Sku;
        yield return Quantity;
        yield return UnitPrice;
    }
    #endregion
}
