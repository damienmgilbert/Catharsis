namespace Catharsis.Extensions;

///<summary>
///Provides extension methods for <see cref="Exception"/> that walk or flatten exception chains, useful for logging
///and root-cause analysis.
///</summary>
public static class ExceptionExtensions
{
    #region Private methods
    static IEnumerable<Exception> FlattenIterator(Exception exception)
    {
        yield return exception;

        if(exception is AggregateException aggregate)
        {
            foreach(Exception inner in aggregate.Flatten().InnerExceptions)
            {
                foreach(Exception nested in FlattenIterator(inner))
                {
                    yield return nested;
                }
            }
        } else if(exception.InnerException is not null)
        {
            foreach(Exception nested in FlattenIterator(exception.InnerException))
            {
                yield return nested;
            }
        }
    }
    #endregion

    #region Public methods

    ///<summary>
    ///Flattens an exception into itself plus every exception it wraps: for an <see cref="AggregateException"/>, its
    ///flattened inner exceptions; otherwise, the chain of <see cref="Exception.InnerException"/> values.
    ///</summary>
    ///<param name="exception">The exception to flatten.</param>
    ///<returns><paramref name="exception"/> followed by each exception it wraps, outermost first.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
    public static IEnumerable<Exception> Flatten(this Exception exception)
    {
        if(exception is null)
        {
            throw new ArgumentNullException(nameof(exception), "Exception must not be null.");
        }

        return FlattenIterator(exception);
    }

    ///<summary>
    ///Joins this exception's message with every wrapped exception's message, in outermost-first order.
    ///</summary>
    ///<param name="exception">The exception to describe.</param>
    ///<param name="separator">The separator placed between messages. Defaults to <c>" -&gt; "</c>.</param>
    ///<returns>The joined messages.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="exception"/> or <paramref name="separator"/> is <c>null</c>.</exception>
    public static string GetAllMessages(this Exception exception, string separator = " -> ")
    {
        if(exception is null)
        {
            throw new ArgumentNullException(nameof(exception), "Exception must not be null.");
        }

        if(separator is null)
        {
            throw new ArgumentNullException(nameof(separator), "Separator must not be null.");
        }

        return string.Join(separator, exception.Flatten().Select(static ex => ex.Message));
    }

    ///<summary>
    ///Walks to the innermost exception in the chain: for an <see cref="AggregateException"/>, the innermost
    ///exception of its first flattened inner exception; otherwise, the end of the <see
    ///cref="Exception.InnerException"/> chain.
    ///</summary>
    ///<param name="exception">The exception to walk.</param>
    ///<returns>The innermost (root cause) exception.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
    public static Exception GetRootCause(this Exception exception)
    {
        if(exception is null)
        {
            throw new ArgumentNullException(nameof(exception), "Exception must not be null.");
        }

        Exception current = exception;

        while(true)
        {
            if(current is AggregateException { InnerExceptions.Count: > 0 } aggregate)
            {
                current = aggregate.InnerExceptions[0];
            } else if(current.InnerException is not null)
            {
                current = current.InnerException;
            } else
            {
                return current;
            }
        }
    }
    #endregion
}
