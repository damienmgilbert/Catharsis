using Catharsis.DesignPatterns.Behavioral;

namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class MementoTests
{
    /// <summary>
    /// Tests that Restore invokes the restore action and returns the original object.
    /// Input: valid object, memento, and restore action.
    /// Expected: restore action is invoked with correct parameters, and the original object is returned.
    /// </summary>
    [TestMethod]
    public void Restore_ValidRestoreAction_InvokesActionAndReturnsObject()
    {
        // Arrange
        var obj = "test object";
        var memento = "test memento";
        var wasCalled = false;
        string? capturedObj = null;
        string? capturedMemento = null;
        void RestoreAction(string o, string m)
        {
            wasCalled = true;
            capturedObj = o;
            capturedMemento = m;
        }

        // Act
        var result = new Memento().Restore(obj, memento, RestoreAction);
        // Assert
        Assert.IsTrue(wasCalled);
        Assert.AreEqual(obj, capturedObj);
        Assert.AreEqual(memento, capturedMemento);
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Tests that Restore handles null object correctly.
    /// Input: null object, valid memento and restore action.
    /// Expected: restore action is invoked with null object, and null is returned.
    /// </summary>
    [TestMethod]
    public void Restore_NullObject_InvokesActionWithNullAndReturnsNull()
    {
        // Arrange
        string? obj = null;
        var memento = "test memento";
        var wasCalled = false;
        string? capturedObj = "not null";
        string? capturedMemento = null;
        void RestoreAction(string? o, string m)
        {
            wasCalled = true;
            capturedObj = o;
            capturedMemento = m;
        }

        // Act
        var result = new Memento().Restore(obj, memento, RestoreAction);
        // Assert
        Assert.IsTrue(wasCalled);
        Assert.IsNull(capturedObj);
        Assert.AreEqual(memento, capturedMemento);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Restore handles null memento correctly.
    /// Input: valid object, null memento, and valid restore action.
    /// Expected: restore action is invoked with null memento, and the object is returned.
    /// </summary>
    [TestMethod]
    public void Restore_NullMemento_InvokesActionWithNullMemento()
    {
        // Arrange
        var obj = "test object";
        string? memento = null;
        var wasCalled = false;
        string? capturedObj = null;
        string? capturedMemento = "not null";
        void RestoreAction(string o, string? m)
        {
            wasCalled = true;
            capturedObj = o;
            capturedMemento = m;
        }

        // Act
        var result = new Memento().Restore(obj, memento, RestoreAction);
        // Assert
        Assert.IsTrue(wasCalled);
        Assert.AreEqual(obj, capturedObj);
        Assert.IsNull(capturedMemento);
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Tests that Restore handles both null object and null memento correctly.
    /// Input: null object, null memento, and valid restore action.
    /// Expected: restore action is invoked with both null values, and null is returned.
    /// </summary>
    [TestMethod]
    public void Restore_NullObjectAndNullMemento_InvokesActionAndReturnsNull()
    {
        // Arrange
        string? obj = null;
        string? memento = null;
        var wasCalled = false;
        string? capturedObj = "not null";
        string? capturedMemento = "not null";
        void RestoreAction(string? o, string? m)
        {
            wasCalled = true;
            capturedObj = o;
            capturedMemento = m;
        }

        // Act
        var result = new Memento().Restore(obj, memento, RestoreAction);
        // Assert
        Assert.IsTrue(wasCalled);
        Assert.IsNull(capturedObj);
        Assert.IsNull(capturedMemento);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Restore works correctly with value types.
    /// Input: value type object and memento with valid restore action.
    /// Expected: restore action is invoked with correct values, and the value is returned.
    /// </summary>
    [TestMethod]
    public void Restore_ValueTypes_InvokesActionAndReturnsValue()
    {
        // Arrange
        var obj = 42;
        var memento = 100;
        var wasCalled = false;
        int capturedObj = 0;
        int capturedMemento = 0;
        void RestoreAction(int o, int m)
        {
            wasCalled = true;
            capturedObj = o;
            capturedMemento = m;
        }

        // Act
        var result = new Memento().Restore(obj, memento, RestoreAction);
        // Assert
        Assert.IsTrue(wasCalled);
        Assert.AreEqual(obj, capturedObj);
        Assert.AreEqual(memento, capturedMemento);
        Assert.AreEqual(obj, result);
    }
}
