using Catharsis.DesignPatterns.Creational;

namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class FactoryMethodPatternTests
{
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
}
