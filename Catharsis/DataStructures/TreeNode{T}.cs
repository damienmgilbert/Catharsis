using System.Collections;

namespace Catharsis.DataStructures;

///<summary>
///A generic tree node that holds a value of type <typeparamref name="T"/> and maintains parent/child relationships with
///traversal helpers.
///</summary>
///<typeparam name="T">The type of value stored in the node.</typeparam>
///<remarks>
///Initializes a new <see cref="TreeNode{T}"/> with the specified value.
///</remarks>
///<param name="value">The value stored in this node.</param>
public sealed class TreeNode<T>(T value) : IEnumerable<TreeNode<T>>
{
    #region Fields
    readonly List<TreeNode<T>> _children = [];

    #endregion
    #region Constructors
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a child node with the specified value and returns the new child.
    ///</summary>
    ///<param name="value">The value for the new child node.</param>
    ///<returns>The newly created child <see cref="TreeNode{T}"/>.</returns>
    public TreeNode<T> AddChild(T value)
    {
        TreeNode<T> child = new(value) { Parent = this };
        _children.Add(child);
        return child;
    }

    ///<summary>
    ///Adds an existing node as a child of this node. The child is detached from its previous parent if it has one.
    ///</summary>
    ///<param name="child">The node to add as a child.</param>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="child"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">Thrown when <paramref name="child"/> is an ancestor of this node.</exception>
    public void AddChild(TreeNode<T> child)
    {
        if (child is null)
        {
            throw new ArgumentNullException(nameof(child), "Child node must not be null.");
        }

        if (IsDescendantOf(child))
        {
            throw new InvalidOperationException("Cannot add an ancestor as a child (would create a cycle).");
        }

        child.Parent?._children.Remove(child);
        child.Parent = this;
        _children.Add(child);
    }

    ///<summary>
    ///Enumerates all nodes in a breadth-first (level-order) traversal starting from this node.
    ///</summary>
    ///<returns>A sequence of nodes in breadth-first order.</returns>
    public IEnumerable<TreeNode<T>> BreadthFirst()
    {
        Queue<TreeNode<T>> queue = new();
        queue.Enqueue(this);

        while (queue.Count > 0)
        {
            TreeNode<T> current = queue.Dequeue();
            yield return current;

            foreach (TreeNode<T> child in current._children)
            {
                queue.Enqueue(child);
            }
        }
    }

    ///<summary>
    ///Enumerates all nodes in a pre-order depth-first traversal starting from this node.
    ///</summary>
    ///<returns>A sequence of nodes in pre-order.</returns>
    public IEnumerable<TreeNode<T>> DepthFirst()
    {
        Stack<TreeNode<T>> stack = new();
        stack.Push(this);

        while (stack.Count > 0)
        {
            TreeNode<T> current = stack.Pop();
            yield return current;

            for (int i = current._children.Count - 1; i >= 0; i--)
            {
                stack.Push(current._children[i]);
            }
        }
    }

    ///<summary>
    ///Default enumeration uses pre-order depth-first traversal.
    ///</summary>
    ///<inheritdoc/>
    public IEnumerator<TreeNode<T>> GetEnumerator() { return DepthFirst().GetEnumerator(); }

    ///<summary>
    ///Determines whether this node is a descendant of the specified <paramref name="ancestor"/>.
    ///</summary>
    ///<param name="ancestor">The potential ancestor node.</param>
    ///<returns><c>true</c> if this node is a descendant of <paramref name="ancestor"/>; otherwise <c>false</c>.</returns>
    public bool IsDescendantOf(TreeNode<T> ancestor)
    {
        TreeNode<T>? current = Parent;

        while (current is not null)
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    ///<summary>
    ///Removes a direct child node.
    ///</summary>
    ///<param name="child">The child to remove.</param>
    ///<returns><c>true</c> if the child was found and removed; otherwise <c>false</c>.</returns>
    public bool RemoveChild(TreeNode<T> child)
    {
        if (!_children.Remove(child))
        {
            return false;
        }

        child.Parent = null;
        return true;
    }

    ///<summary>
    ///Returns the root node of the tree this node belongs to.
    ///</summary>
    ///<returns>The root <see cref="TreeNode{T}"/>.</returns>
    public TreeNode<T> Root()
    {
        TreeNode<T> current = this;

        while (current.Parent is not null)
        {
            current = current.Parent;
        }

        return current;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the children of this node as a read-only list.
    ///</summary>
    public IReadOnlyList<TreeNode<T>> Children => _children;

    ///<summary>
    ///Gets the zero-based depth of this node in the tree (root is 0).
    ///</summary>
    public int Depth
    {
        get
        {
            int depth = 0;
            TreeNode<T>? current = Parent;

            while (current is not null)
            {
                depth++;
                current = current.Parent;
            }

            return depth;
        }
    }

    ///<summary>
    ///Gets whether this node is a leaf (has no children).
    ///</summary>
    public bool IsLeaf => _children.Count == 0;

    ///<summary>
    ///Gets whether this node is a root (has no parent).
    ///</summary>
    public bool IsRoot => Parent is null;

    ///<summary>
    ///Gets the parent of this node, or <c>null</c> if this is a root node.
    ///</summary>
    public TreeNode<T>? Parent { get; private set; }

    ///<summary>
    ///Gets or sets the value stored in this node.
    ///</summary>
    public T Value { get; set; } = value;
    #endregion
}
