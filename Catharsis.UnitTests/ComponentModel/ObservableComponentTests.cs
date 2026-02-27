using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Catharsis;
using Catharsis.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Catharsis.ComponentModel.UnitTests;


/// <summary>
/// Unit tests for the <see cref="ObservableComponent"/> class.
/// </summary>
[TestClass]
public partial class ObservableComponentTests
{
    /// <summary>
    /// Testable implementation of ObservableComponent that exposes protected members for testing.
    /// </summary>
    private class TestableObservableComponent : ObservableComponent
    {
        /// <summary>
        /// Exposes the protected Dispose method for testing.
        /// </summary>
        /// <param name="disposing">True to release managed resources.</param>
        public void PublicDispose(bool disposing)
        {
            Dispose(disposing);
        }

        /// <summary>
        /// Exposes the protected OnPropertyChanged method for testing.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        public void RaisePropertyChanged(string? propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Exposes the protected OnPropertyChanging method for testing.
        /// </summary>
        /// <param name="propertyName">The name of the property that is changing.</param>
        public void RaisePropertyChanging(string? propertyName)
        {
            OnPropertyChanging(propertyName);
        }
    }

    /// <summary>
    /// Tests that SetProperty returns false when the field value equals the new value.
    /// </summary>
    [TestMethod]
    public void SetProperty_SameValue_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = 42;

        // Act
        bool result = component.TestSetProperty(ref field, 42, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(42, field);
    }

    /// <summary>
    /// Tests that SetProperty returns true when the field value differs from the new value.
    /// </summary>
    [TestMethod]
    public void SetProperty_DifferentValue_ReturnsTrue()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = 42;

        // Act
        bool result = component.TestSetProperty(ref field, 100, "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(100, field);
    }

    /// <summary>
    /// Tests that SetProperty does not raise events when the value has not changed.
    /// </summary>
    [TestMethod]
    public void SetProperty_SameValue_DoesNotRaiseEvents()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = 42;
        bool propertyChangingRaised = false;
        bool propertyChangedRaised = false;
        component.PropertyChanging += (s, e) => propertyChangingRaised = true;
        component.PropertyChanged += (s, e) => propertyChangedRaised = true;

        // Act
        component.TestSetProperty(ref field, 42, "TestProperty");

