using Catharsis.DesignPatterns.Structural;

namespace Catharsis.UnitTests.DesignPatterns.Structural;

[TestClass]
public class BridgePatternTests
{
    #region Public methods

    ///<summary>
    ///Tests that Bridge works when all three type parameters are the same. Input: obj = "hello", implementation =
    ///"world", operation = concatenate Expected: Returns concatenated string
    ///</summary>
    [TestMethod]
    public void Bridge_AllSameType_WorksCorrectly()
    {
        // Arrange
        string obj = "hello";
        string impl = "world";
        // Act
        string result = new BridgePattern().Bridge(obj, impl, static (a, b) => $"{a} {b}");
        // Assert
        Assert.AreEqual("hello world", result);
    }

    ///<summary>
    ///Tests that Bridge works with boolean operations. Input: obj = true, implementation = false, operation = logical
    ///AND Expected: Returns false
    ///</summary>
    [TestMethod]
    public void Bridge_BooleanOperations_ReturnsExpectedResult()
    {
        // Arrange
        bool obj = true;
        bool impl = false;
        // Act
        bool result = new BridgePattern().Bridge(obj, impl, static (a, b) => a && b);
        // Assert
        Assert.IsFalse(result);
    }

    ///<summary>
    ///Tests that Bridge works with value types at boundary values. Input: int.MinValue, int.MaxValue, operation = add
    ///Expected: Overflow occurs as expected in unchecked context
    ///</summary>
    [TestMethod]
    public void Bridge_BoundaryValueTypes_HandlesEdgeCases()
    {
        // Arrange
        int minValue = int.MinValue;
        int maxValue = int.MaxValue;
        // Act
        int result = new BridgePattern().Bridge(minValue, maxValue, static (a, b) => unchecked(a + b));
        // Assert
        Assert.AreEqual(-1, result);
    }

    ///<summary>
    ///Tests that Bridge works with complex reference types and returns complex results. Input: obj = List of ints,
    ///implementation = HashSet of ints, operation = combine and count Expected: Operation combines collections and
    ///returns correct count
    ///</summary>
    [TestMethod]
    public void Bridge_ComplexTypes_ReturnsExpectedResult()
    {
        // Arrange
        List<int> list = new List<int> { 1, 2, 3 };
        HashSet<int> set = new HashSet<int> { 3, 4, 5 };
        // Act
        int result = new BridgePattern().Bridge(
                     list,
                     set,
                     static (l, s) =>
                     {
                         HashSet<int> combined = new HashSet<int>(l);
                         combined.UnionWith(s);
                         return combined.Count;
                     });
        // Assert
        Assert.AreEqual(5, result);
    }

    ///<summary>
    ///Tests that Bridge can be used to create tuples from disparate types. Input: obj = string, implementation = int
    ///Expected: Returns tuple containing both values
    ///</summary>
    [TestMethod]
    public void Bridge_CreateTuple_ReturnsExpectedTuple()
    {
        // Arrange
        string obj = "key";
        int impl = 42;
        // Act
        (string a, int b) result = new BridgePattern().Bridge(obj, impl, static (a, b) => (a, b));
        // Assert
        Assert.AreEqual("key", result.Item1);
        Assert.AreEqual(42, result.Item2);
    }

    ///<summary>
    ///Tests that Bridge works with floating-point infinity values. Input: double.PositiveInfinity,
    ///double.NegativeInfinity, operation = add Expected: NaN result due to infinity - infinity
    ///</summary>
    [TestMethod]
    public void Bridge_FloatingPointInfinity_HandlesInfinityOperations()
    {
        // Arrange
        double positiveInfinity = double.PositiveInfinity;
        double negativeInfinity = double.NegativeInfinity;
        // Act
        double result = new BridgePattern().Bridge(positiveInfinity, negativeInfinity, static (a, b) => a + b);
        // Assert
        Assert.IsTrue(double.IsNaN(result));
    }

    ///<summary>
    ///Tests that Bridge works with floating-point special values. Input: double.NaN, double.PositiveInfinity, operation
    ///= combine Expected: NaN result due to NaN operand
    ///</summary>
    [TestMethod]
    public void Bridge_FloatingPointSpecialValues_HandlesNaN()
    {
        // Arrange
        double nan = double.NaN;
        double infinity = double.PositiveInfinity;
        // Act
        double result = new BridgePattern().Bridge(nan, infinity, static (a, b) => a + b);
        // Assert
        Assert.IsTrue(double.IsNaN(result));
    }

