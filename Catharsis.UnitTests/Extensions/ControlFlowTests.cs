using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

///<summary>
///Unit tests for the <see cref="ControlFlow.ReturnIfNot{T}"/> method.
///</summary>
[TestClass]
public class ControlFlowTests
{
    #region Public methods
    ///<summary>
    ///Tests that DoUntil executes action the correct number of times based on when condition becomes true. Input:
    ///condition returns false N times, then true. Expected: action is called N times, condition is called N+1 times,
    ///original object is returned.
    ///</summary>
    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(5)]
    [DataRow(10)]
    public void DoUntil_ConditionBecomesTrueAfterNIterations_ActionCalledNTimes(int iterations)
    {
        // Arrange
        int obj = 100;
        int actionCallCount = 0;
        int conditionCallCount = 0;
        Func<int, bool> condition = x =>
        {
            conditionCallCount++;
            return actionCallCount >= iterations;
        };
        Action<int> action = x => actionCallCount++;

        // Act
        int result = obj.DoUntil(condition, action);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(iterations, actionCallCount);
        Assert.AreEqual(iterations + 1, conditionCallCount);
    }

    ///<summary>
    ///Tests that DoUntil returns the original object when condition is initially true. Input: condition returns true
    ///immediately. Expected: action is never called, original object is returned.
    ///</summary>
    [TestMethod]
    public void DoUntil_ConditionInitiallyTrue_ActionNotCalledAndReturnsOriginalObject()
    {
        // Arrange
        int obj = 42;
        int conditionCallCount = 0;
        Func<int, bool> condition = x =>
        {
            conditionCallCount++;
            return true;
        };
        int actionCallCount = 0;
        Action<int> action = x => actionCallCount++;

        // Act
        int result = obj.DoUntil(condition, action);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(0, actionCallCount);
        Assert.AreEqual(1, conditionCallCount);
    }

    ///<summary>
    ///Tests DoUntil with empty string. Input: empty string. Expected: method works correctly with empty string.
    ///</summary>
    [TestMethod]
    public void DoUntil_EmptyString_WorksCorrectly()
    {
        // Arrange
        string obj = string.Empty;
        Func<string, bool> condition = x => true;
        int actionCallCount = 0;
        Action<string> action = x => actionCallCount++;

        // Act
        string result = obj.DoUntil(condition, action);

        // Assert
        Assert.AreSame(string.Empty, result);
        Assert.AreEqual(0, actionCallCount);
    }

    ///<summary>
    ///Tests DoUntil with boundary value for value types (int.MaxValue). Input: int.MaxValue. Expected: method works
    ///correctly with maximum integer value.
    ///</summary>
    [TestMethod]
    public void DoUntil_IntMaxValue_WorksCorrectly()
    {
        // Arrange
        int obj = int.MaxValue;
        Func<int, bool> condition = x => true;
        int actionCallCount = 0;
        Action<int> action = x => actionCallCount++;

        // Act
        int result = obj.DoUntil(condition, action);

        // Assert
        Assert.AreEqual(int.MaxValue, result);
        Assert.AreEqual(0, actionCallCount);
    }

    ///<summary>
    ///Tests DoUntil with boundary value for value types (int.MinValue). Input: int.MinValue. Expected: method works
    ///correctly with minimum integer value.
    ///</summary>
    [TestMethod]
    public void DoUntil_IntMinValue_WorksCorrectly()
    {
        // Arrange
        int obj = int.MinValue;
        Func<int, bool> condition = x => true;
        int actionCallCount = 0;
        Action<int> action = x => actionCallCount++;

        // Act
        int result = obj.DoUntil(condition, action);

        // Assert
        Assert.AreEqual(int.MinValue, result);
        Assert.AreEqual(0, actionCallCount);
    }

    ///<summary>
    ///Tests that DoUntil works correctly when obj is null for nullable reference types. Input: null object with
    ///nullable reference type. Expected: action and condition receive null, original null is returned.
    ///</summary>
    [TestMethod]
    public void DoUntil_NullObject_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        int conditionCallCount = 0;
        Func<string?, bool> condition = x =>
        {
            conditionCallCount++;
            return true;
        };
        int actionCallCount = 0;
        Action<string?> action = x => actionCallCount++;

        // Act
        string? result = obj.DoUntil(condition, action);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(0, actionCallCount);
    }

    ///<summary>
    ///Tests that DoUntil passes the same object to both condition and action on each iteration. Input: specific object
    ///instance. Expected: condition and action receive the exact same object instance.
    ///</summary>
    [TestMethod]
    public void DoUntil_PassesSameObjectToConditionAndAction_OnEachIteration()
    {
        // Arrange
        string obj = "test";
        object? capturedInCondition = null;
        object? capturedInAction = null;
        int iterations = 0;

        Func<string, bool> condition = s =>
        {
            capturedInCondition = s;
            return iterations > 0;
        };

        Action<string> action = s =>
        {
            capturedInAction = s;
            iterations++;
        };

        // Act
        obj.DoUntil(condition, action);

        // Assert
        Assert.AreSame(obj, capturedInCondition);
        Assert.AreSame(obj, capturedInAction);
    }

    ///<summary>
    ///Tests that DoUntil returns the same reference for reference types. Input: reference type object (string).
    ///Expected: the exact same reference is returned.
    ///</summary>
    [TestMethod]
    public void DoUntil_ReferenceType_ReturnsSameReference()
    {
        // Arrange
        string obj = "test";
        Func<string, bool> condition = static x => true;
        Action<string> action = static x =>
        {
        };

        // Act
        string result = obj.DoUntil(condition, action);

        // Assert
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Tests that DoUntil works correctly with value types. Input: value type (int). Expected: returns the same value.
    ///</summary>
    [TestMethod]
    public void DoUntil_ValueType_ReturnsSameValue()
    {
        // Arrange
        int obj = 42;
        Func<int, bool> condition = static x => true;
        Action<int> action = static x =>
        {
        };

        // Act
        int result = obj.DoUntil(condition, action);

        // Assert
        Assert.AreEqual(obj, result);
    }

    ///<summary>
    ///Tests DoUntil with whitespace-only string. Input: whitespace string. Expected: method works correctly with
    ///whitespace string.
    ///</summary>
    [TestMethod]
    public void DoUntil_WhitespaceString_WorksCorrectly()
    {
        // Arrange
        string obj = "   ";
        Func<string, bool> condition = x => true;
        int actionCallCount = 0;
        Action<string> action = x => actionCallCount++;

        // Act
        string result = obj.DoUntil(condition, action);

        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(0, actionCallCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync returns the original object immediately when condition is initially true.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_ConditionInitiallyTrue_ReturnsObjectWithoutExecutingAction()
    {
        // Arrange
        int obj = 42;
        int conditionCallCount = 0;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            conditionCallCount++;
            return Task.FromResult(true);
        };
        int actionCallCount = 0;
        Func<int, CancellationToken, Task> action = (x, ct) =>
        {
            actionCallCount++;
            return Task.CompletedTask;
        };

        // Act
        int result = await obj.DoUntilAsync(condition, action, CancellationToken.None);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(0, actionCallCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync executes action multiple times until condition becomes true.
    ///</summary>
    [TestMethod]
    [DataRow(2)]
    [DataRow(5)]
    [DataRow(10)]
    public async Task DoUntilAsync_ConditionTrueAfterMultipleIterations_ExecutesActionMultipleTimes(int expectedIterations)
    {
        // Arrange
        int obj = 100;
        int actionExecutionCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (x, ct) => ValueTask.FromResult(actionExecutionCount >= expectedIterations);
        Func<int, CancellationToken, ValueTask> action = (x, ct) =>
        {
            actionExecutionCount++;
            return ValueTask.CompletedTask;
        };

        // Act
        int result = await obj.DoUntilAsync(condition, action);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(expectedIterations, actionExecutionCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync executes action multiple times when condition becomes true after N iterations.
    ///</summary>
    [TestMethod]
    [DataRow(2)]
    [DataRow(5)]
    [DataRow(10)]
    public async Task DoUntilAsync_ConditionTrueAfterNIterations_ExecutesActionNTimes(int iterations)
    {
        // Arrange
        int obj = 42;
        int conditionCallCount = 0;
        int actionCallCount = 0;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            conditionCallCount++;
            return Task.FromResult(actionCallCount >= iterations);
        };
        Func<int, CancellationToken, Task> action = (x, ct) =>
        {
            actionCallCount++;
            return Task.CompletedTask;
        };

        // Act
        int result = await obj.DoUntilAsync(condition, action, CancellationToken.None);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(iterations + 1, conditionCallCount);
        Assert.AreEqual(iterations, actionCallCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync executes action once when condition becomes true after one iteration.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_ConditionTrueAfterOneIteration_ExecutesActionOnce()
    {
        // Arrange
        int obj = 42;
        int conditionCallCount = 0;
        int actionCallCount = 0;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            conditionCallCount++;
            return Task.FromResult(conditionCallCount > 1);
        };
        Func<int, CancellationToken, Task> action = (x, ct) =>
        {
            actionCallCount++;
            return Task.CompletedTask;
        };

        // Act
        int result = await obj.DoUntilAsync(condition, action, CancellationToken.None);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(2, conditionCallCount);
        Assert.AreEqual(1, actionCallCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync returns original object immediately when condition is true on first evaluation (fast
    ///path). Action should not be executed.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_ConditionTrueImmediately_ReturnsObjectWithoutExecutingAction()
    {
        // Arrange
        int obj = 42;
        int actionExecutionCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (x, ct) => ValueTask.FromResult(true);
        Func<int, CancellationToken, ValueTask> action = (x, ct) =>
        {
            actionExecutionCount++;
            return ValueTask.CompletedTask;
        };

        // Act
        int result = await obj.DoUntilAsync(condition, action);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(0, actionExecutionCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync works correctly with default CancellationToken.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_DefaultCancellationToken_WorksCorrectly()
    {
        // Arrange
        int obj = 42;
        int conditionCallCount = 0;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            conditionCallCount++;
            return Task.FromResult(true);
        };

        // Act
        int result = await obj.DoUntilAsync(condition, (x, ct) => Task.CompletedTask);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(1, conditionCallCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync works correctly when obj is null for reference types.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_NullObject_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        int conditionCallCount = 0;
        Func<string?, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            conditionCallCount++;
            return Task.FromResult(true);
        };

        // Act
        string? result = await obj.DoUntilAsync(condition, (x, ct) => Task.CompletedTask, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(1, conditionCallCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync passes the correct CancellationToken to both condition and action.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_PassesCancellationTokenToConditionAndAction()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;
        CancellationToken? capturedConditionToken = null;
        CancellationToken? capturedActionToken = null;
        int actionCallCount = 0;

        Func<int, CancellationToken, Task<bool>> condition = (o, ct) =>
        {
            capturedConditionToken = ct;
            return Task.FromResult(actionCallCount > 0);
        };

        Func<int, CancellationToken, Task> action = (o, ct) =>
        {
            capturedActionToken = ct;
            actionCallCount++;
            return Task.CompletedTask;
        };

        // Act
        await obj.DoUntilAsync(condition, action, token);

        // Assert
        Assert.AreEqual(token, capturedConditionToken);
        Assert.AreEqual(token, capturedActionToken);
    }

    ///<summary>
    ///Tests that DoUntilAsync passes correct CancellationToken to condition and action functions.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_PassesCorrectCancellationToken_VerifiesCancellationToken()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken? conditionReceivedToken = null;
        CancellationToken? actionReceivedToken = null;
        int actionExecutionCount = 0;

        Func<int, CancellationToken, ValueTask<bool>> condition = (x, ct) =>
        {
            conditionReceivedToken = ct;
            return ValueTask.FromResult(actionExecutionCount >= 1);
        };
        Func<int, CancellationToken, ValueTask> action = (x, ct) =>
        {
            actionReceivedToken = ct;
            actionExecutionCount++;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.DoUntilAsync(condition, action, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, conditionReceivedToken);
        Assert.AreEqual(cts.Token, actionReceivedToken);
    }

    ///<summary>
    ///Tests that DoUntilAsync passes correct object value to condition and action functions.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_PassesCorrectObjectToFunctions_VerifiesObjectValue()
    {
        // Arrange
        int obj = 123;
        int? conditionReceivedValue = null;
        int? actionReceivedValue = null;
        int actionExecutionCount = 0;

        Func<int, CancellationToken, ValueTask<bool>> condition = (x, ct) =>
        {
            conditionReceivedValue = x;
            return ValueTask.FromResult(actionExecutionCount >= 1);
        };
        Func<int, CancellationToken, ValueTask> action = (x, ct) =>
        {
            actionReceivedValue = x;
            actionExecutionCount++;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.DoUntilAsync(condition, action);

        // Assert
        Assert.AreEqual(obj, conditionReceivedValue);
        Assert.AreEqual(obj, actionReceivedValue);
    }

    ///<summary>
    ///Tests that DoUntilAsync works correctly with reference types and passes the object correctly.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_ReferenceType_ReturnsOriginalObject()
    {
        // Arrange
        string obj = "test";
        int conditionCallCount = 0;
        Func<string, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            conditionCallCount++;
            return Task.FromResult(true);
        };

        // Act
        string result = await obj.DoUntilAsync(condition, (x, ct) => Task.CompletedTask, CancellationToken.None);

        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(1, conditionCallCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync works correctly with value types.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_ValueType_ReturnsOriginalValue()
    {
        // Arrange
        int obj = 123;
        int conditionCallCount = 0;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            conditionCallCount++;
            return Task.FromResult(true);
        };

        // Act
        int result = await obj.DoUntilAsync(condition, (x, ct) => Task.CompletedTask, CancellationToken.None);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(1, conditionCallCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync handles asynchronous action execution correctly.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_WithAsynchronousAction_ExecutesCorrectly()
    {
        // Arrange
        int obj = 75;
        int actionExecutionCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (x, ct) => ValueTask.FromResult(actionExecutionCount >= 2);
        Func<int, CancellationToken, ValueTask> action = async (x, ct) =>
        {
            await Task.Delay(10, ct);
            actionExecutionCount++;
        };

        // Act
        int result = await obj.DoUntilAsync(condition, action);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(2, actionExecutionCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync handles asynchronous condition evaluation correctly.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_WithAsynchronousCondition_ExecutesCorrectly()
    {
        // Arrange
        int obj = 50;
        int actionExecutionCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = async (x, ct) =>
        {
            await Task.Delay(10, ct);
            return actionExecutionCount >= 3;
        };
        Func<int, CancellationToken, ValueTask> action = (x, ct) =>
        {
            actionExecutionCount++;
            return ValueTask.CompletedTask;
        };

        // Act
        int result = await obj.DoUntilAsync(condition, action);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(3, actionExecutionCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync with custom struct type works correctly.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_WithCustomStruct_ReturnsOriginalValue()
    {
        // Arrange
        TestStruct obj = new TestStruct { Value = 100 };
        Func<TestStruct, CancellationToken, ValueTask<bool>> condition = static (x, ct) => ValueTask.FromResult(true);
        Func<TestStruct, CancellationToken, ValueTask> action = static (x, ct) => ValueTask.CompletedTask;

        // Act
        TestStruct result = await obj.DoUntilAsync(condition, action);

        // Assert
        Assert.AreEqual(obj.Value, result.Value);
    }

    ///<summary>
    ///Tests that DoUntilAsync with default cancellation token works correctly.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_WithDefaultCancellationToken_ExecutesSuccessfully()
    {
        // Arrange
        int obj = 42;
        int actionExecutionCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (x, ct) => ValueTask.FromResult(actionExecutionCount >= 1);
        Func<int, CancellationToken, ValueTask> action = (x, ct) =>
        {
            actionExecutionCount++;
            return ValueTask.CompletedTask;
        };

        // Act
        int result = await obj.DoUntilAsync(condition, action);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(1, actionExecutionCount);
    }

    ///<summary>
    ///Tests that DoUntilAsync works correctly with null reference types.
    ///</summary>
    [TestMethod]
    public async Task DoUntilAsync_WithNullObject_ReturnsNull()
    {
        // Arrange
        string? obj = null;
        Func<string?, CancellationToken, ValueTask<bool>> condition = static (x, ct) => ValueTask.FromResult(true);
        Func<string?, CancellationToken, ValueTask> action = static (x, ct) => ValueTask.CompletedTask;

        // Act
        string? result = await obj.DoUntilAsync(condition, action);

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that DoUntilAsync works correctly with reference types (strings).
    ///</summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("test")]
    [DataRow("   ")]
    [DataRow("a very long string with many characters to test edge cases")]
    public async Task DoUntilAsync_WithStringTypes_ReturnsOriginalValue(string value)
    {
        // Arrange
        Func<string, CancellationToken, ValueTask<bool>> condition = static (x, ct) => ValueTask.FromResult(true);
        Func<string, CancellationToken, ValueTask> action = static (x, ct) => ValueTask.CompletedTask;

        // Act
        string result = await value.DoUntilAsync(condition, action);

        // Assert
        Assert.AreEqual(value, result);
    }

    ///<summary>
    ///Tests that DoUntilAsync works correctly with value types (integers).
    ///</summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(-1)]
    [DataRow(42)]
    public async Task DoUntilAsync_WithValueTypes_ReturnsOriginalValue(int value)
    {
        // Arrange
        Func<int, CancellationToken, ValueTask<bool>> condition = static (x, ct) => ValueTask.FromResult(true);
        Func<int, CancellationToken, ValueTask> action = static (x, ct) => ValueTask.CompletedTask;

        // Act
        int result = await value.DoUntilAsync(condition, action);

        // Assert
        Assert.AreEqual(value, result);
    }

    ///<summary>
    ///Tests that DoWhile returns the original object without executing action when condition is initially false.
    ///</summary>
    [TestMethod]
    public void DoWhile_ConditionInitiallyFalse_ActionNeverExecutedAndObjectReturned()
    {
        // Arrange
        int obj = 10;
        int executionCount = 0;
        Func<int, bool> condition = x => x < 5;
        Action<int> action = x => executionCount++;

        // Act
        int result = obj.DoWhile(condition, action);

        // Assert
        Assert.AreEqual(10, result);
        Assert.AreEqual(0, executionCount);
    }

    ///<summary>
    ///Tests that DoWhile handles null object correctly for reference types and returns null.
    ///</summary>
    [TestMethod]
    public void DoWhile_WithNullObject_HandlesNullAndReturnsNull()
    {
        // Arrange
        string? obj = null;
        int executionCount = 0;
        Func<string?, bool> condition = x => executionCount < 1;
        Action<string?> action = x => executionCount++;

        // Act
        string? result = obj.DoWhile(condition, action);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(1, executionCount);
    }

    ///<summary>
    ///Tests that DoWhile works with string type and complex conditions.
    ///</summary>
    [TestMethod]
    public void DoWhile_WithStringType_WorksCorrectly()
    {
        // Arrange
        string obj = "test";
        int length = 0;
        Func<string, bool> condition = s => length < s.Length;
        Action<string> action = s => length++;

        // Act
        string result = obj.DoWhile(condition, action);

        // Assert
        Assert.AreEqual("test", result);
        Assert.AreEqual(4, length);
    }

    ///<summary>
    ///Tests that DoWhile works correctly with value types and returns the same value.
    ///</summary>
    [TestMethod]
    public void DoWhile_WithValueType_ReturnsOriginalValue()
    {
        // Arrange
        int obj = 42;
        int callCount = 0;
        Func<int, bool> condition = x => callCount < 3;
        Action<int> action = x => callCount++;

        // Act
        int result = obj.DoWhile(condition, action);

        // Assert
        Assert.AreEqual(42, result);
        Assert.AreEqual(3, callCount);
    }

    ///<summary>
    ///Tests that DoWhileAsync returns original object when condition is immediately false. Input: Condition that
    ///returns false, action that tracks execution. Expected: Returns original object, action never executed.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_ConditionImmediatelyFalse_ReturnsOriginalObjectWithoutExecutingAction()
    {
        // Arrange
        int obj = 42;
        int actionExecutionCount = 0;
        Func<int, CancellationToken, Task<bool>> condition = (o, ct) => Task.FromResult(false);
        Func<int, CancellationToken, Task> action = (o, ct) =>
        {
            actionExecutionCount++;
            return Task.CompletedTask;
        };

        // Act
        int result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(42, result);
        Assert.AreEqual(0, actionExecutionCount);
    }

    ///<summary>
    ///Tests that DoWhileAsync returns original object without executing action when condition returns false
    ///asynchronously.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_ConditionReturnsFalseAsynchronously_ReturnsOriginalObjectWithoutExecutingAction()
    {
        // Arrange
        int obj = 42;
        int actionExecutionCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = async (o, ct) =>
        {
            await Task.Delay(1, ct);
            return false;
        };
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecutionCount++;
            return ValueTask.CompletedTask;
        };

        // Act
        int result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(42, result);
        Assert.AreEqual(0, actionExecutionCount);
    }

    ///<summary>
    ///Tests that DoWhileAsync returns original object without executing action when condition returns false
    ///synchronously (fast path).
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_ConditionReturnsFalseSynchronously_ReturnsOriginalObjectWithoutExecutingAction()
    {
        // Arrange
        int obj = 42;
        int actionExecutionCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(false);
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecutionCount++;
            return ValueTask.CompletedTask;
        };

        // Act
        int result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(42, result);
        Assert.AreEqual(0, actionExecutionCount);
    }

    ///<summary>
    ///Tests that DoWhileAsync executes action multiple times when condition returns true multiple times.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_ConditionReturnsTrueMultipleTimes_ExecutesActionMultipleTimes()
    {
        // Arrange
        int obj = 5;
        int conditionCallCount = 0;
        int actionExecutionCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            conditionCallCount++;
            return ValueTask.FromResult(conditionCallCount <= 5);
        };
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecutionCount++;
            return ValueTask.CompletedTask;
        };

        // Act
        int result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(5, result);
        Assert.AreEqual(5, actionExecutionCount);
        Assert.AreEqual(6, conditionCallCount); // Initial + 5 loop iterations
    }

    ///<summary>
    ///Tests that DoWhileAsync executes action once when condition returns true once then false.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_ConditionReturnsTrueOnceThenFalse_ExecutesActionOnce()
    {
        // Arrange
        int obj = 10;
        int callCount = 0;
        int actionExecutionCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            callCount++;
            return ValueTask.FromResult(callCount == 1);
        };
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecutionCount++;
            return ValueTask.CompletedTask;
        };

        // Act
        int result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(10, result);
        Assert.AreEqual(1, actionExecutionCount);
        Assert.AreEqual(2, callCount); // Initial condition + one loop iteration
    }

    ///<summary>
    ///Tests that DoWhileAsync handles condition that completes synchronously with true (uses SlowPath).
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_ConditionReturnsTrueSynchronously_UsesSlowPath()
    {
        // Arrange
        int obj = 7;
        int callCount = 0;
        int actionExecutionCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            callCount++;
            return ValueTask.FromResult(callCount == 1);
        };
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecutionCount++;
            return ValueTask.CompletedTask;
        };

        // Act
        int result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(7, result);
        Assert.AreEqual(1, actionExecutionCount);
    }

    ///<summary>
    ///Tests that DoWhileAsync executes action the expected number of times based on condition. Input: Condition that
    ///returns true N times, then false. Expected: Action executed exactly N times, returns original object.
    ///</summary>
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(5)]
    [DataRow(10)]
    [TestMethod]
    public async Task DoWhileAsync_ConditionTrueMultipleTimes_ExecutesActionCorrectNumberOfTimes(int expectedIterations)
    {
        // Arrange
        int obj = 100;
        int actionExecutionCount = 0;
        int conditionCheckCount = 0;

        Func<int, CancellationToken, Task<bool>> condition = (o, ct) =>
        {
            conditionCheckCount++;
            return Task.FromResult(conditionCheckCount <= expectedIterations);
        };

        Func<int, CancellationToken, Task> action = (o, ct) =>
        {
            actionExecutionCount++;
            return Task.CompletedTask;
        };

        // Act
        int result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(100, result);
        Assert.AreEqual(expectedIterations, actionExecutionCount);
    }

    ///<summary>
    ///Tests that DoWhileAsync correctly passes cancellation token to condition and action delegates.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_PassesCancellationToken_CorrectTokenReceived()
    {
        // Arrange
        int obj = 50;
        CancellationTokenSource cts = new();
        CancellationToken? receivedInCondition = null;
        CancellationToken? receivedInAction = null;
        int callCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            receivedInCondition = ct;
            callCount++;
            return ValueTask.FromResult(callCount == 1);
        };
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            receivedInAction = ct;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.DoWhileAsync(condition, action, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, receivedInCondition);
        Assert.AreEqual(cts.Token, receivedInAction);
    }

    ///<summary>
    ///Tests that DoWhileAsync passes the correct CancellationToken to condition and action. Input: Specific
    ///CancellationToken. Expected: Condition and action receive the same CancellationToken.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_PassesCorrectCancellationTokenToConditionAndAction()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken? receivedInCondition = null;
        CancellationToken? receivedInAction = null;
        bool firstCall = true;

        Func<int, CancellationToken, Task<bool>> condition = (o, ct) =>
        {
            receivedInCondition = ct;
            bool result = firstCall;
            firstCall = false;
            return Task.FromResult(result);
        };

        Func<int, CancellationToken, Task> action = (o, ct) =>
        {
            receivedInAction = ct;
            return Task.CompletedTask;
        };

        // Act
        await obj.DoWhileAsync(condition, action, cts.Token);

        // Assert
        Assert.IsNotNull(receivedInCondition);
        Assert.IsNotNull(receivedInAction);
        Assert.AreEqual(cts.Token, receivedInCondition.Value);
        Assert.AreEqual(cts.Token, receivedInAction.Value);
    }

    ///<summary>
    ///Tests that DoWhileAsync passes the correct object to condition and action. Input: Specific object value.
    ///Expected: Condition and action receive the exact object value.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_PassesCorrectObjectToConditionAndAction()
    {
        // Arrange
        int obj = 777;
        int? receivedInCondition = null;
        int? receivedInAction = null;
        bool firstCall = true;

        Func<int, CancellationToken, Task<bool>> condition = (o, ct) =>
        {
            receivedInCondition = o;
            bool result = firstCall;
            firstCall = false;
            return Task.FromResult(result);
        };

        Func<int, CancellationToken, Task> action = (o, ct) =>
        {
            receivedInAction = o;
            return Task.CompletedTask;
        };

        // Act
        await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(777, receivedInCondition);
        Assert.AreEqual(777, receivedInAction);
    }

    ///<summary>
    ///Tests that DoWhileAsync correctly passes the object to condition and action delegates.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_PassesObjectToConditionAndAction_CorrectObjectReceived()
    {
        // Arrange
        int obj = 99;
        int? receivedInCondition = null;
        int? receivedInAction = null;
        int callCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            receivedInCondition = o;
            callCount++;
            return ValueTask.FromResult(callCount == 1);
        };
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            receivedInAction = o;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(99, receivedInCondition);
        Assert.AreEqual(99, receivedInAction);
    }

    ///<summary>
    ///Tests that DoWhileAsync handles asynchronous condition and action execution.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_WithAsyncConditionAndAction_ExecutesCorrectly()
    {
        // Arrange
        int obj = 20;
        int iterations = 0;
        int actionCount = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = async (o, ct) =>
        {
            await Task.Delay(1, ct);
            iterations++;
            return iterations <= 3;
        };
        Func<int, CancellationToken, ValueTask> action = async (o, ct) =>
        {
            await Task.Delay(1, ct);
            actionCount++;
        };

        // Act
        int result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(20, result);
        Assert.AreEqual(3, actionCount);
        Assert.AreEqual(4, iterations);
    }

    ///<summary>
    ///Tests that DoWhileAsync works with default CancellationToken. Input: No explicit CancellationToken provided (uses
    ///default). Expected: Executes successfully without cancellation.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_WithDefaultCancellationToken_ExecutesSuccessfully()
    {
        // Arrange
        int obj = 99;
        int actionExecutionCount = 0;

        Func<int, CancellationToken, Task<bool>> condition = (o, ct) =>
        {
            actionExecutionCount++;
            return Task.FromResult(actionExecutionCount < 3);
        };

        Func<int, CancellationToken, Task> action = (o, ct) =>
        {
            Assert.IsFalse(ct.IsCancellationRequested);
            return Task.CompletedTask;
        };

        // Act
        int result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(99, result);
        Assert.AreEqual(3, actionExecutionCount);
    }

    ///<summary>
    ///Tests that DoWhileAsync handles empty string object correctly.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        string obj = string.Empty;
        int iterations = 0;
        Func<string, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            iterations++;
            return ValueTask.FromResult(iterations <= 1);
        };
        Func<string, CancellationToken, ValueTask> action = (o, ct) => ValueTask.CompletedTask;

        // Act
        string result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreSame(string.Empty, result);
    }

    ///<summary>
    ///Tests that DoWhileAsync with null reference type object returns null when condition is false.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_WithNullReferenceType_ReturnsNull()
    {
        // Arrange
        string? obj = null;
        Func<string?, CancellationToken, ValueTask<bool>> condition = static (o, ct) => ValueTask.FromResult(false);
        Func<string?, CancellationToken, ValueTask> action = static (o, ct) => ValueTask.CompletedTask;

        // Act
        string? result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that DoWhileAsync with reference type returns the original reference.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_WithReferenceType_ReturnsOriginalReference()
    {
        // Arrange
        string obj = "test";
        int iterations = 0;
        Func<string, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            iterations++;
            return ValueTask.FromResult(iterations <= 2);
        };
        Func<string, CancellationToken, ValueTask> action = (o, ct) => ValueTask.CompletedTask;

        // Act
        string result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Tests that DoWhileAsync returns the same reference for reference types. Input: Reference type object (string).
    ///Expected: Returns the exact same reference.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_WithReferenceType_ReturnsSameReference()
    {
        // Arrange
        string obj = "test";
        Func<string, CancellationToken, Task<bool>> condition = static (o, ct) => Task.FromResult(false);
        Func<string, CancellationToken, Task> action = static (o, ct) => Task.CompletedTask;

        // Act
        string result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Tests that DoWhileAsync correctly handles struct type with multiple iterations.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_WithStructType_HandlesMultipleIterations()
    {
        // Arrange
        DateTime obj = new DateTime(2024, 1, 1);
        int iterations = 0;
        Func<DateTime, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            iterations++;
            return ValueTask.FromResult(iterations <= 4);
        };
        Func<DateTime, CancellationToken, ValueTask> action = (o, ct) => ValueTask.CompletedTask;

        // Act
        DateTime result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(new DateTime(2024, 1, 1), result);
        Assert.AreEqual(5, iterations);
    }

    ///<summary>
    ///Tests that DoWhileAsync handles value type correctly. Input: Integer value type with condition and action.
    ///Expected: Returns the same value.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_WithValueType_ReturnsOriginalValue()
    {
        // Arrange
        int obj = 12345;
        int iterationCount = 0;

        Func<int, CancellationToken, Task<bool>> condition = (o, ct) =>
        {
            iterationCount++;
            return Task.FromResult(iterationCount <= 3);
        };

        Func<int, CancellationToken, Task> action = (o, ct) => Task.CompletedTask;

        // Act
        int result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(12345, result);
    }

    ///<summary>
    ///Tests that DoWhileAsync with zero iterations (condition false from start) returns immediately.
    ///</summary>
    [TestMethod]
    public async Task DoWhileAsync_ZeroIterations_ReturnsImmediately()
    {
        // Arrange
        double obj = 3.14;
        int conditionCalls = 0;
        int actionCalls = 0;
        Func<double, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            conditionCalls++;
            return ValueTask.FromResult(false);
        };
        Func<double, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionCalls++;
            return ValueTask.CompletedTask;
        };

        // Act
        double result = await obj.DoWhileAsync(condition, action);

        // Assert
        Assert.AreEqual(3.14, result);
        Assert.AreEqual(1, conditionCalls);
        Assert.AreEqual(0, actionCalls);
    }

    ///<summary>
    ///Tests that no exception is thrown when the action is null but condition is false. Input: obj=5, condition returns
    ///false, action=null. Expected: No exception is thrown since action is never invoked.
    ///</summary>
    [TestMethod]
    public void If_ActionIsNullAndConditionIsFalse_DoesNotThrowException()
    {
        // Arrange
        int testValue = 5;
        Func<int, bool> condition = static x => false;
        Action<int>? action = null;

        // Act & Assert - Should not throw
        testValue.If(condition, action!);
    }

    ///<summary>
    ///Tests that the action is not executed when the condition evaluates to false. Input: obj=5, condition returns
    ///false. Expected: action is not invoked.
    ///</summary>
    [TestMethod]
    public void If_ConditionIsFalse_DoesNotExecuteAction()
    {
        // Arrange
        int testValue = 5;
        int actionCallCount = 0;

        // Act
        testValue.If(x => x == 10, x => actionCallCount++);

        // Assert
        Assert.AreEqual(0, actionCallCount);
    }

    ///<summary>
    ///Tests that the action is executed when the condition evaluates to true. Input: obj=5, condition returns true.
    ///Expected: action is invoked with the object.
    ///</summary>
    [TestMethod]
    public void If_ConditionIsTrue_ExecutesAction()
    {
        // Arrange
        int testValue = 5;
        int actionCallCount = 0;
        int receivedValue = 0;

        // Act
        testValue.If(
        x => x == 5,
        x =>
        {
            actionCallCount++;
            receivedValue = x;
        });

        // Assert
        Assert.AreEqual(1, actionCallCount);
        Assert.AreEqual(5, receivedValue);
    }

    ///<summary>
    ///Tests that the condition receives the exact object passed to the extension method. Input: obj with specific
    ///value. Expected: condition receives the same object.
    ///</summary>
    [TestMethod]
    public void If_ConditionReceivesCorrectObject_ExecutesWithProperValue()
    {
        // Arrange
        int testValue = 42;
        int? conditionReceivedValue = null;
        int? actionReceivedValue = null;

        // Act
        testValue.If(
        x =>
        {
            conditionReceivedValue = x;
            return true;
        },
        x => actionReceivedValue = x);

        // Assert
        Assert.AreEqual(42, conditionReceivedValue);
        Assert.AreEqual(42, actionReceivedValue);
    }

    ///<summary>
    ///Tests that the method works correctly with empty strings. Input: obj=string.Empty, condition checks for empty
    ///string. Expected: action is executed.
    ///</summary>
    [TestMethod]
    public void If_EmptyString_ExecutesActionWhenConditionIsTrue()
    {
        // Arrange
        string testValue = string.Empty;
        int actionCallCount = 0;
        string? receivedValue = null;

        // Act
        testValue.If(
        x => x == string.Empty,
        x =>
        {
            actionCallCount++;
            receivedValue = x;
        });

        // Assert
        Assert.AreEqual(1, actionCallCount);
        Assert.AreEqual(string.Empty, receivedValue);
    }

    ///<summary>
    ///Tests that the method works correctly with null objects for reference types. Input: obj=null (string), condition
    ///handles null. Expected: action is executed when condition returns true.
    ///</summary>
    [TestMethod]
    public void If_ObjectIsNull_ExecutesActionWhenConditionIsTrue()
    {
        // Arrange
        string? testValue = null;
        int actionCallCount = 0;
        string? receivedValue = "not null";

        // Act
        testValue.If(
        x => x == null,
        x =>
        {
            actionCallCount++;
            receivedValue = x;
        });

        // Assert
        Assert.AreEqual(1, actionCallCount);
        Assert.IsNull(receivedValue);
    }

    ///<summary>
    ///Tests that the method works correctly with reference types. Input: obj="test", condition returns true. Expected:
    ///action is executed with the string object.
    ///</summary>
    [TestMethod]
    public void If_ReferenceType_ExecutesActionWhenConditionIsTrue()
    {
        // Arrange
        string testValue = "test";
        int actionCallCount = 0;
        string? receivedValue = null;

        // Act
        testValue.If(
        x => x.Length == 4,
        x =>
        {
            actionCallCount++;
            receivedValue = x;
        });

        // Assert
        Assert.AreEqual(1, actionCallCount);
        Assert.AreEqual("test", receivedValue);
    }

    ///<summary>
    ///Tests that the method works correctly with value types at boundary values. Input: obj=int.MaxValue, condition
    ///checks for max value. Expected: action is executed.
    ///</summary>
    [TestMethod]
    public void If_ValueTypeAtMaxBoundary_ExecutesActionWhenConditionIsTrue()
    {
        // Arrange
        int testValue = int.MaxValue;
        int actionCallCount = 0;
        int receivedValue = 0;

        // Act
        testValue.If(
        x => x == int.MaxValue,
        x =>
        {
            actionCallCount++;
            receivedValue = x;
        });

        // Assert
        Assert.AreEqual(1, actionCallCount);
        Assert.AreEqual(int.MaxValue, receivedValue);
    }

    ///<summary>
    ///Tests that the method works correctly with value types at minimum boundary values. Input: obj=int.MinValue,
    ///condition checks for min value. Expected: action is executed.
    ///</summary>
    [TestMethod]
    public void If_ValueTypeAtMinBoundary_ExecutesActionWhenConditionIsTrue()
    {
        // Arrange
        int testValue = int.MinValue;
        int actionCallCount = 0;
        int receivedValue = 0;

        // Act
        testValue.If(
        x => x == int.MinValue,
        x =>
        {
            actionCallCount++;
            receivedValue = x;
        });

        // Assert
        Assert.AreEqual(1, actionCallCount);
        Assert.AreEqual(int.MinValue, receivedValue);
    }

    ///<summary>
    ///Tests that the method works correctly with zero value. Input: obj=0, condition checks for zero. Expected: action
    ///is executed.
    ///</summary>
    [TestMethod]
    public void If_ZeroValue_ExecutesActionWhenConditionIsTrue()
    {
        // Arrange
        int testValue = 0;
        int actionCallCount = 0;

        // Act
        testValue.If(x => x == 0, x => actionCallCount++);

        // Assert
        Assert.AreEqual(1, actionCallCount);
    }

    ///<summary>
    ///Tests that IfAsync works correctly with complex reference types.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ComplexReferenceType_WorksCorrectly()
    {
        // Arrange
        var obj = new { Id = 1, Name = "Test" };
        dynamic? receivedObj = null;
        Func<object, CancellationToken, Task<bool>> condition = new Func<object, CancellationToken, Task<bool>>(
                                                                (o, _) =>
                                                                {
                                                                    receivedObj = o;
                                                                    return Task.FromResult(true);
                                                                });
        bool actionExecuted = false;
        Func<object, CancellationToken, Task> action = new Func<object, CancellationToken, Task>(
                                                       (_, _) =>
                                                       {
                                                           actionExecuted = true;
                                                           return Task.CompletedTask;
                                                       });

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.AreEqual(obj, receivedObj);
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync completes successfully when condition is false and returns CompletedTask.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ConditionFalse_ReturnsCompletedTask()
    {
        // Arrange
        int obj = 42;
        Func<int, CancellationToken, ValueTask<bool>> condition = static (o, ct) => ValueTask.FromResult(false);
        Func<int, CancellationToken, ValueTask> action = static (o, ct) => ValueTask.CompletedTask;

        // Act
        ValueTask result = obj.IfAsync(condition, action);

        // Assert
        Assert.IsTrue(result.IsCompletedSuccessfully);
        await result;
    }

    ///<summary>
    ///Tests that IfAsync does not execute action when condition returns false asynchronously (slow path).
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ConditionFalseAsynchronously_DoesNotExecuteAction()
    {
        // Arrange
        int obj = 42;
        bool actionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            return false;
        };
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.IsFalse(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync does not execute action when condition returns false synchronously (fast path).
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ConditionFalseSynchronously_DoesNotExecuteAction()
    {
        // Arrange
        int obj = 42;
        bool actionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(false);
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.IsFalse(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync does not execute the action when the condition returns false.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ConditionReturnsFalse_DoesNotExecuteAction()
    {
        // Arrange
        int obj = 42;
        bool actionExecuted = false;
        Func<int, CancellationToken, Task<bool>> condition = new Func<int, CancellationToken, Task<bool>>((_, _) => Task.FromResult(false));
        Func<int, CancellationToken, Task> action = new Func<int, CancellationToken, Task>(
                                                    (_, _) =>
                                                    {
                                                        actionExecuted = true;
                                                        return Task.CompletedTask;
                                                    });

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.IsFalse(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync executes the action when the condition returns true.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ConditionReturnsTrue_ExecutesAction()
    {
        // Arrange
        int obj = 42;
        bool actionExecuted = false;
        Func<int, CancellationToken, Task<bool>> condition = new Func<int, CancellationToken, Task<bool>>((_, _) => Task.FromResult(true));
        Func<int, CancellationToken, Task> action = new Func<int, CancellationToken, Task>(
                                                    (_, _) =>
                                                    {
                                                        actionExecuted = true;
                                                        return Task.CompletedTask;
                                                    });

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync passes the correct cancellation token to the action function when condition is true.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ConditionTrue_PassesCancellationTokenToAction()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;
        Func<int, CancellationToken, Task<bool>> condition = new Func<int, CancellationToken, Task<bool>>((_, _) => Task.FromResult(true));
        Func<int, CancellationToken, Task> action = new Func<int, CancellationToken, Task>(
                                                    (_, ct) =>
                                                    {
                                                        receivedToken = ct;
                                                        return Task.CompletedTask;
                                                    });

        // Act
        await obj.IfAsync(condition, action, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, receivedToken);
    }

    ///<summary>
    ///Tests that IfAsync passes the correct object to the action function when condition is true.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ConditionTrue_PassesObjectToAction()
    {
        // Arrange
        int obj = 42;
        int? receivedObj = null;
        Func<int, CancellationToken, Task<bool>> condition = new Func<int, CancellationToken, Task<bool>>((_, _) => Task.FromResult(true));
        Func<int, CancellationToken, Task> action = new Func<int, CancellationToken, Task>(
                                                    (o, _) =>
                                                    {
                                                        receivedObj = o;
                                                        return Task.CompletedTask;
                                                    });

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.AreEqual(obj, receivedObj);
    }

    ///<summary>
    ///Tests that IfAsync executes action when condition returns true asynchronously (slow path).
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ConditionTrueAsynchronously_ExecutesAction()
    {
        // Arrange
        int obj = 42;
        bool actionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            return true;
        };
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync executes action when condition returns true synchronously (fast path).
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ConditionTrueSynchronously_ExecutesAction()
    {
        // Arrange
        int obj = 42;
        bool actionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(true);
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync uses default cancellation token when not provided.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_DefaultCancellationToken_ExecutesSuccessfully()
    {
        // Arrange
        int obj = 42;
        bool actionExecuted = false;
        CancellationToken receivedToken = CancellationToken.None;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            receivedToken = ct;
            return ValueTask.FromResult(true);
        };
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.IsTrue(actionExecuted);
        Assert.AreEqual(default(CancellationToken), receivedToken);
    }

    ///<summary>
    ///Tests that IfAsync works with nullable reference types.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_NullableReferenceType_ExecutesCorrectly()
    {
        // Arrange
        string? obj = null;
        bool actionExecuted = false;
        Func<string?, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(o is null);
        Func<string?, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync works correctly when object is null.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_NullObject_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        string? receivedObj = "not null";
        Func<string?, CancellationToken, Task<bool>> condition = new Func<string?, CancellationToken, Task<bool>>(
                                                                 (o, _) =>
                                                                 {
                                                                     receivedObj = o;
                                                                     return Task.FromResult(true);
                                                                 });
        bool actionExecuted = false;
        Func<string?, CancellationToken, Task> action = new Func<string?, CancellationToken, Task>(
                                                        (_, _) =>
                                                        {
                                                            actionExecuted = true;
                                                            return Task.CompletedTask;
                                                        });

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.IsNull(receivedObj);
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync passes the correct object and cancellation token to action when condition is true.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_PassesObjectAndTokenToAction_CorrectValuesReceived()
    {
        // Arrange
        int obj = 42;
        int receivedObj = 0;
        CancellationToken receivedToken = default;
        using CancellationTokenSource cts = new();
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(true);
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            receivedObj = o;
            receivedToken = ct;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfAsync(condition, action, cts.Token);

        // Assert
        Assert.AreEqual(42, receivedObj);
        Assert.AreEqual(cts.Token, receivedToken);
    }

    ///<summary>
    ///Tests that IfAsync passes the correct object to condition function.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_PassesObjectToCondition_CorrectObjectReceived()
    {
        // Arrange
        int obj = 42;
        int receivedObj = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            receivedObj = o;
            return ValueTask.FromResult(false);
        };
        Func<int, CancellationToken, ValueTask> action = (o, ct) => ValueTask.CompletedTask;

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.AreEqual(42, receivedObj);
    }

    ///<summary>
    ///Tests that IfAsync works with reference types.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ReferenceType_ExecutesCorrectly()
    {
        // Arrange
        string obj = "test";
        bool actionExecuted = false;
        Func<string, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(o == "test");
        Func<string, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync works correctly with reference type objects (including null).
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ReferenceTypeObject_WorksCorrectly()
    {
        // Arrange
        string? obj = "test";
        string? receivedObj = null;
        Func<string?, CancellationToken, Task<bool>> condition = new Func<string?, CancellationToken, Task<bool>>(
                                                                 (o, _) =>
                                                                 {
                                                                     receivedObj = o;
                                                                     return Task.FromResult(true);
                                                                 });
        bool actionExecuted = false;
        Func<string?, CancellationToken, Task> action = new Func<string?, CancellationToken, Task>(
                                                        (_, _) =>
                                                        {
                                                            actionExecuted = true;
                                                            return Task.CompletedTask;
                                                        });

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.AreEqual(obj, receivedObj);
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync passes the correct cancellation token to the condition function.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ValidParameters_PassesCancellationTokenToCondition()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;
        Func<int, CancellationToken, Task<bool>> condition = new Func<int, CancellationToken, Task<bool>>(
                                                             (_, ct) =>
                                                             {
                                                                 receivedToken = ct;
                                                                 return Task.FromResult(false);
                                                             });
        Func<int, CancellationToken, Task> action = new Func<int, CancellationToken, Task>((_, _) => Task.CompletedTask);

        // Act
        await obj.IfAsync(condition, action, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, receivedToken);
    }

    ///<summary>
    ///Tests that IfAsync passes the correct object to the condition function.
    ///</summary>
    [TestMethod]
    public async Task IfAsync_ValidParameters_PassesObjectToCondition()
    {
        // Arrange
        int obj = 42;
        int? receivedObj = null;
        Func<int, CancellationToken, Task<bool>> condition = new Func<int, CancellationToken, Task<bool>>(
                                                             (o, _) =>
                                                             {
                                                                 receivedObj = o;
                                                                 return Task.FromResult(false);
                                                             });
        Func<int, CancellationToken, Task> action = new Func<int, CancellationToken, Task>((_, _) => Task.CompletedTask);

        // Act
        await obj.IfAsync(condition, action);

        // Assert
        Assert.AreEqual(obj, receivedObj);
    }

    ///<summary>
    ///Tests that IfAsync works with value types with extreme values.
    ///</summary>
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(1)]
    [TestMethod]
    public async Task IfAsync_ValueTypeExtremeValues_ExecutesCorrectly(int value)
    {
        // Arrange
        bool actionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(o == value);
        Func<int, CancellationToken, ValueTask> action = (o, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await value.IfAsync(condition, action);

        // Assert
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync handles condition returning false and does not execute action with various value types.
    ///</summary>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(0)]
    [DataRow(int.MaxValue)]
    public async Task IfAsync_ValueTypeWithConditionFalse_DoesNotExecuteAction(int value)
    {
        // Arrange
        bool actionExecuted = false;
        Func<int, CancellationToken, Task<bool>> condition = new Func<int, CancellationToken, Task<bool>>((_, _) => Task.FromResult(false));
        Func<int, CancellationToken, Task> action = new Func<int, CancellationToken, Task>(
                                                    (_, _) =>
                                                    {
                                                        actionExecuted = true;
                                                        return Task.CompletedTask;
                                                    });

        // Act
        await value.IfAsync(condition, action);

        // Assert
        Assert.IsFalse(actionExecuted);
    }

    ///<summary>
    ///Tests that IfAsync executes action correctly with various value types when condition is true.
    ///</summary>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(0)]
    [DataRow(int.MaxValue)]
    public async Task IfAsync_ValueTypeWithConditionTrue_ExecutesAction(int value)
    {
        // Arrange
        bool actionExecuted = false;
        Func<int, CancellationToken, Task<bool>> condition = new Func<int, CancellationToken, Task<bool>>((_, _) => Task.FromResult(true));
        Func<int, CancellationToken, Task> action = new Func<int, CancellationToken, Task>(
                                                    (_, _) =>
                                                    {
                                                        actionExecuted = true;
                                                        return Task.CompletedTask;
                                                    });

        // Act
        await value.IfAsync(condition, action);

        // Assert
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that IfElse calls elseAction when condition evaluates to false. Input: Object with condition returning
    ///false. Expected: elseAction is invoked with the object, ifAction is not invoked.
    ///</summary>
    [TestMethod]
    public void IfElse_ConditionFalse_CallsElseAction()
    {
        // Arrange
        string testObject = "test";
        int conditionCallCount = 0;
        Func<string, bool> condition = x =>
        {
            conditionCallCount++;
            return false;
        };
        int ifActionCallCount = 0;
        Action<string> ifAction = x => ifActionCallCount++;
        int elseActionCallCount = 0;
        Action<string> elseAction = x => elseActionCallCount++;

        // Act
        testObject.IfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(0, ifActionCallCount);
        Assert.AreEqual(1, elseActionCallCount);
    }

    ///<summary>
    ///Tests that IfElse calls ifAction when condition evaluates to true. Input: Object with condition returning true.
    ///Expected: ifAction is invoked with the object, elseAction is not invoked.
    ///</summary>
    [TestMethod]
    public void IfElse_ConditionTrue_CallsIfAction()
    {
        // Arrange
        string testObject = "test";
        int conditionCallCount = 0;
        Func<string, bool> condition = x =>
        {
            conditionCallCount++;
            return true;
        };
        int ifActionCallCount = 0;
        Action<string> ifAction = x => ifActionCallCount++;
        int elseActionCallCount = 0;
        Action<string> elseAction = x => elseActionCallCount++;

        // Act
        testObject.IfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(1, ifActionCallCount);
        Assert.AreEqual(0, elseActionCallCount);
    }

    ///<summary>
    ///Tests that IfElse works correctly with floating-point edge values. Input: Double.NaN, Double.PositiveInfinity,
    ///Double.NegativeInfinity. Expected: Correct action is called based on condition.
    ///</summary>
    [TestMethod]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    [DataRow(0.0)]
    [DataRow(-0.0)]
    public void IfElse_DoubleEdgeValues_CallsCorrectAction(double testValue)
    {
        // Arrange
        int conditionCallCount = 0;
        Func<double, bool> condition = x =>
        {
            conditionCallCount++;
            return true;
        };
        int ifActionCallCount = 0;
        Action<double> ifAction = x => ifActionCallCount++;
        int elseActionCallCount = 0;
        Action<double> elseAction = x => elseActionCallCount++;

        // Act
        testValue.IfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(1, ifActionCallCount);
        Assert.AreEqual(0, elseActionCallCount);
    }

    ///<summary>
    ///Tests that IfElse works correctly with empty string. Input: Empty string with condition returning true. Expected:
    ///ifAction is called with empty string.
    ///</summary>
    [TestMethod]
    public void IfElse_EmptyString_CallsIfActionCorrectly()
    {
        // Arrange
        string testObject = string.Empty;
        int conditionCallCount = 0;
        Func<string, bool> condition = x =>
        {
            conditionCallCount++;
            return true;
        };
        int ifActionCallCount = 0;
        string? ifActionReceivedValue = null;
        Action<string> ifAction = x =>
        {
            ifActionCallCount++;
            ifActionReceivedValue = x;
        };
        int elseActionCallCount = 0;
        Action<string> elseAction = x => elseActionCallCount++;

        // Act
        testObject.IfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(1, ifActionCallCount);
        Assert.AreEqual(string.Empty, ifActionReceivedValue);
        Assert.AreEqual(0, elseActionCallCount);
    }

    ///<summary>
    ///Tests that IfElse works correctly with nullable value types. Input: Nullable int with null value. Expected:
    ///ifAction is called with null value.
    ///</summary>
    [TestMethod]
    public void IfElse_NullableValueTypeNull_CallsIfActionCorrectly()
    {
        // Arrange
        int? testObject = null;
        int conditionCallCount = 0;
        Func<int?, bool> condition = x =>
        {
            conditionCallCount++;
            return true;
        };
        int ifActionCallCount = 0;
        Action<int?> ifAction = x => ifActionCallCount++;
        int elseActionCallCount = 0;
        Action<int?> elseAction = x => elseActionCallCount++;

        // Act
        testObject.IfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(1, ifActionCallCount);
        Assert.AreEqual(0, elseActionCallCount);
    }

    ///<summary>
    ///Tests that IfElse works correctly with nullable value types having a value. Input: Nullable int with value 42.
    ///Expected: elseAction is called with value 42.
    ///</summary>
    [TestMethod]
    public void IfElse_NullableValueTypeWithValue_CallsElseActionCorrectly()
    {
        // Arrange
        int? testObject = 42;
        int conditionCallCount = 0;
        Func<int?, bool> condition = x =>
        {
            conditionCallCount++;
            return false;
        };
        int ifActionCallCount = 0;
        Action<int?> ifAction = x => ifActionCallCount++;
        int elseActionCallCount = 0;
        Action<int?> elseAction = x => elseActionCallCount++;

        // Act
        testObject.IfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(0, ifActionCallCount);
        Assert.AreEqual(1, elseActionCallCount);
    }

    ///<summary>
    ///Tests that IfElse works correctly with null object for reference types. Input: Null string object with condition
    ///returning true. Expected: ifAction is called with null object.
    ///</summary>
    [TestMethod]
    public void IfElse_NullReferenceTypeObject_CallsIfActionWithNull()
    {
        // Arrange
        string? testObject = null;
        int conditionCallCount = 0;
        Func<string?, bool> condition = x =>
        {
            conditionCallCount++;
            return true;
        };
        int ifActionCallCount = 0;
        string? ifActionReceivedValue = "not null";
        Action<string?> ifAction = x =>
        {
            ifActionCallCount++;
            ifActionReceivedValue = x;
        };
        int elseActionCallCount = 0;
        Action<string?> elseAction = x => elseActionCallCount++;

        // Act
        testObject.IfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(1, ifActionCallCount);
        Assert.IsNull(ifActionReceivedValue);
        Assert.AreEqual(0, elseActionCallCount);
    }

    ///<summary>
    ///Tests that IfElse works correctly with value types. Input: Integer value with condition returning false.
    ///Expected: elseAction is called with the integer value.
    ///</summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(-1)]
    [DataRow(1)]
    public void IfElse_ValueType_CallsElseActionCorrectly(int testValue)
    {
        // Arrange
        int conditionCallCount = 0;
        Func<int, bool> condition = x =>
        {
            conditionCallCount++;
            return false;
        };
        int ifActionCallCount = 0;
        Action<int> ifAction = x => ifActionCallCount++;
        int elseActionCallCount = 0;
        int elseActionReceivedValue = 0;
        Action<int> elseAction = x =>
        {
            elseActionCallCount++;
            elseActionReceivedValue = x;
        };

        // Act
        testValue.IfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(0, ifActionCallCount);
        Assert.AreEqual(1, elseActionCallCount);
        Assert.AreEqual(testValue, elseActionReceivedValue);
    }

    ///<summary>
    ///Tests that IfElse works correctly with very long strings. Input: Very long string with condition returning true.
    ///Expected: ifAction is called with the long string.
    ///</summary>
    [TestMethod]
    public void IfElse_VeryLongString_CallsIfActionCorrectly()
    {
        // Arrange
        string testObject = new string('a', 10000);
        int conditionCallCount = 0;
        Func<string, bool> condition = x =>
        {
            conditionCallCount++;
            return true;
        };
        int ifActionCallCount = 0;
        string? ifActionReceivedValue = null;
        Action<string> ifAction = x =>
        {
            ifActionCallCount++;
            ifActionReceivedValue = x;
        };
        int elseActionCallCount = 0;
        Action<string> elseAction = x => elseActionCallCount++;

        // Act
        testObject.IfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(1, ifActionCallCount);
        Assert.AreEqual(testObject, ifActionReceivedValue);
        Assert.AreEqual(0, elseActionCallCount);
    }

    ///<summary>
    ///Tests that IfElse works correctly with whitespace string. Input: Whitespace string with condition returning
    ///false. Expected: elseAction is called with whitespace string.
    ///</summary>
    [TestMethod]
    public void IfElse_WhitespaceString_CallsElseActionCorrectly()
    {
        // Arrange
        string testObject = "   ";
        int conditionCallCount = 0;
        Func<string, bool> condition = x =>
        {
            conditionCallCount++;
            return false;
        };
        int ifActionCallCount = 0;
        Action<string> ifAction = x => ifActionCallCount++;
        int elseActionCallCount = 0;
        string? elseActionReceivedValue = null;
        Action<string> elseAction = x =>
        {
            elseActionCallCount++;
            elseActionReceivedValue = x;
        };

        // Act
        testObject.IfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(0, ifActionCallCount);
        Assert.AreEqual(1, elseActionCallCount);
        Assert.AreEqual("   ", elseActionReceivedValue);
    }

    ///<summary>
    ///Tests that IfElseAsync completes the entire async flow correctly when condition is false. Input: Async condition
    ///and elseAction with delays. Expected: Both complete successfully in the correct order.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_AsyncConditionAndElseAction_CompletesSuccessfully()
    {
        // Arrange
        int obj = 42;
        List<string> executionOrder = new System.Collections.Generic.List<string>();

        Func<int, CancellationToken, Task<bool>> condition = async (_, _) =>
        {
            await Task.Delay(10);
            executionOrder.Add("condition");
            return false;
        };
        Func<int, CancellationToken, Task> ifAction = (_, _) =>
        {
            executionOrder.Add("ifAction");
            return Task.CompletedTask;
        };
        Func<int, CancellationToken, Task> elseAction = async (_, _) =>
        {
            await Task.Delay(10);
            executionOrder.Add("elseAction");
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.HasCount(2, executionOrder);
        Assert.AreEqual("condition", executionOrder[0]);
        Assert.AreEqual("elseAction", executionOrder[1]);
    }

    ///<summary>
    ///Tests that IfElseAsync completes the entire async flow correctly when condition is true. Input: Async condition
    ///and ifAction with delays. Expected: Both complete successfully in the correct order.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_AsyncConditionAndIfAction_CompletesSuccessfully()
    {
        // Arrange
        int obj = 42;
        List<string> executionOrder = new System.Collections.Generic.List<string>();

        Func<int, CancellationToken, Task<bool>> condition = async (_, _) =>
        {
            await Task.Delay(10);
            executionOrder.Add("condition");
            return true;
        };
        Func<int, CancellationToken, Task> ifAction = async (_, _) =>
        {
            await Task.Delay(10);
            executionOrder.Add("ifAction");
        };
        Func<int, CancellationToken, Task> elseAction = (_, _) =>
        {
            executionOrder.Add("elseAction");
            return Task.CompletedTask;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.HasCount(2, executionOrder);
        Assert.AreEqual("condition", executionOrder[0]);
        Assert.AreEqual("ifAction", executionOrder[1]);
    }

    ///<summary>
    ///Tests that IfElseAsync executes actions asynchronously in slow path with async elseAction.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_AsyncElseActionInSlowPath_ExecutesCorrectly()
    {
        // Arrange
        int obj = 42;
        bool elseActionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            return false;
        };
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) => ValueTask.CompletedTask;
        Func<int, CancellationToken, ValueTask> elseAction = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            elseActionExecuted = true;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsTrue(elseActionExecuted);
    }

    ///<summary>
    ///Tests that IfElseAsync executes actions asynchronously in slow path with async ifAction.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_AsyncIfActionInSlowPath_ExecutesCorrectly()
    {
        // Arrange
        int obj = 42;
        bool ifActionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            return true;
        };
        Func<int, CancellationToken, ValueTask> ifAction = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            ifActionExecuted = true;
        };
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) => ValueTask.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsTrue(ifActionExecuted);
    }

    ///<summary>
    ///Tests that IfElseAsync executes elseAction when condition returns false. Input: Condition that returns false.
    ///Expected: elseAction is executed, ifAction is not executed.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_ConditionFalse_ExecutesElseAction()
    {
        // Arrange
        int obj = 42;
        bool ifActionExecuted = false;
        bool elseActionExecuted = false;

        Func<int, CancellationToken, Task<bool>> condition = (_, _) => Task.FromResult(false);
        Func<int, CancellationToken, Task> ifAction = (_, _) =>
        {
            ifActionExecuted = true;
            return Task.CompletedTask;
        };
        Func<int, CancellationToken, Task> elseAction = (_, _) =>
        {
            elseActionExecuted = true;
            return Task.CompletedTask;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsFalse(ifActionExecuted);
        Assert.IsTrue(elseActionExecuted);
    }

    ///<summary>
    ///Tests that IfElseAsync executes elseAction when condition returns false asynchronously (slow path).
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_ConditionFalseAsynchronous_ExecutesElseAction()
    {
        // Arrange
        int obj = 42;
        bool ifActionExecuted = false;
        bool elseActionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            return false;
        };
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) =>
        {
            ifActionExecuted = true;
            return ValueTask.CompletedTask;
        };
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) =>
        {
            elseActionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsFalse(ifActionExecuted);
        Assert.IsTrue(elseActionExecuted);
    }

    ///<summary>
    ///Tests that IfElseAsync executes elseAction when condition returns false synchronously.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_ConditionFalseSynchronous_ExecutesElseAction()
    {
        // Arrange
        int obj = 42;
        bool ifActionExecuted = false;
        bool elseActionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(false);
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) =>
        {
            ifActionExecuted = true;
            return ValueTask.CompletedTask;
        };
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) =>
        {
            elseActionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsFalse(ifActionExecuted);
        Assert.IsTrue(elseActionExecuted);
    }

    ///<summary>
    ///Tests that IfElseAsync executes ifAction when condition returns true. Input: Condition that returns true.
    ///Expected: ifAction is executed, elseAction is not executed.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_ConditionTrue_ExecutesIfAction()
    {
        // Arrange
        int obj = 42;
        bool ifActionExecuted = false;
        bool elseActionExecuted = false;

        Func<int, CancellationToken, Task<bool>> condition = (_, _) => Task.FromResult(true);
        Func<int, CancellationToken, Task> ifAction = (_, _) =>
        {
            ifActionExecuted = true;
            return Task.CompletedTask;
        };
        Func<int, CancellationToken, Task> elseAction = (_, _) =>
        {
            elseActionExecuted = true;
            return Task.CompletedTask;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsTrue(ifActionExecuted);
        Assert.IsFalse(elseActionExecuted);
    }

    ///<summary>
    ///Tests that IfElseAsync executes ifAction when condition returns true asynchronously (slow path).
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_ConditionTrueAsynchronous_ExecutesIfAction()
    {
        // Arrange
        int obj = 42;
        bool ifActionExecuted = false;
        bool elseActionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            return true;
        };
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) =>
        {
            ifActionExecuted = true;
            return ValueTask.CompletedTask;
        };
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) =>
        {
            elseActionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsTrue(ifActionExecuted);
        Assert.IsFalse(elseActionExecuted);
    }

    ///<summary>
    ///Tests that IfElseAsync executes ifAction when condition returns true synchronously.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_ConditionTrueSynchronous_ExecutesIfAction()
    {
        // Arrange
        int obj = 42;
        bool ifActionExecuted = false;
        bool elseActionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(true);
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) =>
        {
            ifActionExecuted = true;
            return ValueTask.CompletedTask;
        };
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) =>
        {
            elseActionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsTrue(ifActionExecuted);
        Assert.IsFalse(elseActionExecuted);
    }

    ///<summary>
    ///Tests that IfElseAsync works with default CancellationToken.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_DefaultCancellationToken_ExecutesCorrectly()
    {
        // Arrange
        int obj = 42;
        bool ifActionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(true);
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) =>
        {
            ifActionExecuted = true;
            return ValueTask.CompletedTask;
        };
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) => ValueTask.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsTrue(ifActionExecuted);
    }

    ///<summary>
    ///Tests that IfElseAsync uses default cancellation token when none is provided. Input: No cancellation token
    ///parameter. Expected: Method completes successfully with default token.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_DefaultCancellationToken_WorksCorrectly()
    {
        // Arrange
        int obj = 42;
        CancellationToken receivedToken = CancellationToken.None;

        Func<int, CancellationToken, Task<bool>> condition = (_, ct) =>
        {
            receivedToken = ct;
            return Task.FromResult(true);
        };
        Func<int, CancellationToken, Task> ifAction = (_, _) => Task.CompletedTask;
        Func<int, CancellationToken, Task> elseAction = (_, _) => Task.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(default(CancellationToken), receivedToken);
    }

    ///<summary>
    ///Tests that IfElseAsync works with value types (int boundary values).
    ///</summary>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(1)]
    public async Task IfElseAsync_IntBoundaryValues_ExecutesCorrectly(int value)
    {
        // Arrange
        bool actionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(o >= 0);
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await value.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that IfElseAsync works with value type objects at boundary values. Input: int.MaxValue. Expected: Boundary
    ///value is correctly passed to delegates.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_IntMaxValue_WorksCorrectly()
    {
        // Arrange
        int obj = int.MaxValue;
        int? receivedObj = null;

        Func<int, CancellationToken, Task<bool>> condition = (o, _) =>
        {
            receivedObj = o;
            return Task.FromResult(true);
        };
        Func<int, CancellationToken, Task> ifAction = (_, _) => Task.CompletedTask;
        Func<int, CancellationToken, Task> elseAction = (_, _) => Task.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(obj, receivedObj);
    }

    ///<summary>
    ///Tests that IfElseAsync works with value type objects at boundary values. Input: int.MinValue. Expected: Boundary
    ///value is correctly passed to delegates.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_IntMinValue_WorksCorrectly()
    {
        // Arrange
        int obj = int.MinValue;
        int? receivedObj = null;

        Func<int, CancellationToken, Task<bool>> condition = (o, _) =>
        {
            receivedObj = o;
            return Task.FromResult(true);
        };
        Func<int, CancellationToken, Task> ifAction = (_, _) => Task.CompletedTask;
        Func<int, CancellationToken, Task> elseAction = (_, _) => Task.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(obj, receivedObj);
    }

    ///<summary>
    ///Tests that IfElseAsync works with null object when T is a reference type.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_NullObject_ExecutesCorrectly()
    {
        // Arrange
        string? obj = null;
        bool ifActionExecuted = false;
        Func<string?, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(o == null);
        Func<string?, CancellationToken, ValueTask> ifAction = (o, ct) =>
        {
            ifActionExecuted = true;
            return ValueTask.CompletedTask;
        };
        Func<string?, CancellationToken, ValueTask> elseAction = (o, ct) => ValueTask.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsTrue(ifActionExecuted);
    }

    ///<summary>
    ///Tests that IfElseAsync works with null reference type objects. Input: Null string object. Expected: Null is
    ///correctly passed to delegates.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_NullReferenceTypeObject_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        string? receivedObj = "not null";

        Func<string?, CancellationToken, Task<bool>> condition = (o, _) =>
        {
            receivedObj = o;
            return Task.FromResult(true);
        };
        Func<string?, CancellationToken, Task> ifAction = (_, _) => Task.CompletedTask;
        Func<string?, CancellationToken, Task> elseAction = (_, _) => Task.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsNull(receivedObj);
    }

    ///<summary>
    ///Tests that IfElseAsync passes the cancellation token to condition. Input: Specific CancellationToken. Expected:
    ///Condition receives the same CancellationToken.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_PassesCancellationTokenToCondition()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken? receivedToken = null;

        Func<int, CancellationToken, Task<bool>> condition = (_, ct) =>
        {
            receivedToken = ct;
            return Task.FromResult(true);
        };
        Func<int, CancellationToken, Task> ifAction = (_, _) => Task.CompletedTask;
        Func<int, CancellationToken, Task> elseAction = (_, _) => Task.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, receivedToken);
    }

    ///<summary>
    ///Tests that IfElseAsync passes cancellation token to condition function.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_PassesCancellationTokenToCondition_CorrectToken()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            receivedToken = ct;
            return ValueTask.FromResult(true);
        };
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) => ValueTask.CompletedTask;
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) => ValueTask.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, receivedToken);
    }

    ///<summary>
    ///Tests that IfElseAsync passes the cancellation token to elseAction when condition is false. Input: Specific
    ///CancellationToken and condition that returns false. Expected: elseAction receives the same CancellationToken.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_PassesCancellationTokenToElseAction()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken? receivedToken = null;

        Func<int, CancellationToken, Task<bool>> condition = (_, _) => Task.FromResult(false);
        Func<int, CancellationToken, Task> ifAction = (_, _) => Task.CompletedTask;
        Func<int, CancellationToken, Task> elseAction = (_, ct) =>
        {
            receivedToken = ct;
            return Task.CompletedTask;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, receivedToken);
    }

    ///<summary>
    ///Tests that IfElseAsync passes cancellation token to elseAction when condition is false.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_PassesCancellationTokenToElseAction_CorrectToken()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(false);
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) => ValueTask.CompletedTask;
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) =>
        {
            receivedToken = ct;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, receivedToken);
    }

    ///<summary>
    ///Tests that IfElseAsync passes the cancellation token to ifAction when condition is true. Input: Specific
    ///CancellationToken and condition that returns true. Expected: ifAction receives the same CancellationToken.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_PassesCancellationTokenToIfAction()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken? receivedToken = null;

        Func<int, CancellationToken, Task<bool>> condition = (_, _) => Task.FromResult(true);
        Func<int, CancellationToken, Task> ifAction = (_, ct) =>
        {
            receivedToken = ct;
            return Task.CompletedTask;
        };
        Func<int, CancellationToken, Task> elseAction = (_, _) => Task.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, receivedToken);
    }

    ///<summary>
    ///Tests that IfElseAsync passes cancellation token to ifAction when condition is true.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_PassesCancellationTokenToIfAction_CorrectToken()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(true);
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) =>
        {
            receivedToken = ct;
            return ValueTask.CompletedTask;
        };
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) => ValueTask.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, receivedToken);
    }

    ///<summary>
    ///Tests that IfElseAsync passes the correct object to the condition delegate. Input: Specific object value.
    ///Expected: Condition receives the same object.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_PassesCorrectObjectToCondition()
    {
        // Arrange
        int obj = 42;
        int? receivedObj = null;

        Func<int, CancellationToken, Task<bool>> condition = (o, _) =>
        {
            receivedObj = o;
            return Task.FromResult(true);
        };
        Func<int, CancellationToken, Task> ifAction = (_, _) => Task.CompletedTask;
        Func<int, CancellationToken, Task> elseAction = (_, _) => Task.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(obj, receivedObj);
    }

    ///<summary>
    ///Tests that IfElseAsync passes the correct object to elseAction when condition is false. Input: Specific object
    ///value and condition that returns false. Expected: elseAction receives the same object.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_PassesCorrectObjectToElseAction()
    {
        // Arrange
        int obj = 42;
        int? receivedObj = null;

        Func<int, CancellationToken, Task<bool>> condition = (_, _) => Task.FromResult(false);
        Func<int, CancellationToken, Task> ifAction = (_, _) => Task.CompletedTask;
        Func<int, CancellationToken, Task> elseAction = (o, _) =>
        {
            receivedObj = o;
            return Task.CompletedTask;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(obj, receivedObj);
    }

    ///<summary>
    ///Tests that IfElseAsync passes the correct object to ifAction when condition is true. Input: Specific object value
    ///and condition that returns true. Expected: ifAction receives the same object.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_PassesCorrectObjectToIfAction()
    {
        // Arrange
        int obj = 42;
        int? receivedObj = null;

        Func<int, CancellationToken, Task<bool>> condition = (_, _) => Task.FromResult(true);
        Func<int, CancellationToken, Task> ifAction = (o, _) =>
        {
            receivedObj = o;
            return Task.CompletedTask;
        };
        Func<int, CancellationToken, Task> elseAction = (_, _) => Task.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(obj, receivedObj);
    }

    ///<summary>
    ///Tests that IfElseAsync passes the correct object to condition function.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_PassesObjectToCondition_CorrectValue()
    {
        // Arrange
        int obj = 42;
        int receivedObject = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            receivedObject = o;
            return ValueTask.FromResult(true);
        };
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) => ValueTask.CompletedTask;
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) => ValueTask.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(obj, receivedObject);
    }

    ///<summary>
    ///Tests that IfElseAsync passes the correct object to elseAction when condition is false.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_PassesObjectToElseAction_CorrectValue()
    {
        // Arrange
        int obj = 42;
        int receivedObject = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(false);
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) => ValueTask.CompletedTask;
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) =>
        {
            receivedObject = o;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(obj, receivedObject);
    }

    ///<summary>
    ///Tests that IfElseAsync passes the correct object to ifAction when condition is true.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_PassesObjectToIfAction_CorrectValue()
    {
        // Arrange
        int obj = 42;
        int receivedObject = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(true);
        Func<int, CancellationToken, ValueTask> ifAction = (o, ct) =>
        {
            receivedObject = o;
            return ValueTask.CompletedTask;
        };
        Func<int, CancellationToken, ValueTask> elseAction = (o, ct) => ValueTask.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(obj, receivedObject);
    }

    ///<summary>
    ///Tests that IfElseAsync works with reference type objects. Input: String object. Expected: String object is
    ///correctly passed to delegates.
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_ReferenceTypeObject_WorksCorrectly()
    {
        // Arrange
        string obj = "test string";
        string? receivedObj = null;

        Func<string, CancellationToken, Task<bool>> condition = (o, _) =>
        {
            receivedObj = o;
            return Task.FromResult(true);
        };
        Func<string, CancellationToken, Task> ifAction = (_, _) => Task.CompletedTask;
        Func<string, CancellationToken, Task> elseAction = (_, _) => Task.CompletedTask;

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(obj, receivedObj);
    }

    ///<summary>
    ///Tests that IfElseAsync works with various generic types (string).
    ///</summary>
    [TestMethod]
    public async Task IfElseAsync_StringType_ExecutesCorrectly()
    {
        // Arrange
        string obj = "test";
        bool elseActionExecuted = false;
        Func<string, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(o.Length > 10);
        Func<string, CancellationToken, ValueTask> ifAction = (o, ct) => ValueTask.CompletedTask;
        Func<string, CancellationToken, ValueTask> elseAction = (o, ct) =>
        {
            elseActionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await obj.IfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsTrue(elseActionExecuted);
    }

    ///<summary>
    ///Tests that the method can be chained with other operations.
    ///</summary>
    [TestMethod]
    public void IfNot_Chaining_WorksCorrectly()
    {
        // Arrange
        int value = 5;
        int executionCount = 0;
        Func<int, bool> condition1 = x => x > 10;
        Func<int, bool> condition2 = x => x < 0;
        Action<int> action = x => executionCount++;

        // Act
        value.IfNot(condition1, action);
        value.IfNot(condition2, action);

        // Assert
        Assert.AreEqual(2, executionCount, "Both actions should execute since both conditions are false.");
    }

    ///<summary>
    ///Tests that the action is executed when the condition returns false.
    ///</summary>
    [TestMethod]
    public void IfNot_ConditionReturnsFalse_ExecutesAction()
    {
        // Arrange
        int obj = 5;
        bool actionExecuted = false;
        Func<int, bool> condition = x => x > 10;
        Action<int> action = x => actionExecuted = true;

        // Act
        obj.IfNot(condition, action);

        // Assert
        Assert.IsTrue(actionExecuted, "Action should be executed when condition returns false.");
    }

    ///<summary>
    ///Tests that the action is not executed when the condition returns true.
    ///</summary>
    [TestMethod]
    public void IfNot_ConditionReturnsTrue_DoesNotExecuteAction()
    {
        // Arrange
        int obj = 15;
        bool actionExecuted = false;
        Func<int, bool> condition = x => x > 10;
        Action<int> action = x => actionExecuted = true;

        // Act
        obj.IfNot(condition, action);

        // Assert
        Assert.IsFalse(actionExecuted, "Action should not be executed when condition returns true.");
    }

    ///<summary>
    ///Tests that the method works correctly with nullable value types.
    ///</summary>
    [TestMethod]
    public void IfNot_NullableValueType_WorksCorrectly()
    {
        // Arrange
        int? obj = null;
        int? receivedValue = -1;
        Func<int?, bool> condition = x =>
        {
            receivedValue = x;
            return false;
        };
        Action<int?> action = x =>
        {
        };

        // Act
        obj.IfNot(condition, action);

        // Assert
        Assert.IsNull(receivedValue, "Nullable value type should be passed correctly.");
    }

    ///<summary>
    ///Tests that no exception is thrown when the action is null but condition returns true. The action is never
    ///invoked, so the null reference is not accessed.
    ///</summary>
    [TestMethod]
    public void IfNot_NullActionAndConditionReturnsTrue_DoesNotThrow()
    {
        // Arrange
        int obj = 5;
        Func<int, bool> condition = static x => true;
        Action<int> action = null!;

        // Act & Assert (should not throw)
        obj.IfNot(condition, action);
    }

    ///<summary>
    ///Tests that the method works correctly with a null object (reference type).
    ///</summary>
    [TestMethod]
    public void IfNot_NullObject_PassesNullToConditionAndAction()
    {
        // Arrange
        string? obj = null;
        string? conditionReceived = "not null";
        string? actionReceived = "not null";
        Func<string?, bool> condition = x =>
        {
            conditionReceived = x;
            return false;
        };
        Action<string?> action = x => actionReceived = x;

        // Act
        obj.IfNot(condition, action);

        // Assert
        Assert.IsNull(conditionReceived, "Condition should receive null.");
        Assert.IsNull(actionReceived, "Action should receive null.");
    }

    ///<summary>
    ///Tests that the correct object is passed to both the condition and action.
    ///</summary>
    [TestMethod]
    public void IfNot_PassesCorrectObjectToConditionAndAction()
    {
        // Arrange
        string obj = "test";
        string? conditionReceived = null;
        string? actionReceived = null;
        Func<string, bool> condition = x =>
        {
            conditionReceived = x;
            return false;
        };
        Action<string> action = x => actionReceived = x;

        // Act
        obj.IfNot(condition, action);

        // Assert
        Assert.AreEqual(obj, conditionReceived, "Condition should receive the original object.");
        Assert.AreEqual(obj, actionReceived, "Action should receive the original object.");
    }

    ///<summary>
    ///Tests that the method works correctly with reference types.
    ///</summary>
    [TestMethod]
    [DataRow("", false, true)]
    [DataRow("test", false, true)]
    [DataRow("   ", true, false)]
    public void IfNot_ReferenceTypes_WorksCorrectly(string value, bool conditionResult, bool shouldExecuteAction)
    {
        // Arrange
        bool actionExecuted = false;
        Func<string, bool> condition = x => conditionResult;
        Action<string> action = x => actionExecuted = true;

        // Act
        value.IfNot(condition, action);

        // Assert
        Assert.AreEqual(shouldExecuteAction, actionExecuted);
    }

    ///<summary>
    ///Tests that the method works correctly with value types.
    ///</summary>
    [TestMethod]
    [DataRow(0, false, true)]
    [DataRow(10, false, true)]
    [DataRow(-5, false, true)]
    [DataRow(100, true, false)]
    [DataRow(int.MinValue, false, true)]
    [DataRow(int.MaxValue, true, false)]
    public void IfNot_ValueTypes_WorksCorrectly(int value, bool conditionResult, bool shouldExecuteAction)
    {
        // Arrange
        bool actionExecuted = false;
        Func<int, bool> condition = x => conditionResult;
        Action<int> action = x => actionExecuted = true;

        // Act
        value.IfNot(condition, action);

        // Assert
        Assert.AreEqual(shouldExecuteAction, actionExecuted);
    }

    ///<summary>
    ///Tests that IfNotAsync evaluates the condition exactly once and action exactly once when condition is false.
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ConditionFalse_EvaluatesConditionAndActionOnce()
    {
        // Arrange
        int obj = 10;
        int conditionCallCount = 0;
        int actionCallCount = 0;
        Task<bool> ConditionFunc(int n, CancellationToken ct)
        {
            conditionCallCount++;
            return Task.FromResult(false);
        }

        async Task ActionFunc(int n, CancellationToken ct)
        {
            actionCallCount++;
            await Task.CompletedTask;
        }

        // Act
        await obj.IfNotAsync(ConditionFunc, ActionFunc);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(1, actionCallCount);
    }

    ///<summary>
    ///Tests that IfNotAsync executes the action when the condition returns false.
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ConditionReturnsFalse_ActionExecuted()
    {
        // Arrange
        int obj = 200;
        int conditionCallCount = 0;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            conditionCallCount++;
            return Task.FromResult(false);
        };
        int actionCallCount = 0;
        Func<int, CancellationToken, Task> action = (x, ct) =>
        {
            actionCallCount++;
            return Task.CompletedTask;
        };

        // Act
        await obj.IfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(1, actionCallCount);
    }

    ///<summary>
    ///Tests that IfNotAsync does not execute the action when the condition returns true.
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ConditionReturnsTrue_ActionNotExecuted()
    {
        // Arrange
        int obj = 100;
        int conditionCallCount = 0;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            conditionCallCount++;
            return Task.FromResult(true);
        };
        int actionCallCount = 0;
        Func<int, CancellationToken, Task> action = (x, ct) =>
        {
            actionCallCount++;
            return Task.CompletedTask;
        };

        // Act
        await obj.IfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(0, actionCallCount);
    }

    ///<summary>
    ///Tests that IfNotAsync evaluates the condition exactly once when condition is true.
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ConditionTrue_EvaluatesConditionOnce()
    {
        // Arrange
        int obj = 5;
        int conditionCallCount = 0;
        Task<bool> ConditionFunc(int n, CancellationToken ct)
        {
            conditionCallCount++;
            return Task.FromResult(true);
        }

        async Task ActionFunc(int n, CancellationToken ct) { await Task.CompletedTask; }

        // Act
        await obj.IfNotAsync(ConditionFunc, ActionFunc);

        // Assert
        Assert.AreEqual(1, conditionCallCount);
    }

    ///<summary>
    ///Tests that IfNotAsync works correctly with custom reference types.
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_CustomReferenceType_WorksCorrectly()
    {
        // Arrange
        TestHelper obj = new TestHelper { Value = 100 };
        int conditionCallCount = 0;
        Func<TestHelper, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            conditionCallCount++;
            return Task.FromResult(false);
        };
        bool actionCalled = false;
        Task ActionFunc(TestHelper th, CancellationToken ct)
        {
            actionCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await obj.IfNotAsync(condition, ActionFunc);

        // Assert
        Assert.IsTrue(actionCalled);
        Assert.AreEqual(1, conditionCallCount);
    }

    ///<summary>
    ///Tests that IfNotAsync with default cancellation token uses CancellationToken.None.
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_DefaultCancellationToken_UsesCancellationTokenNone()
    {
        // Arrange
        string obj = "test";
        CancellationToken? capturedToken = null;
        Task<bool> ConditionFunc(string s, CancellationToken ct)
        {
            capturedToken = ct;
            return Task.FromResult(true);
        }

        async Task ActionFunc(string s, CancellationToken ct) { await Task.CompletedTask; }

        // Act
        await obj.IfNotAsync(ConditionFunc, ActionFunc);

        // Assert
        Assert.IsNotNull(capturedToken);
        Assert.AreEqual(CancellationToken.None, capturedToken.Value);
    }

    ///<summary>
    ///Tests that IfNotAsync passes the correct object and cancellation token to the action when condition is false.
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_PassesCorrectParametersToAction()
    {
        // Arrange
        string obj = "test-action";
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;
        string? capturedObj = null;
        CancellationToken? capturedToken = null;
        Func<string, CancellationToken, Task<bool>> condition = (x, ct) => Task.FromResult(false);
        Func<string, CancellationToken, Task> action = (x, ct) =>
        {
            capturedObj = x;
            capturedToken = ct;
            return Task.CompletedTask;
        };

        // Act
        await obj.IfNotAsync(condition, action, token);

        // Assert
        Assert.AreEqual(obj, capturedObj);
        Assert.AreEqual(token, capturedToken);
    }

    ///<summary>
    ///Tests that IfNotAsync passes the correct object and cancellation token to the condition.
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_PassesCorrectParametersToCondition()
    {
        // Arrange
        string obj = "test-object";
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;
        string? capturedObj = null;
        CancellationToken? capturedToken = null;
        Func<string, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            capturedObj = x;
            capturedToken = ct;
            return Task.FromResult(true);
        };

        // Act
        await obj.IfNotAsync(condition, (x, ct) => Task.CompletedTask, token);

        // Assert
        Assert.AreEqual(obj, capturedObj);
        Assert.AreEqual(token, capturedToken);
    }

    ///<summary>
    ///Tests that IfNotAsync works with reference types (string).
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ReferenceType_WorksCorrectly()
    {
        // Arrange
        string obj = "reference-type-test";
        Func<string, CancellationToken, Task<bool>> condition = (x, ct) => Task.FromResult(false);
        bool actionExecuted = false;
        Task ActionFunc(string s, CancellationToken ct)
        {
            actionExecuted = true;
            return Task.CompletedTask;
        }

        // Act
        await obj.IfNotAsync(condition, ActionFunc);

        // Assert
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that IfNotAsync executes action when condition returns false synchronously (fast path).
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ValueTask_ConditionFalseFastPath_ActionExecuted()
    {
        // Arrange
        int testObject = 42;
        bool actionExecuted = false;
        int passedObject = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (obj, ct) => new ValueTask<bool>(false);
        Func<int, CancellationToken, ValueTask> action = (obj, ct) =>
        {
            actionExecuted = true;
            passedObject = obj;
            return ValueTask.CompletedTask;
        };

        // Act
        await testObject.IfNotAsync(condition, action);

        // Assert
        Assert.IsTrue(actionExecuted);
        Assert.AreEqual(testObject, passedObject);
    }

    ///<summary>
    ///Tests that IfNotAsync executes action when condition returns false asynchronously (slow path).
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ValueTask_ConditionFalseSlowPath_ActionExecuted()
    {
        // Arrange
        string testObject = "test";
        bool actionExecuted = false;
        string? passedObject = null;
        Func<string, CancellationToken, ValueTask<bool>> condition = async (obj, ct) =>
        {
            await Task.Delay(10, ct);
            return false;
        };
        Func<string, CancellationToken, ValueTask> action = async (obj, ct) =>
        {
            await Task.Delay(10, ct);
            actionExecuted = true;
            passedObject = obj;
        };

        // Act
        await testObject.IfNotAsync(condition, action);

        // Assert
        Assert.IsTrue(actionExecuted);
        Assert.AreEqual(testObject, passedObject);
    }

    ///<summary>
    ///Tests that IfNotAsync does not execute action when condition returns true synchronously (fast path).
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ValueTask_ConditionTrueFastPath_ActionNotExecuted()
    {
        // Arrange
        int testObject = 42;
        bool actionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (obj, ct) => new ValueTask<bool>(true);
        Func<int, CancellationToken, ValueTask> action = (obj, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await testObject.IfNotAsync(condition, action);

        // Assert
        Assert.IsFalse(actionExecuted);
    }

    ///<summary>
    ///Tests that IfNotAsync does not execute action when condition returns true asynchronously (slow path).
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ValueTask_ConditionTrueSlowPath_ActionNotExecuted()
    {
        // Arrange
        string testObject = "test";
        bool actionExecuted = false;
        Func<string, CancellationToken, ValueTask<bool>> condition = async (obj, ct) =>
        {
            await Task.Delay(10, ct);
            return true;
        };
        Func<string, CancellationToken, ValueTask> action = (obj, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await testObject.IfNotAsync(condition, action);

        // Assert
        Assert.IsFalse(actionExecuted);
    }

    ///<summary>
    ///Tests that IfNotAsync uses default cancellation token when none is provided.
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ValueTask_NoCancellationToken_UsesDefault()
    {
        // Arrange
        int testObject = 42;
        CancellationToken passedToken = CancellationToken.None;
        bool tokenChecked = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (obj, ct) =>
        {
            passedToken = ct;
            tokenChecked = true;
            return new ValueTask<bool>(true);
        };
        Func<int, CancellationToken, ValueTask> action = (obj, ct) => ValueTask.CompletedTask;

        // Act
        await testObject.IfNotAsync(condition, action);

        // Assert
        Assert.IsTrue(tokenChecked);
        Assert.AreEqual(default(CancellationToken), passedToken);
    }

    ///<summary>
    ///Tests that IfNotAsync passes the correct cancellation token to action when executed.
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ValueTask_PassesCorrectCancellationTokenToAction()
    {
        // Arrange
        int testObject = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken passedToken = default;
        Func<int, CancellationToken, ValueTask<bool>> condition = (obj, ct) => new ValueTask<bool>(false);
        Func<int, CancellationToken, ValueTask> action = (obj, ct) =>
        {
            passedToken = ct;
            return ValueTask.CompletedTask;
        };

        // Act
        await testObject.IfNotAsync(condition, action, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, passedToken);
    }

    ///<summary>
    ///Tests that IfNotAsync passes the correct cancellation token to condition.
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ValueTask_PassesCorrectCancellationTokenToCondition()
    {
        // Arrange
        int testObject = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken passedToken = default;
        Func<int, CancellationToken, ValueTask<bool>> condition = (obj, ct) =>
        {
            passedToken = ct;
            return new ValueTask<bool>(true);
        };
        Func<int, CancellationToken, ValueTask> action = (obj, ct) => ValueTask.CompletedTask;

        // Act
        await testObject.IfNotAsync(condition, action, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, passedToken);
    }

    ///<summary>
    ///Tests that IfNotAsync passes the correct object to condition function.
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ValueTask_PassesCorrectObjectToCondition()
    {
        // Arrange
        int testObject = 100;
        int passedToCondition = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (obj, ct) =>
        {
            passedToCondition = obj;
            return new ValueTask<bool>(true);
        };
        Func<int, CancellationToken, ValueTask> action = (obj, ct) => ValueTask.CompletedTask;

        // Act
        await testObject.IfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(testObject, passedToCondition);
    }

    ///<summary>
    ///Tests that IfNotAsync works correctly with reference types including null values.
    ///</summary>
    [TestMethod]
    [DataRow("test", true)]
    [DataRow("", true)]
    [DataRow(null, true)]
    public async Task IfNotAsync_ValueTask_ReferenceTypeVariations_WorksCorrectly(string? testValue, bool conditionResult)
    {
        // Arrange
        bool actionExecuted = false;
        Func<string?, CancellationToken, ValueTask<bool>> condition = (obj, ct) => new ValueTask<bool>(conditionResult);
        Func<string?, CancellationToken, ValueTask> action = (obj, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await testValue.IfNotAsync(condition, action);

        // Assert
        Assert.IsFalse(actionExecuted);
    }

    ///<summary>
    ///Tests that IfNotAsync works correctly with different value types.
    ///</summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(-1)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public async Task IfNotAsync_ValueTask_ValueTypeVariations_WorksCorrectly(int testValue)
    {
        // Arrange
        bool actionExecuted = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (obj, ct) => new ValueTask<bool>(obj < 0);
        Func<int, CancellationToken, ValueTask> action = (obj, ct) =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        await testValue.IfNotAsync(condition, action);

        // Assert
        bool expectedExecution = testValue >= 0;
        Assert.AreEqual(expectedExecution, actionExecuted);
    }

    ///<summary>
    ///Tests that IfNotAsync works with value types (int).
    ///</summary>
    [TestMethod]
    public async Task IfNotAsync_ValueType_WorksCorrectly()
    {
        // Arrange
        int obj = int.MaxValue;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) => Task.FromResult(false);
        bool actionExecuted = false;
        Task ActionFunc(int n, CancellationToken ct)
        {
            actionExecuted = true;
            return Task.CompletedTask;
        }

        // Act
        await obj.IfNotAsync(condition, ActionFunc);

        // Assert
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that ReturnIf can return null from action when condition is true.
    ///</summary>
    [TestMethod]
    public void ReturnIf_ActionReturnsNull_ReturnsNull()
    {
        // Arrange
        string input = "hello";
        Func<string?, bool> condition = static s => s == "hello";
        Func<string?, string?> action = static s => null;

        // Act
        string? result = input.ReturnIf(condition, action);

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that ReturnIf works correctly with boundary integer values.
    ///</summary>
    ///<param name="input">The input integer value to test.</param>
    ///<param name="conditionResult">Whether the condition should return true or false.</param>
    ///<param name="expectedResult">The expected result based on the condition.</param>
    [TestMethod]
    [DataRow(int.MinValue, true, 0)]
    [DataRow(int.MaxValue, true, 0)]
    [DataRow(0, true, 100)]
    [DataRow(int.MinValue, false, int.MinValue)]
    [DataRow(int.MaxValue, false, int.MaxValue)]
    [DataRow(0, false, 0)]
    public void ReturnIf_BoundaryIntegerValues_ReturnsExpectedResult(int input, bool conditionResult, int expectedResult)
    {
        // Arrange
        Func<int, bool> condition = x => conditionResult;
        Func<int, int> action = x => (x == 0) ? 100 : 0;

        // Act
        int result = input.ReturnIf(condition, action);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    ///<summary>
    ///Tests that ReturnIf does not call action when condition is false, verifying lazy evaluation.
    ///</summary>
    [TestMethod]
    public void ReturnIf_ConditionFalse_DoesNotCallAction()
    {
        // Arrange
        int input = 5;
        int conditionCallCount = 0;
        Func<int, bool> condition = x =>
        {
            conditionCallCount++;
            return false;
        };
        int actionCallCount = 0;
        Func<int, int> action = x =>
        {
            actionCallCount++;
            return 10;
        };

        // Act
        int result = input.ReturnIf(condition, action);

        // Assert
        Assert.AreEqual(5, result);
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(0, actionCallCount);
    }

    ///<summary>
    ///Tests that ReturnIf returns the original object when condition evaluates to false for an integer value.
    ///</summary>
    [TestMethod]
    public void ReturnIf_ConditionFalseWithInteger_ReturnsOriginalObject()
    {
        // Arrange
        int input = -5;
        Func<int, bool> condition = static x => x > 0;
        Func<int, int> action = static x => x * 2;

        // Act
        int result = input.ReturnIf(condition, action);

        // Assert
        Assert.AreEqual(-5, result);
    }

    ///<summary>
    ///Tests that ReturnIf returns the original string when condition evaluates to false.
    ///</summary>
    [TestMethod]
    public void ReturnIf_ConditionFalseWithString_ReturnsOriginalObject()
    {
        // Arrange
        string input = "hello";
        Func<string, bool> condition = static s => s.Length > 10;
        Func<string, string> action = static s => s.ToUpper();

        // Act
        string result = input.ReturnIf(condition, action);

        // Assert
        Assert.AreEqual("hello", result);
    }

    ///<summary>
    ///Tests that ReturnIf calls both condition and action when condition is true.
    ///</summary>
    [TestMethod]
    public void ReturnIf_ConditionTrue_CallsConditionAndAction()
    {
        // Arrange
        int input = 5;
        int conditionCallCount = 0;
        int? conditionReceivedValue = null;
        Func<int, bool> condition = x =>
        {
            conditionCallCount++;
            conditionReceivedValue = x;
            return true;
        };
        int actionCallCount = 0;
        int? actionReceivedValue = null;
        Func<int, int> action = x =>
        {
            actionCallCount++;
            actionReceivedValue = x;
            return 10;
        };

        // Act
        int result = input.ReturnIf(condition, action);

        // Assert
        Assert.AreEqual(10, result);
        Assert.AreEqual(1, conditionCallCount);
        Assert.AreEqual(5, conditionReceivedValue);
        Assert.AreEqual(1, actionCallCount);
        Assert.AreEqual(5, actionReceivedValue);
    }

    ///<summary>
    ///Tests that ReturnIf returns the result of action when condition evaluates to true for an integer value.
    ///</summary>
    [TestMethod]
    public void ReturnIf_ConditionTrueWithInteger_ReturnsActionResult()
    {
        // Arrange
        int input = 5;
        Func<int, bool> condition = static x => x > 0;
        Func<int, int> action = static x => x * 2;

        // Act
        int result = input.ReturnIf(condition, action);

        // Assert
        Assert.AreEqual(10, result);
    }

    ///<summary>
    ///Tests that ReturnIf returns the result of action when condition evaluates to true for a string value.
    ///</summary>
    [TestMethod]
    public void ReturnIf_ConditionTrueWithString_ReturnsActionResult()
    {
        // Arrange
        string input = "hello";
        Func<string, bool> condition = static s => s.Length > 0;
        Func<string, string> action = static s => s.ToUpper();

        // Act
        string result = input.ReturnIf(condition, action);

        // Assert
        Assert.AreEqual("HELLO", result);
    }

    ///<summary>
    ///Tests that ReturnIf works correctly with floating-point special values.
    ///</summary>
    ///<param name="input">The input double value to test.</param>
    ///<param name="conditionResult">Whether the condition should return true or false.</param>
    [TestMethod]
    [DataRow(double.NaN, false)]
    [DataRow(double.PositiveInfinity, true)]
    [DataRow(double.NegativeInfinity, true)]
    [DataRow(0.0, false)]
    [DataRow(-0.0, false)]
    public void ReturnIf_FloatingPointSpecialValues_ReturnsExpectedResult(double input, bool conditionResult)
    {
        // Arrange
        Func<double, bool> condition = static d => double.IsInfinity(d);
        Func<double, double> action = static d => 0.0;

        // Act
        double result = input.ReturnIf(condition, action);

        // Assert
        if (conditionResult)
        {
            Assert.AreEqual(0.0, result);
        }
        else if (double.IsNaN(input))
        {
            Assert.IsTrue(double.IsNaN(result));
        }
        else
        {
            Assert.AreEqual(input, result);
        }
    }

    ///<summary>
    ///Tests that ReturnIf does not call action when condition is false, even if action is null.
    ///</summary>
    [TestMethod]
    public void ReturnIf_NullActionAndConditionFalse_ReturnsOriginalWithoutException()
    {
        // Arrange
        int input = 5;
        Func<int, bool> condition = static x => false;
        Func<int, int> action = null!;

        // Act
        int result = input.ReturnIf(condition, action);

        // Assert
        Assert.AreEqual(5, result);
    }

    ///<summary>
    ///Tests that ReturnIf handles null object for reference types and passes it to condition.
    ///</summary>
    [TestMethod]
    public void ReturnIf_NullObjectConditionFalse_ReturnsNull()
    {
        // Arrange
        string? input = null;
        Func<string?, bool> condition = static s => (s != null) && (s.Length > 0);
        Func<string?, string?> action = static s => s?.ToUpper();

        // Act
        string? result = input.ReturnIf(condition, action);

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that ReturnIf works correctly with various string edge cases.
    ///</summary>
    ///<param name="input">The input string value to test.</param>
    ///<param name="shouldTransform">Whether the string should be transformed.</param>
    [TestMethod]
    [DataRow("", true)]
    [DataRow(" ", true)]
    [DataRow("   ", true)]
    [DataRow("a", false)]
    [DataRow("hello world", false)]
    public void ReturnIf_StringEdgeCases_ReturnsExpectedResult(string input, bool shouldTransform)
    {
        // Arrange
        Func<string, bool> condition = static s => string.IsNullOrWhiteSpace(s);
        Func<string, string> action = static s => "EMPTY";

        // Act
        string result = input.ReturnIf(condition, action);

        // Assert
        if (shouldTransform)
        {
            Assert.AreEqual("EMPTY", result);
        }
        else
        {
            Assert.AreEqual(input, result);
        }
    }

    ///<summary>
    ///Tests that ReturnIfAsync properly handles async action that completes asynchronously. Input: condition returning
    ///true, action with Task.Delay. Expected: Waits for action and returns correct result.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_AsyncAction_AwaitsCorrectly()
    {
        // Arrange
        int obj = 42;
        Func<int, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(true);
        Func<int, CancellationToken, Task<int>> action = static async (x, ct) =>
        {
            await Task.Delay(10, ct);
            return x * 2;
        };

        // Act
        int result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(84, result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync properly handles async condition that completes asynchronously. Input: condition with
    ///Task.Delay. Expected: Waits for condition and returns correct result.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_AsyncCondition_AwaitsCorrectly()
    {
        // Arrange
        int obj = 42;
        Func<int, CancellationToken, Task<bool>> condition = static async (x, ct) =>
        {
            await Task.Delay(10, ct);
            return x > 0;
        };
        Func<int, CancellationToken, Task<int>> action = static (x, ct) => Task.FromResult(x * 2);

        // Act
        int result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(84, result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync does not call action when condition returns false. Input: condition returning false.
    ///Expected: Action is not invoked.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ConditionFalse_DoesNotCallAction()
    {
        // Arrange
        int obj = 42;
        bool actionCalled = false;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) => Task.FromResult(false);
        Func<int, CancellationToken, Task<int>> action = (x, ct) =>
        {
            actionCalled = true;
            return Task.FromResult(x);
        };

        // Act
        await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.IsFalse(actionCalled);
    }

    ///<summary>
    ///Tests that ReturnIfAsync returns original object when condition returns false. Input: object value, condition
    ///returning false. Expected: Returns the original object unchanged.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ConditionFalse_ReturnsOriginalObject()
    {
        // Arrange
        int obj = 42;
        Func<int, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(x < 0);
        Func<int, CancellationToken, Task<int>> action = static (x, ct) => Task.FromResult(x * 2);

        // Act
        int result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(42, result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync properly passes the object to action function when condition is true. Input: specific
    ///object value, condition returning true. Expected: Action receives the correct object value.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ConditionTrue_PassesObjectToAction()
    {
        // Arrange
        int obj = 42;
        int? passedToAction = null;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) => Task.FromResult(true);
        Func<int, CancellationToken, Task<int>> action = (x, ct) =>
        {
            passedToAction = x;
            return Task.FromResult(x);
        };

        // Act
        await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(42, passedToAction);
    }

    ///<summary>
    ///Tests that ReturnIfAsync properly passes the object to condition function. Input: specific object value.
    ///Expected: Condition receives the correct object value.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ConditionTrue_PassesObjectToCondition()
    {
        // Arrange
        int obj = 42;
        int? passedToCondition = null;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            passedToCondition = x;
            return Task.FromResult(true);
        };
        Func<int, CancellationToken, Task<int>> action = (x, ct) => Task.FromResult(x);

        // Act
        await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(42, passedToCondition);
    }

    ///<summary>
    ///Tests that ReturnIfAsync returns action result when condition returns true. Input: object value, condition
    ///returning true, action doubling the value. Expected: Returns the doubled value from action.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ConditionTrue_ReturnsActionResult()
    {
        // Arrange
        int obj = 42;
        Func<int, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(x > 0);
        Func<int, CancellationToken, Task<int>> action = static (x, ct) => Task.FromResult(x * 2);

        // Act
        int result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(84, result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync works with default cancellation token. Input: no explicit cancellation token (uses
    ///default). Expected: Executes successfully.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_DefaultCancellationToken_WorksCorrectly()
    {
        // Arrange
        int obj = 42;
        Func<int, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(true);
        Func<int, CancellationToken, Task<int>> action = static (x, ct) => Task.FromResult(x * 2);

        // Act
        int result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(84, result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync works with double.NaN. Input: double.NaN. Expected: Returns correct result based on
    ///condition.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_DoubleNaN_WorksCorrectly()
    {
        // Arrange
        double obj = double.NaN;
        Func<double, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(double.IsNaN(x));
        Func<double, CancellationToken, Task<double>> action = static (x, ct) => Task.FromResult(0.0);

        // Act
        double result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(0.0, result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync works with double.NegativeInfinity. Input: double.NegativeInfinity. Expected: Returns
    ///correct result based on condition.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_DoubleNegativeInfinity_WorksCorrectly()
    {
        // Arrange
        double obj = double.NegativeInfinity;
        Func<double, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(double.IsNegativeInfinity(x));
        Func<double, CancellationToken, Task<double>> action = static (x, ct) => Task.FromResult(double.MinValue);

        // Act
        double result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(double.MinValue, result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync works with double.PositiveInfinity. Input: double.PositiveInfinity. Expected: Returns
    ///correct result based on condition.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_DoublePositiveInfinity_WorksCorrectly()
    {
        // Arrange
        double obj = double.PositiveInfinity;
        Func<double, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(double.IsInfinity(x));
        Func<double, CancellationToken, Task<double>> action = static (x, ct) => Task.FromResult(double.MaxValue);

        // Act
        double result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(double.MaxValue, result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync works with empty string. Input: empty string. Expected: Returns correct result based on
    ///condition.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_EmptyString_WorksCorrectly()
    {
        // Arrange
        string obj = string.Empty;
        Func<string, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(string.IsNullOrEmpty(x));
        Func<string, CancellationToken, Task<string>> action = static (x, ct) => Task.FromResult("default");

        // Act
        string result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual("default", result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync works with value type at int.MaxValue. Input: int.MaxValue. Expected: Returns correct
    ///result based on condition.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_IntMaxValue_WorksCorrectly()
    {
        // Arrange
        int obj = int.MaxValue;
        Func<int, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(x > 0);
        Func<int, CancellationToken, Task<int>> action = static (x, ct) => Task.FromResult(0);

        // Act
        int result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(0, result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync works with value type at int.MinValue. Input: int.MinValue. Expected: Returns correct
    ///result based on condition.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_IntMinValue_WorksCorrectly()
    {
        // Arrange
        int obj = int.MinValue;
        Func<int, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(x < 0);
        Func<int, CancellationToken, Task<int>> action = static (x, ct) => Task.FromResult(0);

        // Act
        int result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(0, result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync works with negative value. Input: negative integer. Expected: Returns correct result
    ///based on condition.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_NegativeValue_WorksCorrectly()
    {
        // Arrange
        int obj = -42;
        Func<int, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(x < 0);
        Func<int, CancellationToken, Task<int>> action = static (x, ct) => Task.FromResult(Math.Abs(x));

        // Act
        int result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(42, result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync works with reference type (non-null object). Input: non-null string object. Expected:
    ///Returns correct result based on condition.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_NonNullReferenceType_WorksCorrectly()
    {
        // Arrange
        string obj = "test";
        Func<string, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(x.Length > 3);
        Func<string, CancellationToken, Task<string>> action = static (x, ct) => Task.FromResult(x.ToUpper());

        // Act
        string result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual("TEST", result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync works with reference type (null object). Input: null string object. Expected: Returns
    ///correct result based on condition.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_NullReferenceType_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        Func<string?, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(x == null);
        Func<string?, CancellationToken, Task<string?>> action = static (x, ct) => Task.FromResult<string?>("replacement");

        // Act
        string? result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual("replacement", result);
    }

    ///<summary>
    ///Tests that ReturnIfAsync passes cancellation token to action function. Input: specific cancellation token,
    ///condition returning true. Expected: Action receives the same cancellation token.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_PassesCancellationTokenToAction()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken? passedToken = null;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) => Task.FromResult(true);
        Func<int, CancellationToken, Task<int>> action = (x, ct) =>
        {
            passedToken = ct;
            return Task.FromResult(x);
        };

        // Act
        await obj.ReturnIfAsync(condition, action, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, passedToken);
    }

    ///<summary>
    ///Tests that ReturnIfAsync passes cancellation token to condition function. Input: specific cancellation token.
    ///Expected: Condition receives the same cancellation token.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_PassesCancellationTokenToCondition()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken? passedToken = null;
        Func<int, CancellationToken, Task<bool>> condition = (x, ct) =>
        {
            passedToken = ct;
            return Task.FromResult(false);
        };
        Func<int, CancellationToken, Task<int>> action = (x, ct) => Task.FromResult(x);

        // Act
        await obj.ReturnIfAsync(condition, action, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, passedToken);
    }

    ///<summary>
    ///Verifies that ReturnIfAsync returns original object when condition returns false asynchronously.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ValueTask_ConditionFalseAsynchronously_ReturnsOriginalObject()
    {
        // Arrange
        int obj = 42;
        bool conditionCalled = false;
        bool actionCalled = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            conditionCalled = true;
            return false;
        };
        Func<int, CancellationToken, ValueTask<int>> action = (o, ct) =>
        {
            actionCalled = true;
            return ValueTask.FromResult(o * 2);
        };

        // Act
        int result = await obj.ReturnIfAsync(condition, action, CancellationToken.None);

        // Assert
        Assert.IsTrue(conditionCalled);
        Assert.IsFalse(actionCalled);
        Assert.AreEqual(42, result);
    }

    ///<summary>
    ///Verifies that ReturnIfAsync returns original object when condition returns false synchronously.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ValueTask_ConditionFalseSynchronously_ReturnsOriginalObject()
    {
        // Arrange
        int obj = 42;
        bool conditionCalled = false;
        bool actionCalled = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            conditionCalled = true;
            return ValueTask.FromResult(false);
        };
        Func<int, CancellationToken, ValueTask<int>> action = (o, ct) =>
        {
            actionCalled = true;
            return ValueTask.FromResult(o * 2);
        };

        // Act
        int result = await obj.ReturnIfAsync(condition, action, CancellationToken.None);

        // Assert
        Assert.IsTrue(conditionCalled);
        Assert.IsFalse(actionCalled);
        Assert.AreEqual(42, result);
    }

    ///<summary>
    ///Verifies that ReturnIfAsync executes action and returns its result when condition returns true asynchronously.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ValueTask_ConditionTrueAsynchronously_ExecutesActionAndReturnsResult()
    {
        // Arrange
        int obj = 42;
        bool conditionCalled = false;
        bool actionCalled = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            conditionCalled = true;
            return true;
        };
        Func<int, CancellationToken, ValueTask<int>> action = (o, ct) =>
        {
            actionCalled = true;
            return ValueTask.FromResult(o * 2);
        };

        // Act
        int result = await obj.ReturnIfAsync(condition, action, CancellationToken.None);

        // Assert
        Assert.IsTrue(conditionCalled);
        Assert.IsTrue(actionCalled);
        Assert.AreEqual(84, result);
    }

    ///<summary>
    ///Verifies that ReturnIfAsync executes action and returns its result when condition returns true synchronously.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ValueTask_ConditionTrueSynchronously_ExecutesActionAndReturnsResult()
    {
        // Arrange
        int obj = 42;
        bool conditionCalled = false;
        bool actionCalled = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            conditionCalled = true;
            return ValueTask.FromResult(true);
        };
        Func<int, CancellationToken, ValueTask<int>> action = (o, ct) =>
        {
            actionCalled = true;
            return ValueTask.FromResult(o * 2);
        };

        // Act
        int result = await obj.ReturnIfAsync(condition, action, CancellationToken.None);

        // Assert
        Assert.IsTrue(conditionCalled);
        Assert.IsTrue(actionCalled);
        Assert.AreEqual(84, result);
    }

    ///<summary>
    ///Verifies that ReturnIfAsync works with default cancellation token.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ValueTask_DefaultCancellationToken_WorksCorrectly()
    {
        // Arrange
        int obj = 42;
        Func<int, CancellationToken, ValueTask<bool>> condition = static (o, ct) => ValueTask.FromResult(true);
        Func<int, CancellationToken, ValueTask<int>> action = static (o, ct) => ValueTask.FromResult(o * 2);

        // Act
        int result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(84, result);
    }

    ///<summary>
    ///Verifies that ReturnIfAsync works correctly with extreme integer values.
    ///</summary>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(0)]
    public async Task ReturnIfAsync_ValueTask_ExtremeIntegerValues_WorksCorrectly(int value)
    {
        // Arrange
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) => ValueTask.FromResult(o == value);
        Func<int, CancellationToken, ValueTask<int>> action = (o, ct) => ValueTask.FromResult(o);

        // Act
        int result = await value.ReturnIfAsync(condition, action, CancellationToken.None);

        // Assert
        Assert.AreEqual(value, result);
    }

    ///<summary>
    ///Verifies that ReturnIfAsync works correctly with nullable value types.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ValueTask_NullableValueType_WorksCorrectly()
    {
        // Arrange
        int? obj = 42;
        Func<int?, CancellationToken, ValueTask<bool>> condition = static (o, ct) => ValueTask.FromResult(o.HasValue);
        Func<int?, CancellationToken, ValueTask<int?>> action = static (o, ct) => ValueTask.FromResult(o * 2);

        // Act
        int? result = await obj.ReturnIfAsync(condition, action, CancellationToken.None);

        // Assert
        Assert.AreEqual(84, result);
    }

    ///<summary>
    ///Verifies that ReturnIfAsync works correctly with null reference type when condition is false.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ValueTask_NullReferenceType_ConditionFalse_ReturnsNull()
    {
        // Arrange
        string? obj = null;
        Func<string?, CancellationToken, ValueTask<bool>> condition = static (o, ct) => ValueTask.FromResult(false);
        Func<string?, CancellationToken, ValueTask<string?>> action = static (o, ct) => ValueTask.FromResult<string?>("not null");

        // Act
        string? result = await obj.ReturnIfAsync(condition, action, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Verifies that ReturnIfAsync passes correct cancellation token to condition and action.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ValueTask_PassesCorrectCancellationTokenToFunctions()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken? conditionReceivedToken = null;
        CancellationToken? actionReceivedToken = null;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            conditionReceivedToken = ct;
            return ValueTask.FromResult(true);
        };
        Func<int, CancellationToken, ValueTask<int>> action = (o, ct) =>
        {
            actionReceivedToken = ct;
            return ValueTask.FromResult(o * 2);
        };

        // Act
        await obj.ReturnIfAsync(condition, action, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, conditionReceivedToken);
        Assert.AreEqual(cts.Token, actionReceivedToken);
    }

    ///<summary>
    ///Verifies that ReturnIfAsync passes correct object to condition and action.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ValueTask_PassesCorrectObjectToFunctions()
    {
        // Arrange
        int obj = 42;
        int? conditionReceivedObject = null;
        int? actionReceivedObject = null;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            conditionReceivedObject = o;
            return ValueTask.FromResult(true);
        };
        Func<int, CancellationToken, ValueTask<int>> action = (o, ct) =>
        {
            actionReceivedObject = o;
            return ValueTask.FromResult(o * 2);
        };

        // Act
        await obj.ReturnIfAsync(condition, action, CancellationToken.None);

        // Assert
        Assert.AreEqual(42, conditionReceivedObject);
        Assert.AreEqual(42, actionReceivedObject);
    }

    ///<summary>
    ///Verifies that ReturnIfAsync works correctly with reference types.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ValueTask_ReferenceType_WorksCorrectly()
    {
        // Arrange
        string obj = "hello";
        Func<string, CancellationToken, ValueTask<bool>> condition = static (o, ct) => ValueTask.FromResult(o.Length > 3);
        Func<string, CancellationToken, ValueTask<string>> action = static (o, ct) => ValueTask.FromResult(o.ToUpper());

        // Act
        string result = await obj.ReturnIfAsync(condition, action, CancellationToken.None);

        // Assert
        Assert.AreEqual("HELLO", result);
    }

    ///<summary>
    ///Verifies that ReturnIfAsync works correctly with various string edge cases.
    ///</summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("abc")]
    public async Task ReturnIfAsync_ValueTask_StringEdgeCases_WorksCorrectly(string value)
    {
        // Arrange
        Func<string, CancellationToken, ValueTask<bool>> condition = static (o, ct) => ValueTask.FromResult(!string.IsNullOrWhiteSpace(o));
        Func<string, CancellationToken, ValueTask<string>> action = static (o, ct) => ValueTask.FromResult(o.ToUpper());

        // Act
        string result = await value.ReturnIfAsync(condition, action, CancellationToken.None);

        // Assert
        if (!string.IsNullOrWhiteSpace(value))
        {
            Assert.AreEqual(value.ToUpper(), result);
        }
        else
        {
            Assert.AreEqual(value, result);
        }
    }

    ///<summary>
    ///Tests that ReturnIfAsync works with zero value. Input: zero integer. Expected: Returns correct result based on
    ///condition.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfAsync_ZeroValue_WorksCorrectly()
    {
        // Arrange
        int obj = 0;
        Func<int, CancellationToken, Task<bool>> condition = static (x, ct) => Task.FromResult(x == 0);
        Func<int, CancellationToken, Task<int>> action = static (x, ct) => Task.FromResult(1);

        // Act
        int result = await obj.ReturnIfAsync(condition, action);

        // Assert
        Assert.AreEqual(1, result);
    }

    ///<summary>
    ///Tests that ReturnIfElse does not invoke ifAction when condition is false. Input: Valid object with condition that
    ///returns false. Expected: Only elseAction is invoked, ifAction is not invoked.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_ConditionFalse_DoesNotInvokeIfAction()
    {
        // Arrange
        int obj = 3;
        bool ifActionInvoked = false;
        bool elseActionInvoked = false;
        Func<int, bool> condition = x => x > 5;
        Func<int, int> ifAction = x =>
        {
            ifActionInvoked = true;
            return x * 2;
        };
        Func<int, int> elseAction = x =>
        {
            elseActionInvoked = true;
            return x / 2;
        };

        // Act
        obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.IsFalse(ifActionInvoked);
        Assert.IsTrue(elseActionInvoked);
    }

    ///<summary>
    ///Tests that ReturnIfElse executes elseAction and returns its result when condition evaluates to false. Input:
    ///Integer value with condition that returns false. Expected: elseAction is executed and its result is returned.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_ConditionFalse_ReturnsElseActionResult()
    {
        // Arrange
        int obj = 3;
        Func<int, bool> condition = static x => x > 5;
        Func<int, int> ifAction = static x => x * 2;
        Func<int, int> elseAction = static x => x / 2;

        // Act
        int result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(1, result);
    }

    ///<summary>
    ///Tests that ReturnIfElse does not invoke elseAction when condition is true. Input: Valid object with condition
    ///that returns true. Expected: Only ifAction is invoked, elseAction is not invoked.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_ConditionTrue_DoesNotInvokeElseAction()
    {
        // Arrange
        int obj = 10;
        bool ifActionInvoked = false;
        bool elseActionInvoked = false;
        Func<int, bool> condition = x => x > 5;
        Func<int, int> ifAction = x =>
        {
            ifActionInvoked = true;
            return x * 2;
        };
        Func<int, int> elseAction = x =>
        {
            elseActionInvoked = true;
            return x / 2;
        };

        // Act
        obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.IsTrue(ifActionInvoked);
        Assert.IsFalse(elseActionInvoked);
    }

    ///<summary>
    ///Tests that ReturnIfElse executes ifAction and returns its result when condition evaluates to true. Input: Integer
    ///value with condition that returns true. Expected: ifAction is executed and its result is returned.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_ConditionTrue_ReturnsIfActionResult()
    {
        // Arrange
        int obj = 10;
        Func<int, bool> condition = static x => x > 5;
        Func<int, int> ifAction = static x => x * 2;
        Func<int, int> elseAction = static x => x / 2;

        // Act
        int result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(20, result);
    }

    ///<summary>
    ///Tests that ReturnIfElse works with double.NaN. Input: double.NaN with condition checking for NaN. Expected:
    ///ifAction is invoked for NaN value.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_DoubleNaN_WorksCorrectly()
    {
        // Arrange
        double obj = double.NaN;
        Func<double, bool> condition = static d => double.IsNaN(d);
        Func<double, double> ifAction = static d => 0.0;
        Func<double, double> elseAction = static d => d;

        // Act
        double result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(0.0, result);
    }

    ///<summary>
    ///Tests that ReturnIfElse works with double.NegativeInfinity. Input: double.NegativeInfinity with condition
    ///checking for negative infinity. Expected: ifAction is invoked for negative infinity value.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_DoubleNegativeInfinity_WorksCorrectly()
    {
        // Arrange
        double obj = double.NegativeInfinity;
        Func<double, bool> condition = static d => double.IsNegativeInfinity(d);
        Func<double, double> ifAction = static d => double.MinValue;
        Func<double, double> elseAction = static d => d;

        // Act
        double result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(double.MinValue, result);
    }

    ///<summary>
    ///Tests that ReturnIfElse works with double.PositiveInfinity. Input: double.PositiveInfinity with condition
    ///checking for infinity. Expected: ifAction is invoked for infinity value.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_DoublePositiveInfinity_WorksCorrectly()
    {
        // Arrange
        double obj = double.PositiveInfinity;
        Func<double, bool> condition = static d => double.IsInfinity(d);
        Func<double, double> ifAction = static d => double.MaxValue;
        Func<double, double> elseAction = static d => d;

        // Act
        double result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(double.MaxValue, result);
    }

    ///<summary>
    ///Tests that ReturnIfElse correctly handles actions that return null for reference types when condition is false.
    ///Input: String object with condition false and elseAction returning null. Expected: Null is returned.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_ElseActionReturnsNull_ReturnsNull()
    {
        // Arrange
        string obj = string.Empty;
        Func<string?, bool> condition = static s => s is { Length: > 0 };
        Func<string?, string?> ifAction = static s => s;
        Func<string?, string?> elseAction = static s => null;

        // Act
        string? result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that ReturnIfElse works with empty string. Input: Empty string with condition checking for empty. Expected:
    ///ifAction is invoked for empty string.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_EmptyString_WorksCorrectly()
    {
        // Arrange
        string obj = string.Empty;
        Func<string, bool> condition = static s => s.Length == 0;
        Func<string, string> ifAction = static s => "empty";
        Func<string, string> elseAction = static s => s;

        // Act
        string result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("empty", result);
    }

    ///<summary>
    ///Tests that ReturnIfElse correctly handles actions that return null for reference types when condition is true.
    ///Input: String object with condition true and ifAction returning null. Expected: Null is returned.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_IfActionReturnsNull_ReturnsNull()
    {
        // Arrange
        string obj = "test";
        Func<string?, bool> condition = static s => s is { Length: > 0 };
        Func<string?, string?> ifAction = static s => null;
        Func<string?, string?> elseAction = static s => s;

        // Act
        string? result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that ReturnIfElse works with boundary value int.MaxValue. Input: int.MaxValue with condition checking for
    ///positive values. Expected: ifAction is invoked and returns expected result.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_IntMaxValue_WorksCorrectly()
    {
        // Arrange
        int obj = int.MaxValue;
        Func<int, bool> condition = static x => x > 0;
        Func<int, int> ifAction = static x => 1;
        Func<int, int> elseAction = static x => x;

        // Act
        int result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(1, result);
    }

    ///<summary>
    ///Tests that ReturnIfElse works with boundary value int.MinValue. Input: int.MinValue with condition checking for
    ///negative values. Expected: ifAction is invoked and returns expected result.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_IntMinValue_WorksCorrectly()
    {
        // Arrange
        int obj = int.MinValue;
        Func<int, bool> condition = static x => x < 0;
        Func<int, int> ifAction = static x => 0;
        Func<int, int> elseAction = static x => x;

        // Act
        int result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(0, result);
    }

    ///<summary>
    ///Tests that ReturnIfElse handles null object for reference types correctly when condition is false. Input: Null
    ///string with condition that handles null and returns false. Expected: elseAction is executed with null and returns
    ///its result.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_NullObjectConditionFalse_ReturnsElseActionResult()
    {
        // Arrange
        string? obj = null;
        Func<string?, bool> condition = static s => s != null;
        Func<string?, string?> ifAction = static s => s;
        Func<string?, string?> elseAction = static s => "was null";

        // Act
        string? result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("was null", result);
    }

    ///<summary>
    ///Tests that ReturnIfElse handles null object for reference types correctly when condition is true. Input: Null
    ///string with condition that handles null and returns true. Expected: ifAction is executed with null and returns
    ///its result.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_NullObjectConditionTrue_ReturnsIfActionResult()
    {
        // Arrange
        string? obj = null;
        Func<string?, bool> condition = static s => s == null;
        Func<string?, string?> ifAction = static s => "null value";
        Func<string?, string?> elseAction = static s => s;

        // Act
        string? result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("null value", result);
    }

    ///<summary>
    ///Tests that ReturnIfElse works correctly with reference types when condition is false. Input: String value with
    ///condition that returns false. Expected: elseAction is executed and returns transformed string.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_ReferenceTypeConditionFalse_ReturnsElseActionResult()
    {
        // Arrange
        string obj = "hi";
        Func<string, bool> condition = static s => s.Length > 3;
        Func<string, string> ifAction = static s => s.ToUpper();
        Func<string, string> elseAction = static s => s.ToLower();

        // Act
        string result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("hi", result);
    }

    ///<summary>
    ///Tests that ReturnIfElse works correctly with reference types when condition is true. Input: String value with
    ///condition that returns true. Expected: ifAction is executed and returns transformed string.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_ReferenceTypeConditionTrue_ReturnsIfActionResult()
    {
        // Arrange
        string obj = "hello";
        Func<string, bool> condition = static s => s.Length > 3;
        Func<string, string> ifAction = static s => s.ToUpper();
        Func<string, string> elseAction = static s => s.ToLower();

        // Act
        string result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("HELLO", result);
    }

    ///<summary>
    ///Tests that ReturnIfElse works with whitespace-only string. Input: Whitespace string with condition checking for
    ///whitespace. Expected: Correct action is invoked based on condition.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_WhitespaceString_WorksCorrectly()
    {
        // Arrange
        string obj = "   ";
        Func<string, bool> condition = static s => string.IsNullOrWhiteSpace(s);
        Func<string, string> ifAction = static s => "whitespace";
        Func<string, string> elseAction = static s => s.Trim();

        // Act
        string result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("whitespace", result);
    }

    ///<summary>
    ///Tests that ReturnIfElse works with zero value. Input: Zero with condition that evaluates to false for zero.
    ///Expected: elseAction is invoked and returns expected result.
    ///</summary>
    [TestMethod]
    public void ReturnIfElse_ZeroValue_WorksCorrectly()
    {
        // Arrange
        int obj = 0;
        Func<int, bool> condition = static x => x > 0;
        Func<int, int> ifAction = static x => 1;
        Func<int, int> elseAction = static x => -1;

        // Act
        int result = obj.ReturnIfElse(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(-1, result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync only executes elseAction and not ifAction when condition is false.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_ConditionFalse_OnlyExecutesElseAction()
    {
        // Arrange
        int obj = 30;
        bool ifActionExecuted = false;
        bool elseActionExecuted = false;

        Func<int, CancellationToken, Task<bool>> condition = (o, ct) => Task.FromResult(false);

        Func<int, CancellationToken, Task<int>> ifAction = (o, ct) =>
        {
            ifActionExecuted = true;
            return Task.FromResult(o + 5);
        };

        Func<int, CancellationToken, Task<int>> elseAction = (o, ct) =>
        {
            elseActionExecuted = true;
            return Task.FromResult(o - 5);
        };

        // Act
        await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsFalse(ifActionExecuted);
        Assert.IsTrue(elseActionExecuted);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync executes and returns result from elseAction when condition returns false.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_ConditionFalse_ReturnsElseActionResult()
    {
        // Arrange
        int obj = 42;
        Func<int, CancellationToken, Task<bool>> condition = static (o, ct) => Task.FromResult(false);
        Func<int, CancellationToken, Task<int>> ifAction = static (o, ct) => Task.FromResult(o + 10);
        Func<int, CancellationToken, Task<int>> elseAction = static (o, ct) => Task.FromResult(o - 10);

        // Act
        int result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(32, result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync executes elseAction when condition returns false asynchronously (slow path).
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_ConditionFalseAsynchronous_ExecutesElseAction()
    {
        // Arrange
        int obj = 10;
        bool conditionCalled = false;
        bool ifActionCalled = false;
        bool elseActionCalled = false;

        Func<int, CancellationToken, ValueTask<bool>> condition = async (x, ct) =>
        {
            await Task.Delay(1, ct);
            conditionCalled = true;
            return false;
        };
        Func<int, CancellationToken, ValueTask<int>> ifAction = (x, ct) =>
        {
            ifActionCalled = true;
            return new ValueTask<int>(x * 2);
        };
        Func<int, CancellationToken, ValueTask<int>> elseAction = (x, ct) =>
        {
            elseActionCalled = true;
            return new ValueTask<int>(x * 3);
        };

        // Act
        int result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(30, result);
        Assert.IsTrue(conditionCalled);
        Assert.IsFalse(ifActionCalled);
        Assert.IsTrue(elseActionCalled);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync executes elseAction when condition returns false synchronously (fast path).
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_ConditionFalseSynchronous_ExecutesElseAction()
    {
        // Arrange
        int obj = 10;
        bool conditionCalled = false;
        bool ifActionCalled = false;
        bool elseActionCalled = false;

        Func<int, CancellationToken, ValueTask<bool>> condition = (x, ct) =>
        {
            conditionCalled = true;
            return new ValueTask<bool>(false);
        };
        Func<int, CancellationToken, ValueTask<int>> ifAction = (x, ct) =>
        {
            ifActionCalled = true;
            return new ValueTask<int>(x * 2);
        };
        Func<int, CancellationToken, ValueTask<int>> elseAction = (x, ct) =>
        {
            elseActionCalled = true;
            return new ValueTask<int>(x * 3);
        };

        // Act
        int result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(30, result);
        Assert.IsTrue(conditionCalled);
        Assert.IsFalse(ifActionCalled);
        Assert.IsTrue(elseActionCalled);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync only executes ifAction and not elseAction when condition is true.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_ConditionTrue_OnlyExecutesIfAction()
    {
        // Arrange
        int obj = 30;
        bool ifActionExecuted = false;
        bool elseActionExecuted = false;

        Func<int, CancellationToken, Task<bool>> condition = (o, ct) => Task.FromResult(true);

        Func<int, CancellationToken, Task<int>> ifAction = (o, ct) =>
        {
            ifActionExecuted = true;
            return Task.FromResult(o + 5);
        };

        Func<int, CancellationToken, Task<int>> elseAction = (o, ct) =>
        {
            elseActionExecuted = true;
            return Task.FromResult(o - 5);
        };

        // Act
        await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.IsTrue(ifActionExecuted);
        Assert.IsFalse(elseActionExecuted);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync executes and returns result from ifAction when condition returns true.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_ConditionTrue_ReturnsIfActionResult()
    {
        // Arrange
        int obj = 42;
        Func<int, CancellationToken, Task<bool>> condition = static (o, ct) => Task.FromResult(true);
        Func<int, CancellationToken, Task<int>> ifAction = static (o, ct) => Task.FromResult(o + 10);
        Func<int, CancellationToken, Task<int>> elseAction = static (o, ct) => Task.FromResult(o - 10);

        // Act
        int result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(52, result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync executes ifAction when condition returns true asynchronously (slow path).
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_ConditionTrueAsynchronous_ExecutesIfAction()
    {
        // Arrange
        int obj = 10;
        bool conditionCalled = false;
        bool ifActionCalled = false;
        bool elseActionCalled = false;

        Func<int, CancellationToken, ValueTask<bool>> condition = async (x, ct) =>
        {
            await Task.Delay(1, ct);
            conditionCalled = true;
            return true;
        };
        Func<int, CancellationToken, ValueTask<int>> ifAction = (x, ct) =>
        {
            ifActionCalled = true;
            return new ValueTask<int>(x * 2);
        };
        Func<int, CancellationToken, ValueTask<int>> elseAction = (x, ct) =>
        {
            elseActionCalled = true;
            return new ValueTask<int>(x * 3);
        };

        // Act
        int result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(20, result);
        Assert.IsTrue(conditionCalled);
        Assert.IsTrue(ifActionCalled);
        Assert.IsFalse(elseActionCalled);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync executes ifAction when condition returns true synchronously (fast path).
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_ConditionTrueSynchronous_ExecutesIfAction()
    {
        // Arrange
        int obj = 10;
        bool conditionCalled = false;
        bool ifActionCalled = false;
        bool elseActionCalled = false;

        Func<int, CancellationToken, ValueTask<bool>> condition = (x, ct) =>
        {
            conditionCalled = true;
            return new ValueTask<bool>(true);
        };
        Func<int, CancellationToken, ValueTask<int>> ifAction = (x, ct) =>
        {
            ifActionCalled = true;
            return new ValueTask<int>(x * 2);
        };
        Func<int, CancellationToken, ValueTask<int>> elseAction = (x, ct) =>
        {
            elseActionCalled = true;
            return new ValueTask<int>(x * 3);
        };

        // Act
        int result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(20, result);
        Assert.IsTrue(conditionCalled);
        Assert.IsTrue(ifActionCalled);
        Assert.IsFalse(elseActionCalled);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync works with default cancellation token.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_DefaultCancellationToken_WorksCorrectly()
    {
        // Arrange
        int obj = 25;
        Func<int, CancellationToken, Task<bool>> condition = static (o, ct) => Task.FromResult(o > 20);
        Func<int, CancellationToken, Task<int>> ifAction = static (o, ct) => Task.FromResult(o * 2);
        Func<int, CancellationToken, Task<int>> elseAction = static (o, ct) => Task.FromResult(o / 2);

        // Act
        int result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(50, result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync works with empty and whitespace strings.
    ///</summary>
    [TestMethod]
    [DataRow("", true, "empty")]
    [DataRow("", false, "not empty")]
    [DataRow("   ", true, "whitespace")]
    [DataRow("   ", false, "not whitespace")]
    public async Task ReturnIfElseAsync_EmptyAndWhitespaceStrings_ReturnsCorrectResult(string input, bool conditionResult, string expected)
    {
        // Arrange
        Func<string, CancellationToken, Task<bool>> condition = (s, ct) => Task.FromResult(conditionResult);
        Func<string, CancellationToken, Task<string>> ifAction = (s, ct) => Task.FromResult(expected);
        Func<string, CancellationToken, Task<string>> elseAction = (s, ct) => Task.FromResult(expected);

        // Act
        string result = await input.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync works with boundary values for integers.
    ///</summary>
    [TestMethod]
    [DataRow(int.MinValue, true, int.MaxValue)]
    [DataRow(int.MaxValue, false, int.MinValue)]
    [DataRow(0, true, 1)]
    [DataRow(0, false, -1)]
    public async Task ReturnIfElseAsync_IntegerBoundaryValues_ReturnsCorrectResult(int input, bool conditionResult, int expected)
    {
        // Arrange
        Func<int, CancellationToken, Task<bool>> condition = (i, ct) => Task.FromResult(conditionResult);
        Func<int, CancellationToken, Task<int>> ifAction = (i, ct) => Task.FromResult(expected);
        Func<int, CancellationToken, Task<int>> elseAction = (i, ct) => Task.FromResult(expected);

        // Act
        int result = await input.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync works with null object when T is a reference type.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_NullObject_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        Func<string?, CancellationToken, Task<bool>> condition = static (s, ct) => Task.FromResult(s is null);
        Func<string?, CancellationToken, Task<string?>> ifAction = static (s, ct) => Task.FromResult<string?>("was null");
        Func<string?, CancellationToken, Task<string?>> elseAction = static (s, ct) => Task.FromResult(s);

        // Act
        string? result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("was null", result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync correctly passes cancellation token to condition and action.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_PassesCancellationToken_ToConditionAndAction()
    {
        // Arrange
        int obj = 5;
        CancellationToken conditionToken = default;
        CancellationToken actionToken = default;
        CancellationTokenSource cts = new CancellationTokenSource();

        Func<int, CancellationToken, ValueTask<bool>> condition = (x, ct) =>
        {
            conditionToken = ct;
            return new ValueTask<bool>(true);
        };
        Func<int, CancellationToken, ValueTask<int>> ifAction = (x, ct) =>
        {
            actionToken = ct;
            return new ValueTask<int>(x * 2);
        };
        Func<int, CancellationToken, ValueTask<int>> elseAction = (x, ct) => new ValueTask<int>(x * 3);

        // Act
        await obj.ReturnIfElseAsync(condition, ifAction, elseAction, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, conditionToken);
        Assert.AreEqual(cts.Token, actionToken);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync passes the correct cancellation token to all functions.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_PassesCancellationTokenToFunctions()
    {
        // Arrange
        int obj = 50;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken receivedInCondition = default;
        CancellationToken receivedInAction = default;

        Func<int, CancellationToken, Task<bool>> condition = (o, ct) =>
        {
            receivedInCondition = ct;
            return Task.FromResult(false);
        };

        Func<int, CancellationToken, Task<int>> ifAction = (o, ct) => Task.FromResult(o * 2);

        Func<int, CancellationToken, Task<int>> elseAction = (o, ct) =>
        {
            receivedInAction = ct;
            return Task.FromResult(o / 2);
        };

        // Act
        await obj.ReturnIfElseAsync(condition, ifAction, elseAction, cts.Token);

        // Assert
        Assert.AreEqual(cts.Token, receivedInCondition);
        Assert.AreEqual(cts.Token, receivedInAction);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync passes the correct object to condition, and the selected action.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_PassesCorrectObjectToFunctions()
    {
        // Arrange
        int obj = 100;
        int receivedInCondition = 0;
        int receivedInAction = 0;

        Func<int, CancellationToken, Task<bool>> condition = (o, ct) =>
        {
            receivedInCondition = o;
            return Task.FromResult(true);
        };

        Func<int, CancellationToken, Task<int>> ifAction = (o, ct) =>
        {
            receivedInAction = o;
            return Task.FromResult(o * 2);
        };

        Func<int, CancellationToken, Task<int>> elseAction = (o, ct) => Task.FromResult(o / 2);

        // Act
        await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(100, receivedInCondition);
        Assert.AreEqual(100, receivedInAction);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync correctly passes the object to condition and selected action.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_PassesObjectCorrectly_ToConditionAndActions()
    {
        // Arrange
        string obj = "test";
        string? conditionReceived = null;
        string? actionReceived = null;

        Func<string, CancellationToken, ValueTask<bool>> condition = (x, ct) =>
        {
            conditionReceived = x;
            return new ValueTask<bool>(x.Length > 3);
        };
        Func<string, CancellationToken, ValueTask<string>> ifAction = (x, ct) =>
        {
            actionReceived = x;
            return new ValueTask<string>(x.ToUpper());
        };
        Func<string, CancellationToken, ValueTask<string>> elseAction = (x, ct) =>
        {
            actionReceived = x;
            return new ValueTask<string>(x.ToLower());
        };

        // Act
        string result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("TEST", result);
        Assert.AreEqual(obj, conditionReceived);
        Assert.AreEqual(obj, actionReceived);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync works with string type.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_StringType_ReturnsCorrectResult()
    {
        // Arrange
        string obj = "test";
        Func<string, CancellationToken, Task<bool>> condition = static (s, ct) => Task.FromResult(s.Length > 5);
        Func<string, CancellationToken, Task<string>> ifAction = static (s, ct) => Task.FromResult(s.ToUpper());
        Func<string, CancellationToken, Task<string>> elseAction = static (s, ct) => Task.FromResult(s.ToLower());

        // Act
        string result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("test", result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync with all async operations (slow path) works correctly.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_WithAllAsyncOperations_WorksCorrectly()
    {
        // Arrange
        int obj = 50;
        Func<int, CancellationToken, ValueTask<bool>> condition = static async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x >= 50;
        };
        Func<int, CancellationToken, ValueTask<int>> ifAction = static async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x + 10;
        };
        Func<int, CancellationToken, ValueTask<int>> elseAction = static async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x - 10;
        };

        // Act
        int result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(60, result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync with async elseAction that returns different value works correctly.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_WithAsyncElseAction_WorksCorrectly()
    {
        // Arrange
        int obj = 100;
        Func<int, CancellationToken, ValueTask<bool>> condition = static (x, ct) => new ValueTask<bool>(false);
        Func<int, CancellationToken, ValueTask<int>> ifAction = static (x, ct) => new ValueTask<int>(x / 2);
        Func<int, CancellationToken, ValueTask<int>> elseAction = static async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        };

        // Act
        int result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(200, result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync with async ifAction that returns different value works correctly.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_WithAsyncIfAction_WorksCorrectly()
    {
        // Arrange
        int obj = 100;
        Func<int, CancellationToken, ValueTask<bool>> condition = static (x, ct) => new ValueTask<bool>(true);
        Func<int, CancellationToken, ValueTask<int>> ifAction = static async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x / 2;
        };
        Func<int, CancellationToken, ValueTask<int>> elseAction = static (x, ct) => new ValueTask<int>(x * 2);

        // Act
        int result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(50, result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync works with default cancellation token.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_WithDefaultCancellationToken_WorksCorrectly()
    {
        // Arrange
        int obj = 7;
        Func<int, CancellationToken, ValueTask<bool>> condition = static (x, ct) => new ValueTask<bool>(x > 5);
        Func<int, CancellationToken, ValueTask<int>> ifAction = static (x, ct) => new ValueTask<int>(x * 10);
        Func<int, CancellationToken, ValueTask<int>> elseAction = static (x, ct) => new ValueTask<int>(x * 5);

        // Act
        int result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(70, result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync with empty string works correctly.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_WithEmptyString_WorksCorrectly()
    {
        // Arrange
        string obj = string.Empty;
        Func<string, CancellationToken, ValueTask<bool>> condition = static (x, ct) => new ValueTask<bool>(string.IsNullOrEmpty(x));
        Func<string, CancellationToken, ValueTask<string>> ifAction = static (x, ct) => new ValueTask<string>("empty");
        Func<string, CancellationToken, ValueTask<string>> elseAction = static (x, ct) => new ValueTask<string>(x);

        // Act
        string result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("empty", result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync works with null object (reference type).
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_WithNullObject_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        Func<string?, CancellationToken, ValueTask<bool>> condition = static (x, ct) => new ValueTask<bool>(x == null);
        Func<string?, CancellationToken, ValueTask<string?>> ifAction = static (x, ct) => new ValueTask<string?>("was null");
        Func<string?, CancellationToken, ValueTask<string?>> elseAction = static (x, ct) => new ValueTask<string?>(x);

        // Act
        string? result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("was null", result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync works with reference types (strings).
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_WithReferenceType_WorksCorrectly()
    {
        // Arrange
        string obj = "hello";
        Func<string, CancellationToken, ValueTask<bool>> condition = static (x, ct) => new ValueTask<bool>(x.StartsWith("h"));
        Func<string, CancellationToken, ValueTask<string>> ifAction = static (x, ct) => new ValueTask<string>($"{x} world");
        Func<string, CancellationToken, ValueTask<string>> elseAction = static (x, ct) => new ValueTask<string>($"{x} there");

        // Act
        string result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("hello world", result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync works with value types at boundaries (int.MinValue, int.MaxValue, 0).
    ///</summary>
    [TestMethod]
    [DataRow(int.MinValue, true, int.MinValue + 1)]
    [DataRow(int.MaxValue, true, int.MaxValue)]
    [DataRow(0, false, 1)]
    [DataRow(-1, false, 0)]
    public async Task ReturnIfElseAsync_WithValueTypeBoundaries_WorksCorrectly(int input, bool conditionResult, int expected)
    {
        // Arrange
        Func<int, CancellationToken, ValueTask<bool>> condition = (x, ct) => new ValueTask<bool>(conditionResult);
        Func<int, CancellationToken, ValueTask<int>> ifAction = (x, ct) => new ValueTask<int>((x == int.MaxValue) ? x : (x + 1));
        Func<int, CancellationToken, ValueTask<int>> elseAction = (x, ct) => new ValueTask<int>(x + 1);

        // Act
        int result = await input.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that ReturnIfElseAsync with whitespace-only string works correctly.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfElseAsync_WithWhitespaceString_WorksCorrectly()
    {
        // Arrange
        string obj = "   ";
        Func<string, CancellationToken, ValueTask<bool>> condition = static (x, ct) => new ValueTask<bool>(string.IsNullOrWhiteSpace(x));
        Func<string, CancellationToken, ValueTask<string>> ifAction = static (x, ct) => new ValueTask<string>("trimmed");
        Func<string, CancellationToken, ValueTask<string>> elseAction = static (x, ct) => new ValueTask<string>(x.Trim());

        // Act
        string result = await obj.ReturnIfElseAsync(condition, ifAction, elseAction);

        // Assert
        Assert.AreEqual("trimmed", result);
    }

    ///<summary>
    ///Tests ReturnIfNot with boolean value type. Input: false, condition true, expected: original value.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_BooleanFalse_ReturnsOriginalWhenConditionTrue()
    {
        // Arrange
        bool input = false;

        // Act
        bool result = input.ReturnIfNot(static x => !x, static x => true);

        // Assert
        Assert.AreEqual(input, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with boolean value type. Input: true, condition false, expected: action result.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_BooleanTrue_ReturnsActionResultWhenConditionFalse()
    {
        // Arrange
        bool input = true;
        bool expected = false;

        // Act
        bool result = input.ReturnIfNot(x => !x, x => expected);

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that ReturnIfNot calls action when condition is false.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_ConditionFalse_CallsAction()
    {
        // Arrange
        int input = 10;
        bool actionCalled = false;

        // Act
        int result = input.ReturnIfNot(
                     x => x > 100,
                     x =>
                     {
                         actionCalled = true;
                         return x * 2;
                     });

        // Assert
        Assert.IsTrue(actionCalled);
        Assert.AreEqual(20, result);
    }

    ///<summary>
    ///Tests that ReturnIfNot returns the result of action when condition evaluates to false.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_ConditionFalse_ReturnsActionResult()
    {
        // Arrange
        int input = 10;
        int expected = 20;

        // Act
        int result = input.ReturnIfNot(static x => x > 100, static x => x * 2);

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that ReturnIfNot does not call action when condition is true.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_ConditionTrue_DoesNotCallAction()
    {
        // Arrange
        int input = 150;
        bool actionCalled = false;

        // Act
        int result = input.ReturnIfNot(
                     x => x > 100,
                     x =>
                     {
                         actionCalled = true;
                         return x * 2;
                     });

        // Assert
        Assert.IsFalse(actionCalled);
        Assert.AreEqual(input, result);
    }

    ///<summary>
    ///Tests that ReturnIfNot returns the original object when condition evaluates to true.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_ConditionTrue_ReturnsOriginalObject()
    {
        // Arrange
        int input = 150;

        // Act
        int result = input.ReturnIfNot(static x => x > 100, static x => x * 2);

        // Assert
        Assert.AreEqual(input, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with double.NaN. Input: double.NaN, condition checks if not NaN, expected: action result.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_DoubleNaN_ReturnsActionResultWhenConditionFalse()
    {
        // Arrange
        double input = double.NaN;
        double expected = 0.0;

        // Act
        double result = input.ReturnIfNot(x => !double.IsNaN(x), x => expected);

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with double.NegativeInfinity. Input: double.NegativeInfinity, condition false, expected: action
    ///result.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_DoubleNegativeInfinity_ReturnsActionResultWhenConditionFalse()
    {
        // Arrange
        double input = double.NegativeInfinity;
        double expected = -1.0;

        // Act
        double result = input.ReturnIfNot(x => x > 0, x => expected);

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with double.PositiveInfinity. Input: double.PositiveInfinity, condition true, expected:
    ///original value.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_DoublePositiveInfinity_ReturnsOriginalWhenConditionTrue()
    {
        // Arrange
        double input = double.PositiveInfinity;

        // Act
        double result = input.ReturnIfNot(static x => double.IsInfinity(x), static x => 0.0);

        // Assert
        Assert.AreEqual(input, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with string edge cases: empty string. Input: empty string, condition checks if length > 5,
    ///expected: action result.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_EmptyString_ReturnsActionResult()
    {
        // Arrange
        string input = string.Empty;
        string expected = "filled";

        // Act
        string result = input.ReturnIfNot(x => x.Length > 5, x => expected);

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with numeric boundary: int.MaxValue. Input: int.MaxValue, condition true, expected: original
    ///value.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_IntMaxValue_ReturnsOriginalWhenConditionTrue()
    {
        // Arrange
        int input = int.MaxValue;

        // Act
        int result = input.ReturnIfNot(static x => x > 0, static x => -1);

        // Assert
        Assert.AreEqual(input, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with numeric boundary: int.MinValue. Input: int.MinValue, condition false, expected: action
    ///result.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_IntMinValue_ReturnsActionResultWhenConditionFalse()
    {
        // Arrange
        int input = int.MinValue;
        int expected = 0;

        // Act
        int result = input.ReturnIfNot(x => x > 0, x => expected);

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with negative integer value. Input: negative number, condition false, expected: action result.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_NegativeInteger_ReturnsActionResultWhenConditionFalse()
    {
        // Arrange
        int input = -42;
        int expected = 42;

        // Act
        int result = input.ReturnIfNot(static x => x > 0, static x => Math.Abs(x));

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that ReturnIfNot does not throw when action is null but condition is true (action not invoked).
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_NullActionAndConditionTrue_ReturnsOriginalObject()
    {
        // Arrange
        int input = 150;
        Func<int, int>? action = null;

        // Act
        int result = input.ReturnIfNot(static x => x > 100, action!);

        // Assert
        Assert.AreEqual(input, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with null reference type object and condition returning false.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_NullObjectAndConditionFalse_ReturnsActionResult()
    {
        // Arrange
        string? input = null;
        string expected = "default";

        // Act
        string? result = input.ReturnIfNot(x => x != null, x => expected);

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with null reference type object and condition returning true.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_NullObjectAndConditionTrue_ReturnsNull()
    {
        // Arrange
        string? input = null;

        // Act
        string? result = input.ReturnIfNot(static x => x == null, static x => "default");

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests ReturnIfNot with reference type: custom object. Verifies that action result is returned when condition is
    ///false.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_ReferenceType_ReturnsActionResultWhenConditionFalse()
    {
        // Arrange
        object input = new object();
        object expected = new object();

        // Act
        object result = input.ReturnIfNot(x => x == null, x => expected);

        // Assert
        Assert.AreSame(expected, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with reference type: custom object. Verifies that the same reference is returned when condition
    ///is true.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_ReferenceType_ReturnsSameReferenceWhenConditionTrue()
    {
        // Arrange
        object input = new object();

        // Act
        object result = input.ReturnIfNot(static x => x != null, static x => new object());

        // Assert
        Assert.AreSame(input, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with whitespace-only string. Input: whitespace string, condition checks if not whitespace,
    ///expected: original.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_WhitespaceString_ReturnsOriginalWhenConditionTrue()
    {
        // Arrange
        string input = "   ";

        // Act
        string result = input.ReturnIfNot(static x => string.IsNullOrWhiteSpace(x), static x => "replaced");

        // Assert
        Assert.AreEqual(input, result);
    }

    ///<summary>
    ///Tests ReturnIfNot with zero value. Input: 0, condition checks if non-zero, expected: action result.
    ///</summary>
    [TestMethod]
    public void ReturnIfNot_Zero_ReturnsActionResultWhenConditionFalse()
    {
        // Arrange
        int input = 0;
        int expected = 1;

        // Act
        int result = input.ReturnIfNot(x => x != 0, x => expected);

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync invokes action and returns its result when asynchronously completed condition returns
    ///false.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_AsyncConditionFalse_InvokesActionAndReturnsResult()
    {
        // Arrange
        string obj = "test";
        bool conditionCalled = false;
        bool actionCalled = false;
        Func<string, CancellationToken, ValueTask<bool>> condition = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            conditionCalled = true;
            return false;
        };
        Func<string, CancellationToken, ValueTask<string>> action = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            actionCalled = true;
            return $"{o}_modified";
        };

        // Act
        string result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual("test_modified", result);
        Assert.IsTrue(conditionCalled);
        Assert.IsTrue(actionCalled);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync returns original object when asynchronously completed condition returns true.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_AsyncConditionTrue_ReturnsOriginalObject()
    {
        // Arrange
        string obj = "test";
        bool conditionCalled = false;
        bool actionCalled = false;
        Func<string, CancellationToken, ValueTask<bool>> condition = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            conditionCalled = true;
            return true;
        };
        Func<string, CancellationToken, ValueTask<string>> action = async (o, ct) =>
        {
            await Task.Delay(10, ct);
            actionCalled = true;
            return $"{o}_modified";
        };

        // Act
        string result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(conditionCalled);
        Assert.IsFalse(actionCalled);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync returns action result when condition returns false. Input: condition that returns
    ///false, action that returns modified value. Expected: Action is called and its result is returned.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_ConditionReturnsFalse_ReturnsActionResult()
    {
        // Arrange
        int obj = 42;
        bool actionCalled = false;
        Func<int, CancellationToken, Task<bool>> condition = (o, ct) => Task.FromResult(false);
        Func<int, CancellationToken, Task<int>> action = (o, ct) =>
        {
            actionCalled = true;
            return Task.FromResult(o * 2);
        };

        // Act
        int result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(84, result);
        Assert.IsTrue(actionCalled, "Action should be called when condition returns false");
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync returns original object when condition returns true. Input: condition that returns
    ///true, action that would modify the value. Expected: Original object is returned and action is not called.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_ConditionReturnsTrue_ReturnsOriginalObject()
    {
        // Arrange
        int obj = 42;
        bool actionCalled = false;
        Func<int, CancellationToken, Task<bool>> condition = (o, ct) => Task.FromResult(true);
        Func<int, CancellationToken, Task<int>> action = (o, ct) =>
        {
            actionCalled = true;
            return Task.FromResult(o * 2);
        };

        // Act
        int result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(42, result);
        Assert.IsFalse(actionCalled, "Action should not be called when condition returns true");
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync works with default cancellation token. Input: no explicit cancellation token provided
    ///(uses default). Expected: Method executes successfully with default token.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_DefaultCancellationToken_ExecutesSuccessfully()
    {
        // Arrange
        int obj = 42;
        Func<int, CancellationToken, Task<bool>> condition = static (o, ct) => Task.FromResult(false);
        Func<int, CancellationToken, Task<int>> action = static (o, ct) => Task.FromResult(o * 2);

        // Act
        int result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(84, result);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync with empty and whitespace strings.
    ///</summary>
    ///<param name="value">The string value to test.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    public async Task ReturnIfNotAsync_EmptyOrWhitespaceString_WorksCorrectly(string value)
    {
        // Arrange
        Func<string, CancellationToken, ValueTask<bool>> condition = static (o, ct) => ValueTask.FromResult(string.IsNullOrWhiteSpace(o));
        Func<string, CancellationToken, ValueTask<string>> action = static (o, ct) => ValueTask.FromResult("default");

        // Act
        string result = await value.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(value, result);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync works with empty string. Input: empty string, condition returns false, action returns
    ///non-empty string. Expected: Action result is returned.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_EmptyString_ReturnsActionResult()
    {
        // Arrange
        string obj = string.Empty;
        Func<string, CancellationToken, Task<bool>> condition = static (o, ct) => Task.FromResult(false);
        Func<string, CancellationToken, Task<string>> action = static (o, ct) => Task.FromResult("non-empty");

        // Act
        string result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual("non-empty", result);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync returns original object for edge case with int.MaxValue. Input: int.MaxValue,
    ///condition returns true. Expected: Original int.MaxValue is returned.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_IntMaxValue_ReturnsOriginalObject()
    {
        // Arrange
        int obj = int.MaxValue;
        Func<int, CancellationToken, Task<bool>> condition = static (o, ct) => Task.FromResult(true);
        Func<int, CancellationToken, Task<int>> action = static (o, ct) => Task.FromResult(0);

        // Act
        int result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(int.MaxValue, result);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync returns original object for edge case with int.MinValue. Input: int.MinValue,
    ///condition returns true. Expected: Original int.MinValue is returned.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_IntMinValue_ReturnsOriginalObject()
    {
        // Arrange
        int obj = int.MinValue;
        Func<int, CancellationToken, Task<bool>> condition = static (o, ct) => Task.FromResult(true);
        Func<int, CancellationToken, Task<int>> action = static (o, ct) => Task.FromResult(0);

        // Act
        int result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(int.MinValue, result);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync works with nullable reference types. Input: nullable string object, condition returns
    ///false, action returns non-null string. Expected: Action result is returned.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_NullableReferenceType_ReturnsActionResult()
    {
        // Arrange
        string? obj = null;
        Func<string?, CancellationToken, Task<bool>> condition = static (o, ct) => Task.FromResult(false);
        Func<string?, CancellationToken, Task<string?>> action = static (o, ct) => Task.FromResult<string?>("replacement");

        // Act
        string? result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual("replacement", result);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync works correctly with reference types (null object).
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_NullObjectConditionFalse_InvokesAction()
    {
        // Arrange
        string? obj = null;
        Func<string?, CancellationToken, ValueTask<bool>> condition = static (o, ct) => ValueTask.FromResult(false);
        Func<string?, CancellationToken, ValueTask<string?>> action = static (o, ct) => ValueTask.FromResult<string?>("replacement");

        // Act
        string? result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual("replacement", result);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync passes correct parameters to action function. Input: obj and cancellation token,
    ///condition returns false. Expected: Action receives the correct object and cancellation token.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_PassesCorrectParametersToAction()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        int? receivedObj = null;
        CancellationToken? receivedToken = null;

        Func<int, CancellationToken, Task<bool>> condition = (o, ct) => Task.FromResult(false);
        Func<int, CancellationToken, Task<int>> action = (o, ct) =>
        {
            receivedObj = o;
            receivedToken = ct;
            return Task.FromResult(o * 2);
        };

        // Act
        await obj.ReturnIfNotAsync(condition, action, cts.Token);

        // Assert
        Assert.AreEqual(42, receivedObj);
        Assert.AreEqual(cts.Token, receivedToken);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync passes correct parameters to condition function. Input: obj and cancellation token.
    ///Expected: Condition receives the correct object and cancellation token.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_PassesCorrectParametersToCondition()
    {
        // Arrange
        int obj = 42;
        CancellationTokenSource cts = new CancellationTokenSource();
        int? receivedObj = null;
        CancellationToken? receivedToken = null;

        Func<int, CancellationToken, Task<bool>> condition = (o, ct) =>
        {
            receivedObj = o;
            receivedToken = ct;
            return Task.FromResult(true);
        };
        Func<int, CancellationToken, Task<int>> action = (o, ct) => Task.FromResult(o * 2);

        // Act
        await obj.ReturnIfNotAsync(condition, action, cts.Token);

        // Assert
        Assert.AreEqual(42, receivedObj);
        Assert.AreEqual(cts.Token, receivedToken);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync correctly passes object to both condition and action.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_PassesObjectToConditionAndAction_Correctly()
    {
        // Arrange
        int obj = 99;
        int conditionReceivedValue = 0;
        int actionReceivedValue = 0;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            conditionReceivedValue = o;
            return ValueTask.FromResult(false);
        };
        Func<int, CancellationToken, ValueTask<int>> action = (o, ct) =>
        {
            actionReceivedValue = o;
            return ValueTask.FromResult(o + 1);
        };

        // Act
        await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(obj, conditionReceivedValue);
        Assert.AreEqual(obj, actionReceivedValue);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync works with reference types. Input: string object, condition returns false, action
    ///returns modified string. Expected: Action result is returned.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_ReferenceType_ReturnsActionResult()
    {
        // Arrange
        string obj = "test";
        Func<string, CancellationToken, Task<bool>> condition = static (o, ct) => Task.FromResult(false);
        Func<string, CancellationToken, Task<string>> action = static (o, ct) => Task.FromResult(o.ToUpper());

        // Act
        string result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual("TEST", result);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync with special double values (NaN, Infinity).
    ///</summary>
    ///<param name="value">The special double value.</param>
    [TestMethod]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    public async Task ReturnIfNotAsync_SpecialDoubleValues_HandlesCorrectly(double value)
    {
        // Arrange
        Func<double, CancellationToken, ValueTask<bool>> condition = static (o, ct) => ValueTask.FromResult(false);
        Func<double, CancellationToken, ValueTask<double>> action = static (o, ct) => ValueTask.FromResult(0.0);

        // Act
        double result = await value.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(0.0, result);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync invokes action and returns its result when synchronously completed condition returns
    ///false.
    ///</summary>
    ///<param name="value">The test value.</param>
    ///<param name="multiplier">The multiplier to apply in action.</param>
    [TestMethod]
    [DataRow(0, 2)]
    [DataRow(42, 3)]
    [DataRow(-100, 5)]
    [DataRow(int.MaxValue, 1)]
    [DataRow(int.MinValue, 1)]
    public async Task ReturnIfNotAsync_SyncConditionFalse_InvokesActionAndReturnsResult(int value, int multiplier)
    {
        // Arrange
        bool conditionCalled = false;
        bool actionCalled = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            conditionCalled = true;
            return ValueTask.FromResult(false);
        };
        Func<int, CancellationToken, ValueTask<int>> action = (o, ct) =>
        {
            actionCalled = true;
            return ValueTask.FromResult(o * multiplier);
        };

        // Act
        int result = await value.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(value * multiplier, result);
        Assert.IsTrue(conditionCalled);
        Assert.IsTrue(actionCalled);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync returns original object when synchronously completed condition returns true.
    ///</summary>
    ///<param name="value">The test value.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(42)]
    [DataRow(-100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public async Task ReturnIfNotAsync_SyncConditionTrue_ReturnsOriginalObject(int value)
    {
        // Arrange
        bool conditionCalled = false;
        bool actionCalled = false;
        Func<int, CancellationToken, ValueTask<bool>> condition = (o, ct) =>
        {
            conditionCalled = true;
            return ValueTask.FromResult(true);
        };
        Func<int, CancellationToken, ValueTask<int>> action = (o, ct) =>
        {
            actionCalled = true;
            return ValueTask.FromResult(o * 2);
        };

        // Act
        int result = await value.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(value, result);
        Assert.IsTrue(conditionCalled);
        Assert.IsFalse(actionCalled);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync works with value types. Input: double value, condition returns false, action returns
    ///modified value. Expected: Action result is returned.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_ValueType_ReturnsActionResult()
    {
        // Arrange
        double obj = 3.14;
        Func<double, CancellationToken, Task<bool>> condition = static (o, ct) => Task.FromResult(false);
        Func<double, CancellationToken, Task<double>> action = static (o, ct) => Task.FromResult(o * 2);

        // Act
        double result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(6.28, result, 0.001);
    }

    ///<summary>
    ///Tests that ReturnIfNotAsync works correctly with value type objects.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNotAsync_ValueTypeObject_WorksCorrectly()
    {
        // Arrange
        double obj = 3.14159;
        Func<double, CancellationToken, ValueTask<bool>> condition = static (o, ct) => ValueTask.FromResult(o < 0);
        Func<double, CancellationToken, ValueTask<double>> action = static (o, ct) => ValueTask.FromResult(Math.Abs(o));

        // Act
        double result = await obj.ReturnIfNotAsync(condition, action);

        // Assert
        Assert.AreEqual(3.14159, result);
    }

    ///<summary>
    ///Tests that ReturnIfNull works correctly with empty string (non-null). Empty string should be returned without
    ///invoking action.
    ///</summary>
    [TestMethod]
    public void ReturnIfNull_EmptyStringObject_ReturnsEmptyString()
    {
        // Arrange
        string obj = string.Empty;
        bool actionInvoked = false;
        Func<string> action = () =>
        {
            actionInvoked = true;
            return "replacement";
        };

        // Act
        string result = obj.ReturnIfNull(action);

        // Assert
        Assert.AreEqual(string.Empty, result);
        Assert.IsFalse(actionInvoked);
    }

    ///<summary>
    ///Tests that ReturnIfNull works correctly with arrays when array is not null.
    ///</summary>
    [TestMethod]
    public void ReturnIfNull_NonNullArray_ReturnsOriginalArray()
    {
        // Arrange
        int[] obj = new[] { 1, 2, 3 };
        Func<int[]> action = static () => new[] { 4, 5, 6 };

        // Act
        int[] result = obj.ReturnIfNull(action);

        // Assert
        Assert.AreSame(obj, result);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    ///<summary>
    ///Tests that ReturnIfNull returns the original object when it is not null. The action should not be invoked.
    ///</summary>
    [TestMethod]
    public void ReturnIfNull_NonNullObject_ReturnsOriginalObjectWithoutInvokingAction()
    {
        // Arrange
        string obj = "test";
        bool actionInvoked = false;
        Func<string> action = () =>
        {
            actionInvoked = true;
            return "replacement";
        };

        // Act
        string result = obj.ReturnIfNull(action);

        // Assert
        Assert.AreEqual("test", result);
        Assert.IsFalse(actionInvoked);
    }

    ///<summary>
    ///Tests that ReturnIfNull works correctly with arrays when array is null.
    ///</summary>
    [TestMethod]
    public void ReturnIfNull_NullArray_ReturnsActionResult()
    {
        // Arrange
        int[]? obj = null;
        int[] replacement = new[] { 4, 5, 6 };
        Func<int[]> action = () => replacement;

        // Act
        int[]? result = obj.ReturnIfNull(action);

        // Assert
        Assert.AreSame(replacement, result);
        CollectionAssert.AreEqual(new[] { 4, 5, 6 }, result);
    }

    ///<summary>
    ///Tests that ReturnIfNull invokes action exactly once when object is null.
    ///</summary>
    [TestMethod]
    public void ReturnIfNull_NullObject_InvokesActionExactlyOnce()
    {
        // Arrange
        string? obj = null;
        int invocationCount = 0;
        Func<string> action = () =>
        {
            invocationCount++;
            return "replacement";
        };

        // Act
        string? result = obj.ReturnIfNull(action);

        // Assert
        Assert.AreEqual("replacement", result);
        Assert.AreEqual(1, invocationCount);
    }

    ///<summary>
    ///Tests that ReturnIfNull returns the result of action when object is null and action returns non-null.
    ///</summary>
    [TestMethod]
    public void ReturnIfNull_NullObjectActionReturnsNonNull_ReturnsActionResult()
    {
        // Arrange
        string? obj = null;
        Func<string> action = static () => "replacement";

        // Act
        string? result = obj.ReturnIfNull(action);

        // Assert
        Assert.AreEqual("replacement", result);
    }

    ///<summary>
    ///Tests that ReturnIfNull returns null when object is null and action returns null.
    ///</summary>
    [TestMethod]
    public void ReturnIfNull_NullObjectActionReturnsNull_ReturnsNull()
    {
        // Arrange
        string? obj = null;
        Func<string?> action = static () => null;

        // Act
        string? result = obj.ReturnIfNull(action);

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that ReturnIfNull works correctly with whitespace string (non-null). Whitespace string should be returned
    ///without invoking action.
    ///</summary>
    [TestMethod]
    public void ReturnIfNull_WhitespaceStringObject_ReturnsWhitespaceString()
    {
        // Arrange
        string obj = "   ";
        bool actionInvoked = false;
        Func<string> action = () =>
        {
            actionInvoked = true;
            return "replacement";
        };

        // Act
        string result = obj.ReturnIfNull(action);

        // Assert
        Assert.AreEqual("   ", result);
        Assert.IsFalse(actionInvoked);
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync returns null when action returns null. Input: Null object and action that returns
    ///null. Expected: Returns null wrapped in Task.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_ActionReturnsNull_ReturnsNull()
    {
        // Arrange
        string? testObject = null;
        Func<CancellationToken, Task<string?>> action = static ct => Task.FromResult<string?>(null);

        // Act
        string? result = await testObject.ReturnIfNullAsync(action, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync handles async action that completes asynchronously. Input: Null object and action
    ///that uses Task.Delay before returning result. Expected: Awaits action completion and returns its result.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_AsyncAction_AwaitsAndReturnsResult()
    {
        // Arrange
        string? testObject = null;
        string expectedResult = "async result";
        bool actionExecuted = false;
        Func<CancellationToken, Task<string?>> action = async ct =>
        {
            await Task.Delay(10, ct);
            actionExecuted = true;
            return expectedResult;
        };

        // Act
        string? result = await testObject.ReturnIfNullAsync(action, CancellationToken.None);

        // Assert
        Assert.AreEqual(expectedResult, result);
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync handles async action execution properly. Input: Null obj with action that performs
    ///async delay. Expected: Awaits action properly and returns result.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_AsyncAction_AwaitsCorrectly()
    {
        // Arrange
        string? obj = null;
        bool actionExecuted = false;
        Func<CancellationToken, ValueTask<string?>> action = async ct =>
        {
            await Task.Delay(10, ct);
            actionExecuted = true;
            return "async result";
        };

        // Act
        string? result = await obj.ReturnIfNullAsync(action);

        // Assert
        Assert.AreEqual("async result", result);
        Assert.IsTrue(actionExecuted);
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync uses default CancellationToken when not specified. Input: Null object, action that
    ///checks for default token, no explicit cancellation token. Expected: Action receives default CancellationToken.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_DefaultCancellationToken_PassesDefaultToken()
    {
        // Arrange
        string? testObject = null;
        CancellationToken? receivedToken = null;
        Func<CancellationToken, Task<string?>> action = ct =>
        {
            receivedToken = ct;
            return Task.FromResult<string?>("result");
        };

        // Act
        await testObject.ReturnIfNullAsync(action);

        // Assert
        Assert.IsTrue(receivedToken.HasValue);
        Assert.AreEqual(default(CancellationToken), receivedToken.Value);
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync uses default cancellation token when not provided. Input: No explicit cancellation
    ///token (uses default parameter). Expected: Works correctly with default cancellation token.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_DefaultCancellationToken_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        CancellationToken? receivedToken = null;
        Func<CancellationToken, ValueTask<string?>> action = ct =>
        {
            receivedToken = ct;
            return ValueTask.FromResult<string?>("result");
        };

        // Act
        string? result = await obj.ReturnIfNullAsync(action);

        // Assert
        Assert.AreEqual("result", result);
        Assert.IsNotNull(receivedToken);
        Assert.AreEqual(default(CancellationToken), receivedToken.Value);
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync returns the original non-null object without calling action. Input: Non-null string
    ///object and valid action. Expected: Returns the original object; action is not invoked.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_NonNullObject_ReturnsOriginalObjectWithoutCallingAction()
    {
        // Arrange
        string testObject = "test value";
        bool actionCalled = false;
        Func<CancellationToken, Task<string>> action = ct =>
        {
            actionCalled = true;
            return Task.FromResult("replacement");
        };

        // Act
        string result = await testObject.ReturnIfNullAsync(action, CancellationToken.None);

        // Assert
        Assert.AreEqual(testObject, result);
        Assert.IsFalse(actionCalled, "Action should not be called when object is not null.");
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync calls action and returns its result when object is null. Input: Null object and
    ///valid action returning a FileName object. Expected: Action is called and its result is returned.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_NullObject_CallsActionAndReturnsResult()
    {
        // Arrange
        string? testObject = null;
        string expectedResult = "replacement value";
        Func<CancellationToken, Task<string?>> action = ct => Task.FromResult<string?>(expectedResult);

        // Act
        string? result = await testObject.ReturnIfNullAsync(action, CancellationToken.None);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync passes the correct CancellationToken to the action. Input: Null object, action that
    ///verifies the cancellation token, and specific CancellationToken. Expected: Action receives the correct
    ///cancellation token.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_NullObject_PassesCorrectCancellationTokenToAction()
    {
        // Arrange
        string? testObject = null;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken? receivedToken = null;
        Func<CancellationToken, Task<string?>> action = ct =>
        {
            receivedToken = ct;
            return Task.FromResult<string?>("result");
        };

        // Act
        await testObject.ReturnIfNullAsync(action, cts.Token);

        // Assert
        Assert.IsTrue(receivedToken.HasValue);
        Assert.AreEqual(cts.Token, receivedToken.Value);
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync works with object type. Input: Null object of type object. Expected: Action is
    ///called and returns FileName object.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_ObjectType_WorksCorrectly()
    {
        // Arrange
        object? obj = null;
        object replacement = new object();
        Func<CancellationToken, ValueTask<object?>> action = ct => ValueTask.FromResult<object?>(replacement);

        // Act
        object? result = await obj.ReturnIfNullAsync(action);

        // Assert
        Assert.AreSame(replacement, result);
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync returns the original object when it is not null. Input: Non-null object. Expected:
    ///Returns the original object without invoking action.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_ObjNotNull_ReturnsOriginalObject()
    {
        // Arrange
        string obj = "test";
        bool actionCalled = false;
        Func<CancellationToken, ValueTask<string>> action = ct =>
        {
            actionCalled = true;
            return ValueTask.FromResult("replacement");
        };
        CancellationToken cancellation = default;

        // Act
        string result = await obj.ReturnIfNullAsync(action, cancellation);

        // Assert
        Assert.AreEqual("test", result);
        Assert.IsFalse(actionCalled, "Action should not be called when obj is not null");
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync passes the cancellation token to action. Input: Null object, action that verifies
    ///the cancellation token. Expected: Action receives the correct cancellation token.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_ObjNull_PassesCancellationTokenToAction()
    {
        // Arrange
        string? obj = null;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken? receivedToken = null;
        Func<CancellationToken, ValueTask<string?>> action = ct =>
        {
            receivedToken = ct;
            return ValueTask.FromResult<string?>("result");
        };

        // Act
        await obj.ReturnIfNullAsync(action, cts.Token);

        // Assert
        Assert.IsNotNull(receivedToken);
        Assert.AreEqual(cts.Token, receivedToken.Value);
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync returns null when obj is null and action returns null. Input: Null object, action
    ///that returns null. Expected: Returns null from action.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_ObjNullAndActionReturnsNull_ReturnsNull()
    {
        // Arrange
        string? obj = null;
        bool actionCalled = false;
        Func<CancellationToken, ValueTask<string?>> action = ct =>
        {
            actionCalled = true;
            return ValueTask.FromResult<string?>(null);
        };
        CancellationToken cancellation = default;

        // Act
        string? result = await obj.ReturnIfNullAsync(action, cancellation);

        // Assert
        Assert.IsNull(result);
        Assert.IsTrue(actionCalled, "Action should be called when obj is null");
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync calls action and returns its result when obj is null. Input: Null object, action
    ///that returns a non-null value. Expected: Action is called and its result is returned.
    ///</summary>
    [TestMethod]
    public async Task ReturnIfNullAsync_ObjNullAndActionReturnsValue_ReturnsActionResult()
    {
        // Arrange
        string? obj = null;
        bool actionCalled = false;
        Func<CancellationToken, ValueTask<string?>> action = ct =>
        {
            actionCalled = true;
            return ValueTask.FromResult<string?>("replacement");
        };
        CancellationToken cancellation = default;

        // Act
        string? result = await obj.ReturnIfNullAsync(action, cancellation);

        // Assert
        Assert.AreEqual("replacement", result);
        Assert.IsTrue(actionCalled, "Action should be called when obj is null");
    }

    ///<summary>
    ///Tests that ReturnIfNullAsync works with different reference types. Input: Various reference types (object, custom
    ///class). Expected: Correct behavior for each type.
    ///</summary>
    [TestMethod]
    [DataRow(null, "default", "default", DisplayName = "Null object returns action result")]
    [DataRow("value", "default", "value", DisplayName = "Non-null object returns original")]
    public async Task ReturnIfNullAsync_VariousScenarios_WorksCorrectly(string? input, string actionResult, string expected)
    {
        // Arrange
        Func<CancellationToken, ValueTask<string?>> action = ct => ValueTask.FromResult<string?>(actionResult);

        // Act
        string? result = await input.ReturnIfNullAsync(action);

        // Assert
        Assert.AreEqual(expected, result);
    }
    #endregion

    ///<summary>
    ///Helper class for testing mutable state in DoUntil tests.
    ///</summary>
    private class Counter
    {
        #region Public properties
        public int Value { get; set; }
        #endregion
    }

    ///<summary>
    ///Test helper struct for DoUntilAsync tests.
    ///</summary>
    private struct TestStruct
    {
        #region Public properties
        public int Value { get; set; }
        #endregion
    }

    ///<summary>
    ///Helper class for testing with custom reference types.
    ///</summary>
    private class TestHelper
    {
        #region Public properties
        public int Value { get; set; }
        #endregion
    }
}