using Catharsis;
using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="HistoryStack"/> class.
///</summary>
[TestClass]
public class HistoryStackTests
{
    [TestMethod]
    public void Push_AddsItemToStack()
    {
        HistoryStack<int> s = new();
        s.Push(1);
        Assert.HasCount(1, s);
        Assert.AreEqual(1, s.Peek());
    }

    [TestMethod]
    public void Peek_EmptyStack_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(static () => new HistoryStack<int>().Peek());
    }

    [TestMethod]
    public void Undo_MovesItemToRedoStack()
    {
        HistoryStack<int> s = new();
        s.Push(1); s.Push(2);

        int undone = s.Undo();

        Assert.AreEqual(2, undone);
        Assert.HasCount(1, s);
        Assert.IsTrue(s.CanRedo);
    }

    [TestMethod]
    public void Undo_EmptyStack_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(static () => new HistoryStack<int>().Undo());
    }

    [TestMethod]
    public void Redo_RestoresUndoneItem()
    {
        HistoryStack<int> s = new();
        s.Push(1); s.Push(2);
        s.Undo();

        int redone = s.Redo();

        Assert.AreEqual(2, redone);
        Assert.HasCount(2, s);
        Assert.IsFalse(s.CanRedo);
    }

    [TestMethod]
    public void Redo_NothingToRedo_Throws()
    {
        HistoryStack<int> s = new();
        s.Push(1);
        Assert.ThrowsExactly<InvalidOperationException>(() => s.Redo());
    }

    [TestMethod]
    public void Push_ClearsRedoHistory()
    {
        HistoryStack<int> s = new();
        s.Push(1); s.Push(2);
        s.Undo();
        Assert.IsTrue(s.CanRedo);

        s.Push(3);

        Assert.IsFalse(s.CanRedo);
    }

    [TestMethod]
    public void Clear_ResetsAll()
    {
        HistoryStack<int> s = new();
        s.Push(1); s.Push(2);
        s.Undo();
        s.Clear();
        Assert.IsEmpty(s);
        Assert.IsFalse(s.CanUndo);
        Assert.IsFalse(s.CanRedo);
    }
}
