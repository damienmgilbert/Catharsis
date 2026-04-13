using System.Collections;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Tests for the <see cref="ValidatableComponent"/> class.
///</summary>
[TestClass]
public partial class ValidatableComponentTests
{
    #region Public methods

    ///<summary>
    ///Tests that GetErrors returns an empty enumerable when propertyName is empty string and no errors exist.
    ///</summary>
    [TestMethod]
    public void GetErrors_EmptyPropertyNameWithNoErrors_ReturnsEmptyEnumerable()
    {
        // Arrange
        TestValidatableComponent component = new();

        // Act
        IEnumerable result = component.GetErrors(string.Empty);

        // Assert
        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(Array.Empty<string>(), result.Cast<string>().ToArray());
    }

    ///<summary>
    ///Tests that GetErrors returns an empty enumerable when propertyName is null and no errors exist.
    ///</summary>
    [TestMethod]
    public void GetErrors_NullPropertyNameWithNoErrors_ReturnsEmptyEnumerable()
    {
        // Arrange
        TestValidatableComponent component = new();

        // Act
        IEnumerable result = component.GetErrors(null);

        // Assert
        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(Array.Empty<string>(), result.Cast<string>().ToArray());
    }

    ///<summary>
    ///Tests that GetErrors returns an empty collection when the property has no errors.
    ///</summary>
    [TestMethod]
    public void GetErrors_PropertyNameWithNoErrors_ReturnsEmptyCollection()
    {
        // Arrange
        TestValidatableComponent component = new();

        // Act
        IEnumerable result = component.GetErrors("SomeProperty");

        // Assert
        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(Array.Empty<string>(), result.Cast<string>().ToArray());
    }

    ///<summary>
    ///Tests that <see cref="ValidatableComponent.HasErrors"/> returns true after setting errors via <see
    ///cref="ValidatableComponent.SetErrors"/>.
    ///</summary>
    [TestMethod]
    public void HasErrors_AfterSettingErrors_ReturnsTrue()
    {
        // Arrange
        TestValidatableComponent component = new();
        List<string> errors = ["Error 1", "Error 2"];

        // Act
        component.SetErrorsPublic(errors, "TestProperty");
        bool hasErrors = component.HasErrors;

        // Assert
        Assert.IsTrue(hasErrors);
    }

    ///<summary>
    ///Tests that <see cref="ValidatableComponent.HasErrors"/> returns false after setting errors with a list containing
    ///only whitespace strings.
    ///</summary>
    [TestMethod]
    public void HasErrors_AfterSettingErrorsWithOnlyWhitespace_ReturnsFalse()
    {
        // Arrange
        TestValidatableComponent component = new();

        // Act
        component.SetErrorsPublic(new List<string> { "   ", "\t", "\n" }, "TestProperty");
        bool hasErrors = component.HasErrors;

        // Assert
        Assert.IsFalse(hasErrors);
    }

    ///<summary>
    ///Tests that <see cref="ValidatableComponent.HasErrors"/> returns false when no errors have been added to the
    ///component.
    ///</summary>
    [TestMethod]
    public void HasErrors_WhenNoErrorsAdded_ReturnsFalse()
    {
        // Arrange
        TestValidatableComponent component = new();

        // Act
        bool hasErrors = component.HasErrors;

        // Assert
        Assert.IsFalse(hasErrors);
    }

    ///<summary>
    ///Tests that OnErrorsChanged can be overridden in derived classes.
    ///</summary>
    [TestMethod]
    public void OnErrorsChanged_WhenOverridden_AllowsCustomBehavior()
    {
        // Arrange
        OverridableTestComponent component = new();

        // Act
        component.InvokeOnErrorsChanged("TestProperty");

        // Assert
        Assert.IsTrue(component.OnErrorsChangedWasCalled);
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with boolean value.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_BooleanValue_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty("TestProperty", true);
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with double NaN value.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_DoubleNaN_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty("TestProperty", double.NaN);
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with double negative infinity.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_DoubleNegativeInfinity_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty("TestProperty", double.NegativeInfinity);
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with double positive infinity.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_DoublePositiveInfinity_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty("TestProperty", double.PositiveInfinity);
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with empty property name.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_EmptyPropertyName_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty(string.Empty, "value");
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with integer maximum value.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_IntMaxValue_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty("TestProperty", int.MaxValue);
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with integer minimum value.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_IntMinValue_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty("TestProperty", int.MinValue);
    }

