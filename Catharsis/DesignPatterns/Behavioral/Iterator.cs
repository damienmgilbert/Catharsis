namespace Catharsis.DesignPatterns.Behavioral;

/// <summary>
/// Implements the Iterator design pattern.
/// </summary>
public class Iterator
{
    /// <summary>
    /// Iterator — extracts elements from <paramref name="obj"/> via
    /// <paramref name="getElements"/> and applies <paramref name="action"/> to each.
    /// </summary>
    /// <typeparam name="T">The type of the aggregate.</typeparam>
    /// <typeparam name="TElement">The type of the elements in the aggregate.</typeparam>
    /// <param name="obj">The aggregate object.</param>
    /// <param name="getElements">A delegate that produces a sequence of elements from the aggregate.</param>
    /// <param name="action">The action to apply to each element.</param>
    /// <returns>The original <paramref name="obj"/> after all elements have been visited.</returns>
    public T Iterate<T, TElement>(T obj, Func<T, IEnumerable<TElement>> getElements, Action<TElement> action)
    {
        if (getElements is null) throw new ArgumentNullException(nameof(getElements), "GetElements function must not be null.");
        if (action is null) throw new ArgumentNullException(nameof(action), "Action must not be null.");

        foreach (var element in getElements(obj))
        {
            action(element);
        }

        return obj;
    }
}
