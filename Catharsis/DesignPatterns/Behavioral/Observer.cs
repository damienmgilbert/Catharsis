namespace Catharsis.DesignPatterns.Behavioral;

///<summary>
///Implements the Observer design pattern.
///</summary>
public class Observer
{
    #region Public methods
    ///<summary>
    ///Observer — notifies each observer in <paramref name="observers"/> of the current state of <paramref name="obj"/>.
    ///</summary>
    ///<typeparam name="T">The type of the subject.</typeparam>
    ///<param name="obj">The subject whose state is being observed.</param>
    ///<param name="observers">The observers to notify.</param>
    ///<returns>The original <paramref name="obj"/> after all observers have been notified.</returns>
    public T Notify<T>(T obj, params Action<T>[] observers)
    {
        if(observers is null)
        {
            throw new ArgumentNullException(nameof(observers), "Observers must not be null.");
        }

        foreach(Action<T> observer in observers)
        {
            observer(obj);
        }

        return obj;
    }
    #endregion
}
