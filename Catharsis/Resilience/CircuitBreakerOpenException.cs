namespace Catharsis.Resilience;

///<summary>
///The exception thrown by <see cref="CircuitBreaker"/> when a call is rejected because the circuit is open.
///</summary>
public sealed class CircuitBreakerOpenException : Exception
{
    #region Public methods
    ///<summary>
    ///Creates an instance with a default message.
    ///</summary>
    public CircuitBreakerOpenException() : base("The circuit breaker is open.")
    {
    }

    ///<summary>
    ///Creates an instance with the specified message.
    ///</summary>
    ///<param name="message">The message that describes the error.</param>
    public CircuitBreakerOpenException(string message) : base(message)
    {
    }

    ///<summary>
    ///Creates an instance with the specified message and inner exception.
    ///</summary>
    ///<param name="message">The message that describes the error.</param>
    ///<param name="innerException">The exception that caused this exception.</param>
    public CircuitBreakerOpenException(string message, Exception innerException) : base(message, innerException)
    {
    }
    #endregion
}
