using Catharsis.Generics;

namespace Catharsis.Domain;
// The behavior half of the Customer partial class: every state change goes through here so the rules live in one place.

public sealed partial class Customer
{
    #region Private methods
    private static string RequireEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        string trimmed = email.Trim();
        int at = trimmed.IndexOf('@', StringComparison.Ordinal);

        if(at <= 0 || at != trimmed.LastIndexOf('@') || at == trimmed.Length - 1 || trimmed.Contains(' ', StringComparison.Ordinal))
        {
            throw new ArgumentException($"'{email}' is not a valid email address.", nameof(email));
        }

        return trimmed;
    }

    private static string RequireName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return name.Trim();
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Awards loyalty points.
    ///</summary>
    ///<param name="points">How many points to add. Must be positive.</param>
    ///<returns>The new balance.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="points"/> is not positive.</exception>
    ///<exception cref="OverflowException">The balance would exceed <see cref="int.MaxValue"/>.</exception>
    public int AddPoints(int points)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(points);

        LoyaltyPoints = checked(LoyaltyPoints + points);
        return LoyaltyPoints;
    }

    ///<summary>
    ///Changes the email address.
    ///</summary>
    ///<param name="email">The new address. Must look like an address.</param>
    ///<exception cref="ArgumentException"><paramref name="email"/> is not valid.</exception>
    public void ChangeEmail(string email) => Email = RequireEmail(email);

    ///<summary>
    ///Spends loyalty points. Running short is an expected outcome, so it is reported as a failed result rather than an
    ///exception, and the balance is left unchanged.
    ///</summary>
    ///<param name="points">How many points to spend. Must be positive.</param>
    ///<returns>The remaining balance on success; a message on failure.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="points"/> is not positive.</exception>
    public Result<int, string> RedeemPoints(int points)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(points);

        if(points > LoyaltyPoints)
        {
            return Result<int, string>.Fail($"Cannot redeem {points} points; only {LoyaltyPoints} are available.");
        }

        LoyaltyPoints -= points;
        return Result<int, string>.Ok(LoyaltyPoints);
    }

    ///<summary>
    ///Changes the display name.
    ///</summary>
    ///<param name="name">The new name. Must not be blank.</param>
    ///<exception cref="ArgumentException"><paramref name="name"/> is blank.</exception>
    public void Rename(string name) => Name = RequireName(name);
    #endregion
}
