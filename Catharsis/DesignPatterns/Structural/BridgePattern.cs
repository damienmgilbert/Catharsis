namespace Catharsis.DesignPatterns.Structural;

///<summary>
///Implements the Bridge design pattern.
///</summary>
public class BridgePattern
{
    #region Public methods
    ///<summary>
    ///Bridge — decouples the abstraction <paramref name="obj"/> from its <paramref name="implementation"/> by applying
    ///<paramref name="operation"/>.
    ///</summary>
    ///<typeparam name="T">The abstraction type.</typeparam>
    ///<typeparam name="TImpl">The implementation type.</typeparam>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="obj">The abstraction object.</param>
    ///<param name="implementation">The implementation to bridge to.</param>
    ///<param name="operation">A delegate that combines the abstraction with its implementation.</param>
    ///<returns>The result of applying the bridged operation.</returns>
    public static TResult Bridge<T, TImpl, TResult>(T obj, TImpl implementation, Func<T, TImpl, TResult> operation)
    {
        if(operation is null)
        {
            throw new ArgumentNullException(nameof(operation), "Bridge operation must not be null.");
        }

        return operation(obj, implementation);
    }
    #endregion
}
