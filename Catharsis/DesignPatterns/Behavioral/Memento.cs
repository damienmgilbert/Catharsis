namespace Catharsis.DesignPatterns.Behavioral;

///<summary>
///Implements the Memento design pattern.
///</summary>
public class Memento
{
    #region Public methods
    ///<summary>
    ///Memento (restore) — restores <paramref name="obj"/> from <paramref name="memento"/> using the supplied <paramref
    ///name="restore"/> action.
    ///</summary>
    ///<typeparam name="T">The originator type.</typeparam>
    ///<typeparam name="TMemento">The memento type that stores the captured state.</typeparam>
    ///<param name="obj">The originator to restore.</param>
    ///<param name="memento">The memento containing the state to restore.</param>
    ///<param name="restore">An action that applies the memento state to the originator.</param>
    ///<returns>The restored <paramref name="obj"/>.</returns>
    public T Restore<T, TMemento>(T obj, TMemento memento, Action<T, TMemento> restore)
    {
        if(restore is null)
        {
            throw new ArgumentNullException(nameof(restore), "Restore action must not be null.");
        }

        restore(obj, memento);
        return obj;
    }

    ///<summary>
    ///Memento (capture) — takes a snapshot of <paramref name="obj"/> via <paramref name="capture"/> and returns the
    ///object together with the memento.
    ///</summary>
    ///<typeparam name="T">The originator type.</typeparam>
    ///<typeparam name="TMemento">The memento type that stores the captured state.</typeparam>
    ///<param name="obj">The originator whose state is captured.</param>
    ///<param name="capture">A delegate that produces a memento from the current state.</param>
    ///<returns>A tuple containing the original <paramref name="obj"/> and the captured <typeparamref name="TMemento"/>.</returns>
    public (T Object, TMemento Memento) Snapshot<T, TMemento>(T obj, Func<T, TMemento> capture)
    {
        if(capture is null)
        {
            throw new ArgumentNullException(nameof(capture), "Capture function must not be null.");
        }

        return (obj, capture(obj));
    }
    #endregion
}
