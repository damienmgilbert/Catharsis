namespace Catharsis.DesignPatterns.Behavioral;

/// <summary>
/// Implements the Visitor design pattern.
/// </summary>
public class Visitor
{
    /// <summary>
    /// Visitor — applies <paramref name="visitor"/> to <paramref name="obj"/>
    /// via the supplied <paramref name="visit"/> action.
    /// </summary>
    /// <typeparam name="T">The element type being visited.</typeparam>
    /// <typeparam name="TVisitor">The visitor type.</typeparam>
    /// <param name="obj">The element to visit.</param>
    /// <param name="visitor">The visitor instance.</param>
    /// <param name="visit">An action that applies the visitor to the element.</param>
    /// <returns>The original <paramref name="obj"/> after the visit.</returns>
    public T Accept<T, TVisitor>(T obj, TVisitor visitor, Action<TVisitor, T> visit)
    {
        if (visitor is null) throw new ArgumentNullException(nameof(visitor), "Visitor must not be null.");
        if (visit is null) throw new ArgumentNullException(nameof(visit), "Visit action must not be null.");
        visit(visitor, obj);
        return obj;
    }

    /// <summary>
    /// Visitor — applies <paramref name="visitor"/> to <paramref name="obj"/>
    /// via the supplied <paramref name="visit"/> function, returning the result.
    /// </summary>
    /// <typeparam name="T">The element type being visited.</typeparam>
    /// <typeparam name="TVisitor">The visitor type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="obj">The element to visit.</param>
    /// <param name="visitor">The visitor instance.</param>
    /// <param name="visit">A function that applies the visitor to the element and returns a result.</param>
    /// <returns>The result of the visit.</returns>
    public TResult Accept<T, TVisitor, TResult>(T obj, TVisitor visitor, Func<TVisitor, T, TResult> visit)
    {
        if (visitor is null) throw new ArgumentNullException(nameof(visitor), "Visitor must not be null.");
        if (visit is null) throw new ArgumentNullException(nameof(visit), "Visit function must not be null.");
        return visit(visitor, obj);
    }
}