    [TestMethod]
    public void ValidateProperty_NullPropertyNameAndNullValue_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty(null, null);
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with null value.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_NullValue_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty("TestProperty", null);
    }

    ///<summary>
    ///Tests that ValidateProperty can be overridden and is called multiple times.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_OverriddenMethod_CalledMultipleTimes()
    {
        // Arrange
        OverriddenValidatableComponent component = new();

        // Act
        component.PublicValidateProperty("Property1", 1);
        component.PublicValidateProperty("Property2", 2);
        component.PublicValidateProperty("Property3", 3);

        // Assert
        Assert.AreEqual(3, component.CallCount);
    }

    ///<summary>
    ///Tests that ValidateProperty can be overridden and is called when invoked.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_OverriddenMethod_IsCalled()
    {
        // Arrange
        OverriddenValidatableComponent component = new();

        // Act
        component.PublicValidateProperty("TestProperty", "test");

        // Assert
        Assert.AreEqual(1, component.CallCount);
    }

    ///<summary>
    ///Tests that ValidateProperty can be overridden and receives correct property name.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_OverriddenMethod_ReceivesCorrectPropertyName()
    {
        // Arrange
        OverriddenValidatableComponent component = new();
        string propertyName = "MyProperty";

        // Act
        component.PublicValidateProperty(propertyName, 123);

        // Assert
        Assert.AreEqual(propertyName, component.LastPropertyName);
    }

    ///<summary>
    ///Tests that ValidateProperty can be overridden and receives correct value.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_OverriddenMethod_ReceivesCorrectValue()
    {
        // Arrange
        OverriddenValidatableComponent component = new();
        int value = 456;

        // Act
        component.PublicValidateProperty("TestProperty", value);

        // Assert
        Assert.AreEqual(value, component.LastValue);
    }

    ///<summary>
    ///Tests that ValidateProperty can be overridden and receives null property name correctly.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_OverriddenMethodWithNullPropertyName_ReceivesNull()
    {
        // Arrange
        OverriddenValidatableComponent component = new();

        // Act
        component.PublicValidateProperty(null, "value");

        // Assert
        Assert.IsNull(component.LastPropertyName);
    }

    ///<summary>
    ///Tests that ValidateProperty can be overridden and receives null value correctly.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_OverriddenMethodWithNullValue_ReceivesNull()
    {
        // Arrange
        OverriddenValidatableComponent component = new();

        // Act
        component.PublicValidateProperty("TestProperty", null);

        // Assert
        Assert.IsNull(component.LastValue);
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with property name containing special characters.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_SpecialCharactersInPropertyName_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty("Property@#$%^&*()", 42);
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with string value.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_StringValue_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty("TestProperty", "test value");
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with very long property name.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_VeryLongPropertyName_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();
        string longPropertyName = new('A', 10000);

        // Act & Assert
        component.PublicValidateProperty(longPropertyName, true);
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with whitespace-only property name.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_WhitespacePropertyName_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty("   ", 123);
    }

    ///<summary>
    ///Tests that ValidateProperty does not throw when called with zero value.
    ///</summary>
    [TestMethod]
    public void ValidateProperty_ZeroValue_DoesNotThrow()
    {
        // Arrange
        TestableValidatableComponent component = new();

        // Act & Assert
        component.PublicValidateProperty("TestProperty", 0);
    }
    #endregion

    ///<summary>
    ///Concrete test implementation of <see cref="ValidatableComponent"/> for testing purposes. Exposes protected
    ///methods as public to allow testing.
    ///</summary>
    sealed class TestValidatableComponent : ValidatableComponent
    {
        #region Public methods
        public void AddErrorPublic(string error, string? propertyName = null) { AddError(error, propertyName); }
        public void ClearAllErrorsPublic() { ClearAllErrors(); }
        public void ClearErrorsPublic(string? propertyName = null) { ClearErrors(propertyName); }
        public void SetErrorsPublic(IEnumerable<string> errors, string? propertyName = null) { SetErrors(errors, propertyName); }
        #endregion
    }

    ///<summary>
    ///Testable implementation of ValidatableComponent that exposes protected members.
    ///</summary>
    class TestableValidatableComponent : ValidatableComponent
    {
        #region Public methods

        ///<summary>
        ///Exposes the protected ValidateProperty method for testing.
        ///</summary>
        public void PublicValidateProperty(string? propertyName, object? value) { ValidateProperty(propertyName, value); }
        #endregion
    }

    ///<summary>
    ///Testable implementation of ValidatableComponent that overrides ValidateProperty to track calls.
    ///</summary>
    class OverriddenValidatableComponent : ValidatableComponent
    {
        #region Protected methods
        protected override void ValidateProperty(string? propertyName, object? value)
        {
            LastPropertyName = propertyName;
            LastValue = value;
            CallCount++;
            base.ValidateProperty(propertyName, value);
        }
        #endregion

        #region Public methods
        public void PublicValidateProperty(string? propertyName, object? value) { ValidateProperty(propertyName, value); }
        #endregion

        #region Public properties
        public int CallCount { get; private set; }

        public string? LastPropertyName { get; private set; }

        public object? LastValue { get; private set; }
        #endregion
    }

    ///<summary>
    ///Helper class to test virtual method override capability.
    ///</summary>
    class OverridableTestComponent : ValidatableComponent
    {
        #region Protected methods
        protected override void OnErrorsChanged(string? propertyName)
        {
            OnErrorsChangedWasCalled = true;
            base.OnErrorsChanged(propertyName);
        }
        #endregion

        #region Public methods
        public void InvokeOnErrorsChanged(string? propertyName) { OnErrorsChanged(propertyName); }
        #endregion

        #region Public properties
        public bool OnErrorsChangedWasCalled { get; private set; }
        #endregion
    }
}