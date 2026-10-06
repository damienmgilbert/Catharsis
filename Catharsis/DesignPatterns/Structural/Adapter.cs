namespace Catharsis.DesignPatterns.Structural;

///<summary>
///Implements the Adapter design pattern.
///</summary>
public class Adapter
{
    #region Public methods

    ///<summary>
    ///Adapter — converts <paramref name="obj"/> to <typeparamref name="TResult"/> using the supplied <paramref
    ///name="adapter"/> delegate.
    ///</summary>
    ///<typeparam name="T">The type of the source object (adaptee).</typeparam>
    ///<typeparam name="TResult">The target type (adapted interface).</typeparam>
    ///<param name="obj">The object to adapt.</param>
    ///<param name="adapter">A delegate that converts the source to the target type.</param>
    ///<returns>The adapted representation of <paramref name="obj"/>.</returns>
    public static TResult Adapt<T, TResult>(T obj, Func<T, TResult> adapter)
    {
        if(adapter is null)
        {
            throw new ArgumentNullException(nameof(adapter), "Adapter function must not be null.");
        }

        return adapter(obj);
    }
    #endregion
}
