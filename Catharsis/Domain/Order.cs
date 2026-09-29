using Catharsis.Operators;

namespace Catharsis.Domain;

///<summary>
///An order, the aggregate root of a set of <see cref="OrderLine"/>s priced in one currency. The class is declared
///<c>partial</c> and split by concern: this file holds the data and how it is read and extended, while
///<c>Order.Lifecycle.cs</c> holds the state transitions that raise domain events.
///</summary>
///<param name="id">The order identity.</param>
///<param name="currency">The three-letter currency every line must use.</param>
///<exception cref="ArgumentException"><paramref name="currency"/> is not a three-letter code.</exception>
public sealed partial class Order(Guid id, string currency) : AggregateRoot<Guid>(id)
{
    #region Fields
    readonly List<OrderLine> _lines = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a line.
    ///</summary>
    ///<param name="line">The line to add.</param>
    ///<returns>This order for chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="line"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">The order is no longer a draft, or the line uses a different currency.</exception>
    public Order AddLine(OrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        if(Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException($"Lines cannot be added to a {Status.ToStringFast()} order.");
        }

        if(line.UnitPrice.Currency != Currency)
        {
            throw new InvalidOperationException($"The order is in {Currency} but the line is priced in {line.UnitPrice.Currency}.");
        }

        _lines.Add(line);
        return this;
    }

    ///<summary>
    ///Adds a line built from its parts.
    ///</summary>
    ///<param name="sku">The product code.</param>
    ///<param name="quantity">How many units.</param>
    ///<param name="unitPrice">The price of one unit.</param>
    ///<returns>This order for chaining.</returns>
    ///<exception cref="InvalidOperationException">The order is no longer a draft, or the price uses a different currency.</exception>
    public Order AddLine(string sku, int quantity, Money unitPrice) => AddLine(new OrderLine(sku, quantity, unitPrice));
    #endregion

    #region Public properties
    ///<summary>Gets the currency every line must use.</summary>
    public string Currency { get; } = new Money(0m, currency).Currency;

    ///<summary>Gets where the order is in its life.</summary>
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    ///<summary>Gets the lines added so far.</summary>
    public IReadOnlyList<OrderLine> Lines => _lines;

    ///<summary>Gets the sum of every line.</summary>
    public Money Total => _lines.Aggregate(new Money(0m, Currency), static (sum, line) => sum + line.Total);
    #endregion
}