        // Assert
        Assert.IsFalse(propertyChangingRaised);
        Assert.IsFalse(propertyChangedRaised);
    }

    /// <summary>
    /// Tests that SetProperty raises PropertyChanging event when the value changes.
    /// </summary>
    [TestMethod]
    public void SetProperty_DifferentValue_RaisesPropertyChangingEvent()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = 42;
        bool eventRaised = false;
        string? eventPropertyName = null;
        component.PropertyChanging += (s, e) =>
        {
            eventRaised = true;
            eventPropertyName = e.PropertyName;
        };

        // Act
        component.TestSetProperty(ref field, 100, "TestProperty");

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("TestProperty", eventPropertyName);
    }

    /// <summary>
    /// Tests that SetProperty raises PropertyChanged event when the value changes.
    /// </summary>
    [TestMethod]
    public void SetProperty_DifferentValue_RaisesPropertyChangedEvent()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = 42;
        bool eventRaised = false;
        string? eventPropertyName = null;
        component.PropertyChanged += (s, e) =>
        {
            eventRaised = true;
            eventPropertyName = e.PropertyName;
        };

        // Act
        component.TestSetProperty(ref field, 100, "TestProperty");

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual("TestProperty", eventPropertyName);
    }

    /// <summary>
    /// Tests that SetProperty raises PropertyChanging before PropertyChanged.
    /// </summary>
    [TestMethod]
    public void SetProperty_DifferentValue_RaisesEventsInCorrectOrder()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = 42;
        var eventOrder = new List<string>();
        component.PropertyChanging += (s, e) => eventOrder.Add("Changing");
        component.PropertyChanged += (s, e) => eventOrder.Add("Changed");

        // Act
        component.TestSetProperty(ref field, 100, "TestProperty");

        // Assert
        Assert.AreEqual(2, eventOrder.Count);
        Assert.AreEqual("Changing", eventOrder[0]);
        Assert.AreEqual("Changed", eventOrder[1]);
    }

    /// <summary>
    /// Tests that SetProperty updates the field before raising PropertyChanged event.
    /// </summary>
    [TestMethod]
    public void SetProperty_DifferentValue_UpdatesFieldBeforePropertyChangedEvent()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = 42;
        int fieldValueDuringEvent = 0;
        component.PropertyChanged += (s, e) => fieldValueDuringEvent = field;

        // Act
        component.TestSetProperty(ref field, 100, "TestProperty");

        // Assert
        Assert.AreEqual(100, fieldValueDuringEvent);
    }

    /// <summary>
    /// Tests that SetProperty with null property name raises events with null property name.
    /// </summary>
    [TestMethod]
    public void SetProperty_NullPropertyName_RaisesEventsWithNullPropertyName()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = 42;
        string? changingPropertyName = "NotNull";
        string? changedPropertyName = "NotNull";
        component.PropertyChanging += (s, e) => changingPropertyName = e.PropertyName;
        component.PropertyChanged += (s, e) => changedPropertyName = e.PropertyName;

        // Act
        component.TestSetProperty(ref field, 100, null);

        // Assert
        Assert.IsNull(changingPropertyName);
        Assert.IsNull(changedPropertyName);
    }

    /// <summary>
    /// Tests that SetProperty with empty string property name raises events with empty property name.
    /// </summary>
    [TestMethod]
    public void SetProperty_EmptyPropertyName_RaisesEventsWithEmptyPropertyName()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = 42;
        string? changingPropertyName = null;
        string? changedPropertyName = null;
        component.PropertyChanging += (s, e) => changingPropertyName = e.PropertyName;
        component.PropertyChanged += (s, e) => changedPropertyName = e.PropertyName;

        // Act
        component.TestSetProperty(ref field, 100, string.Empty);

        // Assert
        Assert.AreEqual(string.Empty, changingPropertyName);
        Assert.AreEqual(string.Empty, changedPropertyName);
    }

    /// <summary>
    /// Tests that SetProperty with whitespace property name raises events with whitespace property name.
    /// </summary>
    [TestMethod]
    public void SetProperty_WhitespacePropertyName_RaisesEventsWithWhitespacePropertyName()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = 42;
        string? changingPropertyName = null;
        string? changedPropertyName = null;
        component.PropertyChanging += (s, e) => changingPropertyName = e.PropertyName;
        component.PropertyChanged += (s, e) => changedPropertyName = e.PropertyName;

        // Act
        component.TestSetProperty(ref field, 100, "   ");

        // Assert
        Assert.AreEqual("   ", changingPropertyName);
        Assert.AreEqual("   ", changedPropertyName);
    }

    /// <summary>
    /// Tests that SetProperty works correctly with reference types (string).
    /// </summary>
    [TestMethod]
    public void SetProperty_StringType_UpdatesFieldAndRaisesEvents()
    {
        // Arrange
        var component = new TestObservableComponent();
        string? field = "old";
        bool eventRaised = false;
        component.PropertyChanged += (s, e) => eventRaised = true;

        // Act
        bool result = component.TestSetProperty(ref field, "new", "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual("new", field);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that SetProperty returns false when both string values are null.
    /// </summary>
    [TestMethod]
    public void SetProperty_BothStringValuesNull_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        string? field = null;

        // Act
        bool result = component.TestSetProperty(ref field, null, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(field);
    }

    /// <summary>
    /// Tests that SetProperty returns true when changing string from null to non-null.
    /// </summary>
    [TestMethod]
    public void SetProperty_StringFromNullToValue_ReturnsTrue()
    {
        // Arrange
        var component = new TestObservableComponent();
        string? field = null;

        // Act
        bool result = component.TestSetProperty(ref field, "value", "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual("value", field);
    }

    /// <summary>
    /// Tests that SetProperty returns true when changing string from non-null to null.
    /// </summary>
    [TestMethod]
    public void SetProperty_StringFromValueToNull_ReturnsTrue()
    {
        // Arrange
        var component = new TestObservableComponent();
        string? field = "value";

        // Act
        bool result = component.TestSetProperty(ref field, null, "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(field);
    }

    /// <summary>
    /// Tests that SetProperty returns false when both string values are empty.
    /// </summary>
    [TestMethod]
    public void SetProperty_BothStringValuesEmpty_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        string field = string.Empty;

        // Act
        bool result = component.TestSetProperty(ref field, string.Empty, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(string.Empty, field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly with nullable value types when both are null.
    /// </summary>
    [TestMethod]
    public void SetProperty_NullableIntBothNull_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        int? field = null;

        // Act
        bool result = component.TestSetProperty(ref field, null, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly when changing nullable int from null to value.
    /// </summary>
    [TestMethod]
    public void SetProperty_NullableIntFromNullToValue_ReturnsTrue()
    {
        // Arrange
        var component = new TestObservableComponent();
        int? field = null;

        // Act
        bool result = component.TestSetProperty(ref field, 42, "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(42, field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly when changing nullable int from value to null.
    /// </summary>
    [TestMethod]
    public void SetProperty_NullableIntFromValueToNull_ReturnsTrue()
    {
        // Arrange
        var component = new TestObservableComponent();
        int? field = 42;

        // Act
        bool result = component.TestSetProperty(ref field, null, "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly with double NaN values (NaN equals NaN using EqualityComparer).
    /// </summary>
    [TestMethod]
    public void SetProperty_DoubleNaN_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        double field = double.NaN;

        // Act
        bool result = component.TestSetProperty(ref field, double.NaN, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.IsTrue(double.IsNaN(field));
    }

    /// <summary>
    /// Tests that SetProperty works correctly with double positive infinity.
    /// </summary>
    [TestMethod]
    public void SetProperty_DoublePositiveInfinity_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        double field = double.PositiveInfinity;

        // Act
        bool result = component.TestSetProperty(ref field, double.PositiveInfinity, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(double.PositiveInfinity, field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly with double negative infinity.
    /// </summary>
    [TestMethod]
    public void SetProperty_DoubleNegativeInfinity_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        double field = double.NegativeInfinity;

        // Act
        bool result = component.TestSetProperty(ref field, double.NegativeInfinity, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(double.NegativeInfinity, field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly when changing from NaN to a normal value.
    /// </summary>
    [TestMethod]
    public void SetProperty_DoubleFromNaNToValue_ReturnsTrue()
    {
        // Arrange
        var component = new TestObservableComponent();
        double field = double.NaN;

        // Act
        bool result = component.TestSetProperty(ref field, 42.0, "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(42.0, field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly with integer minimum value.
    /// </summary>
    [TestMethod]
    public void SetProperty_IntMinValue_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = int.MinValue;

        // Act
        bool result = component.TestSetProperty(ref field, int.MinValue, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(int.MinValue, field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly with integer maximum value.
    /// </summary>
    [TestMethod]
    public void SetProperty_IntMaxValue_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = int.MaxValue;

        // Act
        bool result = component.TestSetProperty(ref field, int.MaxValue, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(int.MaxValue, field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly when changing from minimum to maximum integer value.
    /// </summary>
    [TestMethod]
    public void SetProperty_IntFromMinToMax_ReturnsTrue()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = int.MinValue;

        // Act
        bool result = component.TestSetProperty(ref field, int.MaxValue, "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(int.MaxValue, field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly with zero value.
    /// </summary>
    [TestMethod]
    public void SetProperty_IntZeroValue_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = 0;

        // Act
        bool result = component.TestSetProperty(ref field, 0, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(0, field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly when changing from zero to non-zero.
    /// </summary>
    [TestMethod]
    public void SetProperty_IntFromZeroToNonZero_ReturnsTrue()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = 0;

        // Act
        bool result = component.TestSetProperty(ref field, 42, "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(42, field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly with boolean values.
    /// </summary>
    [TestMethod]
    public void SetProperty_BooleanFromFalseToTrue_ReturnsTrue()
    {
        // Arrange
        var component = new TestObservableComponent();
        bool field = false;

        // Act
        bool result = component.TestSetProperty(ref field, true, "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(field);
    }

    /// <summary>
    /// Tests that SetProperty returns false when boolean values are the same.
    /// </summary>
    [TestMethod]
    public void SetProperty_BooleanSameValue_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        bool field = true;

        // Act
        bool result = component.TestSetProperty(ref field, true, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.IsTrue(field);
    }

    /// <summary>
    /// Tests that SetProperty works correctly with custom reference types.
    /// </summary>
    [TestMethod]
    public void SetProperty_CustomObjectType_UpdatesFieldAndRaisesEvents()
    {
        // Arrange
        var component = new TestObservableComponent();
        var oldObject = new object();
        var newObject = new object();
        object? field = oldObject;
        bool eventRaised = false;
        component.PropertyChanged += (s, e) => eventRaised = true;

        // Act
        bool result = component.TestSetProperty(ref field, newObject, "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.AreSame(newObject, field);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that SetProperty returns false when same object reference is set.
    /// </summary>
    [TestMethod]
    public void SetProperty_SameObjectReference_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        var obj = new object();
        object? field = obj;

        // Act
        bool result = component.TestSetProperty(ref field, obj, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.AreSame(obj, field);
    }

    /// <summary>
    /// Tests that SetProperty with default value for value type returns false.
    /// </summary>
    [TestMethod]
    public void SetProperty_DefaultValueType_ReturnsFalse()
    {
        // Arrange
        var component = new TestObservableComponent();
        int field = default;

        // Act
        bool result = component.TestSetProperty(ref field, default, "TestProperty");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(default(int), field);
    }

    /// <summary>
    /// Tests that SetProperty with very long string updates correctly.
    /// </summary>
    [TestMethod]
    public void SetProperty_VeryLongString_UpdatesFieldAndRaisesEvents()
    {
        // Arrange
        var component = new TestObservableComponent();
        string? field = "short";
        string longString = new string('x', 10000);

        // Act
        bool result = component.TestSetProperty(ref field, longString, "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(longString, field);
    }

    /// <summary>
    /// Tests that SetProperty with string containing special characters works correctly.
    /// </summary>
    [TestMethod]
    public void SetProperty_StringWithSpecialCharacters_UpdatesFieldAndRaisesEvents()
    {
        // Arrange
        var component = new TestObservableComponent();
        string? field = "normal";
        string specialString = "Hello\r\n\t\0World!@#$%^&*()";

        // Act
        bool result = component.TestSetProperty(ref field, specialString, "TestProperty");

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(specialString, field);
    }

    /// <summary>
    /// Helper class to expose protected SetProperty method for testing.
    /// </summary>
    private class TestObservableComponent : ObservableComponent
    {
        public bool TestSetProperty<T>(ref T field, T value, string? propertyName)
        {
            return SetProperty(ref field, value, propertyName);
        }
    }
    #region OnPropertyChanged Tests

    #endregion

    #region Test Helper Class

    #endregion

}