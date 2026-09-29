namespace Catharsis.Domain;

// The lifecycle half of the Order partial class: every state change lives here and records the matching event.
public sealed partial class Order
{
    #region Public methods
    ///<summary>
    ///Confirms the order, locking its lines, and raises <see cref="OrderConfirmed"/>.
    ///</summary>
    ///<exception cref="InvalidOperationException">The order is not a draft, or has no lines.</exception>
    public void Confirm()
    {
        if(Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException($"Only a draft order can be confirmed, but this one is {Status}.");
        }

        if(_lines.Count == 0)
        {
            throw new InvalidOperationException("An order with no lines cannot be confirmed.");
        }

        Status = OrderStatus.Confirmed;
        Raise(new OrderConfirmed(Id, Total));
    }

    ///<summary>
    ///Cancels the order and raises <see cref="OrderCancelled"/>.
    ///</summary>
    ///<param name="reason">Why the order is being cancelled.</param>
    ///<exception cref="ArgumentException"><paramref name="reason"/> is blank.</exception>
    ///<exception cref="InvalidOperationException">The order is already cancelled.</exception>
    public void Cancel(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if(Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("The order is already cancelled.");
        }

        bool wasConfirmed = Status == OrderStatus.Confirmed;

        Status = OrderStatus.Cancelled;
        Raise(new OrderCancelled(Id, reason, wasConfirmed));
    }
    #endregion
}
