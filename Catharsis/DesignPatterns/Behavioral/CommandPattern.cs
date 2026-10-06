namespace Catharsis.DesignPatterns.Behavioral;

///<summary>
///Implements the Command design pattern.
///</summary>
public class CommandPattern
{
    #region Public methods

    ///<summary>
    ///Command — executes the <paramref name="execute"/> action on <paramref name="obj"/> and optionally records an
    public static T Command<T>(T obj, Action<T> execute, Action<T>? undo = null, ICollection<Action<T>>? undoHistory = null)
    {
        if(execute is null)
        {
            throw new ArgumentNullException(nameof(execute), "Execute action must not be null.");
        }

        execute(obj);

        if((undo is not null) && (undoHistory is not null))
        {
            undoHistory.Add(undo);
        }

        return obj;
    }
    #endregion
}
