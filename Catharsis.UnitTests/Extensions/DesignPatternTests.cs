using Catharsis.Collections;
using Catharsis.DataStructures;
using Catharsis.DesignPatterns.Behavioral;
using Catharsis.DesignPatterns.Creational;
using Catharsis.DesignPatterns.Structural;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Collections.Concurrent;
using System.Text;

namespace Catharsis.Extensions.UnitTests;
/// <summary>
/// Unit tests for the <see cref = "DesignPattern.Iterate{T, TElement}"/> method.
/// </summary>
[TestClass]
public partial class DesignPatternTests
{
    /// <summary>
    /// Tests that Iterate handles empty collection correctly without calling action.
    /// </summary>
    [TestMethod]
    public void Iterate_EmptyCollection_ReturnsObjectWithoutCallingAction()
    {
        // Arrange
        var obj = "test";
        var callCount = 0;
        Func<string, IEnumerable<char>> getElements = s => new List<char>();
        Action<char> action = c => callCount++;
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(0, callCount);
    }

    /// <summary>
    /// Tests that Iterate calls action once for a single element collection.
    /// </summary>
    [TestMethod]
    public void Iterate_SingleElement_CallsActionOnce()
    {
        // Arrange
        var obj = "A";
        var capturedElements = new List<char>();
        Func<string, IEnumerable<char>> getElements = s => s;
        Action<char> action = c => capturedElements.Add(c);
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(1, capturedElements.Count);
        Assert.AreEqual('A', capturedElements[0]);
    }

    /// <summary>
    /// Tests that Iterate calls action for each element in the correct order.
    /// </summary>
    [TestMethod]
    public void Iterate_MultipleElements_CallsActionForEachElementInOrder()
    {
        // Arrange
        var obj = new List<int>
        {
            1,
            2,
            3,
            4,
            5
        };
        var capturedElements = new List<int>();
        Func<List<int>, IEnumerable<int>> getElements = list => list;
        Action<int> action = i => capturedElements.Add(i);
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(5, capturedElements.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, capturedElements);
    }

