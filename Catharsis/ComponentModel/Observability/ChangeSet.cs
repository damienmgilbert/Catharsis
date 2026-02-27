namespace Catharsis.ComponentModel.Observability;

///<summary>
///A collection of <see cref="ChangeEntry"/> records representing a group of property changes that can be queried,
///filtered, and applied as a unit.
///</summary>
///<remarks>
///<see cref="ChangeSet"/> supports undo/redo by maintaining separate stacks for uncommitted and undone changes. Use
///<see cref="Record"/> to add changes, <see cref="Undo"/> and <see cref="Redo"/> to navigate the history, and <see
///cref="AcceptAll"/> to commit changes.
///</remarks>
public sealed class ChangeSet
{
    #region Fields
    readonly Stack<ChangeEntry> _redoStack = new();
    readonly Stack<ChangeEntry> _undoStack = new();
    #endregion

    #region Public methods
    ///<summary>
    ///Accepts all changes, clearing both the undo and redo stacks.
    ///</summary>
    public void AcceptAll()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    ///<summary>
    ///Clears all changes without accepting them, resetting both stacks.
    ///</summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    ///<summary>
    ///Gets all uncommitted changes in chronological order (oldest first).
    ///</summary>
    ///<returns>A read-only list of change entries.</returns>
    public IReadOnlyList<ChangeEntry> GetAll() { return _undoStack.Reverse().ToList(); }

    ///<summary>
    ///Gets all uncommitted changes for the specified property.
    ///</summary>
    ///<param name="propertyName">The property name to filter by.</param>
    ///<returns>A read-only list of matching change entries.</returns>
    public IReadOnlyList<ChangeEntry> GetByProperty(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        return _undoStack
            .Where(e => string.Equals(e.PropertyName, propertyName, StringComparison.Ordinal))
            .Reverse()
            .ToList();
    }

    ///<summary>
    ///Records a new property change, clearing the redo stack.
    ///</summary>
    ///<param name="entry">The change entry to record.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="entry"/> is <c>null</c>.
    ///</exception>
    public void Record(ChangeEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _undoStack.Push(entry);
        _redoStack.Clear();
    }

    ///<summary>
    ///Records a new property change, clearing the redo stack.
    ///</summary>
    ///<param name="propertyName">The name of the property that changed.</param>
    ///<param name="oldValue">The value before the change.</param>
    ///<param name="newValue">The value after the change.</param>
    public void Record(string propertyName, object? oldValue, object? newValue)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        Record(new ChangeEntry(propertyName, oldValue, newValue));
    }

    ///<summary>
    ///Redoes the most recently undone change by popping it from the redo stack and pushing it back onto the undo stack.
    ///</summary>
    ///<returns>
    ///The <see cref="ChangeEntry"/> that was redone, or <c>null</c> if the redo stack is empty.
    ///</returns>
    public ChangeEntry? Redo()
    {
        if(_redoStack.Count == 0)
        {
            return null;
        }

        ChangeEntry entry = _redoStack.Pop();
        _undoStack.Push(entry);
        return entry;
    }

    ///<summary>
    ///Undoes the most recent change by popping it from the undo stack and pushing it onto the redo stack.
    ///</summary>
    ///<returns>
    ///The <see cref="ChangeEntry"/> that was undone, or <c>null</c> if the undo stack is empty.
    ///</returns>
    public ChangeEntry? Undo()
    {
        if(_undoStack.Count == 0)
        {
            return null;
        }

        ChangeEntry entry = _undoStack.Pop();
        _redoStack.Push(entry);
        return entry;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a value indicating whether there are changes that can be redone.
    ///</summary>
    public bool CanRedo => _redoStack.Count > 0;

    ///<summary>
    ///Gets a value indicating whether there are changes that can be undone.
    ///</summary>
    public bool CanUndo => _undoStack.Count > 0;

    ///<summary>
    ///Gets the total number of uncommitted changes (undo stack size).
    ///</summary>
    public int Count => _undoStack.Count;

    ///<summary>
    ///Gets a value indicating whether there are any uncommitted changes.
    ///</summary>
    public bool HasChanges => _undoStack.Count > 0;
    #endregion
}