    ///<summary>
    ///Tests that Bridge works with reference types including null values. Input: obj = null (nullable reference),
    ///implementation = null (nullable reference) Expected: Operation is invoked with null values and returns expected
    ///result
    ///</summary>
    [TestMethod]
    public void Bridge_NullableReferenceTypes_HandlesNullValues()
    {
        // Arrange
        string? nullObj = null;
        string? nullImpl = null;
        // Act
        string result = new BridgePattern().Bridge(nullObj, nullImpl, static (obj, impl) => $"{((obj == null) ? "null" : obj)}_{((impl == null) ? "null" : impl)}");
        // Assert
        Assert.AreEqual("null_null", result);
    }

    ///<summary>
    ///Tests that Bridge allows operation to return null when TResult is nullable. Input: obj = any value,
    ///implementation = any value, operation = returns null Expected: Null is returned from Bridge
    ///</summary>
    [TestMethod]
    public void Bridge_OperationReturnsNull_ReturnsNull()
    {
        // Arrange
        int obj = 42;
        string impl = "test";
        // Act
        string? result = new BridgePattern().Bridge(obj, impl, static (o, i) => (string?)null);
        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that Bridge works correctly when obj and implementation are the same type. Input: obj = 5, implementation =
    ///10, both int Expected: Operation receives both values correctly
    ///</summary>
    [TestMethod]
    public void Bridge_SameTypeForObjAndImpl_WorksCorrectly()
    {
        // Arrange
        int obj = 5;
        int impl = 10;
        // Act
        int result = new BridgePattern().Bridge(obj, impl, static (a, b) => a * b);
        // Assert
        Assert.AreEqual(50, result);
    }

    ///<summary>
    ///Tests that Bridge passes the correct obj and implementation parameters to the operation. Input: obj = specific
    ///value, implementation = specific value, operation = capturing lambda Expected: Operation receives the exact obj
    ///and implementation values passed to Bridge
    ///</summary>
    [TestMethod]
    public void Bridge_ValidOperation_PassesCorrectParametersToOperation()
    {
        // Arrange
        string expectedObj = "testObject";
        int expectedImpl = 123;
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

    ///<summary>
    ///Tests that Bridge correctly invokes the operation and returns the expected result. Input: Various combinations of
    ///value types, reference types, and result types Expected: Operation is invoked with correct parameters and result
    ///is returned
    ///</summary>
    [TestMethod]
    [DataRow("hello", 5, "hello5", DisplayName = "String + Int -> String")]
    [DataRow(10, 20, 30, DisplayName = "Int + Int -> Int")]
    [DataRow(3.14, 2.71, 5.85, DisplayName = "Double + Double -> Double")]
    public void Bridge_ValidOperation_ReturnsExpectedResult(object objValue, object implValue, object expected)
    {
        // Arrange & Act & Assert based on type
        if((objValue is string strObj) && (implValue is int intImpl) && (expected is string strExpected))
        {
            string result = new BridgePattern().Bridge(strObj, intImpl, static (s, i) => $"{s}{i}");
            Assert.AreEqual(strExpected, result);
        } else if((objValue is int intObj) && (implValue is int intImpl2) && (expected is int intExpected))
        {
            int result = new BridgePattern().Bridge(intObj, intImpl2, static (a, b) => a + b);
            Assert.AreEqual(intExpected, result);
        } else if((objValue is double dblObj) && (implValue is double dblImpl) && (expected is double dblExpected))
        {
            double result = new BridgePattern().Bridge(dblObj, dblImpl, static (a, b) => a + b);
            Assert.AreEqual(dblExpected, result, 0.0001);
        }
    }
    #endregion

    ///<summary>
    ///Test mediator type used for testing.
    ///</summary>
    class TestMediator
    {
    }

    ///<summary>
    ///Custom mediator type for testing complex scenarios.
    ///</summary>
    class CustomMediator
    {
        #region Public properties
        public int ProcessingId { get; set; }
        #endregion
    }

    ///<summary>
    ///Custom request type for testing complex scenarios.
    ///</summary>
    class CustomRequest
    {
        #region Public properties
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        #endregion
    }

    ///<summary>
    ///Custom response type for testing complex scenarios.
    ///</summary>
    class CustomResponse
    {
        #region Public properties
        public int ProcessedBy { get; set; }

        public int RequestId { get; set; }
        #endregion
    }
}
