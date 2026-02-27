using Catharsis.DesignPatterns.Creational;

namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class AbstractFactoryPatternTests
{
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
}
