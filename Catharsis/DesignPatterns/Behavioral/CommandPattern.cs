namespace Catharsis.DesignPatterns.Behavioral;

/// <summary>
/// Implements the Command design pattern.
/// </summary>
public class CommandPattern
{
    /// <summary>
    /// Command — executes the <paramref name="execute"/> action on <paramref name="obj"/>
    /// and optionally records an <paramref name="undo"/> action in <paramref name="undoHistory"/>
    /// for later reversal.
    /// </summary>
    /// <typeparam name="T">The type of the receiver.</typeparam>
    /// <param name="obj">The receiver the command acts upon.</param>
    /// <param name="execute">The action to execute.</param>
    /// <param name="undo">An optional action that reverses the effect of <paramref name="execute"/>.</param>
    /// <param name="undoHistory">An optional collection where <paramref name="undo"/> is recorded for later replay.</param>
    /// <returns>The original <paramref name="obj"/> after execution.</returns>
    public T Command<T>(T obj, Action<T> execute, Action<T>? undo = null, ICollection<Action<T>>? undoHistory = null)
    {
        if (execute is null) throw new ArgumentNullException(nameof(execute), "Execute action must not be null.");

        execute(obj);

        if (undo is not null && undoHistory is not null)
        {
            undoHistory.Add(undo);
        }

        return obj;
    }
}
