namespace Catharsis.Domain;

///<summary>
///A customer, split across two files as a <c>partial</c> class: this file holds the identity and state and how they are
///read, while <c>Customer.Behavior.cs</c> holds the operations that change that state and enforce its rules. Keeping the
///halves apart lets the data shape be scanned at a glance and the rules grow independently.
///</summary>
///<param name="id">The customer identity.</param>
///<param name="name">The customer's display name. Must not be blank.</param>
///<param name="email">The customer's email address. Must look like an address.</param>
///<exception cref="ArgumentException"><paramref name="name"/> is blank or <paramref name="email"/> is not valid.</exception>
public sealed partial class Customer(Guid id, string name, string email) : Entity<Guid>(id)
{
    #region Public properties
    ///<summary>Gets the display name.</summary>
    public string Name { get; private set; } = RequireName(name);

    ///<summary>Gets the email address.</summary>
    public string Email { get; private set; } = RequireEmail(email);

    ///<summary>Gets the loyalty points currently held.</summary>
    public int LoyaltyPoints { get; private set; }
    #endregion
}
