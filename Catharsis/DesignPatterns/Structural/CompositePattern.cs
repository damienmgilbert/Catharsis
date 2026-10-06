namespace Catharsis.DesignPatterns.Structural;

///<summary>
///Implements the Composite design pattern.
///</summary>
public class CompositePattern
{
    #region Public methods
    ///<summary>
    ///Composite — applies <paramref name="action"/> to <paramref name="obj"/> and recursively to all descendants
    ///returned by <paramref name="getChildren"/> in a pre-order depth-first traversal.
    ///</summary>
    ///<typeparam name="T">The component type in the composite tree.</typeparam>
    ///<param name="obj">The root component.</param>
    ///<param name="getChildren">A delegate that returns the children of a component.</param>
    ///<param name="action">The action to apply to each component in the tree.</param>
    ///<returns>The original <paramref name="obj"/> after the action has been applied to the entire tree.</returns>
    public static T Composite<T>(T obj, Func<T, IEnumerable<T>> getChildren, Action<T> action)
    {
        if(getChildren is null)
        {
            throw new ArgumentNullException(nameof(getChildren), "GetChildren function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        Visit(obj, getChildren, action);
        return obj;

        static void Visit(T node, Func<T, IEnumerable<T>> getChildren, Action<T> action)
        {
            action(node);

            foreach(T child in getChildren(node))
            {
                Visit(child, getChildren, action);
            }
        }
    }
    #endregion
}
