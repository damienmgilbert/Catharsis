using Catharsis.DesignPatterns.Behavioral;

namespace Catharsis.UnitTests.DesignPatterns.Behavioral;

[TestClass]
public class TemplateMethodTests
{
    #region Public methods

    ///<summary>
    ///Tests that Template returns complex result types correctly.
    ///</summary>
    [TestMethod]
    public void Template_ComplexResultType_ReturnsCorrectly()
    {
        // Arrange
        int obj = 5;
        Action<int> setup = n =>
        {
        };
        Func<int, (int Value, string Text)> operation = n => (n, n.ToString());
        Action<int> teardown = n =>
        {
        };
        // Act
        (int Value, string Text) result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual(5, result.Value);
        Assert.AreEqual("5", result.Text);
    }

    ///<summary>
    ///Tests that Template correctly returns different result types.
    ///</summary>
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
        int result = new TemplateMethod().Template(input, setup, operation, teardown);
        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that Template works with null context object when T is nullable reference type.
    ///</summary>
    [TestMethod]
    public void Template_NullContext_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        bool setupReceived = false;
        bool operationReceived = false;
        bool teardownReceived = false;
        Action<string?> setup = s => setupReceived = s == null;
        Func<string?, bool> operation = s =>
        {
            operationReceived = s == null;
            return s == null;
        };
        Action<string?> teardown = s => teardownReceived = s == null;
        // Act
        bool result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(setupReceived);
        Assert.IsTrue(operationReceived);
        Assert.IsTrue(teardownReceived);
    }

    ///<summary>
    ///Tests that Template works with reference type context and returns correct result.
    ///</summary>
    [TestMethod]
    public void Template_ReferenceTypeContext_WorksCorrectly()
    {
        // Arrange
        List<int> obj = new List<int> { 1, 2, 3 };
        bool setupCalled = false;
        bool teardownCalled = false;
        Action<List<int>> setup = list => setupCalled = true;
        Func<List<int>, int> operation = list => list.Count;
        Action<List<int>> teardown = list => teardownCalled = true;
        // Act
        int result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual(3, result);
        Assert.IsTrue(setupCalled);
        Assert.IsTrue(teardownCalled);
    }

    ///<summary>
    ///Tests that Template allows setup to modify state before operation.
    ///</summary>
    [TestMethod]
    public void Template_SetupModifiesState_OperationSeesModification()
    {
        // Arrange
        List<int> obj = new List<int>();
        Action<List<int>> setup = list => list.Add(1);
        Func<List<int>, int> operation = list => list.Count;
        Action<List<int>> teardown = list => list.Clear();
        // Act
        int result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual(1, result);
        Assert.AreEqual(0, obj.Count); // Teardown was called and cleared the list
    }

    ///<summary>
    ///Tests that Template executes setup, operation, and teardown in correct order and returns the result from
    ///operation.
    ///</summary>
    [TestMethod]
    public void Template_ValidDelegates_ExecutesInCorrectOrderAndReturnsResult()
    {
        // Arrange
        string obj = "test";
        List<string> executionOrder = new List<string>();
        Action<string> setup = s => executionOrder.Add("setup");
        Func<string, int> operation = s =>
        {
            executionOrder.Add("operation");
            return s.Length;
        };
        Action<string> teardown = s => executionOrder.Add("teardown");
        // Act
        int result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual(4, result);
        CollectionAssert.AreEqual(new[] { "setup", "operation", "teardown" }, executionOrder);
    }

    ///<summary>
    ///Tests that Template invokes each delegate exactly once.
    ///</summary>
    [TestMethod]
    public void Template_ValidDelegates_InvokesEachDelegateOnce()
    {
        // Arrange
        string obj = "test";
        int setupCount = 0;
        int operationCount = 0;
        int teardownCount = 0;
        Action<string> setup = s => setupCount++;
        Func<string, int> operation = s =>
        {
            operationCount++;
            return s.Length;
        };
        Action<string> teardown = s => teardownCount++;
        // Act
        int result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual(4, result);
        Assert.AreEqual(1, setupCount);
        Assert.AreEqual(1, operationCount);
        Assert.AreEqual(1, teardownCount);
    }

    ///<summary>
    ///Tests that Template passes the context object to all delegates.
    ///</summary>
    [TestMethod]
    public void Template_ValidDelegates_PassesContextToAllDelegates()
    {
        // Arrange
        int obj = 42;
        int setupReceived = 0;
        int operationReceived = 0;
        int teardownReceived = 0;
        Action<int> setup = n => setupReceived = n;
        Func<int, string> operation = n =>
        {
            operationReceived = n;
            return n.ToString();
        };
        Action<int> teardown = n => teardownReceived = n;
        // Act
        string result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual("42", result);
        Assert.AreEqual(42, setupReceived);
        Assert.AreEqual(42, operationReceived);
        Assert.AreEqual(42, teardownReceived);
    }

    ///<summary>
    ///Tests that Template works with value type context and returns correct result.
    ///</summary>
    [TestMethod]
    public void Template_ValueTypeContext_WorksCorrectly()
    {
        // Arrange
        int obj = 10;
        bool setupCalled = false;
        bool teardownCalled = false;
        Action<int> setup = n => setupCalled = true;
        Func<int, int> operation = n => n * 2;
        Action<int> teardown = n => teardownCalled = true;
        // Act
        int result = new TemplateMethod().Template(obj, setup, operation, teardown);
        // Assert
        Assert.AreEqual(20, result);
        Assert.IsTrue(setupCalled);
        Assert.IsTrue(teardownCalled);
    }
    #endregion
}
