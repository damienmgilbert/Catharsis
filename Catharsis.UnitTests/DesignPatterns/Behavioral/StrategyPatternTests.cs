using Catharsis.DesignPatterns.Behavioral;

namespace Catharsis.UnitTests.DesignPatterns.Behavioral;

[TestClass]
public class StrategyPatternTests
{
    #region Public methods

    ///<summary>
    ///Tests that Strategy correctly applies complex transformation logic. Input: integer with complex calculation
    ///strategy. Expected: Correct calculation result returned.
    ///</summary>
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

    ///<summary>
    ///Tests that Strategy handles empty string input. Input: empty string with length calculation strategy. Expected:
    ///Returns 0.
    ///</summary>
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

    ///<summary>
    ///Tests that Strategy correctly handles identity transformation. Input: object with strategy that returns the same
    ///object. Expected: Same object returned.
    ///</summary>
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

    ///<summary>
    ///Tests that Strategy handles edge case with maximum integer value. Input: int.MaxValue with transformation
    ///strategy. Expected: Correctly transformed value.
    ///</summary>
    [TestMethod]
    public void Strategy_MaxIntValue_ReturnsTransformedValue()
    {
        // Arrange
        int obj = int.MaxValue;
        Func<int, long> strategy = i => ((long)i) * 2;
        // Act
        long result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual(4294967294L, result);
    }

    ///<summary>
    ///Tests that Strategy handles edge case with minimum integer value. Input: int.MinValue with transformation
    ///strategy. Expected: Correctly transformed value.
    ///</summary>
    [TestMethod]
    public void Strategy_MinIntValue_ReturnsTransformedValue()
    {
        // Arrange
        int obj = int.MinValue;
        Func<int, long> strategy = i => ((long)i) * 2;
        // Act
        long result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual(-4294967296L, result);
    }

    ///<summary>
    ///Tests that Strategy passes null object to strategy function when obj is null for reference types. Input: null
    ///string object with strategy that checks for null. Expected: Strategy receives null and returns expected value.
    ///</summary>
    [TestMethod]
    public void Strategy_NullObjectParameter_PassesNullToStrategy()
    {
        // Arrange
        string? obj = null;
        Func<string?, string> strategy = s => (s == null) ? "was null" : "was not null";
        // Act
        string result = new StrategyPattern().Strategy(obj, strategy);
        // Assert
        Assert.AreEqual("was null", result);
    }

    ///<summary>
    ///Tests that Strategy handles transformation from reference type to value type. Input: string with strategy
    ///converting to int. Expected: Correctly converted value.
    ///</summary>
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

    ///<summary>
    ///Tests that Strategy returns null when the strategy function returns null for nullable return type. Input: object
    ///with strategy that returns null. Expected: null returned.
    ///</summary>
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

    ///<summary>
    ///Tests that Strategy correctly applies a valid strategy function to transform a reference type. Input: string
    ///object with strategy that converts to uppercase. Expected: Transformed string returned.
    ///</summary>
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

    ///<summary>
    ///Tests that Strategy correctly applies a valid strategy function to transform a value type. Input: integer with
    ///strategy that converts to string. Expected: String representation of integer returned.
    ///</summary>
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

    ///<summary>
    ///Tests that Strategy handles transformation from value type to different value type. Input: double with strategy
    ///converting to int. Expected: Correctly converted value.
    ///</summary>
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

    ///<summary>
    ///Tests that Strategy handles whitespace-only string input. Input: whitespace string with trim and length strategy.
    ///Expected: Returns 0 after trimming.
    ///</summary>
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
    #endregion
}
