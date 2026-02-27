using Catharsis.DesignPatterns.Behavioral;

namespace Catharsis.UnitTests.DesignPatterns.Behavioral;

[TestClass]
public class VisitorTests
{
    #region Public methods

    ///<summary>
    ///Tests that Accept properly chains with other fluent operations by returning the original object.
    ///</summary>
    [TestMethod]
    public void Accept_ChainedOperations_AllowsFluentChaining()
    {
        // Arrange
        string obj = "test";
        string visitor1 = "visitor1";
        string visitor2 = "visitor2";
        bool visit1Called = false;
        bool visit2Called = false;
        Action<string, string> visit1 = (v, o) => visit1Called = true;
        Action<string, string> visit2 = (v, o) => visit2Called = true;
        // Act
        Visitor v = new Visitor();
        string result = v.Accept(v.Accept(obj, visitor1, visit1), visitor2, visit2);
        // Assert
        Assert.IsTrue(visit1Called);
        Assert.IsTrue(visit2Called);
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Tests that Accept works with complex reference types.
    ///</summary>
    [TestMethod]
    public void Accept_ComplexReferenceType_WorksCorrectly()
    {
        // Arrange
        List<int> obj = new System.Collections.Generic.List<int> { 1, 2, 3 };
        Dictionary<string, int> visitor = new System.Collections.Generic.Dictionary<string, int>();
        bool visitWasCalled = false;
        Action<System.Collections.Generic.Dictionary<string, int>, System.Collections.Generic.List<int>> visit = (v, o) => visitWasCalled = true;
        // Act
        List<int> result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Tests that Accept works with complex reference types. Input: Complex object, visitor, and visit function.
    ///Expected: The visit function processes the complex types correctly.
    ///</summary>
    [TestMethod]
    public void Accept_ComplexReferenceTypes_ReturnsExpectedResult()
    {
        // Arrange
        var obj = new { Id = 1, Name = "Test" };
        var visitor = new { ProcessorId = 42 };
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

    ///<summary>
    ///Tests that Accept works with different generic type combinations.
    ///</summary>
    [TestMethod]
    public void Accept_DifferentGenericTypes_WorksCorrectly()
    {
        // Arrange
        int obj = 123;
        string visitor = "string visitor";
        bool visitWasCalled = false;
        Action<string, int> visit = (v, o) => visitWasCalled = true;
        // Act
        int result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreEqual(obj, result);
    }

    ///<summary>
    ///Tests that Accept correctly handles different return types. Input: String object, string visitor, visit function
    ///returning boolean. Expected: The visit function returns the expected boolean result.
    ///</summary>
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

    ///<summary>
    ///Tests that Accept works with empty string as object.
    ///</summary>
    [TestMethod]
    public void Accept_EmptyStringObject_WorksCorrectly()
    {
        // Arrange
        string obj = string.Empty;
        string visitor = "visitor";
        bool visitWasCalled = false;
        string? capturedObj = null;
        Action<string, string> visit = (v, o) =>
        {
            visitWasCalled = true;
            capturedObj = o;
        };
        // Act
        string result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreSame(string.Empty, capturedObj);
        Assert.AreSame(string.Empty, result);
    }

    ///<summary>
    ///Tests that Accept works with empty strings. Input: Empty string as object and visitor. Expected: The visit
    ///function processes empty strings correctly.
    ///</summary>
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

    ///<summary>
    ///Tests that Accept works correctly with extreme integer values.
    ///</summary>
    [TestMethod]
    public void Accept_ExtremeBoundaryValues_WorksCorrectly()
    {
        // Arrange
        int obj = int.MaxValue;
        int visitor = int.MinValue;
        bool visitWasCalled = false;
        int capturedObj = 0;
        int capturedVisitor = 0;
        Action<int, int> visit = (v, o) =>
        {
            visitWasCalled = true;
            capturedVisitor = v;
            capturedObj = o;
        };
        // Act
        int result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreEqual(int.MinValue, capturedVisitor);
        Assert.AreEqual(int.MaxValue, capturedObj);
        Assert.AreEqual(int.MaxValue, result);
    }

    ///<summary>
    ///Tests that Accept works with extreme integer values. Input: int.MaxValue as object, int.MinValue as visitor.
    ///Expected: The visit function processes extreme values correctly.
    ///</summary>
    [TestMethod]
    public void Accept_ExtremeIntegerValues_ReturnsExpectedResult()
    {
        // Arrange
        const int obj = int.MaxValue;
        const int visitor = int.MinValue;
        const long expectedResult = ((long)int.MaxValue) + int.MinValue;
        Func<int, int, long> visit = (v, o) => ((long)v) + o;
        // Act
        long result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    ///<summary>
    ///Tests that Accept works correctly when the object being visited is null. Input: Null object, valid visitor and
    ///visit function. Expected: The visit function is invoked with null object and returns expected result.
    ///</summary>
    [TestMethod]
    public void Accept_NullObject_InvokesVisitFunctionWithNull()
    {
        // Arrange
        string? obj = null;
        const string visitor = "visitor";
        const string expectedResult = "visitor processed null";
        Func<string, string?, string> visit = (v, o) => $"{v} processed {((o == null) ? "null" : o)}";
        // Act
        string result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    ///<summary>
    ///Tests that Accept handles a null object (when T is nullable) and passes it to the visit action.
    ///</summary>
    [TestMethod]
    public void Accept_NullObjectWithNullableType_InvokesVisitActionWithNull()
    {
        // Arrange
        string? obj = null;
        string visitor = "visitor";
        bool visitWasCalled = false;
        string? capturedObj = "not null";
        Action<string, string?> visit = (v, o) =>
        {
            visitWasCalled = true;
            capturedObj = o;
        };
        // Act
        string? result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.IsNull(capturedObj);
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that Accept invokes the visit action exactly once.
    ///</summary>
    [TestMethod]
    public void Accept_ValidInputs_InvokesVisitActionExactlyOnce()
    {
        // Arrange
        string obj = "test";
        string visitor = "visitor";
        int callCount = 0;
        Action<string, string> visit = (v, o) => callCount++;
        // Act
        new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(1, callCount);
    }

    ///<summary>
    ///Tests that Accept returns the same instance that was passed as the object parameter.
    ///</summary>
    [TestMethod]
    public void Accept_ValidInputs_ReturnsSameObjectInstance()
    {
        // Arrange
        object obj = new object();
        object visitor = new object();
        Action<object, object> visit = (v, o) =>
        {
        };
        // Act
        object result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Tests that Accept invokes the visit function with the correct parameters. Input: Valid object, visitor, and visit
    ///function. Expected: The visit function receives the correct visitor and object parameters.
    ///</summary>
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

    ///<summary>
    ///Tests that Accept returns the expected result when all parameters are valid. Input: A valid object, visitor, and
    ///visit function. Expected: The visit function is invoked and its result is returned.
    ///</summary>
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

    ///<summary>
    ///Tests that Accept invokes the visit action with correct parameters for a reference type object.
    ///</summary>
    [TestMethod]
    public void Accept_ValidReferenceTypeObject_InvokesVisitActionWithCorrectParameters()
    {
        // Arrange
        string obj = "test object";
        string visitor = "test visitor";
        bool visitWasCalled = false;
        string? capturedVisitor = null;
        string? capturedObj = null;
        Action<string, string> visit = (v, o) =>
        {
            visitWasCalled = true;
            capturedVisitor = v;
            capturedObj = o;
        };
        // Act
        string result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreSame(visitor, capturedVisitor);
        Assert.AreSame(obj, capturedObj);
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Tests that Accept invokes the visit action with correct parameters for a value type object.
    ///</summary>
    [TestMethod]
    public void Accept_ValidValueTypeObject_InvokesVisitActionWithCorrectParameters()
    {
        // Arrange
        int obj = 42;
        int visitor = 100;
        bool visitWasCalled = false;
        int capturedVisitor = 0;
        int capturedObj = 0;
        Action<int, int> visit = (v, o) =>
        {
            visitWasCalled = true;
            capturedVisitor = v;
            capturedObj = o;
        };
        // Act
        int result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreEqual(visitor, capturedVisitor);
        Assert.AreEqual(obj, capturedObj);
        Assert.AreEqual(obj, result);
    }

    ///<summary>
    ///Tests that Accept works correctly with value types. Input: Integer object, visitor, and visit function. Expected:
    ///The visit function is invoked and returns the calculated result.
    ///</summary>
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

    ///<summary>
    ///Tests that Accept works with very long strings. Input: Very long strings for both object and visitor. Expected:
    ///The visit function processes long strings correctly.
    ///</summary>
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

    ///<summary>
    ///Tests that Accept respects side effects performed by the visit action.
    ///</summary>
    [TestMethod]
    public void Accept_VisitActionWithSideEffects_SideEffectsAreExecuted()
    {
        // Arrange
        List<int> obj = new System.Collections.Generic.List<int>();
        int visitor = 42;
        Action<int, System.Collections.Generic.List<int>> visit = (v, o) => o.Add(v);
        // Act
        List<int> result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.AreEqual(1, obj.Count);
        Assert.AreEqual(42, obj[0]);
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Tests that Accept works with whitespace-only string as object.
    ///</summary>
    [TestMethod]
    public void Accept_WhitespaceStringObject_WorksCorrectly()
    {
        // Arrange
        string obj = "   ";
        string visitor = "visitor";
        bool visitWasCalled = false;
        Action<string, string> visit = (v, o) => visitWasCalled = true;
        // Act
        string result = new Visitor().Accept(obj, visitor, visit);
        // Assert
        Assert.IsTrue(visitWasCalled);
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Tests that Accept works with whitespace-only strings. Input: Whitespace strings as object and visitor. Expected:
    ///The visit function processes whitespace strings correctly.
    ///</summary>
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
    #endregion
}
