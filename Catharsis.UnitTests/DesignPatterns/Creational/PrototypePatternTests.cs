using Catharsis.DesignPatterns.Creational;

namespace Catharsis.UnitTests.DesignPatterns.Creational;

[TestClass]
public class PrototypePatternTests
{
    #region Public methods
    ///<summary>
    ///Tests that Prototype returns null when the clone function returns null for reference types. Input: A clone
    ///function that returns null. Expected: Returns null.
    ///</summary>
    [TestMethod]
    public void Prototype_CloneFunctionReturnsNull_ReturnsNull()
    {
        // Arrange
        string obj = "test";
        Func<string?, string?> clone = static _ => null;
        // Act
        string? result = new PrototypePattern().Prototype<string?>(obj, clone);
        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that Prototype works correctly with double special values. Input: double.NaN, double.PositiveInfinity,
    ///double.NegativeInfinity, and normal values. Expected: Clone function is invoked and returns the expected value.
    ///</summary>
    [TestMethod]
    public void Prototype_DoubleNaN_ReturnsCloneFunctionResult()
    {
        // Arrange
        double value = double.NaN;
        Func<double, double> clone = static x => double.IsNaN(x) ? 0.0 : x;
        // Act
        double result = new PrototypePattern().Prototype(value, clone);
        // Assert
        Assert.AreEqual(0.0, result);
    }

    ///<summary>
    ///Tests that Prototype works correctly with double.NegativeInfinity. Input: double.NegativeInfinity. Expected:
    ///Clone function is invoked and returns the expected value.
    ///</summary>
    [TestMethod]
    public void Prototype_DoubleNegativeInfinity_ReturnsCloneFunctionResult()
    {
        // Arrange
        double value = double.NegativeInfinity;
        Func<double, double> clone = static x => x;
        // Act
        double result = new PrototypePattern().Prototype(value, clone);
        // Assert
        Assert.AreEqual(double.NegativeInfinity, result);
    }

    ///<summary>
    ///Tests that Prototype works correctly with double.PositiveInfinity. Input: double.PositiveInfinity. Expected:
    ///Clone function is invoked and returns the expected value.
    ///</summary>
    [TestMethod]
    public void Prototype_DoublePositiveInfinity_ReturnsCloneFunctionResult()
    {
        // Arrange
        double value = double.PositiveInfinity;
        Func<double, double> clone = static x => x;
        // Act
        double result = new PrototypePattern().Prototype(value, clone);
        // Assert
        Assert.AreEqual(double.PositiveInfinity, result);
    }

    ///<summary>
    ///Tests that Prototype works correctly with integer boundary values. Input: int.MinValue, int.MaxValue, 0,
    ///negative, and positive values. Expected: Clone function is invoked and returns the expected value.
    ///</summary>
    [TestMethod]
    [DataRow(int.MinValue, DisplayName = "int.MinValue")]
    [DataRow(int.MaxValue, DisplayName = "int.MaxValue")]
    [DataRow(0, DisplayName = "Zero")]
    [DataRow(-1, DisplayName = "Negative")]
    [DataRow(42, DisplayName = "Positive")]
    public void Prototype_IntegerBoundaryValues_ReturnsCloneFunctionResult(int value)
    {
        // Arrange
        int expected = value * 2;
        Func<int, int> clone = static x => x * 2;
        // Act
        int result = new PrototypePattern().Prototype(value, clone);
        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that Prototype works correctly with reference types. Input: A reference object and a clone function that
    ///returns a different instance. Expected: Returns the cloned object from the clone function.
    ///</summary>
    [TestMethod]
    public void Prototype_ReferenceType_ReturnsClonedObject()
    {
        // Arrange
        object original = new object();
        object clonedObject = new object();
        Func<object, object> clone = _ => clonedObject;
        // Act
        object result = new PrototypePattern().Prototype(original, clone);
        // Assert
        Assert.AreSame(clonedObject, result);
        Assert.AreNotSame(original, result);
    }

    ///<summary>
    ///Tests that Prototype correctly invokes the clone function and returns the result for various string inputs.
    ///Input: Different string values including null, empty, and whitespace. Expected: Clone function result is
    ///returned.
    ///</summary>
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
        string? result = new PrototypePattern().Prototype(original, clone);
        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that Prototype works with strings containing special characters. Input: Strings with special and control
    ///characters. Expected: Clone function is invoked and returns the expected result.
    ///</summary>
    [TestMethod]
    [DataRow("\n\r\t", DisplayName = "Control characters")]
    [DataRow("Hello\0World", DisplayName = "Null character")]
    [DataRow("Unicode: \u00A9 \u2764", DisplayName = "Unicode characters")]
    [DataRow("Special: !@#$%^&*()", DisplayName = "Special characters")]
    public void Prototype_StringsWithSpecialCharacters_ReturnsCloneFunctionResult(string input)
    {
        // Arrange
        string expected = "processed";
        Func<string, string> clone = _ => expected;
        // Act
        string result = new PrototypePattern().Prototype(input, clone);
        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that Prototype invokes the clone delegate with the original object. Input: An integer value and a clone
    ///function that captures the input. Expected: Clone function is called with the original object.
    ///</summary>
    [TestMethod]
    public void Prototype_ValidCloneFunction_InvokesCloneDelegateWithOriginalObject()
    {
        // Arrange
        int obj = 42;
        bool wasCalled = false;
        int passedValue = 0;
        Func<int, int> clone = x =>
        {
            wasCalled = true;
            passedValue = x;
            return x * 2;
        };
        // Act
        int result = new PrototypePattern().Prototype(obj, clone);
        // Assert
        Assert.IsTrue(wasCalled);
        Assert.AreEqual(obj, passedValue);
        Assert.AreEqual(84, result);
    }

    ///<summary>
    ///Tests that Prototype works with very long strings. Input: A very long string. Expected: Clone function is invoked
    ///and returns the expected result.
    ///</summary>
    [TestMethod]
    public void Prototype_VeryLongString_ReturnsCloneFunctionResult()
    {
        // Arrange
        string longString = new string('a', 10000);
        string expectedClone = new string('b', 10000);
        Func<string, string> clone = _ => expectedClone;
        // Act
        string result = new PrototypePattern().Prototype(longString, clone);
        // Assert
        Assert.AreEqual(expectedClone, result);
    }
    #endregion
}
