namespace Catharsis.Resilience;

///<summary>
///The exception thrown by <see cref="BulkheadPolicy"/> when a call is rejected because both the execution slots and
///the waiting queue are full.
///</summary>
public sealed class BulkheadRejectedException : Exception
{
    #region Public methods
    ///<summary>
    ///Creates an instance with a default message.
    ///</summary>
    public BulkheadRejectedException() : base("The bulkhead has no available execution slots or queue capacity.")
    {
    }

    ///<summary>
    ///Creates an instance with the specified message.
    ///</summary>
    ///<param name="message">The message that describes the error.</param>
    public BulkheadRejectedException(string message) : base(message)
    {
    }

    ///<summary>
    ///Creates an instance with the specified message and inner exception.
    ///</summary>
    ///<param name="message">The message that describes the error.</param>
    ///<param name="innerException">The exception that caused this exception.</param>
    public BulkheadRejectedException(string message, Exception innerException) : base(message, innerException)
    {
    }
    #endregion
}
