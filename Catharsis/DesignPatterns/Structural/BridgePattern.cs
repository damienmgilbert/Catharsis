namespace Catharsis.DesignPatterns.Structural;

///<summary>
///Implements the Bridge design pattern.
///</summary>
public class BridgePattern
{
    #region Public methods

    ///<summary>
    ///Bridge — decouples the abstraction <paramref name="obj"/> from its <paramref name="implementation"/> by applying
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