    /// <summary>
    /// Tests that Iterate works correctly when obj is null (for nullable reference types).
    /// </summary>
    [TestMethod]
    public void Iterate_NullObject_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        var callCount = 0;
        Func<string?, IEnumerable<char>> getElements = s => s ?? string.Empty;
        Action<char> action = c => callCount++;
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(0, callCount);
    }

    /// <summary>
    /// Tests that Iterate works correctly with value types.
    /// </summary>
    [TestMethod]
    public void Iterate_ValueTypeObject_WorksCorrectly()
    {
        // Arrange
        var obj = 42;
        var capturedElements = new List<char>();
        Func<int, IEnumerable<char>> getElements = i => i.ToString();
        Action<char> action = c => capturedElements.Add(c);
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(2, capturedElements.Count);
        CollectionAssert.AreEqual(new[] { '4', '2' }, capturedElements);
    }

    /// <summary>
    /// Tests that Iterate returns the same reference (reference equality) for reference types.
    /// </summary>
    [TestMethod]
    public void Iterate_ReferenceType_ReturnsSameReference()
    {
        // Arrange
        var obj = new List<string>
        {
            "a",
            "b",
            "c"
        };
        Func<List<string>, IEnumerable<string>> getElements = list => list;
        Action<string> action = s =>
        {
        };
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.IsTrue(ReferenceEquals(obj, result));
    }

    /// <summary>
    /// Tests that Iterate works correctly with complex element types.
    /// </summary>
    [TestMethod]
    public void Iterate_ComplexElementType_WorksCorrectly()
    {
        // Arrange
        var obj = new Dictionary<string, int>
        {
            {
                "one",
                1
            },
            {
                "two",
                2
            },
            {
                "three",
                3
            }
        };
        var capturedKeys = new List<string>();
        Func<Dictionary<string, int>, IEnumerable<KeyValuePair<string, int>>> getElements = dict => dict;
        Action<KeyValuePair<string, int>> action = kvp => capturedKeys.Add(kvp.Key);
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(3, capturedKeys.Count);
        CollectionAssert.Contains(capturedKeys, "one");
        CollectionAssert.Contains(capturedKeys, "two");
        CollectionAssert.Contains(capturedKeys, "three");
    }

    /// <summary>
    /// Tests that Iterate allows action to modify external state.
    /// </summary>
    [TestMethod]
    public void Iterate_ActionModifiesExternalState_StateIsModified()
    {
        // Arrange
        var obj = new[]
        {
            1,
            2,
            3,
            4,
            5
        };
        var sum = 0;
        Func<int[], IEnumerable<int>> getElements = arr => arr;
        Action<int> action = i => sum += i;
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(15, sum);
    }

    /// <summary>
    /// Tests that Iterate works with edge case of int.MaxValue elements count simulation.
    /// </summary>
    [TestMethod]
    public void Iterate_LargeCollection_WorksCorrectly()
    {
        // Arrange
        var obj = new List<int>(1000);
        for (int i = 0; i < 1000; i++)
        {
            obj.Add(i);
        }

        var count = 0;
        Func<List<int>, IEnumerable<int>> getElements = list => list;
        Action<int> action = i => count++;
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(1000, count);
    }

    /// <summary>
    /// Tests that Strategy correctly applies a valid strategy function to transform a reference type.
    /// Input: string object with strategy that converts to uppercase.
    /// Expected: Transformed string returned.
    /// </summary>
    [TestMethod]
    public void Strategy_ValidStrategyWithReferenceType_ReturnsTransformedValue()
    {
        // Arrange
        string obj = "hello";
        Func<string, string> strategy = s => s.ToUpper();
        // Act
        string result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual("HELLO", result);
    }

    /// <summary>
    /// Tests that Strategy correctly applies a valid strategy function to transform a value type.
    /// Input: integer with strategy that converts to string.
    /// Expected: String representation of integer returned.
    /// </summary>
    [TestMethod]
    public void Strategy_ValidStrategyWithValueType_ReturnsTransformedValue()
    {
        // Arrange
        int obj = 123;
        Func<int, string> strategy = i => $"Number: {i}";
        // Act
        string result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual("Number: 123", result);
    }

    /// <summary>
    /// Tests that Strategy returns null when the strategy function returns null for nullable return type.
    /// Input: object with strategy that returns null.
    /// Expected: null returned.
    /// </summary>
    [TestMethod]
    public void Strategy_StrategyReturnsNull_ReturnsNull()
    {
        // Arrange
        int obj = 42;
        Func<int, string?> strategy = _ => null;
        // Act
        string? result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Strategy passes null object to strategy function when obj is null for reference types.
    /// Input: null string object with strategy that checks for null.
    /// Expected: Strategy receives null and returns expected value.
    /// </summary>
    [TestMethod]
    public void Strategy_NullObjectParameter_PassesNullToStrategy()
    {
        // Arrange
        string? obj = null;
        Func<string?, string> strategy = s => s == null ? "was null" : "was not null";
        // Act
        string result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual("was null", result);
    }

    /// <summary>
    /// Tests that Strategy correctly handles identity transformation.
    /// Input: object with strategy that returns the same object.
    /// Expected: Same object returned.
    /// </summary>
    [TestMethod]
    public void Strategy_IdentityTransformation_ReturnsSameObject()
    {
        // Arrange
        string obj = "test";
        Func<string, string> strategy = s => s;
        // Act
        string result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Tests that Strategy correctly applies complex transformation logic.
    /// Input: integer with complex calculation strategy.
    /// Expected: Correct calculation result returned.
    /// </summary>
    [TestMethod]
    public void Strategy_ComplexTransformation_ReturnsCorrectResult()
    {
        // Arrange
        int obj = 10;
        Func<int, int> strategy = x => (x * x) + (x * 2) + 5;
        // Act
        int result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual(125, result); // (10 * 10) + (10 * 2) + 5 = 125
    }

    /// <summary>
    /// Tests that Strategy handles transformation from value type to different value type.
    /// Input: double with strategy converting to int.
    /// Expected: Correctly converted value.
    /// </summary>
    [TestMethod]
    public void Strategy_ValueTypeToValueType_ReturnsConvertedValue()
    {
        // Arrange
        double obj = 42.7;
        Func<double, int> strategy = d => (int)d;
        // Act
        int result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual(42, result);
    }

    /// <summary>
    /// Tests that Strategy handles transformation from reference type to value type.
    /// Input: string with strategy converting to int.
    /// Expected: Correctly converted value.
    /// </summary>
    [TestMethod]
    public void Strategy_ReferenceTypeToValueType_ReturnsConvertedValue()
    {
        // Arrange
        string obj = "42";
        Func<string, int> strategy = s => int.Parse(s);
        // Act
        int result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual(42, result);
    }

    /// <summary>
    /// Tests that Strategy handles edge case with maximum integer value.
    /// Input: int.MaxValue with transformation strategy.
    /// Expected: Correctly transformed value.
    /// </summary>
    [TestMethod]
    public void Strategy_MaxIntValue_ReturnsTransformedValue()
    {
        // Arrange
        int obj = int.MaxValue;
        Func<int, long> strategy = i => (long)i * 2;
        // Act
        long result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual(4294967294L, result);
    }

    /// <summary>
    /// Tests that Strategy handles edge case with minimum integer value.
    /// Input: int.MinValue with transformation strategy.
    /// Expected: Correctly transformed value.
    /// </summary>
    [TestMethod]
    public void Strategy_MinIntValue_ReturnsTransformedValue()
    {
        // Arrange
        int obj = int.MinValue;
        Func<int, long> strategy = i => (long)i * 2;
        // Act
        long result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual(-4294967296L, result);
    }

    /// <summary>
    /// Tests that Strategy handles empty string input.
    /// Input: empty string with length calculation strategy.
    /// Expected: Returns 0.
    /// </summary>
    [TestMethod]
    public void Strategy_EmptyString_ReturnsCorrectResult()
    {
        // Arrange
        string obj = string.Empty;
        Func<string, int> strategy = s => s.Length;
        // Act
        int result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    /// Tests that Strategy handles whitespace-only string input.
    /// Input: whitespace string with trim and length strategy.
    /// Expected: Returns 0 after trimming.
    /// </summary>
    [TestMethod]
    public void Strategy_WhitespaceString_ReturnsCorrectResult()
    {
        // Arrange
        string obj = "   ";
        Func<string, int> strategy = s => s.Trim().Length;
        // Act
        int result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual(0, result);
    }

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


    /// <summary>
    /// Tests that Decorate applies multiple decorators in sequence, left-to-right order.
    /// Input: object and multiple decorators.
    /// Expected: decorators applied in order, each receiving the result of the previous.
    /// </summary>
    [TestMethod]
    public void Decorate_MultipleDecorators_AppliesInOrder()
    {
        // Arrange
        int obj = 5;
        Func<int, int> addTen = x => x + 10; // 5 + 10 = 15
        Func<int, int> multiplyByTwo = x => x * 2; // 15 * 2 = 30
        Func<int, int> subtractThree = x => x - 3; // 30 - 3 = 27
        // Act
        int result = new Decorator().Decorate(obj, addTen, multiplyByTwo, subtractThree);
        // Assert
        Assert.AreEqual(27, result);
    }

    /// <summary>
    /// Tests that Decorate verifies order matters by applying decorators in different sequence.
    /// Input: same decorators but different order.
    /// Expected: different results demonstrating order-dependent application.
    /// </summary>
    [TestMethod]
    public void Decorate_DifferentOrder_ProducesDifferentResult()
    {
        // Arrange
        int obj = 5;
        Func<int, int> addTen = x => x + 10;
        Func<int, int> multiplyByTwo = x => x * 2;
        // Act
        int result1 = new Decorator().Decorate(obj, addTen, multiplyByTwo); // (5 + 10) * 2 = 30
        int result2 = new Decorator().Decorate(obj, multiplyByTwo, addTen); // (5 * 2) + 10 = 20
        // Assert
        Assert.AreEqual(30, result1);
        Assert.AreEqual(20, result2);
        Assert.AreNotEqual(result1, result2);
    }

    /// <summary>
    /// Tests that Decorate works correctly with reference types.
    /// Input: string object and string transformation decorators.
    /// Expected: decorators applied in sequence to string.
    /// </summary>
    [TestMethod]
    public void Decorate_ReferenceType_AppliesDecorators()
    {
        // Arrange
        string obj = "hello";
        Func<string, string> toUpper = s => s.ToUpper();
        Func<string, string> addExclamation = s => s + "!";
        // Act
        string result = new Decorator().Decorate(obj, toUpper, addExclamation);
        // Assert
        Assert.AreEqual("HELLO!", result);
    }

    /// <summary>
    /// Tests that Decorate handles null object with reference types when decorators allow it.
    /// Input: null string and decorator that handles null.
    /// Expected: decorator processes null input correctly.
    /// </summary>
    [TestMethod]
    public void Decorate_NullObjectReferenceType_AppliesDecorators()
    {
        // Arrange
        string? obj = null;
        Func<string?, string?> decorator = s => s == null ? "was null" : s.ToUpper();
        // Act
        string? result = new Decorator().Decorate(obj, decorator);
        // Assert
        Assert.AreEqual("was null", result);
    }

    /// <summary>
    /// Tests that Decorate allows a decorator to return null for reference types.
    /// Input: non-null object and decorator that returns null.
    /// Expected: null returned and subsequent decorators receive null.
    /// </summary>
    [TestMethod]
    public void Decorate_DecoratorReturnsNull_SubsequentDecoratorsReceiveNull()
    {
        // Arrange
        string obj = "test";
        Func<string?, string?> returnsNull = s => null;
        Func<string?, string?> checksNull = s => s == null ? "received null" : s;
        // Act
        string? result = new Decorator().Decorate(obj, returnsNull, checksNull);
        // Assert
        Assert.AreEqual("received null", result);
    }

    /// <summary>
    /// Tests that Decorate works with identity decorator that returns input unchanged.
    /// Input: object and identity decorator.
    /// Expected: original object returned unchanged.
    /// </summary>
    [TestMethod]
    public void Decorate_IdentityDecorator_ReturnsOriginalObject()
    {
        // Arrange
        int obj = 100;
        Func<int, int> identity = x => x;
        // Act
        int result = new Decorator().Decorate(obj, identity);
        // Assert
        Assert.AreEqual(100, result);
    }

    /// <summary>
    /// Tests that Decorate correctly chains multiple identity and transformation decorators.
    /// Input: mix of identity and transformation decorators.
    /// Expected: only transformations affect the result, identities pass through.
    /// </summary>
    [TestMethod]
    public void Decorate_MixedIdentityAndTransformations_AppliesCorrectly()
    {
        // Arrange
        int obj = 10;
        Func<int, int> identity = x => x;
        Func<int, int> addFive = x => x + 5;
        // Act
        int result = new Decorator().Decorate(obj, identity, addFive, identity, addFive, identity);
        // Assert
        Assert.AreEqual(20, result);
    }

    /// <summary>
    /// Tests that Decorate with no explicit decorators (params empty) returns original object.
    /// Input: object with no decorators passed.
    /// Expected: original object returned.
    /// </summary>
    [TestMethod]
    public void Decorate_NoDecoratorsProvided_ReturnsOriginalObject()
    {
        // Arrange
        int obj = 99;
        // Act
        int result = new Decorator().Decorate(obj);
        // Assert
        Assert.AreEqual(99, result);
    }

    /// <summary>
    /// Tests that Decorate handles extreme values correctly with value types.
    /// Input: int.MaxValue and decorator operations.
    /// Expected: decorators applied respecting overflow behavior.
    /// </summary>
    [TestMethod]
    public void Decorate_ExtremeValueTypes_AppliesDecorators()
    {
        // Arrange
        int obj = int.MaxValue;
        Func<int, int> identity = x => x;
        // Act
        int result = new Decorator().Decorate(obj, identity);
        // Assert
        Assert.AreEqual(int.MaxValue, result);
    }

    /// <summary>
    /// Tests that Decorate handles many decorators correctly.
    /// Input: large number of decorators.
    /// Expected: all decorators applied in sequence.
    /// </summary>
    [TestMethod]
    public void Decorate_ManyDecorators_AppliesAll()
    {
        // Arrange
        int obj = 0;
        Func<int, int> incrementor = x => x + 1;
        Func<int, int>[] decorators = new Func<int, int>[100];
        for (int i = 0; i < 100; i++)
        {
            decorators[i] = incrementor;
        }

        // Act
        int result = new Decorator().Decorate(obj, decorators);
        // Assert
        Assert.AreEqual(100, result);
    }

    /// <summary>
    /// Tests that Composite processes a single node with no children correctly.
    /// </summary>
    [TestMethod]
    public void Composite_SingleNodeWithNoChildren_CallsActionOnce()
    {
        // Arrange
        var root = new TreeNode("root");
        var visitedNodes = new List<string>();
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        var result = new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(1, visitedNodes.Count);
        Assert.AreEqual("root", visitedNodes[0]);
        Assert.AreSame(root, result);
    }

    /// <summary>
    /// Tests that Composite returns the original object after processing.
    /// </summary>
    [TestMethod]
    public void Composite_AnyValidInput_ReturnsOriginalObject()
    {
        // Arrange
        var root = new TreeNode("root");
        root.AddChild(new TreeNode("child1"));
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n =>
        {
        };
        // Act
        var result = new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreSame(root, result);
    }

    /// <summary>
    /// Tests that Composite traverses a flat tree (root with multiple direct children) in pre-order.
    /// </summary>
    [TestMethod]
    public void Composite_FlatTreeWithMultipleChildren_VisitsAllNodesInPreOrder()
    {
        // Arrange
        var root = new TreeNode("root");
        root.AddChild(new TreeNode("child1"));
        root.AddChild(new TreeNode("child2"));
        root.AddChild(new TreeNode("child3"));
        var visitedNodes = new List<string>();
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(4, visitedNodes.Count);
        Assert.AreEqual("root", visitedNodes[0]);
        Assert.AreEqual("child1", visitedNodes[1]);
        Assert.AreEqual("child2", visitedNodes[2]);
        Assert.AreEqual("child3", visitedNodes[3]);
    }

    /// <summary>
    /// Tests that Composite traverses a deep nested tree in pre-order depth-first manner.
    /// </summary>
    [TestMethod]
    public void Composite_DeepNestedTree_VisitsAllNodesInPreOrderDepthFirst()
    {
        // Arrange
        var root = new TreeNode("root");
        var child1 = new TreeNode("child1");
        var child2 = new TreeNode("child2");
        var grandchild1 = new TreeNode("grandchild1");
        var grandchild2 = new TreeNode("grandchild2");
        root.AddChild(child1);
        root.AddChild(child2);
        child1.AddChild(grandchild1);
        child1.AddChild(grandchild2);
        var visitedNodes = new List<string>();
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(5, visitedNodes.Count);
        Assert.AreEqual("root", visitedNodes[0]);
        Assert.AreEqual("child1", visitedNodes[1]);
        Assert.AreEqual("grandchild1", visitedNodes[2]);
        Assert.AreEqual("grandchild2", visitedNodes[3]);
        Assert.AreEqual("child2", visitedNodes[4]);
    }

    /// <summary>
    /// Tests that Composite handles a complex tree with multiple levels and branches correctly.
    /// </summary>
    [TestMethod]
    public void Composite_ComplexTreeStructure_VisitsAllNodesInCorrectOrder()
    {
        // Arrange
        var root = new TreeNode("A");
        var b = new TreeNode("B");
        var c = new TreeNode("C");
        var d = new TreeNode("D");
        var e = new TreeNode("E");
        var f = new TreeNode("F");
        var g = new TreeNode("G");
        root.AddChild(b);
        root.AddChild(c);
        b.AddChild(d);
        b.AddChild(e);
        c.AddChild(f);
        f.AddChild(g);
        var visitedNodes = new List<string>();
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(7, visitedNodes.Count);
        Assert.AreEqual("A", visitedNodes[0]);
        Assert.AreEqual("B", visitedNodes[1]);
        Assert.AreEqual("D", visitedNodes[2]);
        Assert.AreEqual("E", visitedNodes[3]);
        Assert.AreEqual("C", visitedNodes[4]);
        Assert.AreEqual("F", visitedNodes[5]);
        Assert.AreEqual("G", visitedNodes[6]);
    }

    /// <summary>
    /// Tests that Composite works correctly when the root object is null (for nullable reference types).
    /// </summary>
    [TestMethod]
    public void Composite_NullRootObject_CallsActionWithNull()
    {
        // Arrange
        TreeNode? root = null;
        var actionCalled = false;
        TreeNode? receivedNode = new TreeNode("dummy");
        Func<TreeNode?, IEnumerable<TreeNode?>> getChildren = n => Array.Empty<TreeNode?>();
        Action<TreeNode?> action = n =>
        {
            actionCalled = true;
            receivedNode = n;
        };
        // Act
        var result = new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.IsTrue(actionCalled);
        Assert.IsNull(receivedNode);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Composite allows the action to modify the nodes during traversal.
    /// </summary>
    [TestMethod]
    public void Composite_ActionModifiesNodes_NodesAreModified()
    {
        // Arrange
        var root = new TreeNode("root");
        var child1 = new TreeNode("child1");
        var child2 = new TreeNode("child2");
        root.AddChild(child1);
        root.AddChild(child2);
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => n.Name = n.Name.ToUpper();
        // Act
        new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual("ROOT", root.Name);
        Assert.AreEqual("CHILD1", child1.Name);
        Assert.AreEqual("CHILD2", child2.Name);
    }

    /// <summary>
    /// Tests that Composite handles empty children collections correctly.
    /// </summary>
    [TestMethod]
    public void Composite_GetChildrenReturnsEmptyCollection_ProcessesOnlyRoot()
    {
        // Arrange
        var root = new TreeNode("root");
        var visitCount = 0;
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => new List<TreeNode>();
        Action<TreeNode> action = n => visitCount++;
        // Act
        new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(1, visitCount);
    }

    /// <summary>
    /// Tests that Composite handles nodes with varying numbers of children correctly.
    /// </summary>
    [TestMethod]
    public void Composite_MixedTreeSomeNodesHaveChildrenSomeDont_VisitsAllNodes()
    {
        // Arrange
        var root = new TreeNode("root");
        var child1 = new TreeNode("child1");
        var child2 = new TreeNode("child2");
        var grandchild = new TreeNode("grandchild");
        root.AddChild(child1);
        root.AddChild(child2);
        child1.AddChild(grandchild);
        // child2 has no children
        var visitedNodes = new List<string>();
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(4, visitedNodes.Count);
        CollectionAssert.AreEqual(new[] { "root", "child1", "grandchild", "child2" }, visitedNodes);
    }

    /// <summary>
    /// Tests that Composite works with value types.
    /// </summary>
    [TestMethod]
    public void Composite_ValueTypeAsNode_WorksCorrectly()
    {
        // Arrange
        var root = 1;
        var visitedValues = new List<int>();
        Func<int, IEnumerable<int>> getChildren = n => n < 3 ? new[]
        {
            n + 1
        }

        : Array.Empty<int>();
        Action<int> action = n => visitedValues.Add(n);
        // Act
        var result = new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(1, result);
        Assert.AreEqual(3, visitedValues.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, visitedValues);
    }



    /// <summary>
    /// Tests that Mediate correctly invokes route function and returns the result with reference types.
    /// </summary>
    [TestMethod]
    public void Mediate_ValidInputsWithReferenceTypes_ReturnsExpectedResult()
    {
        // Arrange
        string obj = "Hello";
        TestMediator mediator = new TestMediator();
        int expectedResult = 5;
        Func<TestMediator, string, int> route = (m, o) => o.Length;
        // Act
        int result = new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that Mediate correctly invokes route function and returns the result with value types.
    /// </summary>
    [TestMethod]
    public void Mediate_ValidInputsWithValueTypes_ReturnsExpectedResult()
    {
        // Arrange
        int obj = 42;
        TestMediator mediator = new TestMediator();
        string expectedResult = "42";
        Func<TestMediator, int, string> route = (m, o) => o.ToString();
        // Act
        string result = new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that Mediate works correctly when obj parameter is null with nullable reference type.
    /// </summary>
    [TestMethod]
    public void Mediate_NullObjWithNullableType_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        TestMediator mediator = new TestMediator();
        Func<TestMediator, string?, bool> route = (m, o) => o == null;
        // Act
        bool result = new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Mediate works with extreme numeric values.
    /// </summary>
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(1)]
    [TestMethod]
    public void Mediate_NumericBoundaryValues_ReturnsExpectedResult(int value)
    {
        // Arrange
        TestMediator mediator = new TestMediator();
        Func<TestMediator, int, long> route = (m, o) => (long)o * 2;
        long expectedResult = (long)value * 2;
        // Act
        long result = new Mediator().Mediate(value, mediator, route);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that Mediate works with various string inputs including empty and whitespace.
    /// </summary>
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("test")]
    [DataRow("a very long string that contains many characters to test edge cases")]
    [TestMethod]
    public void Mediate_VariousStringInputs_ReturnsExpectedResult(string value)
    {
        // Arrange
        TestMediator mediator = new TestMediator();
        Func<TestMediator, string, int> route = (m, o) => o.Length;
        int expectedResult = value.Length;
        // Act
        int result = new Mediator().Mediate(value, mediator, route);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that Mediate works with complex custom types.
    /// </summary>
    [TestMethod]
    public void Mediate_ComplexCustomTypes_ReturnsExpectedResult()
    {
        // Arrange
        CustomRequest obj = new CustomRequest
        {
            Id = 123,
            Name = "Test"
        };
        CustomMediator mediator = new CustomMediator
        {
            ProcessingId = 999
        };
        Func<CustomMediator, CustomRequest, CustomResponse> route = (m, o) => new CustomResponse
        {
            RequestId = o.Id,
            ProcessedBy = m.ProcessingId
        };
        // Act
        CustomResponse result = new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(123, result.RequestId);
        Assert.AreEqual(999, result.ProcessedBy);
    }

    /// <summary>
    /// Tests that Mediate returns the exact result produced by route function.
    /// </summary>
    [TestMethod]
    public void Mediate_RouteReturnsSpecificObject_ReturnsSameObject()
    {
        // Arrange
        string obj = "input";
        TestMediator mediator = new TestMediator();
        CustomResponse expectedResponse = new CustomResponse
        {
            RequestId = 42,
            ProcessedBy = 1
        };
        Func<TestMediator, string, CustomResponse> route = (m, o) => expectedResponse;
        // Act
        CustomResponse result = new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.AreSame(expectedResponse, result);
    }

    /// <summary>
    /// Tests that Mediate works when mediator and obj are the same type.
    /// </summary>
    [TestMethod]
    public void Mediate_MediatorAndObjSameType_WorksCorrectly()
    {
        // Arrange
        int obj = 10;
        int mediator = 20;
        Func<int, int, int> route = (m, o) => m + o;
        // Act
        int result = new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(30, result);
    }

    /// <summary>
    /// Tests that Mediate works when all generic types are the same.
    /// </summary>
    [TestMethod]
    public void Mediate_AllGenericTypesSame_WorksCorrectly()
    {
        // Arrange
        string obj = "test";
        string mediator = "mediator";
        Func<string, string, string> route = (m, o) => m + o;
        // Act
        string result = new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual("mediatortest", result);
    }

    /// <summary>
    /// Test mediator type used for testing.
    /// </summary>
    private class TestMediator
    {
    }

    /// <summary>
    /// Custom mediator type for testing complex scenarios.
    /// </summary>
    private class CustomMediator
    {
        public int ProcessingId { get; set; }
    }

    /// <summary>
    /// Custom request type for testing complex scenarios.
    /// </summary>
    private class CustomRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Custom response type for testing complex scenarios.
    /// </summary>
    private class CustomResponse
    {
        public int RequestId { get; set; }
        public int ProcessedBy { get; set; }
    }

    /// <summary>
    /// Tests that Bridge correctly invokes the operation and returns the expected result.
    /// Input: Various combinations of value types, reference types, and result types
    /// Expected: Operation is invoked with correct parameters and result is returned
    /// </summary>
    [TestMethod]
    [DataRow("hello", 5, "hello5", DisplayName = "String + Int -> String")]
    [DataRow(10, 20, 30, DisplayName = "Int + Int -> Int")]
    [DataRow(3.14, 2.71, 5.85, DisplayName = "Double + Double -> Double")]
    public void Bridge_ValidOperation_ReturnsExpectedResult(object objValue, object implValue, object expected)
    {
        // Arrange & Act & Assert based on type
        if (objValue is string strObj && implValue is int intImpl && expected is string strExpected)
        {
            var result = new BridgePattern().Bridge(strObj, intImpl, (s, i) => s + i.ToString());
            Assert.AreEqual(strExpected, result);
        }
        else if (objValue is int intObj && implValue is int intImpl2 && expected is int intExpected)
        {
            var result = new BridgePattern().Bridge(intObj, intImpl2, (a, b) => a + b);
            Assert.AreEqual(intExpected, result);
        }
        else if (objValue is double dblObj && implValue is double dblImpl && expected is double dblExpected)
        {
            var result = new BridgePattern().Bridge(dblObj, dblImpl, (a, b) => a + b);
            Assert.AreEqual(dblExpected, result, 0.0001);
        }
    }

    /// <summary>
    /// Tests that Bridge passes the correct obj and implementation parameters to the operation.
    /// Input: obj = specific value, implementation = specific value, operation = capturing lambda
    /// Expected: Operation receives the exact obj and implementation values passed to Bridge
    /// </summary>
    [TestMethod]
    public void Bridge_ValidOperation_PassesCorrectParametersToOperation()
    {
        // Arrange
        var expectedObj = "testObject";
        var expectedImpl = 123;
        string? actualObj = null;
        int? actualImpl = null;
        Func<string, int, bool> operation = (obj, impl) =>
        {
            actualObj = obj;
            actualImpl = impl;
            return true;
        };
        // Act
        new BridgePattern().Bridge(expectedObj, expectedImpl, operation);
        // Assert
        Assert.AreEqual(expectedObj, actualObj);
        Assert.AreEqual(expectedImpl, actualImpl);
    }

    /// <summary>
    /// Tests that Bridge works with reference types including null values.
    /// Input: obj = null (nullable reference), implementation = null (nullable reference)
    /// Expected: Operation is invoked with null values and returns expected result
    /// </summary>
    [TestMethod]
    public void Bridge_NullableReferenceTypes_HandlesNullValues()
    {
        // Arrange
        string? nullObj = null;
        string? nullImpl = null;
        // Act
        var result = new BridgePattern().Bridge(nullObj, nullImpl, (obj, impl) => (obj == null ? "null" : obj) + "_" + (impl == null ? "null" : impl));
        // Assert
        Assert.AreEqual("null_null", result);
    }

    /// <summary>
    /// Tests that Bridge works with complex reference types and returns complex results.
    /// Input: obj = List of ints, implementation = HashSet of ints, operation = combine and count
    /// Expected: Operation combines collections and returns correct count
    /// </summary>
    [TestMethod]
    public void Bridge_ComplexTypes_ReturnsExpectedResult()
    {
        // Arrange
        var list = new List<int>
        {
            1,
            2,
            3
        };
        var set = new HashSet<int>
        {
            3,
            4,
            5
        };
        // Act
        var result = new BridgePattern().Bridge(list, set, (l, s) =>
        {
            var combined = new HashSet<int>(l);
            combined.UnionWith(s);
            return combined.Count;
        });
        // Assert
        Assert.AreEqual(5, result);
    }

    /// <summary>
    /// Tests that Bridge allows operation to return null when TResult is nullable.
    /// Input: obj = any value, implementation = any value, operation = returns null
    /// Expected: Null is returned from Bridge
    /// </summary>
    [TestMethod]
    public void Bridge_OperationReturnsNull_ReturnsNull()
    {
        // Arrange
        var obj = 42;
        var impl = "test";
        // Act
        var result = new BridgePattern().Bridge(obj, impl, (o, i) => (string?)null);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Bridge works with value types at boundary values.
    /// Input: int.MinValue, int.MaxValue, operation = add
    /// Expected: Overflow occurs as expected in unchecked context
    /// </summary>
    [TestMethod]
    public void Bridge_BoundaryValueTypes_HandlesEdgeCases()
    {
        // Arrange
        var minValue = int.MinValue;
        var maxValue = int.MaxValue;
        // Act
        var result = new BridgePattern().Bridge(minValue, maxValue, (a, b) => unchecked(a + b));
        // Assert
        Assert.AreEqual(-1, result);
    }

    /// <summary>
    /// Tests that Bridge works with floating-point special values.
    /// Input: double.NaN, double.PositiveInfinity, operation = combine
    /// Expected: NaN result due to NaN operand
    /// </summary>
    [TestMethod]
    public void Bridge_FloatingPointSpecialValues_HandlesNaN()
    {
        // Arrange
        var nan = double.NaN;
        var infinity = double.PositiveInfinity;
        // Act
        var result = new BridgePattern().Bridge(nan, infinity, (a, b) => a + b);
        // Assert
        Assert.IsTrue(double.IsNaN(result));
    }

    /// <summary>
    /// Tests that Bridge works with floating-point infinity values.
    /// Input: double.PositiveInfinity, double.NegativeInfinity, operation = add
    /// Expected: NaN result due to infinity - infinity
    /// </summary>
    [TestMethod]
    public void Bridge_FloatingPointInfinity_HandlesInfinityOperations()
    {
        // Arrange
        var positiveInfinity = double.PositiveInfinity;
        var negativeInfinity = double.NegativeInfinity;
        // Act
        var result = new BridgePattern().Bridge(positiveInfinity, negativeInfinity, (a, b) => a + b);
        // Assert
        Assert.IsTrue(double.IsNaN(result));
    }

    /// <summary>
    /// Tests that Bridge works correctly when obj and implementation are the same type.
    /// Input: obj = 5, implementation = 10, both int
    /// Expected: Operation receives both values correctly
    /// </summary>
    [TestMethod]
    public void Bridge_SameTypeForObjAndImpl_WorksCorrectly()
    {
        // Arrange
        var obj = 5;
        var impl = 10;
        // Act
        var result = new BridgePattern().Bridge(obj, impl, (a, b) => a * b);
        // Assert
        Assert.AreEqual(50, result);
    }

    /// <summary>
    /// Tests that Bridge works when all three type parameters are the same.
    /// Input: obj = "hello", implementation = "world", operation = concatenate
    /// Expected: Returns concatenated string
    /// </summary>
    [TestMethod]
    public void Bridge_AllSameType_WorksCorrectly()
    {
        // Arrange
        var obj = "hello";
        var impl = "world";
        // Act
        var result = new BridgePattern().Bridge(obj, impl, (a, b) => a + " " + b);
        // Assert
        Assert.AreEqual("hello world", result);
    }

    /// <summary>
    /// Tests that Bridge works with boolean operations.
    /// Input: obj = true, implementation = false, operation = logical AND
    /// Expected: Returns false
    /// </summary>
    [TestMethod]
    public void Bridge_BooleanOperations_ReturnsExpectedResult()
    {
        // Arrange
        var obj = true;
        var impl = false;
        // Act
        var result = new BridgePattern().Bridge(obj, impl, (a, b) => a && b);
        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that Bridge can be used to create tuples from disparate types.
    /// Input: obj = string, implementation = int
    /// Expected: Returns tuple containing both values
    /// </summary>
    [TestMethod]
    public void Bridge_CreateTuple_ReturnsExpectedTuple()
    {
        // Arrange
        var obj = "key";
        var impl = 42;
        // Act
        var result = new BridgePattern().Bridge(obj, impl, (a, b) => (a, b));
        // Assert
        Assert.AreEqual("key", result.Item1);
        Assert.AreEqual(42, result.Item2);
    }

    /// <summary>
    /// Tests that Adapt successfully converts a string to an integer using a valid adapter.
    /// Input: "42" with adapter that parses to int
    /// Expected: Returns 42
    /// </summary>
    [TestMethod]
    public void Adapt_ValidAdapterStringToInt_ReturnsConvertedValue()
    {
        // Arrange
        string obj = "42";
        Func<string, int> adapter = s => int.Parse(s);
        // Act
        int result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual(42, result);
    }

    /// <summary>
    /// Tests that Adapt successfully converts an integer to a string using a valid adapter.
    /// Input: 42 with adapter that converts to string
    /// Expected: Returns "42"
    /// </summary>
    [TestMethod]
    public void Adapt_ValidAdapterIntToString_ReturnsConvertedValue()
    {
        // Arrange
        int obj = 42;
        Func<int, string> adapter = i => i.ToString();
        // Act
        string result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual("42", result);
    }

    /// <summary>
    /// Tests that Adapt handles null source object by passing null to adapter.
    /// Input: null string with adapter that handles null
    /// Expected: Returns default value from adapter
    /// </summary>
    [TestMethod]
    public void Adapt_NullSourceObject_PassesNullToAdapter()
    {
        // Arrange
        string? obj = null;
        Func<string?, int> adapter = s => s == null ? -1 : s.Length;
        // Act
        int result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual(-1, result);
    }

    /// <summary>
    /// Tests that Adapt returns null when adapter returns null for nullable reference type.
    /// Input: "test" with adapter that returns null
    /// Expected: Returns null
    /// </summary>
    [TestMethod]
    public void Adapt_AdapterReturnsNull_ReturnsNull()
    {
        // Arrange
        string obj = "test";
        Func<string, string?> adapter = _ => null;
        // Act
        string? result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Adapt works with value type to value type conversion.
    /// Input: 42 with adapter that converts int to double
    /// Expected: Returns 42.0
    /// </summary>
    [TestMethod]
    public void Adapt_ValueTypeToValueType_ReturnsConvertedValue()
    {
        // Arrange
        int obj = 42;
        Func<int, double> adapter = i => i * 1.0;
        // Act
        double result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual(42.0, result);
    }

    /// <summary>
    /// Tests that Adapt works with reference type to reference type conversion.
    /// Input: "test" with adapter that converts to object
    /// Expected: Returns object containing "test"
    /// </summary>
    [TestMethod]
    public void Adapt_ReferenceTypeToReferenceType_ReturnsConvertedValue()
    {
        // Arrange
        string obj = "test";
        Func<string, object> adapter = s => s as object;
        // Act
        object result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual("test", result);
    }

    /// <summary>
    /// Tests that Adapt works with identity conversion (same type).
    /// Input: "test" with adapter that returns same value
    /// Expected: Returns "test"
    /// </summary>
    [TestMethod]
    public void Adapt_IdentityConversion_ReturnsSameValue()
    {
        // Arrange
        string obj = "test";
        Func<string, string> adapter = s => s;
        // Act
        string result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual("test", result);
    }

    /// <summary>
    /// Tests that Adapt works with complex object transformation.
    /// Input: DateTime with adapter that extracts year
    /// Expected: Returns the year value
    /// </summary>
    [TestMethod]
    public void Adapt_ComplexObjectTransformation_ReturnsTransformedValue()
    {
        // Arrange
        DateTime obj = new DateTime(2024, 1, 15);
        Func<DateTime, int> adapter = dt => dt.Year;
        // Act
        int result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual(2024, result);
    }

    /// <summary>
    /// Tests that Adapt works with adapter that captures external state.
    /// Input: 5 with adapter that adds captured value
    /// Expected: Returns 15 (5 + 10)
    /// </summary>
    [TestMethod]
    public void Adapt_AdapterWithClosure_ReturnsCorrectValue()
    {
        // Arrange
        int obj = 5;
        int addValue = 10;
        Func<int, int> adapter = i => i + addValue;
        // Act
        int result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual(15, result);
    }

    /// <summary>
    /// Tests that Adapt handles empty string input correctly.
    /// Input: empty string with adapter that returns length
    /// Expected: Returns 0
    /// </summary>
    [TestMethod]
    public void Adapt_EmptyString_ReturnsExpectedValue()
    {
        // Arrange
        string obj = string.Empty;
        Func<string, int> adapter = s => s.Length;
        // Act
        int result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    /// Tests that Adapt handles whitespace-only string input correctly.
    /// Input: whitespace string with adapter that trims and checks
    /// Expected: Returns true (trimmed is empty)
    /// </summary>
    [TestMethod]
    public void Adapt_WhitespaceString_ReturnsExpectedValue()
    {
        // Arrange
        string obj = "   ";
        Func<string, bool> adapter = s => string.IsNullOrWhiteSpace(s);
        // Act
        bool result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Adapt handles adapter converting to nullable value type.
    /// Input: "42" with adapter that parses to nullable int
    /// Expected: Returns 42
    /// </summary>
    [TestMethod]
    public void Adapt_ToNullableValueType_ReturnsExpectedValue()
    {
        // Arrange
        string obj = "42";
        Func<string, int?> adapter = s => int.TryParse(s, out int val) ? val : null;
        // Act
        int? result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual(42, result);
    }

    /// <summary>
    /// Tests that Adapt returns null for nullable value type when adapter returns null.
    /// Input: "invalid" with adapter that parses to nullable int
    /// Expected: Returns null
    /// </summary>
    [TestMethod]
    public void Adapt_ToNullableValueTypeReturnsNull_ReturnsNull()
    {
        // Arrange
        string obj = "invalid";
        Func<string, int?> adapter = s => int.TryParse(s, out int val) ? val : null;
        // Act
        int? result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Adapt handles very long string input correctly.
    /// Input: long string (10000 characters) with adapter that returns length
    /// Expected: Returns 10000
    /// </summary>
    [TestMethod]
    public void Adapt_VeryLongString_ReturnsExpectedValue()
    {
        // Arrange
        string obj = new string('a', 10000);
        Func<string, int> adapter = s => s.Length;
        // Act
        int result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual(10000, result);
    }

    /// <summary>
    /// Tests that Adapt handles extreme integer values correctly.
    /// Input: int.MaxValue with adapter that converts to long
    /// Expected: Returns int.MaxValue as long
    /// </summary>
    [TestMethod]
    public void Adapt_MaxIntValue_ReturnsExpectedValue()
    {
        // Arrange
        int obj = int.MaxValue;
        Func<int, long> adapter = i => i;
        // Act
        long result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual(int.MaxValue, result);
    }

    /// <summary>
    /// Tests that Adapt handles minimum integer values correctly.
    /// Input: int.MinValue with adapter that converts to long
    /// Expected: Returns int.MinValue as long
    /// </summary>
    [TestMethod]
    public void Adapt_MinIntValue_ReturnsExpectedValue()
    {
        // Arrange
        int obj = int.MinValue;
        Func<int, long> adapter = i => i;
        // Act
        long result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.AreEqual(int.MinValue, result);
    }

    /// <summary>
    /// Tests that Adapt handles zero value correctly.
    /// Input: 0 with adapter that checks for zero
    /// Expected: Returns true
    /// </summary>
    [TestMethod]
    public void Adapt_ZeroValue_ReturnsExpectedValue()
    {
        // Arrange
        int obj = 0;
        Func<int, bool> adapter = i => i == 0;
        // Act
        bool result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Adapt handles floating-point NaN correctly.
    /// Input: double.NaN with adapter that checks IsNaN
    /// Expected: Returns true
    /// </summary>
    [TestMethod]
    public void Adapt_DoubleNaN_ReturnsExpectedValue()
    {
        // Arrange
        double obj = double.NaN;
        Func<double, bool> adapter = d => double.IsNaN(d);
        // Act
        bool result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Adapt handles floating-point positive infinity correctly.
    /// Input: double.PositiveInfinity with adapter that checks IsPositiveInfinity
    /// Expected: Returns true
    /// </summary>
    [TestMethod]
    public void Adapt_DoublePositiveInfinity_ReturnsExpectedValue()
    {
        // Arrange
        double obj = double.PositiveInfinity;
        Func<double, bool> adapter = d => double.IsPositiveInfinity(d);
        // Act
        bool result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Adapt handles floating-point negative infinity correctly.
    /// Input: double.NegativeInfinity with adapter that checks IsNegativeInfinity
    /// Expected: Returns true
    /// </summary>
    [TestMethod]
    public void Adapt_DoubleNegativeInfinity_ReturnsExpectedValue()
    {
        // Arrange
        double obj = double.NegativeInfinity;
        Func<double, bool> adapter = d => double.IsNegativeInfinity(d);
        // Act
        bool result = new Adapter().Adapt(obj, adapter);
        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Accept invokes the visit action with correct parameters for a reference type object.
    /// </summary>
    [TestMethod]
    public void Accept_ValidReferenceTypeObject_InvokesVisitActionWithCorrectParameters()
    {
        // Arrange
        var obj = "test object";
        var visitor = "test visitor";
        var visitWasCalled = false;
        string? capturedVisitor = null;
        string? capturedObj = null;
        Action<string, string> visit = (v, o) =>
        {
            visitWasCalled = true;
            capturedVisitor = v;
            capturedObj = o;
        };
        // Act
        var result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreSame(visitor, capturedVisitor);
        Assert.AreSame(obj, capturedObj);
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Tests that Accept invokes the visit action with correct parameters for a value type object.
    /// </summary>
    [TestMethod]
    public void Accept_ValidValueTypeObject_InvokesVisitActionWithCorrectParameters()
    {
        // Arrange
        var obj = 42;
        var visitor = 100;
        var visitWasCalled = false;
        int capturedVisitor = 0;
        int capturedObj = 0;
        Action<int, int> visit = (v, o) =>
        {
            visitWasCalled = true;
            capturedVisitor = v;
            capturedObj = o;
        };
        // Act
        var result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreEqual(visitor, capturedVisitor);
        Assert.AreEqual(obj, capturedObj);
        Assert.AreEqual(obj, result);
    }

    /// <summary>
    /// Tests that Accept handles a null object (when T is nullable) and passes it to the visit action.
    /// </summary>
    [TestMethod]
    public void Accept_NullObjectWithNullableType_InvokesVisitActionWithNull()
    {
        // Arrange
        string? obj = null;
        var visitor = "visitor";
        var visitWasCalled = false;
        string? capturedObj = "not null";
        Action<string, string?> visit = (v, o) =>
        {
            visitWasCalled = true;
            capturedObj = o;
        };
        // Act
        var result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.IsNull(capturedObj);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Accept returns the same instance that was passed as the object parameter.
    /// </summary>
    [TestMethod]
    public void Accept_ValidInputs_ReturnsSameObjectInstance()
    {
        // Arrange
        var obj = new object();
        var visitor = new object();
        Action<object, object> visit = (v, o) =>
        {
        };
        // Act
        var result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Tests that Accept invokes the visit action exactly once.
    /// </summary>
    [TestMethod]
    public void Accept_ValidInputs_InvokesVisitActionExactlyOnce()
    {
        // Arrange
        var obj = "test";
        var visitor = "visitor";
        var callCount = 0;
        Action<string, string> visit = (v, o) => callCount++;
        // Act
        new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(1, callCount);
    }

    /// <summary>
    /// Tests that Accept works with different generic type combinations.
    /// </summary>
    [TestMethod]
    public void Accept_DifferentGenericTypes_WorksCorrectly()
    {
        // Arrange
        var obj = 123;
        var visitor = "string visitor";
        var visitWasCalled = false;
        Action<string, int> visit = (v, o) =>
        {
            visitWasCalled = true;
        };
        // Act
        var result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreEqual(obj, result);
    }

    /// <summary>
    /// Tests that Accept works with complex reference types.
    /// </summary>
    [TestMethod]
    public void Accept_ComplexReferenceType_WorksCorrectly()
    {
        // Arrange
        var obj = new System.Collections.Generic.List<int>
        {
            1,
            2,
            3
        };
        var visitor = new System.Collections.Generic.Dictionary<string, int>();
        var visitWasCalled = false;
        Action<System.Collections.Generic.Dictionary<string, int>, System.Collections.Generic.List<int>> visit = (v, o) =>
        {
            visitWasCalled = true;
        };
        // Act
        var result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Tests that Accept properly chains with other fluent operations by returning the original object.
    /// </summary>
    [TestMethod]
    public void Accept_ChainedOperations_AllowsFluentChaining()
    {
        // Arrange
        var obj = "test";
        var visitor1 = "visitor1";
        var visitor2 = "visitor2";
        var visit1Called = false;
        var visit2Called = false;
        Action<string, string> visit1 = (v, o) => visit1Called = true;
        Action<string, string> visit2 = (v, o) => visit2Called = true;
        // Act
        var v = new Visitor();
        var result = v.Accept(v.Accept(obj, visitor1, visit1), visitor2, visit2);
        // Assert
        Assert.IsTrue(visit1Called);
        Assert.IsTrue(visit2Called);
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Tests that Accept respects side effects performed by the visit action.
    /// </summary>
    [TestMethod]
    public void Accept_VisitActionWithSideEffects_SideEffectsAreExecuted()
    {
        // Arrange
        var obj = new System.Collections.Generic.List<int>();
        var visitor = 42;
        Action<int, System.Collections.Generic.List<int>> visit = (v, o) =>
        {
            o.Add(v);
        };
        // Act
        var result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(1, obj.Count);
        Assert.AreEqual(42, obj[0]);
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Tests that Accept works correctly with extreme integer values.
    /// </summary>
    [TestMethod]
    public void Accept_ExtremeBoundaryValues_WorksCorrectly()
    {
        // Arrange
        var obj = int.MaxValue;
        var visitor = int.MinValue;
        var visitWasCalled = false;
        int capturedObj = 0;
        int capturedVisitor = 0;
        Action<int, int> visit = (v, o) =>
        {
            visitWasCalled = true;
            capturedVisitor = v;
            capturedObj = o;
        };
        // Act
        var result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreEqual(int.MinValue, capturedVisitor);
        Assert.AreEqual(int.MaxValue, capturedObj);
        Assert.AreEqual(int.MaxValue, result);
    }

    /// <summary>
    /// Tests that Accept works with empty string as object.
    /// </summary>
    [TestMethod]
    public void Accept_EmptyStringObject_WorksCorrectly()
    {
        // Arrange
        var obj = string.Empty;
        var visitor = "visitor";
        var visitWasCalled = false;
        string? capturedObj = null;
        Action<string, string> visit = (v, o) =>
        {
            visitWasCalled = true;
            capturedObj = o;
        };
        // Act
        var result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreSame(string.Empty, capturedObj);
        Assert.AreSame(string.Empty, result);
    }

    /// <summary>
    /// Tests that Accept works with whitespace-only string as object.
    /// </summary>
    [TestMethod]
    public void Accept_WhitespaceStringObject_WorksCorrectly()
    {
        // Arrange
        var obj = "   ";
        var visitor = "visitor";
        var visitWasCalled = false;
        Action<string, string> visit = (v, o) => visitWasCalled = true;
        // Act
        var result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Tests that Singleton adds and returns the object on first access.
    /// </summary>
    [TestMethod]
    public void Singleton_FirstAccess_AddsAndReturnsObject()
    {
        // Arrange
        var obj = "test-value";
        var key = "key1";
        var cache = new ConcurrentDictionary<string, string>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
        Assert.AreEqual(obj, cache[key]);
    }

    /// <summary>
    /// Tests that Singleton returns cached instance on subsequent access with same key,
    /// ignoring the new object parameter.
    /// </summary>
    [TestMethod]
    public void Singleton_SubsequentAccessSameKey_ReturnsCachedInstance()
    {
        // Arrange
        var firstObj = "first-value";
        var secondObj = "second-value";
        var key = "key1";
        var cache = new ConcurrentDictionary<string, string>();
        // Act
        var firstResult = new SingletonPattern().Singleton(firstObj, key, cache);
        var secondResult = new SingletonPattern().Singleton(secondObj, key, cache);
        // Assert
        Assert.AreEqual(firstObj, firstResult);
        Assert.AreEqual(firstObj, secondResult);
        Assert.AreNotEqual(secondObj, secondResult);
        Assert.AreEqual(1, cache.Count);
    }

    /// <summary>
    /// Tests that Singleton maintains separate instances for different keys.
    /// </summary>
    [TestMethod]
    public void Singleton_DifferentKeys_MaintainsSeparateInstances()
    {
        // Arrange
        var obj1 = "value1";
        var obj2 = "value2";
        var key1 = "key1";
        var key2 = "key2";
        var cache = new ConcurrentDictionary<string, string>();
        // Act
        var result1 = new SingletonPattern().Singleton(obj1, key1, cache);
        var result2 = new SingletonPattern().Singleton(obj2, key2, cache);
        // Assert
        Assert.AreEqual(obj1, result1);
        Assert.AreEqual(obj2, result2);
        Assert.AreEqual(2, cache.Count);
        Assert.AreEqual(obj1, cache[key1]);
        Assert.AreEqual(obj2, cache[key2]);
    }

    /// <summary>
    /// Tests that Singleton works correctly with null object for reference types.
    /// </summary>
    [TestMethod]
    public void Singleton_NullObjectForReferenceType_CachesNull()
    {
        // Arrange
        string? obj = null;
        var key = "key1";
        var cache = new ConcurrentDictionary<string, string?>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.IsNull(result);
        Assert.IsTrue(cache.ContainsKey(key));
        Assert.IsNull(cache[key]);
    }

    /// <summary>
    /// Tests that Singleton works correctly with value types.
    /// </summary>
    [DataRow(0)]
    [DataRow(42)]
    [DataRow(-1)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [TestMethod]
    public void Singleton_ValueType_CachesAndReturnsValue(int value)
    {
        // Arrange
        var key = "key1";
        var cache = new ConcurrentDictionary<string, int>();
        // Act
        var firstResult = new SingletonPattern().Singleton(value, key, cache);
        var secondResult = new SingletonPattern().Singleton(value + 1, key, cache);
        // Assert
        Assert.AreEqual(value, firstResult);
        Assert.AreEqual(value, secondResult);
        Assert.AreEqual(1, cache.Count);
    }

    /// <summary>
    /// Tests that Singleton works with different key types.
    /// </summary>
    [TestMethod]
    public void Singleton_IntKey_WorksCorrectly()
    {
        // Arrange
        var obj = "test-value";
        var key = 123;
        var cache = new ConcurrentDictionary<int, string>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    /// <summary>
    /// Tests that Singleton works with Guid keys.
    /// </summary>
    [TestMethod]
    public void Singleton_GuidKey_WorksCorrectly()
    {
        // Arrange
        var obj = "test-value";
        var key = Guid.NewGuid();
        var cache = new ConcurrentDictionary<Guid, string>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    /// <summary>
    /// Tests that Singleton works with empty string key.
    /// </summary>
    [TestMethod]
    public void Singleton_EmptyStringKey_WorksCorrectly()
    {
        // Arrange
        var obj = "test-value";
        var key = string.Empty;
        var cache = new ConcurrentDictionary<string, string>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    /// <summary>
    /// Tests that Singleton works with very long string key.
    /// </summary>
    [TestMethod]
    public void Singleton_VeryLongStringKey_WorksCorrectly()
    {
        // Arrange
        var obj = "test-value";
        var key = new string('a', 10000);
        var cache = new ConcurrentDictionary<string, string>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    /// <summary>
    /// Tests that Singleton works with special characters in string key.
    /// </summary>
    [TestMethod]
    public void Singleton_SpecialCharactersInKey_WorksCorrectly()
    {
        // Arrange
        var obj = "test-value";
        var key = "key!@#$%^&*()_+-=[]{}|;':\",./<>?\t\n\r";
        var cache = new ConcurrentDictionary<string, string>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    /// <summary>
    /// Tests that Singleton correctly handles pre-populated cache.
    /// </summary>
    [TestMethod]
    public void Singleton_PrePopulatedCache_ReturnsExistingValue()
    {
        // Arrange
        var existingValue = "existing-value";
        var newValue = "new-value";
        var key = "key1";
        var cache = new ConcurrentDictionary<string, string>();
        cache.TryAdd(key, existingValue);
        // Act
        var result = new SingletonPattern().Singleton(newValue, key, cache);
        // Assert
        Assert.AreEqual(existingValue, result);
        Assert.AreNotEqual(newValue, result);
        Assert.AreEqual(1, cache.Count);
    }

    /// <summary>
    /// Tests that Prototype correctly invokes the clone function and returns the result for various string inputs.
    /// Input: Different string values including null, empty, and whitespace.
    /// Expected: Clone function result is returned.
    /// </summary>
    [TestMethod]
    [DataRow("original", "cloned", DisplayName = "Non-empty string")]
    [DataRow("", "result", DisplayName = "Empty string")]
    [DataRow("   ", "trimmed", DisplayName = "Whitespace string")]
    [DataRow(null, "from-null", DisplayName = "Null string")]
    public void Prototype_StringInputs_ReturnsCloneFunctionResult(string? original, string expected)
    {
        // Arrange
        Func<string?, string> clone = _ => expected;
        // Act
        var result = new PrototypePattern().Prototype(original, clone);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that Prototype works correctly with integer boundary values.
    /// Input: int.MinValue, int.MaxValue, 0, negative, and positive values.
    /// Expected: Clone function is invoked and returns the expected value.
    /// </summary>
    [TestMethod]
    [DataRow(int.MinValue, DisplayName = "int.MinValue")]
    [DataRow(int.MaxValue, DisplayName = "int.MaxValue")]
    [DataRow(0, DisplayName = "Zero")]
    [DataRow(-1, DisplayName = "Negative")]
    [DataRow(42, DisplayName = "Positive")]
    public void Prototype_IntegerBoundaryValues_ReturnsCloneFunctionResult(int value)
    {
        // Arrange
        var expected = value * 2;
        Func<int, int> clone = x => x * 2;
        // Act
        var result = new PrototypePattern().Prototype(value, clone);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that Prototype works correctly with double special values.
    /// Input: double.NaN, double.PositiveInfinity, double.NegativeInfinity, and normal values.
    /// Expected: Clone function is invoked and returns the expected value.
    /// </summary>
    [TestMethod]
    public void Prototype_DoubleNaN_ReturnsCloneFunctionResult()
    {
        // Arrange
        var value = double.NaN;
        Func<double, double> clone = x => double.IsNaN(x) ? 0.0 : x;
        // Act
        var result = new PrototypePattern().Prototype(value, clone);
        // Assert
        Assert.AreEqual(0.0, result);
    }

    /// <summary>
    /// Tests that Prototype works correctly with double.PositiveInfinity.
    /// Input: double.PositiveInfinity.
    /// Expected: Clone function is invoked and returns the expected value.
    /// </summary>
    [TestMethod]
    public void Prototype_DoublePositiveInfinity_ReturnsCloneFunctionResult()
    {
        // Arrange
        var value = double.PositiveInfinity;
        Func<double, double> clone = x => x;
        // Act
        var result = new PrototypePattern().Prototype(value, clone);
        // Assert
        Assert.AreEqual(double.PositiveInfinity, result);
    }

    /// <summary>
    /// Tests that Prototype works correctly with double.NegativeInfinity.
    /// Input: double.NegativeInfinity.
    /// Expected: Clone function is invoked and returns the expected value.
    /// </summary>
    [TestMethod]
    public void Prototype_DoubleNegativeInfinity_ReturnsCloneFunctionResult()
    {
        // Arrange
        var value = double.NegativeInfinity;
        Func<double, double> clone = x => x;
        // Act
        var result = new PrototypePattern().Prototype(value, clone);
        // Assert
        Assert.AreEqual(double.NegativeInfinity, result);
    }

    /// <summary>
    /// Tests that Prototype invokes the clone delegate with the original object.
    /// Input: An integer value and a clone function that captures the input.
    /// Expected: Clone function is called with the original object.
    /// </summary>
    [TestMethod]
    public void Prototype_ValidCloneFunction_InvokesCloneDelegateWithOriginalObject()
    {
        // Arrange
        var obj = 42;
        var wasCalled = false;
        int passedValue = 0;
        Func<int, int> clone = x =>
        {
            wasCalled = true;
            passedValue = x;
            return x * 2;
        };
        // Act
        var result = new PrototypePattern().Prototype(obj, clone);
        // Assert
        Assert.IsTrue(wasCalled);
        Assert.AreEqual(obj, passedValue);
        Assert.AreEqual(84, result);
    }

    /// <summary>
    /// Tests that Prototype works correctly with reference types.
    /// Input: A reference object and a clone function that returns a different instance.
    /// Expected: Returns the cloned object from the clone function.
    /// </summary>
    [TestMethod]
    public void Prototype_ReferenceType_ReturnsClonedObject()
    {
        // Arrange
        var original = new object();
        var clonedObject = new object();
        Func<object, object> clone = _ => clonedObject;
        // Act
        var result = new PrototypePattern().Prototype(original, clone);
        // Assert
        Assert.AreSame(clonedObject, result);
        Assert.AreNotSame(original, result);
    }

    /// <summary>
    /// Tests that Prototype returns null when the clone function returns null for reference types.
    /// Input: A clone function that returns null.
    /// Expected: Returns null.
    /// </summary>
    [TestMethod]
    public void Prototype_CloneFunctionReturnsNull_ReturnsNull()
    {
        // Arrange
        var obj = "test";
        Func<string, string?> clone = _ => null;
        // Act
        var result = new PrototypePattern().Prototype(obj, clone);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Prototype works with very long strings.
    /// Input: A very long string.
    /// Expected: Clone function is invoked and returns the expected result.
    /// </summary>
    [TestMethod]
    public void Prototype_VeryLongString_ReturnsCloneFunctionResult()
    {
        // Arrange
        var longString = new string('a', 10000);
        var expectedClone = new string('b', 10000);
        Func<string, string> clone = _ => expectedClone;
        // Act
        var result = new PrototypePattern().Prototype(longString, clone);
        // Assert
        Assert.AreEqual(expectedClone, result);
    }

    /// <summary>
    /// Tests that Prototype works with strings containing special characters.
    /// Input: Strings with special and control characters.
    /// Expected: Clone function is invoked and returns the expected result.
    /// </summary>
    [TestMethod]
    [DataRow("\n\r\t", DisplayName = "Control characters")]
    [DataRow("Hello\0World", DisplayName = "Null character")]
    [DataRow("Unicode: \u00A9 \u2764", DisplayName = "Unicode characters")]
    [DataRow("Special: !@#$%^&*()", DisplayName = "Special characters")]
    public void Prototype_StringsWithSpecialCharacters_ReturnsCloneFunctionResult(string input)
    {
        // Arrange
        var expected = "processed";
        Func<string, string> clone = _ => expected;
        // Act
        var result = new PrototypePattern().Prototype(input, clone);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Verifies that Notify with an empty observers array does not invoke any observers
    /// and returns the original object.
    /// </summary>
    [TestMethod]
    public void Notify_EmptyObservers_ReturnsOriginalObjectWithoutInvokingObservers()
    {
        // Arrange
        var obj = "test";
        // Act
        var result = new Observer().Notify(obj);
        // Assert
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Verifies that Notify with a single observer invokes that observer exactly once
    /// with the correct object and returns the original object.
    /// </summary>
    [TestMethod]
    public void Notify_SingleObserver_InvokesObserverOnceAndReturnsOriginalObject()
    {
        // Arrange
        var obj = 42;
        var invocationCount = 0;
        var receivedValue = 0;
        Action<int> observer = x =>
        {
            invocationCount++;
            receivedValue = x;
        };
        // Act
        var result = new Observer().Notify(obj, observer);
        // Assert
        Assert.AreEqual(1, invocationCount);
        Assert.AreEqual(obj, receivedValue);
        Assert.AreEqual(obj, result);
    }

    /// <summary>
    /// Verifies that Notify with multiple observers invokes all observers in order
    /// with the correct object and returns the original object.
    /// </summary>
    [TestMethod]
    public void Notify_MultipleObservers_InvokesAllObserversInOrderAndReturnsOriginalObject()
    {
        // Arrange
        var obj = "notify";
        var invocationOrder = new List<int>();
        Action<string> observer1 = x => invocationOrder.Add(1);
        Action<string> observer2 = x => invocationOrder.Add(2);
        Action<string> observer3 = x => invocationOrder.Add(3);
        // Act
        var result = new Observer().Notify(obj, observer1, observer2, observer3);
        // Assert
        Assert.AreEqual(3, invocationOrder.Count);
        Assert.AreEqual(1, invocationOrder[0]);
        Assert.AreEqual(2, invocationOrder[1]);
        Assert.AreEqual(3, invocationOrder[2]);
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Verifies that Notify works correctly with reference types and all observers
    /// receive the same object reference.
    /// </summary>
    [TestMethod]
    public void Notify_WithReferenceType_AllObserversReceiveSameReference()
    {
        // Arrange
        var obj = new List<int>
        {
            1,
            2,
            3
        };
        List<int>? receivedByObserver1 = null;
        List<int>? receivedByObserver2 = null;
        Action<List<int>> observer1 = x => receivedByObserver1 = x;
        Action<List<int>> observer2 = x => receivedByObserver2 = x;
        // Act
        var result = new Observer().Notify(obj, observer1, observer2);
        // Assert
        Assert.AreSame(obj, receivedByObserver1);
        Assert.AreSame(obj, receivedByObserver2);
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Verifies that Notify works correctly with value types.
    /// </summary>
    [TestMethod]
    public void Notify_WithValueType_AllObserversReceiveCorrectValue()
    {
        // Arrange
        var obj = 99;
        var receivedByObserver1 = 0;
        var receivedByObserver2 = 0;
        Action<int> observer1 = x => receivedByObserver1 = x;
        Action<int> observer2 = x => receivedByObserver2 = x;
        // Act
        var result = new Observer().Notify(obj, observer1, observer2);
        // Assert
        Assert.AreEqual(obj, receivedByObserver1);
        Assert.AreEqual(obj, receivedByObserver2);
        Assert.AreEqual(obj, result);
    }

    /// <summary>
    /// Verifies that Notify works correctly with nullable reference types.
    /// </summary>
    [TestMethod]
    public void Notify_WithNullableReferenceType_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        string? receivedValue = "not null";
        Action<string?> observer = x => receivedValue = x;
        // Act
        var result = new Observer().Notify(obj, observer);
        // Assert
        Assert.IsNull(receivedValue);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that Notify can be chained fluently by using the returned object.
    /// </summary>
    [TestMethod]
    public void Notify_FluentChaining_WorksCorrectly()
    {
        // Arrange
        var obj = 100;
        var invocationCount = 0;
        Action<int> observer = x => invocationCount++;
        // Act
        var n = new Observer();
        var result = n.Notify(n.Notify(n.Notify(obj, observer), observer), observer);
        // Assert
        Assert.AreEqual(3, invocationCount);
        Assert.AreEqual(obj, result);
    }

    /// <summary>
    /// Verifies that Notify with a large number of observers invokes all of them.
    /// </summary>
    [TestMethod]
    public void Notify_WithManyObservers_InvokesAllObservers()
    {
        // Arrange
        var obj = "many";
        var invocationCount = 0;
        var observers = new Action<string>[100];
        for (int i = 0; i < observers.Length; i++)
        {
            observers[i] = x => invocationCount++;
        }

        // Act
        var result = new Observer().Notify(obj, observers);
        // Assert
        Assert.AreEqual(100, invocationCount);
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Verifies that Notify works correctly when observers modify shared state.
    /// </summary>
    [TestMethod]
    public void Notify_ObserversModifySharedState_AllModificationsApplied()
    {
        // Arrange
        var counter = 0;
        var obj = "state";
        Action<string> observer1 = x => counter += 10;
        Action<string> observer2 = x => counter += 20;
        Action<string> observer3 = x => counter += 30;
        // Act
        var result = new Observer().Notify(obj, observer1, observer2, observer3);
        // Assert
        Assert.AreEqual(60, counter);
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Verifies that Build applies no steps when an empty steps array is provided.
    /// The finalizer should be called on the unmodified object.
    /// </summary>
    [TestMethod]
    public void Build_EmptyStepsArray_CallsFinalizerOnUnmodifiedObject()
    {
        // Arrange
        var obj = new StringBuilder("initial");
        Func<StringBuilder, string> finalizer = sb => sb.ToString();
        var steps = Array.Empty<Action<StringBuilder>>();
        // Act
        var result = new Builder().Build(obj, finalizer, steps);
        // Assert
        Assert.AreEqual("initial", result);
    }

    /// <summary>
    /// Verifies that Build applies no steps when no steps parameters are provided.
    /// The finalizer should be called on the unmodified object.
    /// </summary>
    [TestMethod]
    public void Build_NoStepsProvided_CallsFinalizerOnUnmodifiedObject()
    {
        // Arrange
        var obj = new StringBuilder("initial");
        Func<StringBuilder, string> finalizer = sb => sb.ToString();
        // Act
        var result = new Builder().Build(obj, finalizer);
        // Assert
        Assert.AreEqual("initial", result);
    }

    /// <summary>
    /// Verifies that Build applies a single step before calling the finalizer.
    /// </summary>
    [TestMethod]
    public void Build_SingleStep_AppliesStepThenCallsFinalizer()
    {
        // Arrange
        var obj = new StringBuilder("initial");
        Func<StringBuilder, string> finalizer = sb => sb.ToString();
        Action<StringBuilder> step = sb => sb.Append(" modified");
        // Act
        var result = new Builder().Build(obj, finalizer, step);
        // Assert
        Assert.AreEqual("initial modified", result);
    }

    /// <summary>
    /// Verifies that Build applies multiple steps in the correct order before calling the finalizer.
    /// </summary>
    [TestMethod]
    public void Build_MultipleSteps_AppliesStepsInOrderThenCallsFinalizer()
    {
        // Arrange
        var obj = new StringBuilder();
        Func<StringBuilder, string> finalizer = sb => sb.ToString();
        Action<StringBuilder> step1 = sb => sb.Append("first");
        Action<StringBuilder> step2 = sb => sb.Append(" second");
        Action<StringBuilder> step3 = sb => sb.Append(" third");
        // Act
        var result = new Builder().Build(obj, finalizer, step1, step2, step3);
        // Assert
        Assert.AreEqual("first second third", result);
    }

    /// <summary>
    /// Verifies that Build applies steps in exact order, where later steps can modify
    /// the changes made by earlier steps.
    /// </summary>
    [TestMethod]
    public void Build_StepsAppliedInOrder_LaterStepsCanOverrideEarlierSteps()
    {
        // Arrange
        var obj = new List<int>();
        Func<List<int>, int> finalizer = list => list.Count;
        Action<List<int>> step1 = list => list.Add(1);
        Action<List<int>> step2 = list => list.Add(2);
        Action<List<int>> step3 = list => list.Clear();
        Action<List<int>> step4 = list => list.Add(100);
        // Act
        var result = new Builder().Build(obj, finalizer, step1, step2, step3, step4);
        // Assert
        Assert.AreEqual(1, result);
        Assert.AreEqual(100, obj[0]);
    }

    /// <summary>
    /// Verifies that Build works correctly with value types as the object being built.
    /// </summary>
    [TestMethod]
    public void Build_ValueTypeObject_AppliesStepsAndReturnsResult()
    {
        // Arrange
        int obj = 10;
        Func<int, string> finalizer = i => i.ToString();
        // Note: Value type cannot be modified by steps, but steps can still be called
        // Act
        var result = new Builder().Build(obj, finalizer);
        // Assert
        Assert.AreEqual("10", result);
    }

    /// <summary>
    /// Verifies that Build works correctly when TResult is the same type as T.
    /// </summary>
    [TestMethod]
    public void Build_ResultTypeSameAsObjectType_AppliesStepsAndReturnsResult()
    {
        // Arrange
        var obj = new StringBuilder("start");
        Func<StringBuilder, StringBuilder> finalizer = sb => sb;
        Action<StringBuilder> step = sb => sb.Append(" end");
        // Act
        var result = new Builder().Build(obj, finalizer, step);
        // Assert
        Assert.AreEqual("start end", result.ToString());
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Verifies that Build works correctly when the object is null (for reference types).
    /// Steps and finalizer receive null as the parameter.
    /// </summary>
    [TestMethod]
    public void Build_NullObject_PassesNullToStepsAndFinalizer()
    {
        // Arrange
        StringBuilder? obj = null;
        bool stepCalled = false;
        bool finalizerCalled = false;
        Func<StringBuilder?, string> finalizer = sb =>
        {
            finalizerCalled = true;
            return sb == null ? "null" : "not null";
        };
        Action<StringBuilder?> step = sb =>
        {
            stepCalled = true;
            Assert.IsNull(sb);
        };
        // Act
        var result = new Builder().Build(obj, finalizer, step);
        // Assert
        Assert.IsTrue(stepCalled);
        Assert.IsTrue(finalizerCalled);
        Assert.AreEqual("null", result);
    }

    /// <summary>
    /// Verifies that Build correctly transforms an object to a different result type.
    /// </summary>
    [TestMethod]
    public void Build_DifferentResultType_TransformsObjectCorrectly()
    {
        // Arrange
        var obj = new List<int>();
        Func<List<int>, int> finalizer = list => list.Sum();
        Action<List<int>> step1 = list => list.Add(10);
        Action<List<int>> step2 = list => list.Add(20);
        Action<List<int>> step3 = list => list.Add(30);
        // Act
        var result = new Builder().Build(obj, finalizer, step1, step2, step3);
        // Assert
        Assert.AreEqual(60, result);
    }

    /// <summary>
    /// Verifies that Build passes the same object instance to all steps and the finalizer.
    /// </summary>
    [TestMethod]
    public void Build_SameObjectPassedToAllSteps_VerifiesObjectIdentity()
    {
        // Arrange
        var obj = new StringBuilder();
        StringBuilder? capturedInStep1 = null;
        StringBuilder? capturedInStep2 = null;
        StringBuilder? capturedInFinalizer = null;
        Func<StringBuilder, string> finalizer = sb =>
        {
            capturedInFinalizer = sb;
            return sb.ToString();
        };
        Action<StringBuilder> step1 = sb => capturedInStep1 = sb;
        Action<StringBuilder> step2 = sb => capturedInStep2 = sb;
        // Act
        new Builder().Build(obj, finalizer, step1, step2);
        // Assert
        Assert.AreSame(obj, capturedInStep1);
        Assert.AreSame(obj, capturedInStep2);
        Assert.AreSame(obj, capturedInFinalizer);
    }

    /// <summary>
    /// Verifies that Build works with complex object graphs and nested transformations.
    /// </summary>
    [TestMethod]
    public void Build_ComplexObjectGraph_AppliesStepsCorrectly()
    {
        // Arrange
        var obj = new Dictionary<string, int>();
        Func<Dictionary<string, int>, int> finalizer = dict => dict.Values.Sum();
        Action<Dictionary<string, int>> step1 = dict => dict["a"] = 10;
        Action<Dictionary<string, int>> step2 = dict => dict["b"] = 20;
        Action<Dictionary<string, int>> step3 = dict => dict["a"] = dict["a"] * 2;
        // Act
        var result = new Builder().Build(obj, finalizer, step1, step2, step3);
        // Assert
        Assert.AreEqual(40, result); // a=20, b=20
    }

    /// <summary>
    /// Tests that Template executes setup, operation, and teardown in correct order
    /// and returns the result from operation.
    /// </summary>
    [TestMethod]
    public void Template_ValidDelegates_ExecutesInCorrectOrderAndReturnsResult()
    {
        // Arrange
        var obj = "test";
        var executionOrder = new List<string>();
        Action<string> setup = s => executionOrder.Add("setup");
        Func<string, int> operation = s =>
        {
            executionOrder.Add("operation");
            return s.Length;
        };
        Action<string> teardown = s => executionOrder.Add("teardown");
        // Act
        var result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual(4, result);
        CollectionAssert.AreEqual(new[] { "setup", "operation", "teardown" }, executionOrder);
    }

    /// <summary>
    /// Tests that Template passes the context object to all delegates.
    /// </summary>
    [TestMethod]
    public void Template_ValidDelegates_PassesContextToAllDelegates()
    {
        // Arrange
        var obj = 42;
        var setupReceived = 0;
        var operationReceived = 0;
        var teardownReceived = 0;
        Action<int> setup = n => setupReceived = n;
        Func<int, string> operation = n =>
        {
            operationReceived = n;
            return n.ToString();
        };
        Action<int> teardown = n => teardownReceived = n;
        // Act
        var result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual("42", result);
        Assert.AreEqual(42, setupReceived);
        Assert.AreEqual(42, operationReceived);
        Assert.AreEqual(42, teardownReceived);
    }

    /// <summary>
    /// Tests that Template works with value type context and returns correct result.
    /// </summary>
    [TestMethod]
    public void Template_ValueTypeContext_WorksCorrectly()
    {
        // Arrange
        var obj = 10;
        var setupCalled = false;
        var teardownCalled = false;
        Action<int> setup = n => setupCalled = true;
        Func<int, int> operation = n => n * 2;
        Action<int> teardown = n => teardownCalled = true;
        // Act
        var result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual(20, result);
        Assert.IsTrue(setupCalled);
        Assert.IsTrue(teardownCalled);
    }

    /// <summary>
    /// Tests that Template works with reference type context and returns correct result.
    /// </summary>
    [TestMethod]
    public void Template_ReferenceTypeContext_WorksCorrectly()
    {
        // Arrange
        var obj = new List<int>
        {
            1,
            2,
            3
        };
        var setupCalled = false;
        var teardownCalled = false;
        Action<List<int>> setup = list => setupCalled = true;
        Func<List<int>, int> operation = list => list.Count;
        Action<List<int>> teardown = list => teardownCalled = true;
        // Act
        var result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual(3, result);
        Assert.IsTrue(setupCalled);
        Assert.IsTrue(teardownCalled);
    }

    /// <summary>
    /// Tests that Template works with null context object when T is nullable reference type.
    /// </summary>
    [TestMethod]
    public void Template_NullContext_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        var setupReceived = false;
        var operationReceived = false;
        var teardownReceived = false;
        Action<string?> setup = s => setupReceived = s == null;
        Func<string?, bool> operation = s =>
        {
            operationReceived = s == null;
            return s == null;
        };
        Action<string?> teardown = s => teardownReceived = s == null;
        // Act
        var result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(setupReceived);
        Assert.IsTrue(operationReceived);
        Assert.IsTrue(teardownReceived);
    }

    /// <summary>
    /// Tests that Template invokes each delegate exactly once.
    /// </summary>
    [TestMethod]
    public void Template_ValidDelegates_InvokesEachDelegateOnce()
    {
        // Arrange
        var obj = "test";
        var setupCount = 0;
        var operationCount = 0;
        var teardownCount = 0;
        Action<string> setup = s => setupCount++;
        Func<string, int> operation = s =>
        {
            operationCount++;
            return s.Length;
        };
        Action<string> teardown = s => teardownCount++;
        // Act
        var result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual(4, result);
        Assert.AreEqual(1, setupCount);
        Assert.AreEqual(1, operationCount);
        Assert.AreEqual(1, teardownCount);
    }

    /// <summary>
    /// Tests that Template correctly returns different result types.
    /// </summary>
    [TestMethod]
    [DataRow("hello", 5, DisplayName = "String to int")]
    [DataRow("world!", 6, DisplayName = "String to int with punctuation")]
    [DataRow("", 0, DisplayName = "Empty string to zero")]
    public void Template_DifferentInputs_ReturnsCorrectResult(string input, int expected)
    {
        // Arrange
        Action<string> setup = s =>
        {
        };
        Func<string, int> operation = s => s.Length;
        Action<string> teardown = s =>
        {
        };
        // Act
        var result = new TemplateMethod().Template(input, setup, operation, teardown);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that Template returns complex result types correctly.
    /// </summary>
    [TestMethod]
    public void Template_ComplexResultType_ReturnsCorrectly()
    {
        // Arrange
        var obj = 5;
        Action<int> setup = n =>
        {
        };
        Func<int, (int Value, string Text)> operation = n => (n, n.ToString());
        Action<int> teardown = n =>
        {
        };
        // Act
        var result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual(5, result.Value);
        Assert.AreEqual("5", result.Text);
    }

    /// <summary>
    /// Tests that Template allows setup to modify state before operation.
    /// </summary>
    [TestMethod]
    public void Template_SetupModifiesState_OperationSeesModification()
    {
        // Arrange
        var obj = new List<int>();
        Action<List<int>> setup = list => list.Add(1);
        Func<List<int>, int> operation = list => list.Count;
        Action<List<int>> teardown = list => list.Clear();
        // Act
        var result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual(1, result);
        Assert.AreEqual(0, obj.Count); // Teardown was called and cleared the list
    }

    /// <summary>
    /// Tests that Build works correctly with value types.
    /// </summary>
    [TestMethod]
    public void Build_ValueType_ReturnsModifiedValue()
    {
        // Arrange
        int value = 10;
        // Act
        // Note: For value types, the modification won't persist on the original
        // but Build should still execute and return the value
        var result = new Builder().Build(value, v =>
        { /* no-op on value type */
        });
        // Assert
        Assert.AreEqual(10, result);
    }

    /// <summary>
    /// Tests that Build works with reference types that are initially null.
    /// </summary>
    [TestMethod]
    public void Build_NullReferenceTypeObject_ExecutesStepsWithNull()
    {
        // Arrange
        TestObject? obj = null;
        bool stepExecuted = false;
        // Act
        var result = new Builder().Build(obj, o =>
        {
            stepExecuted = true;
            Assert.IsNull(o);
        });
        // Assert
        Assert.IsNull(result);
        Assert.IsTrue(stepExecuted);
    }

    /// <summary>
    /// Tests that Build with string type works correctly.
    /// </summary>
    [TestMethod]
    public void Build_StringType_ExecutesStepsCorrectly()
    {
        // Arrange
        string obj = "test";
        bool stepExecuted = false;
        // Act
        var result = new Builder().Build(obj, s =>
        {
            stepExecuted = true;
            Assert.AreEqual("test", s);
        });
        // Assert
        Assert.AreEqual("test", result);
        Assert.IsTrue(stepExecuted);
    }

    /// <summary>
    /// Tests that Accept returns the expected result when all parameters are valid.
    /// Input: A valid object, visitor, and visit function.
    /// Expected: The visit function is invoked and its result is returned.
    /// </summary>
    [TestMethod]
    public void Accept_ValidParameters_ReturnsExpectedResult()
    {
        // Arrange
        const string obj = "element";
        const string visitor = "visitor";
        const string expectedResult = "visitor processed element";
        Func<string, string, string> visit = (v, o) => $"{v} processed {o}";
        // Act
        string result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that Accept works correctly with value types.
    /// Input: Integer object, visitor, and visit function.
    /// Expected: The visit function is invoked and returns the calculated result.
    /// </summary>
    [TestMethod]
    public void Accept_ValueTypes_ReturnsExpectedResult()
    {
        // Arrange
        const int obj = 42;
        const int visitor = 10;
        const int expectedResult = 52;
        Func<int, int, int> visit = (v, o) => v + o;
        // Act
        int result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that Accept works correctly when the object being visited is null.
    /// Input: Null object, valid visitor and visit function.
    /// Expected: The visit function is invoked with null object and returns expected result.
    /// </summary>
    [TestMethod]
    public void Accept_NullObject_InvokesVisitFunctionWithNull()
    {
        // Arrange
        string? obj = null;
        const string visitor = "visitor";
        const string expectedResult = "visitor processed null";
        Func<string, string?, string> visit = (v, o) => $"{v} processed {(o == null ? "null" : o)}";
        // Act
        string result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that Accept invokes the visit function with the correct parameters.
    /// Input: Valid object, visitor, and visit function.
    /// Expected: The visit function receives the correct visitor and object parameters.
    /// </summary>
    [TestMethod]
    public void Accept_ValidParameters_InvokesVisitFunctionWithCorrectArguments()
    {
        // Arrange
        const string obj = "testElement";
        const string visitor = "testVisitor";
        string? capturedVisitor = null;
        string? capturedObj = null;
        Func<string, string, string> visit = (v, o) =>
        {
            capturedVisitor = v;
            capturedObj = o;
            return "result";
        };
        // Act
        new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(visitor, capturedVisitor);
        Assert.AreEqual(obj, capturedObj);
    }

    /// <summary>
    /// Tests that Accept works with complex reference types.
    /// Input: Complex object, visitor, and visit function.
    /// Expected: The visit function processes the complex types correctly.
    /// </summary>
    [TestMethod]
    public void Accept_ComplexReferenceTypes_ReturnsExpectedResult()
    {
        // Arrange
        var obj = new
        {
            Id = 1,
            Name = "Test"
        };
        var visitor = new
        {
            ProcessorId = 42
        };
        const int expectedResult = 43;
        Func<object, object, int> visit = (v, o) =>
        {
            dynamic dv = v;
            dynamic dobj = o;
            return dv.ProcessorId + dobj.Id;
        };
        // Act
        int result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that Accept correctly handles different return types.
    /// Input: String object, string visitor, visit function returning boolean.
    /// Expected: The visit function returns the expected boolean result.
    /// </summary>
    [TestMethod]
    public void Accept_DifferentReturnType_ReturnsExpectedResult()
    {
        // Arrange
        const string obj = "test";
        const string visitor = "test";
        Func<string, string, bool> visit = (v, o) => v == o;
        // Act
        bool result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Accept works with extreme integer values.
    /// Input: int.MaxValue as object, int.MinValue as visitor.
    /// Expected: The visit function processes extreme values correctly.
    /// </summary>
    [TestMethod]
    public void Accept_ExtremeIntegerValues_ReturnsExpectedResult()
    {
        // Arrange
        const int obj = int.MaxValue;
        const int visitor = int.MinValue;
        const long expectedResult = (long)int.MaxValue + int.MinValue;
        Func<int, int, long> visit = (v, o) => (long)v + o;
        // Act
        long result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that Accept works with empty strings.
    /// Input: Empty string as object and visitor.
    /// Expected: The visit function processes empty strings correctly.
    /// </summary>
    [TestMethod]
    public void Accept_EmptyStrings_ReturnsExpectedResult()
    {
        // Arrange
        string obj = string.Empty;
        string visitor = string.Empty;
        const string expectedResult = "0";
        Func<string, string, string> visit = (v, o) => (v.Length + o.Length).ToString();
        // Act
        string result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that Accept works with whitespace-only strings.
    /// Input: Whitespace strings as object and visitor.
    /// Expected: The visit function processes whitespace strings correctly.
    /// </summary>
    [TestMethod]
    public void Accept_WhitespaceStrings_ReturnsExpectedResult()
    {
        // Arrange
        const string obj = "   ";
        const string visitor = "\t\n";
        const string expectedResult = "5";
        Func<string, string, string> visit = (v, o) => (v.Length + o.Length).ToString();
        // Act
        string result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that Accept works with very long strings.
    /// Input: Very long strings for both object and visitor.
    /// Expected: The visit function processes long strings correctly.
    /// </summary>
    [TestMethod]
    public void Accept_VeryLongStrings_ReturnsExpectedResult()
    {
        // Arrange
        string obj = new string('a', 10000);
        string visitor = new string('b', 5000);
        const int expectedResult = 15000;
        Func<string, string, int> visit = (v, o) => v.Length + o.Length;
        // Act
        int result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    /// <summary>
    /// Tests that AbstractFactory invokes the create function with valid inputs and returns the expected result.
    /// Input: Valid object, factory, and create function.
    /// Expected: The create function is invoked with factory and obj in correct order, and its result is returned.
    /// </summary>
    [TestMethod]
    public void AbstractFactory_ValidInputs_InvokesCreateAndReturnsResult()
    {
        // Arrange
        var sourceObj = "source";
        var factoryObj = new object();
        var expectedResult = 42;
        int createCallCount = 0;
        Func<object, string, int> create = (f, o) => { createCallCount++; return expectedResult; };
        // Act
        var result = new AbstractFactoryPattern().AbstractFactory(sourceObj, factoryObj, create);
        // Assert
        Assert.AreEqual(expectedResult, result);
        Assert.AreEqual(1, createCallCount);
    }

    /// <summary>
    /// Tests that AbstractFactory works correctly with value types.
    /// Input: Value type object, factory, and create function.
    /// Expected: The create function is invoked and returns the correct result.
    /// </summary>
    [TestMethod]
    public void AbstractFactory_ValueTypes_InvokesCreateAndReturnsResult()
    {
        // Arrange
        var sourceObj = 10;
        var factoryObj = 20;
        var expectedResult = 30;
        int createCallCount = 0;
        Func<int, int, int> create = (f, o) => { createCallCount++; return expectedResult; };
        // Act
        var result = new AbstractFactoryPattern().AbstractFactory(sourceObj, factoryObj, create);
        // Assert
        Assert.AreEqual(expectedResult, result);
        Assert.AreEqual(1, createCallCount);
    }

    /// <summary>
    /// Tests that AbstractFactory works correctly when obj is null.
    /// Input: Null object, valid factory, and create function.
    /// Expected: The create function is invoked with null obj and returns the result.
    /// </summary>
    [TestMethod]
    public void AbstractFactory_NullObj_InvokesCreateWithNullObj()
    {
        // Arrange
        string? sourceObj = null;
        var factoryObj = new object();
        var expectedResult = "result";
        int createCallCount = 0;
        Func<object, string?, string> create = (f, o) => { createCallCount++; return expectedResult; };
        // Act
        var result = new AbstractFactoryPattern().AbstractFactory(sourceObj, factoryObj, create);
        // Assert
        Assert.AreEqual(expectedResult, result);
        Assert.AreEqual(1, createCallCount);
    }

    /// <summary>
    /// Tests that AbstractFactory invokes create function with parameters in correct order.
    /// Input: Valid object, factory, and create function that validates parameter order.
    /// Expected: Create function receives factory as first parameter and obj as second parameter.
    /// </summary>
    [TestMethod]
    public void AbstractFactory_ValidInputs_InvokesCreateWithCorrectParameterOrder()
    {
        // Arrange
        var sourceObj = "sourceValue";
        var factoryObj = "factoryValue";
        object? capturedFactory = null;
        object? capturedObj = null;
        Func<string, string, bool> createFunc = (factory, obj) =>
        {
            capturedFactory = factory;
            capturedObj = obj;
            return true;
        };
        // Act
        new AbstractFactoryPattern().AbstractFactory(sourceObj, factoryObj, createFunc);
        // Assert
        Assert.AreEqual(factoryObj, capturedFactory);
        Assert.AreEqual(sourceObj, capturedObj);
    }

    /// <summary>
    /// Tests that AbstractFactory works with different type combinations.
    /// Input: String object, int factory, and create function returning double.
    /// Expected: Create function is invoked and returns the correct result.
    /// </summary>
    [TestMethod]
    public void AbstractFactory_MixedTypes_ReturnsExpectedResult()
    {
        // Arrange
        var sourceObj = "test";
        var factoryObj = 100;
        var expectedResult = 3.14;
        int createCallCount = 0;
        Func<int, string, double> create = (f, o) => { createCallCount++; return expectedResult; };
        // Act
        var result = new AbstractFactoryPattern().AbstractFactory(sourceObj, factoryObj, create);
        // Assert
        Assert.AreEqual(expectedResult, result);
        Assert.AreEqual(1, createCallCount);
    }

    /// <summary>
    /// Tests that AbstractFactory works correctly when returning null as TResult.
    /// Input: Valid object, factory, and create function that returns null.
    /// Expected: Null is returned from AbstractFactory.
    /// </summary>
    [TestMethod]
    public void AbstractFactory_CreateReturnsNull_ReturnsNull()
    {
        // Arrange
        var sourceObj = "source";
        var factoryObj = new object();
        int createCallCount = 0;
        Func<object, string, string?> create = (f, o) => { createCallCount++; return null; };
        // Act
        var result = new AbstractFactoryPattern().AbstractFactory(sourceObj, factoryObj, create);
        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(1, createCallCount);
    }

    /// <summary>
    /// Tests that AbstractFactory works with complex reference types.
    /// Input: Complex object types for all type parameters.
    /// Expected: Create function is invoked and returns the expected complex result.
    /// </summary>
    [TestMethod]
    public void AbstractFactory_ComplexReferenceTypes_WorksCorrectly()
    {
        // Arrange
        var sourceObj = new object();
        var factoryObj = new object();
        var expectedResult = new object();
        int createCallCount = 0;
        Func<object, object, object> create = (f, o) => { createCallCount++; return expectedResult; };
        // Act
        var result = new AbstractFactoryPattern().AbstractFactory(sourceObj, factoryObj, create);
        // Assert
        Assert.AreSame(expectedResult, result);
        Assert.AreEqual(1, createCallCount);
    }

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

    /// <summary>
    /// Tests that FactoryMethod correctly transforms a string to int using the provided factory.
    /// Input: string "123" with factory that parses to int.
    /// Expected: int value 123.
    /// </summary>
    [TestMethod]
    public void FactoryMethod_ValidStringToInt_ReturnsTransformedValue()
    {
        // Arrange
        var input = "123";
        Func<string, int> factory = s => int.Parse(s);
        // Act
        var result = new FactoryMethodPattern().FactoryMethod(input, factory);
        // Assert
        Assert.AreEqual(123, result);
    }

    /// <summary>
    /// Tests that FactoryMethod handles null input object by passing it to the factory.
    /// Input: null string with factory that handles null.
    /// Expected: factory receives null and returns expected result.
    /// </summary>
    [TestMethod]
    public void FactoryMethod_NullInputObject_PassesNullToFactory()
    {
        // Arrange
        string? input = null;
        Func<string?, string> factory = s => s ?? "default";
        // Act
        var result = new FactoryMethodPattern().FactoryMethod(input, factory);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests that FactoryMethod works with value types.
    /// Input: int with factory that transforms to string.
    /// Expected: correct string representation.
    /// </summary>
    [TestMethod]
    public void FactoryMethod_ValueTypeToReferenceType_ReturnsTransformedValue()
    {
        // Arrange
        var input = 42;
        Func<int, string> factory = i => i.ToString();
        // Act
        var result = new FactoryMethodPattern().FactoryMethod(input, factory);
        // Assert
        Assert.AreEqual("42", result);
    }

    /// <summary>
    /// Tests that FactoryMethod works when factory returns null.
    /// Input: string with factory that returns null.
    /// Expected: null result.
    /// </summary>
    [TestMethod]
    public void FactoryMethod_FactoryReturnsNull_ReturnsNull()
    {
        // Arrange
        var input = "test";
        Func<string, string?> factory = _ => null;
        // Act
        var result = new FactoryMethodPattern().FactoryMethod(input, factory);
        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that FactoryMethod works with identity transformation.
    /// Input: object with factory that returns the same object.
    /// Expected: same object reference returned.
    /// </summary>
    [TestMethod]
    public void FactoryMethod_IdentityFactory_ReturnsSameObject()
    {
        // Arrange
        var input = "test";
        Func<string, string> factory = s => s;
        // Act
        var result = new FactoryMethodPattern().FactoryMethod(input, factory);
        // Assert
        Assert.AreSame(input, result);
    }

    /// <summary>
    /// Tests that FactoryMethod works with complex type transformations.
    /// Input: various types with different factory transformations.
    /// Expected: correct transformation for each type combination.
    /// </summary>
    [TestMethod]
    [DataRow(0, DisplayName = "Zero value")]
    [DataRow(int.MinValue, DisplayName = "Minimum int value")]
    [DataRow(int.MaxValue, DisplayName = "Maximum int value")]
    [DataRow(-1, DisplayName = "Negative value")]
    [DataRow(42, DisplayName = "Positive value")]
    public void FactoryMethod_IntToDouble_ReturnsCorrectTransformation(int input)
    {
        // Arrange
        Func<int, double> factory = i => i * 2.5;
        var expected = input * 2.5;
        // Act
        var result = new FactoryMethodPattern().FactoryMethod(input, factory);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that FactoryMethod works with empty string input.
    /// Input: empty string with factory that returns its length.
    /// Expected: 0.
    /// </summary>
    [TestMethod]
    public void FactoryMethod_EmptyString_ReturnsExpectedResult()
    {
        // Arrange
        var input = string.Empty;
        Func<string, int> factory = s => s.Length;
        // Act
        var result = new FactoryMethodPattern().FactoryMethod(input, factory);
        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    /// Tests that FactoryMethod works with whitespace string input.
    /// Input: whitespace-only string with factory that trims and returns length.
    /// Expected: 0 after trimming.
    /// </summary>
    [TestMethod]
    public void FactoryMethod_WhitespaceString_ReturnsExpectedResult()
    {
        // Arrange
        var input = "   ";
        Func<string, int> factory = s => s.Trim().Length;
        // Act
        var result = new FactoryMethodPattern().FactoryMethod(input, factory);
        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    /// Tests that FactoryMethod works with default value type.
    /// Input: default int (0) with factory that checks for default.
    /// Expected: true indicating it's default.
    /// </summary>
    [TestMethod]
    public void FactoryMethod_DefaultValueType_ReturnsExpectedResult()
    {
        // Arrange
        var input = default(int);
        Func<int, bool> factory = i => i == default;
        // Act
        var result = new FactoryMethodPattern().FactoryMethod(input, factory);
        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/>
    /// invokes the route action with the correct mediator and object parameters.
    /// </summary>
    [TestMethod]
    public void Mediate_WithValidParameters_InvokesRouteWithCorrectArguments()
    {
        // Arrange
        string obj = "test-object";
        string mediator = "test-mediator";
        string? capturedMediator = null;
        string? capturedObj = null;
        Action<string, string> route = (m, o) =>
        {
            capturedMediator = m;
            capturedObj = o;
        };
        // Act
        new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(mediator, capturedMediator);
        Assert.AreEqual(obj, capturedObj);
    }

    /// <summary>
    /// Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/>
    /// returns the original object after executing the route action.
    /// </summary>
    [TestMethod]
    public void Mediate_WithValidParameters_ReturnsOriginalObject()
    {
        // Arrange
        string obj = "test-object";
        string mediator = "mediator";
        Action<string, string> route = (m, o) =>
        {
        };
        // Act
        string result = new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.AreSame(obj, result);
    }

    /// <summary>
    /// Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/>
    /// works correctly with value types.
    /// </summary>
    [TestMethod]
    public void Mediate_WithValueTypeObject_WorksCorrectly()
    {
        // Arrange
        int obj = 42;
        string mediator = "mediator";
        int capturedObj = 0;
        Action<string, int> route = (m, o) =>
        {
            capturedObj = o;
        };
        // Act
        int result = new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(obj, capturedObj);
    }

    /// <summary>
    /// Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/>
    /// works correctly when the object parameter is null (nullable reference type).
    /// </summary>
    [TestMethod]
    public void Mediate_WithNullObject_InvokesRouteWithNull()
    {
        // Arrange
        string? obj = null;
        string mediator = "mediator";
        string? capturedObj = "not-null";
        Action<string, string?> route = (m, o) =>
        {
            capturedObj = o;
        };
        // Act
        string? result = new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.IsNull(result);
        Assert.IsNull(capturedObj);
    }

    /// <summary>
    /// Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/>
    /// actually executes the route action.
    /// </summary>
    [TestMethod]
    public void Mediate_WithValidParameters_ExecutesRoute()
    {
        // Arrange
        string obj = "test";
        string mediator = "mediator";
        bool routeExecuted = false;
        Action<string, string> route = (m, o) =>
        {
            routeExecuted = true;
        };
        // Act
        new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.IsTrue(routeExecuted);
    }

    /// <summary>
    /// Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/>
    /// works with different concrete types for T and TMediator.
    /// </summary>
    [TestMethod]
    public void Mediate_WithDifferentGenericTypes_WorksCorrectly()
    {
        // Arrange
        int obj = 100;
        double mediator = 3.14;
        int capturedObj = 0;
        double capturedMediator = 0.0;
        Action<double, int> route = (m, o) =>
        {
            capturedMediator = m;
            capturedObj = o;
        };
        // Act
        int result = new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(mediator, capturedMediator);
        Assert.AreEqual(obj, capturedObj);
    }

    /// <summary>
    /// Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/>
    /// allows the route action to modify external state.
    /// </summary>
    [TestMethod]
    public void Mediate_RouteActionModifiesState_StateIsModified()
    {
        // Arrange
        string obj = "input";
        string mediator = "mediator";
        string externalState = "initial";
        Action<string, string> route = (m, o) =>
        {
            externalState = $"{m}-{o}";
        };
        // Act
        new Mediator().Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual("mediator-input", externalState);
    }


}