namespace Catharsis.DesignPatterns.Structural;

///<summary>
///Implements the Proxy design pattern.
///</summary>
public class ProxyPattern
{
    #region Public methods
    ///<summary>
    ///Proxy — intercepts <paramref name="operation"/> on <paramref name="obj"/> with optional <paramref name="before"/>
    ///and <paramref name="after"/> cross-cutting actions.
    ///</summary>
    ///<typeparam name="T">The type of the real subject.</typeparam>
    ///<typeparam name="TResult">The result type of the proxied operation.</typeparam>
    ///<param name="obj">The real subject.</param>
    ///<param name="operation">The operation to proxy.</param>
    ///<param name="before">An optional action executed before the operation.</param>
    ///<param name="after">An optional action executed after the operation.</param>
    ///<returns>The result of <paramref name="operation"/>.</returns>
    public static TResult Proxy<T, TResult>(T obj, Func<T, TResult> operation, Action<T>? before = null, Action<T>? after = null)
    {
        if (operation is null)
        {
            throw new ArgumentNullException(nameof(operation), "Proxy operation must not be null.");
        }

        before?.Invoke(obj);
        TResult? result = operation(obj);
        after?.Invoke(obj);

        return result;
    }
    #endregion
}
