using Catharsis.DesignPatterns.Structural;

namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class AdapterTests
{
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
}
