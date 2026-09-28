namespace Catharsis.Diagnostics;

///<summary>
///Wraps a value and re-validates a configured invariant predicate on every read and write, throwing immediately if
///the invariant is ever violated instead of letting invalid state propagate silently.
///</summary>
///<typeparam name="T">The type of the guarded value.</typeparam>
///<param name="initialValue">The initial value. Must satisfy <paramref name="invariant"/>.</param>
///<param name="invariant">The predicate that every value assigned to <see cref="Value"/> must satisfy.</param>
///<param name="message">The message used when the invariant is violated, or <c>null</c> for a default message.</param>
///<exception cref="ArgumentNullException"><paramref name="invariant"/> is <c>null</c>.</exception>
///<exception cref="ArgumentException"><paramref name="initialValue"/> does not satisfy <paramref name="invariant"/>.</exception>
public sealed class InvariantGuard<T>(T initialValue, Func<T, bool> invariant, string? message = null)
{
    #region Fields
    readonly Func<T, bool> _invariant = invariant ?? throw new ArgumentNullException(nameof(invariant));
    readonly string _message = message ?? "Value violates the configured invariant.";
    T _value = ValidateInitialValue(initialValue, invariant, message);
    #endregion

    #region Private methods
    static T ValidateInitialValue(T initialValue, Func<T, bool> invariant, string? message)
    {
        ArgumentNullException.ThrowIfNull(invariant);

        if(!invariant(initialValue))
        {
            throw new ArgumentException(message ?? "Value violates the configured invariant.", nameof(initialValue));
        }

        return initialValue;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets or sets the guarded value. Setting a value that violates the configured invariant throws instead of
    ///storing it.
    ///</summary>
    ///<exception cref="InvalidOperationException">The assigned value does not satisfy the invariant.</exception>
    public T Value
    {
        get => _value;
        set
        {
            if(!_invariant(value))
            {
                throw new InvalidOperationException(_message);
            }

            _value = value;
        }
    }
    #endregion
}
