using Catharsis.DataStructures;

namespace Catharsis.Patterns.Composed;

///<summary>
///Iterators over <see cref="TreeNode{T}"/> beyond the depth-first and breadth-first walks the node already offers:
///leaves, ancestors, level-by-level groups and root-to-leaf paths. All are lazy, and none uses recursion, so deep trees
///cannot overflow the stack.
///</summary>
public static class TreeIterator
{
    #region Public methods
    ///<summary>Enumerates the nodes with no children, in depth-first order.</summary>
    ///<typeparam name="T">The node value type.</typeparam>
    ///<param name="root">The node to start from.</param>
    ///<exception cref="ArgumentNullException"><paramref name="root"/> is <c>null</c>.</exception>
    public static IEnumerable<TreeNode<T>> Leaves<T>(TreeNode<T> root)
    {
        ArgumentNullException.ThrowIfNull(root);

        return root.DepthFirst().Where(static node => node.IsLeaf);
    }

    ///<summary>Enumerates the ancestors of <paramref name="node"/>, from its parent up to the root.</summary>
    ///<typeparam name="T">The node value type.</typeparam>
    ///<param name="node">The node whose ancestors to enumerate.</param>
    ///<exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    public static IEnumerable<TreeNode<T>> Ancestors<T>(TreeNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        return AncestorsIterator(node);
    }

    ///<summary>Groups the nodes by depth: first the root, then its children, then their children, and so on.</summary>
    ///<typeparam name="T">The node value type.</typeparam>
    ///<param name="root">The node to start from.</param>
    ///<returns>One list per level, top to bottom.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="root"/> is <c>null</c>.</exception>
    public static IEnumerable<IReadOnlyList<TreeNode<T>>> Levels<T>(TreeNode<T> root)
    {
        ArgumentNullException.ThrowIfNull(root);

        return LevelsIterator(root);
    }

    ///<summary>Enumerates every path from <paramref name="root"/> to a leaf as a list of values.</summary>
    ///<typeparam name="T">The node value type.</typeparam>
    ///<param name="root">The node to start from.</param>
    ///<returns>One list of values per leaf, each beginning with the root's value.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="root"/> is <c>null</c>.</exception>
    public static IEnumerable<IReadOnlyList<T>> Paths<T>(TreeNode<T> root)
    {
        ArgumentNullException.ThrowIfNull(root);

        return PathsIterator(root);
    }
    #endregion

    #region Private methods
    static IEnumerable<TreeNode<T>> AncestorsIterator<T>(TreeNode<T> node)
    {
        for (TreeNode<T>? current = node.Parent; current is not null; current = current.Parent)
        {
            yield return current;
        }
    }

    static IEnumerable<IReadOnlyList<TreeNode<T>>> LevelsIterator<T>(TreeNode<T> root)
    {
        List<TreeNode<T>> level = [root];

        while (level.Count > 0)
        {
            yield return level;

            level = [.. level.SelectMany(static node => node.Children)];
        }
    }

    static IEnumerable<IReadOnlyList<T>> PathsIterator<T>(TreeNode<T> root)
    {
        Stack<(TreeNode<T> Node, List<T> Path)> pending = new();
        pending.Push((root, [root.Value]));

        while (pending.Count > 0)
        {
            (TreeNode<T> node, List<T> path) = pending.Pop();

            if (node.IsLeaf)
            {
                yield return path;
                continue;
            }

            for (int i = node.Children.Count - 1; i >= 0; i--)
            {
                TreeNode<T> child = node.Children[i];
                pending.Push((child, [.. path, child.Value]));
            }
        }
    }
    #endregion
}
