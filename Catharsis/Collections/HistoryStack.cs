using System.Collections;

namespace Catharsis.Collections;

///<summary>
///A stack with built-in undo/redo semantics. Push and Pop operations are recorded so they can be reversed with <see
///cref="Undo"/> and replayed with <see cref="Redo"/>.
///</summary>
///<typeparam name="T">The type of elements stored in the stack.</typeparam>
public sealed class HistoryStack<T> : IEnumerable<T>, IReadOnlyCollection<T>
{
    #region Fields
    readonly Stack<T> _stack = new();
    readonly Stack<T> _undone = new();
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<summary>
    ///Removes all items and clears both undo and redo history.
    ///</summary>
    public void Clear()
    {
        _stack.Clear();
        _undone.Clear();
    }

    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator() { return _stack.GetEnumerator(); }

    ///<summary>
    ///Returns the item at the top of the stack without removing it.
    ///</summary>
    ///<returns>The item at the top.</returns>
    ///<exception cref="InvalidOperationException">Thrown when the stack is empty.</exception>
    public T Peek()
    {
        if(_stack.Count == 0)
        {
            throw new InvalidOperationException("The stack is empty.");
        }

        return _stack.Peek();
    }

    ///<summary>
    ///Pushes an item onto the stack and clears the redo history.
    ///</summary>
    ///<param name="item">The item to push.</param>
    public void Push(T item)
    {
        _stack.Push(item);
        _undone.Clear();
    }

    ///<summary>
    ///Redoes the last undone operation by moving the item back onto the stack.
    ///</summary>
    ///<returns>The item that was redone.</returns>
    ///<exception cref="InvalidOperationException">Thrown when there is nothing to redo.</exception>
    public T Redo()
    {
        if(_undone.Count == 0)
        {
            throw new InvalidOperationException("There is nothing to redo.");
        }

        T item = _undone.Pop();
        _stack.Push(item);
        return item;
    }

    ///<summary>
    ///Undoes the last push by moving the top item to the redo stack.
    ///</summary>
    ///<returns>The item that was undone.</returns>
    ///<exception cref="InvalidOperationException">Thrown when there is nothing to undo.</exception>
    public T Undo()
    {
        if(_stack.Count == 0)
        {
            throw new InvalidOperationException("There is nothing to undo.");
        }

        T item = _stack.Pop();
        _undone.Push(item);
        return item;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets whether a <see cref="Redo"/> operation is available.
    ///</summary>
    public bool CanRedo => _undone.Count > 0;

    ///<summary>
    ///Gets whether an <see cref="Undo"/> operation is available.
    ///</summary>
    public bool CanUndo => _stack.Count > 0;

    ///<summary>
    ///Gets the number of items currently on the stack.
    ///</summary>
    public int Count => _stack.Count;
    #endregion
}
