using Catharsis.Collections;
using Catharsis.DesignPatterns.Behavioral;

namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class CommandPatternTests
{
    /// <summary>
    /// Tests that Command executes the action on the object and returns the same instance.
    /// Input: obj with valid execute action, no undo or history.
    /// Expected: Execute action is called, same object instance is returned.
    /// </summary>
    [TestMethod]
    public void Command_ValidExecuteWithoutUndoOrHistory_ExecutesActionAndReturnsObject()
    {
        // Arrange
        var obj = 42;
        var executeCallCount = 0;
        Action<int> execute = x => executeCallCount++;
        // Act
        var result = new CommandPattern().Command(obj, execute);
        // Assert
        Assert.AreEqual(1, executeCallCount);
        Assert.AreEqual(obj, result);
    }

    /// <summary>
    /// Tests that Command executes action but does not add undo to history when undoHistory is null.
    /// Input: obj with execute and undo actions, but null undoHistory.
    /// Expected: Execute is called, undo is not added to any collection, same object returned.
    /// </summary>
    [TestMethod]
    public void Command_UndoProvidedButHistoryIsNull_ExecutesWithoutAddingToHistory()
    {
        // Arrange
        var obj = "test";
        var executeCallCount = 0;
        Action<string> execute = x => executeCallCount++;
        Action<string> undo = x =>
        {
        };
        // Act
        var result = new CommandPattern().Command(obj, execute, undo, null);
        // Assert
        Assert.AreEqual(1, executeCallCount);
        Assert.AreEqual(obj, result);
    }

    /// <summary>
    /// Tests that Command executes action but does not add to history when undo is null.
    /// Input: obj with execute action and undoHistory, but null undo.
    /// Expected: Execute is called, nothing added to undoHistory, same object returned.
    /// </summary>
    [TestMethod]
    public void Command_HistoryProvidedButUndoIsNull_ExecutesWithoutAddingToHistory()
    {
        // Arrange
        var obj = 100;
        var executeCallCount = 0;
        Action<int> execute = x => executeCallCount++;
        var undoHistory = new List<Action<int>>();
        // Act
        var result = new CommandPattern().Command(obj, execute, null, undoHistory);
        // Assert
        Assert.AreEqual(1, executeCallCount);
        Assert.AreEqual(0, undoHistory.Count);
        Assert.AreEqual(obj, result);
    }

    /// <summary>
    /// Tests that Command executes action and adds undo to history when both are provided.
    /// Input: obj with execute, undo, and undoHistory all provided.
    /// Expected: Execute is called, undo is added to undoHistory, same object returned.
    /// </summary>
    [TestMethod]
    public void Command_BothUndoAndHistoryProvided_ExecutesAndAddsUndoToHistory()
    {
        // Arrange
        var obj = "value";
        var executeCallCount = 0;
        Action<string> execute = x => executeCallCount++;
        Action<string> undo = x =>
        {
        };
        var undoHistory = new List<Action<string>>();
        // Act
        var result = new CommandPattern().Command(obj, execute, undo, undoHistory);
        // Assert
        Assert.AreEqual(1, executeCallCount);
        Assert.AreEqual(1, undoHistory.Count);
        Assert.AreSame(undo, undoHistory[0]);
        Assert.AreEqual(obj, result);
    }

    /// <summary>
    /// Tests that Command passes the correct object to the execute action.
    /// Input: obj with execute action that captures the parameter.
    /// Expected: Execute action receives the exact object that was passed.
    /// </summary>
    [TestMethod]
    public void Command_ExecuteAction_ReceivesCorrectObject()
    {
        // Arrange
        var obj = new
        {
            Id = 123,
            Name = "Test"
        };
        object? receivedObj = null;
        Action<object> execute = x => receivedObj = x;
        // Act
        new CommandPattern().Command(obj, execute);
        // Assert
        Assert.AreSame(obj, receivedObj);
    }

    /// <summary>
    /// Tests that Command works correctly with value types.
    /// Input: int value with execute action.
    /// Expected: Execute is called with the value, same value is returned.
    /// </summary>
    [TestMethod]
    public void Command_WithValueType_ExecutesAndReturnsValue()
    {
        // Arrange
        var obj = 999;
        var capturedValue = 0;
        Action<int> execute = x => capturedValue = x;
        // Act
        var result = new CommandPattern().Command(obj, execute);
        // Assert
        Assert.AreEqual(999, capturedValue);
        Assert.AreEqual(999, result);
    }

    /// <summary>
    /// Tests that Command works correctly with reference types and maintains instance identity.
    /// Input: reference type object with execute action.
    /// Expected: Execute is called, same reference is returned (not a copy).
    /// </summary>
    [TestMethod]
    public void Command_WithReferenceType_ReturnsIdenticalReference()
    {
        // Arrange
        var obj = new List<int>
        {
            1,
            2,
            3
        };
        Action<List<int>> execute = x => x.Add(4);
        // Act
        var result = new CommandPattern().Command(obj, execute);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(4, result.Count);
    }

    /// <summary>
    /// Tests that Command adds multiple undo actions to history when called multiple times.
    /// Input: Multiple Command calls with different undo actions and shared undoHistory.
    /// Expected: All undo actions are added to undoHistory in order.
    /// </summary>
    [TestMethod]
    public void Command_MultipleCallsWithSharedHistory_AddsAllUndoActionsInOrder()
    {
        // Arrange
        var obj = 0;
        var undoHistory = new List<Action<int>>();
        Action<int> undo1 = x =>
        {
        };
        Action<int> undo2 = x =>
        {
        };
        Action<int> undo3 = x =>
        {
        };
        // Act
        new CommandPattern().Command(obj, x =>
        {
        }, undo1, undoHistory);
        new CommandPattern().Command(obj, x =>
        {
        }, undo2, undoHistory);
        new CommandPattern().Command(obj, x =>
        {
        }, undo3, undoHistory);
        // Assert
        Assert.AreEqual(3, undoHistory.Count);
        Assert.AreSame(undo1, undoHistory[0]);
        Assert.AreSame(undo2, undoHistory[1]);
        Assert.AreSame(undo3, undoHistory[2]);
    }

    /// <summary>
    /// Tests that Command works with mocked ICollection to verify Add is called correctly.
    /// Input: obj with execute, undo, and mocked undoHistory.
    /// Expected: Add method is called exactly once with the undo action.
    /// </summary>
    [TestMethod]
    public void Command_WithMockedCollection_CallsAddOnce()
    {
        // Arrange
        var obj = "test";
        Action<string> execute = x =>
        {
        };
        Action<string> undo = x =>
        {
        };
        int addCallCount = 0;
        Action<string>? addedItem = null;
        var history = new TrackingCollection<Action<string>>(item =>
        {
            addCallCount++;
            addedItem = item;
        });
        // Act
        new CommandPattern().Command(obj, execute, undo, history);
        // Assert
        Assert.AreEqual(1, addCallCount);
        Assert.AreSame(undo, addedItem);
    }

    /// <summary>
    /// Tests Command with nullable reference type object.
    /// Input: Non-null string object with execute action.
    /// Expected: Execute is called, same string is returned.
    /// </summary>
    [TestMethod]
    public void Command_WithNullableReferenceType_ExecutesAndReturnsObject()
    {
        // Arrange
        string? obj = "nullable string";
        var executeCallCount = 0;
        Action<string?> execute = x => executeCallCount++;
        // Act
        var result = new CommandPattern().Command(obj, execute);
        // Assert
        Assert.AreEqual(1, executeCallCount);
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Tests that Command correctly handles edge case with empty undoHistory collection.
    /// Input: obj with execute, undo, and empty undoHistory.
    /// Expected: Execute is called, undo is added to initially empty history.
    /// </summary>
    [TestMethod]
    public void Command_WithEmptyUndoHistory_AddsUndoSuccessfully()
    {
        // Arrange
        var obj = 42;
        Action<int> execute = x =>
        {
        };
        Action<int> undo = x =>
        {
        };
        var undoHistory = new List<Action<int>>();
        // Act
        new CommandPattern().Command(obj, execute, undo, undoHistory);
        // Assert
        Assert.AreEqual(1, undoHistory.Count);
        Assert.AreSame(undo, undoHistory[0]);
    }

    /// <summary>
    /// Tests parameterized scenarios for undo and undoHistory combinations.
    /// Input: Various combinations of undo and undoHistory (null/non-null).
    /// Expected: Undo is added to history only when both are non-null.
    /// </summary>
    [TestMethod]
    [DataRow(false, false, 0, DisplayName = "Both null - no addition")]
    [DataRow(true, false, 0, DisplayName = "Undo provided, history null - no addition")]
    [DataRow(false, true, 0, DisplayName = "History provided, undo null - no addition")]
    [DataRow(true, true, 1, DisplayName = "Both provided - undo added")]
    public void Command_UndoHistoryCombinations_AddsUndoOnlyWhenBothProvided(bool provideUndo, bool provideHistory, int expectedHistoryCount)
    {
        // Arrange
        var obj = 10;
        Action<int> execute = x =>
        {
        };
        Action<int>? undo = provideUndo ? (x =>
        {
        }) : null;
        ICollection<Action<int>>? undoHistory = provideHistory ? new List<Action<int>>() : null;
        // Act
        new CommandPattern().Command(obj, execute, undo, undoHistory);
        // Assert
        if (provideHistory)
        {
            Assert.AreEqual(expectedHistoryCount, undoHistory!.Count);
        }
    }

    /// <summary>
    /// Tests that Command with complex object types executes correctly.
    /// Input: Dictionary object with execute action that modifies it.
    /// Expected: Execute is called, dictionary is modified, same instance returned.
    /// </summary>
    [TestMethod]
    public void Command_WithComplexType_ExecutesAndReturnsObject()
    {
        // Arrange
        var obj = new Dictionary<string, int>
        {
            {
                "a",
                1
            }
        };
        Action<Dictionary<string, int>> execute = x => x["b"] = 2;
        // Act
        var result = new CommandPattern().Command(obj, execute);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(2, result["b"]);
    }

    /// <summary>
    /// Tests that Command can be chained fluently.
    /// Input: Multiple chained Command calls.
    /// Expected: All execute actions are called, same object returned for chaining.
    /// </summary>
    [TestMethod]
    public void Command_ChainedCalls_AllExecuteActionsCalledAndObjectReturned()
    {
        // Arrange
        var obj = new List<int> { 0 };
        var undoHistory = new List<Action<List<int>>>();
        // Act
        var cmd = new CommandPattern();
        var result = cmd.Command(obj, x => x[0] = x[0] + 1, x => x[0] = x[0] - 1, undoHistory);
        result = cmd.Command(result, x => x[0] = x[0] * 2, x => x[0] = x[0] / 2, undoHistory);
        result = cmd.Command(result, x => x[0] = x[0] + 10, x => x[0] = x[0] - 10, undoHistory);
        // Assert
        Assert.AreEqual(12, obj[0]); // ((0 + 1) * 2) + 10 = 12
        Assert.AreEqual(3, undoHistory.Count);
        Assert.AreSame(obj, result);
    }
}
