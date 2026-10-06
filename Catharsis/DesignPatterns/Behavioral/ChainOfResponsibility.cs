namespace Catharsis.DesignPatterns.Behavioral;

///<summary>
///Implements the Chain of Responsibility design pattern.
///</summary>
public class ChainOfResponsibility
{
    #region Public methods

    ///<summary>
    ///Chain of Responsibility — passes <paramref name="obj"/> through <paramref name="handlers"/> in order until one
    ///returns <c>true</c>, indicating the request was handled.
    ///</summary>
    ///<typeparam name="T">The type of the request object.</typeparam>
    ///<param name="obj">The request to pass through the chain.</param>
    ///<param name="handlers">Ordered handlers; each returns <c>true</c> if it handled the request.</param>
    ///<returns>The original <paramref name="obj"/> after the chain completes.</returns>
    public static T Chain<T>(T obj, params Func<T, bool>[] handlers)
    {
        if(handlers is null)
        {
            throw new ArgumentNullException(nameof(handlers), "Handlers must not be null.");
        }

        foreach(Func<T, bool> handler in handlers)
        {
            if(handler(obj))
            {
                break;
            }
        }

        return obj;
    }
    #endregion
}
