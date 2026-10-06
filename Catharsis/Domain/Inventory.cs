using Catharsis.Generics;

namespace Catharsis.Domain;

///<summary>
///Tracks stock levels by SKU and reserves stock for orders. Reservation reports failure through a ///<see
///cref="Result{T, TError}"/> rather than an exception, since running out of stock is an expected outcome, and it is
///all-or-nothing: an order is never partly reserved. SKUs are case-insensitive. Safe for concurrent use.
///</summary>
public sealed class Inventory
{
    #region Fields
    private readonly Lock _gate = new();
    private readonly Dictionary<string, int> _stock = new(StringComparer.OrdinalIgnoreCase);
    #endregion

    #region Private methods
    private int StockOfLocked(string sku) => _stock.GetValueOrDefault(sku);
    #endregion

    #region Public methods
    ///<summary>
    ///Reserves stock for every line of an order, or for none of them.
    ///</summary>
    ///<param name="order">The order to reserve for.</param>
    ///<returns>
    ///On success, the total units reserved. On failure, a message naming the first SKU that is short; stock is left
    ///unchanged.
    ///</returns>
    ///<exception cref="ArgumentNullException"><paramref name="order"/> is <c>null</c>.</exception>
    public Result<int, string> Reserve(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        Dictionary<string, int> needed = new(StringComparer.OrdinalIgnoreCase);

        foreach(OrderLine line in order.Lines)
        {
            needed[line.Sku] = needed.GetValueOrDefault(line.Sku) + line.Quantity;
        }

        lock(_gate)
        {
            foreach((string sku, int quantity) in needed)
            {
                int available = StockOfLocked(sku);

                if(available < quantity)
                {
                    return Result<int, string>.Fail($"Insufficient stock for '{sku}': need {quantity}, have {available}.");
                }
            }

            foreach((string sku, int quantity) in needed)
            {
                _stock[sku] -= quantity;
            }
        }

        return Result<int, string>.Ok(needed.Values.Sum());
    }

        ///<summary>
///Adds units to a SKU's stock.
///</summary>
    ///<param name="sku">The product code.</param>
    ///<param name="quantity">How many units to add. Must be positive.</param>
    ///<returns>The new stock level.</returns>
    ///<exception cref="ArgumentException"><paramref name="sku"/> is blank.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="quantity"/> is not positive.</exception>
    public int Restock(string sku, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        lock(_gate)
        {
            return _stock[sku] = StockOfLocked(sku) + quantity;
        }
    }

    ///<summary>
    ///Gets the units currently in stock for a SKU.
    ///</summary>
    ///<param name="sku">The product code.</param>
    ///<returns>The stock level, or 0 for an unknown SKU.</returns>
    ///<exception cref="ArgumentException"><paramref name="sku"/> is blank.</exception>
    public int StockOf(string sku)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        lock(_gate)
        {
            return StockOfLocked(sku);
        }
    }
    #endregion
}
