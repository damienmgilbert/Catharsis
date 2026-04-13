using Catharsis.DesignPatterns.Behavioral;

namespace Catharsis.UnitTests.DesignPatterns.Behavioral;

[TestClass]
public class MediatorTests
{
    #region Public methods

    ///<summary>
    ///Tests that Mediate works when all generic types are the same.
    ///</summary>
    [TestMethod]
    public void Mediate_AllGenericTypesSame_WorksCorrectly()
    {
        // Arrange
        string obj = "test";
        string mediator = "mediator";
        Func<string, string, string> route = static (m, o) => $"{m}{o}";
        // Act
        string result = Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual("mediatortest", result);
    }

    ///<summary>
    ///Tests that Mediate works with complex custom types.
    ///</summary>
    [TestMethod]
    public void Mediate_ComplexCustomTypes_ReturnsExpectedResult()
    {
        // Arrange
        CustomRequest obj = new() { Id = 123, Name = "Test" };
        CustomMediator mediator = new() { ProcessingId = 999 };
        Func<CustomMediator, CustomRequest, CustomResponse> route = static (m, o) => new CustomResponse { RequestId = o.Id, ProcessedBy = m.ProcessingId };
        // Act
        CustomResponse result = Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(123, result.RequestId);
        Assert.AreEqual(999, result.ProcessedBy);
    }

    ///<summary>
    ///Tests that Mediate works when mediator and obj are the same type.
    ///</summary>
    [TestMethod]
    public void Mediate_MediatorAndObjSameType_WorksCorrectly()
    {
        // Arrange
        int obj = 10;
        int mediator = 20;
        Func<int, int, int> route = static (m, o) => m + o;
        // Act
        int result = Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(30, result);
    }

    ///<summary>
    ///Tests that Mediate works correctly when obj parameter is null with nullable reference type.
    ///</summary>
    [TestMethod]
    public void Mediate_NullObjWithNullableType_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        TestMediator mediator = new();
        Func<TestMediator, string?, bool> route = static (m, o) => o == null;
        // Act
        bool result = Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.IsTrue(result);
    }

    ///<summary>
    ///Tests that Mediate works with extreme numeric values.
    ///</summary>
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(1)]
    [TestMethod]
    public void Mediate_NumericBoundaryValues_ReturnsExpectedResult(int value)
    {
        // Arrange
        TestMediator mediator = new();
        Func<TestMediator, int, long> route = static (m, o) => ((long)o) * 2;
        long expectedResult = ((long)value) * 2;
        // Act
        long result = Mediator.Mediate(value, mediator, route);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    ///<summary>
    ///Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/> allows the
    ///route action to modify external state.
    ///</summary>
    [TestMethod]
    public void Mediate_RouteActionModifiesState_StateIsModified()
    {
        // Arrange
        string obj = "input";
        string mediator = "mediator";
        string externalState = "initial";
        Action<string, string> route = (m, o) => externalState = $"{m}-{o}";
        // Act
        Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual("mediator-input", externalState);
    }

    ///<summary>
    ///Tests that Mediate returns the exact result produced by route function.
    ///</summary>
    [TestMethod]
    public void Mediate_RouteReturnsSpecificObject_ReturnsSameObject()
    {
        // Arrange
        string obj = "input";
        TestMediator mediator = new();
        CustomResponse expectedResponse = new() { RequestId = 42, ProcessedBy = 1 };
        Func<TestMediator, string, CustomResponse> route = (m, o) => expectedResponse;
        // Act
        CustomResponse result = Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.AreSame(expectedResponse, result);
    }

    ///<summary>
    ///Tests that Mediate correctly invokes route function and returns the result with reference types.
    ///</summary>
    [TestMethod]
    public void Mediate_ValidInputsWithReferenceTypes_ReturnsExpectedResult()
    {
        // Arrange
        string obj = "Hello";
        TestMediator mediator = new();
        int expectedResult = 5;
        Func<TestMediator, string, int> route = static (m, o) => o.Length;
        // Act
        int result = Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    ///<summary>
    ///Tests that Mediate correctly invokes route function and returns the result with value types.
    ///</summary>
    [TestMethod]
    public void Mediate_ValidInputsWithValueTypes_ReturnsExpectedResult()
    {
        // Arrange
        int obj = 42;
        TestMediator mediator = new();
        string expectedResult = "42";
        Func<TestMediator, int, string> route = static (m, o) => o.ToString();
        // Act
        string result = Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    ///<summary>
    ///Tests that Mediate works with various string inputs including empty and whitespace.
    ///</summary>
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("test")]
    [DataRow("a very long string that contains many characters to test edge cases")]
    [TestMethod]
    public void Mediate_VariousStringInputs_ReturnsExpectedResult(string value)
    {
        // Arrange
        TestMediator mediator = new();
        Func<TestMediator, string, int> route = static (m, o) => o.Length;
        int expectedResult = value.Length;
        // Act
        int result = Mediator.Mediate(value, mediator, route);
        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    ///<summary>
    ///Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/> works with
    ///different concrete types for T and TMediator.
    ///</summary>
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
        int result = Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(mediator, capturedMediator);
        Assert.AreEqual(obj, capturedObj);
    }

    ///<summary>
    ///Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/> works
    ///correctly when the object parameter is null (nullable reference type).
    ///</summary>
    [TestMethod]
    public void Mediate_WithNullObject_InvokesRouteWithNull()
    {
        // Arrange
        string? obj = null;
        string mediator = "mediator";
        string? capturedObj = "not-null";
        Action<string, string?> route = (m, o) => capturedObj = o;
        // Act
        string? result = Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.IsNull(result);
        Assert.IsNull(capturedObj);
    }

    ///<summary>
    ///Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/> actually
    ///executes the route action.
    ///</summary>
    [TestMethod]
    public void Mediate_WithValidParameters_ExecutesRoute()
    {
        // Arrange
        string obj = "test";
        string mediator = "mediator";
        bool routeExecuted = false;
        Action<string, string> route = (m, o) => routeExecuted = true;
        // Act
        Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.IsTrue(routeExecuted);
    }

    ///<summary>
    ///Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/> invokes the
    ///route action with the correct mediator and object parameters.
    ///</summary>
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
        Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(mediator, capturedMediator);
        Assert.AreEqual(obj, capturedObj);
    }

    ///<summary>
    ///Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/> returns the
    ///original object after executing the route action.
    ///</summary>
    [TestMethod]
    public void Mediate_WithValidParameters_ReturnsOriginalObject()
    {
        // Arrange
        string obj = "test-object";
        string mediator = "mediator";
        Action<string, string> route = static (m, o) =>
        {
        };
        // Act
        string result = Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Tests that <see cref = "DesignPattern.Mediate{T, TMediator}(T, TMediator, Action{TMediator, T})"/> works
    ///correctly with value types.
    ///</summary>
    [TestMethod]
    public void Mediate_WithValueTypeObject_WorksCorrectly()
    {
        // Arrange
        int obj = 42;
        string mediator = "mediator";
        int capturedObj = 0;
        Action<string, int> route = (m, o) => capturedObj = o;
        // Act
        int result = Mediator.Mediate(obj, mediator, route);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(obj, capturedObj);
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
