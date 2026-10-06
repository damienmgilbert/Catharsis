using Catharsis.Collections;
using Catharsis.DesignPatterns.Behavioral;

namespace Catharsis.UnitTests.DesignPatterns.Behavioral;

[TestClass]
public class CommandPatternTests
{
    #region Public methods

    ///<summary>
    ///Tests that Command executes action and adds undo to history when both are provided. Input: obj with execute,
    ///undo, and undoHistory all provided. Expected: Execute is called, undo is added to undoHistory, same object
    ///returned.
    ///</summary>
    [TestMethod]
    public void Command_BothUndoAndHistoryProvided_ExecutesAndAddsUndoToHistory()
    {
        // Arrange
        string obj = "value";
        int executeCallCount = 0;
        Action<string> execute = x => executeCallCount++;
        Action<string> undo = x =>
        {
        };
        List<Action<string>> undoHistory = [];
        // Act
        string result = CommandPattern.Command(obj, execute, undo, undoHistory);
        // Assert
        Assert.AreEqual(1, executeCallCount);
        Assert.HasCount(1, undoHistory);
        Assert.AreSame(undo, undoHistory[0]);
        Assert.AreEqual(obj, result);
    }

    ///<summary>
    ///Tests that Command can be chained fluently. Input: Multiple chained Command calls. Expected: All execute actions
    ///are called, same object returned for chaining.
    ///</summary>
    [TestMethod]
    public void Command_ChainedCalls_AllExecuteActionsCalledAndObjectReturned()
    {
        // Arrange
        List<int> obj = [0];
        List<Action<List<int>>> undoHistory = [];
        // Act
        List<int> result = CommandPattern.Command(obj, static x => x[0] = x[0] + 1, static x => x[0] = x[0] - 1, undoHistory);
        result = CommandPattern.Command(result, static x => x[0] = x[0] * 2, static x => x[0] = x[0] / 2, undoHistory);
        result = CommandPattern.Command(result, static x => x[0] = x[0] + 10, static x => x[0] = x[0] - 10, undoHistory);
        // Assert
        Assert.AreEqual(12, obj[0]); // ((0 + 1) * 2) + 10 = 12
        Assert.HasCount(3, undoHistory);
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Tests that Command passes the correct object to the execute action. Input: obj with execute action that captures
    ///the parameter. Expected: Execute action receives the exact object that was passed.
    ///</summary>
    [TestMethod]
    public void Command_ExecuteAction_ReceivesCorrectObject()
    {
        // Arrange
        var obj = new { Id = 123, Name = "Test" };
        object? receivedObj = null;
        Action<object> execute = x => receivedObj = x;
        // Act
        CommandPattern.Command(obj, execute);
        // Assert
        Assert.AreSame(obj, receivedObj);
    }

    ///<summary>
    ///Tests that Command executes action but does not add to history when undo is null. Input: obj with execute action
    ///and undoHistory, but null undo. Expected: Execute is called, nothing added to undoHistory, same object returned.
    ///</summary>
    [TestMethod]
    public void Command_HistoryProvidedButUndoIsNull_ExecutesWithoutAddingToHistory()
    {
        // Arrange
        int obj = 100;
        int executeCallCount = 0;
        Action<int> execute = x => executeCallCount++;
        List<Action<int>> undoHistory = [];
        // Act
        int result = CommandPattern.Command(obj, execute, null, undoHistory);
        // Assert
        Assert.AreEqual(1, executeCallCount);
        Assert.IsEmpty(undoHistory);
        Assert.AreEqual(obj, result);
    }

    ///<summary>
    ///Tests that Command adds multiple undo actions to history when called multiple times. Input: Multiple Command
    ///calls with different undo actions and shared undoHistory. Expected: All undo actions are added to undoHistory in
    ///order.
    ///</summary>
    [TestMethod]
    public void Command_MultipleCallsWithSharedHistory_AddsAllUndoActionsInOrder()
    {
        // Arrange
        int obj = 0;
        List<Action<int>> undoHistory = [];
        Action<int> undo1 = static x =>
        {
        };
        Action<int> undo2 = static x =>
        {
        };
        Action<int> undo3 = static x =>
        {
        };
        // Act
        CommandPattern.Command(
        obj,
        static x =>
        {
        },
        undo1,
        undoHistory);
        CommandPattern.Command(
        obj,
        static x =>
        {
        },
        undo2,
        undoHistory);
        CommandPattern.Command(
        obj,
        static x =>
        {
        },
        undo3,
        undoHistory);
        // Assert
        Assert.HasCount(3, undoHistory);
        Assert.AreSame(undo1, undoHistory[0]);
        Assert.AreSame(undo2, undoHistory[1]);
        Assert.AreSame(undo3, undoHistory[2]);
    }

    ///<summary>
    ///Tests parameterized scenarios for undo and undoHistory combinations. Input: Various combinations of undo and
    ///undoHistory (null/non-null). Expected: Undo is added to history only when both are non-null.
    ///</summary>
    [TestMethod]
    [DataRow(false, false, 0, DisplayName = "Both null - no addition")]
    [DataRow(true, false, 0, DisplayName = "Undo provided, history null - no addition")]
    [DataRow(false, true, 0, DisplayName = "History provided, undo null - no addition")]
    [DataRow(true, true, 1, DisplayName = "Both provided - undo added")]
    public void Command_UndoHistoryCombinations_AddsUndoOnlyWhenBothProvided(bool provideUndo, bool provideHistory, int expectedHistoryCount)
    {
        // Arrange
        int obj = 10;
        Action<int> execute = static x =>
        {
        };
        Action<int>? undo = provideUndo
                            ? (static x =>
        {
        })
                            : null;
        ICollection<Action<int>>? undoHistory = provideHistory ? (new List<Action<int>>()) : null;
        // Act
        CommandPattern.Command(obj, execute, undo, undoHistory);
        // Assert
        if (provideHistory)
        {
            Assert.HasCount(expectedHistoryCount, undoHistory!);
        }
    }

    ///<summary>
    ///Tests that Command executes action but does not add undo to history when undoHistory is null. Input: obj with
    ///execute and undo actions, but null undoHistory. Expected: Execute is called, undo is not added to any collection,
    ///same object returned.
    ///</summary>
    [TestMethod]
    public void Command_UndoProvidedButHistoryIsNull_ExecutesWithoutAddingToHistory()
    {
        // Arrange
        string obj = "test";
        int executeCallCount = 0;
        Action<string> execute = x => executeCallCount++;
        Action<string> undo = x =>
        {
        };
        // Act
        string result = CommandPattern.Command(obj, execute, undo, null);
        // Assert
        Assert.AreEqual(1, executeCallCount);
        Assert.AreEqual(obj, result);
    }

    ///<summary>
    ///Tests that Command executes the action on the object and returns the same instance. Input: obj with valid execute
    ///action, no undo or history. Expected: Execute action is called, same object instance is returned.
    ///</summary>
    [TestMethod]
    public void Command_ValidExecuteWithoutUndoOrHistory_ExecutesActionAndReturnsObject()
    {
        // Arrange
        int obj = 42;
        int executeCallCount = 0;
        Action<int> execute = x => executeCallCount++;
        // Act
        int result = CommandPattern.Command(obj, execute);
        // Assert
        Assert.AreEqual(1, executeCallCount);
        Assert.AreEqual(obj, result);
    }

    ///<summary>
    ///Tests that Command with complex object types executes correctly. Input: Dictionary object with execute action
    ///that modifies it. Expected: Execute is called, dictionary is modified, same instance returned.
    ///</summary>
    [TestMethod]
    public void Command_WithComplexType_ExecutesAndReturnsObject()
    {
        // Arrange
        Dictionary<string, int> obj = new() { { "a", 1 } };
        Action<Dictionary<string, int>> execute = static x => x["b"] = 2;
        // Act
        Dictionary<string, int> result = CommandPattern.Command(obj, execute);
        // Assert
        Assert.AreSame(obj, result);
        Assert.HasCount(2, result);
        Assert.AreEqual(2, result["b"]);
    }

    ///<summary>
    ///Tests that Command correctly handles edge case with empty undoHistory collection. Input: obj with execute, undo,
    ///and empty undoHistory. Expected: Execute is called, undo is added to initially empty history.
    ///</summary>
    [TestMethod]
    public void Command_WithEmptyUndoHistory_AddsUndoSuccessfully()
    {
        // Arrange
        int obj = 42;
        Action<int> execute = static x =>
        {
        };
        Action<int> undo = static x =>
        {
        };
        List<Action<int>> undoHistory = [];
        // Act
        CommandPattern.Command(obj, execute, undo, undoHistory);
        // Assert
        Assert.HasCount(1, undoHistory);
        Assert.AreSame(undo, undoHistory[0]);
    }

    ///<summary>
    ///Tests that Command works with mocked ICollection to verify Add is called correctly. Input: obj with execute,
    ///undo, and mocked undoHistory. Expected: Add method is called exactly once with the undo action.
    ///</summary>
    [TestMethod]
    public void Command_WithMockedCollection_CallsAddOnce()
    {
        // Arrange
        string obj = "test";
        Action<string> execute = x =>
        {
        };
        Action<string> undo = x =>
        {
        };
        int addCallCount = 0;
        Action<string>? addedItem = null;
        TrackingCollection<Action<string>> history = new(
                                                     item =>
                                                     {
                                                         addCallCount++;
                                                         addedItem = item;
                                                     });
        // Act
        CommandPattern.Command(obj, execute, undo, history);
        // Assert
        Assert.AreEqual(1, addCallCount);
        Assert.AreSame(undo, addedItem);
    }

    ///<summary>
    ///Tests Command with nullable reference type object. Input: Non-null string object with execute action. Expected:
    ///Execute is called, same string is returned.
    ///</summary>
    [TestMethod]
    public void Command_WithNullableReferenceType_ExecutesAndReturnsObject()
    {
        // Arrange
        string? obj = "nullable string";
        int executeCallCount = 0;
        Action<string?> execute = x => executeCallCount++;
        // Act
        string result = CommandPattern.Command(obj, execute);
        // Assert
        Assert.AreEqual(1, executeCallCount);
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Tests that Command works correctly with reference types and maintains instance identity. Input: reference type
    ///object with execute action. Expected: Execute is called, same reference is returned (not a copy).
    ///</summary>
    [TestMethod]
    public void Command_WithReferenceType_ReturnsIdenticalReference()
    {
        // Arrange
        List<int> obj = [1, 2, 3];
        Action<List<int>> execute = static x => x.Add(4);
        // Act
        List<int> result = CommandPattern.Command(obj, execute);
        // Assert
        Assert.AreSame(obj, result);
        Assert.HasCount(4, result);
    }

    ///<summary>
    ///Tests that Command works correctly with value types. Input: int value with execute action. Expected: Execute is
    ///called with the value, same value is returned.
    ///</summary>
    [TestMethod]
    public void Command_WithValueType_ExecutesAndReturnsValue()
    {
        // Arrange
        int obj = 999;
        int capturedValue = 0;
        Action<int> execute = x => capturedValue = x;
        // Act
        int result = CommandPattern.Command(obj, execute);
        // Assert
        Assert.AreEqual(999, capturedValue);
        Assert.AreEqual(999, result);
    }
    #endregion
}
