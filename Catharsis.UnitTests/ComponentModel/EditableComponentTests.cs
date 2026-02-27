using System.Reflection;

namespace Catharsis.ComponentModel.UnitTests;


/// <summary>
/// Tests for the <see cref="EditableComponent"/> class.
/// </summary>
[TestClass]
public partial class EditableComponentTests
{
    /// <summary>
    /// Tests that Dispose with disposing=true sets IsEditing to false.
    /// </summary>
    [TestMethod]
    public void Dispose_DisposingTrue_SetsIsEditingToFalse()
    {
        // Arrange
        var component = new TestEditableComponent();
        component.BeginEdit();
        Assert.IsTrue(component.IsEditing);

        // Act
        component.Dispose();

        // Assert
        Assert.IsFalse(component.IsEditing);
    }

    /// <summary>
    /// Tests that Dispose with disposing=true when IsEditing is already false keeps it false.
    /// </summary>
    [TestMethod]
    public void Dispose_DisposingTrueWhenNotEditing_IsEditingRemainsFalse()
    {
        // Arrange
        var component = new TestEditableComponent();
        Assert.IsFalse(component.IsEditing);

        // Act
        component.Dispose();

        // Assert
        Assert.IsFalse(component.IsEditing);
    }

    /// <summary>
    /// Tests that Dispose can be called multiple times without error.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var component = new TestEditableComponent();
        component.BeginEdit();

        // Act
        component.Dispose();
        component.Dispose();

        // Assert
        Assert.IsFalse(component.IsEditing);
    }

    /// <summary>
    /// Tests that after disposal, IsEditing remains false even after multiple dispose calls.
    /// </summary>
    [TestMethod]
    public void Dispose_MultipleCallsAfterBeginEdit_IsEditingStaysFalse()
    {
        // Arrange
        var component = new TestEditableComponent();
        component.BeginEdit();
        Assert.IsTrue(component.IsEditing);

        // Act
        component.Dispose();
        Assert.IsFalse(component.IsEditing);
        component.Dispose();

        // Assert
        Assert.IsFalse(component.IsEditing);
    }

    /// <summary>
    /// Tests that Dispose sets IsEditing to false regardless of previous state.
    /// </summary>
    [TestMethod]
    public void Dispose_VariousIsEditingStates_AlwaysSetsToFalse()
    {
        // Arrange - IsEditing = true
        var component1 = new TestEditableComponent();
        component1.BeginEdit();

        // Arrange - IsEditing = false
        var component2 = new TestEditableComponent();

        // Act
        component1.Dispose();
        component2.Dispose();

        // Assert
        Assert.IsFalse(component1.IsEditing);
        Assert.IsFalse(component2.IsEditing);
    }

    /// <summary>
    /// Helper class for testing EditableComponent.
    /// </summary>
    private class TestEditableComponent : EditableComponent
    {
        private string? _testProperty;

    }

    /// <summary>
    /// Tests that CancelEdit sets IsEditing to false when in edit mode with snapshot.
    /// </summary>
    [TestMethod]
    public void CancelEdit_WhenEditingWithSnapshot_SetsIsEditingToFalse()
    {
        // Arrange
        var component = new TestEditableComponent();
        component.BeginEdit();

        // Act
        component.CancelEdit();

        // Assert
        Assert.IsFalse(component.IsEditing);
    }

    /// <summary>
    /// Tests that CancelEdit can be called multiple times when not editing without error.
    /// </summary>
    [TestMethod]
    public void CancelEdit_CalledMultipleTimesWhenNotEditing_DoesNotThrow()
    {
        // Arrange
        var component = new TestEditableComponent();

        // Act & Assert
        component.CancelEdit();
        component.CancelEdit();
        component.CancelEdit();

        Assert.IsFalse(component.IsEditing);
    }

    /// <summary>
    /// Tests that EndEdit does nothing when not currently in edit mode.
    /// </summary>
    [TestMethod]
    public void EndEdit_WhenNotEditing_DoesNothing()
    {
        // Arrange
        TestEditableComponent component = new();
        bool initialEditingState = component.IsEditing;

        // Act
        component.EndEdit();

        // Assert
        Assert.IsFalse(initialEditingState);
        Assert.IsFalse(component.IsEditing);
    }

    /// <summary>
    /// Tests that EndEdit successfully ends edit mode and sets IsEditing to false.
    /// </summary>
    [TestMethod]
    public void EndEdit_WhenEditing_SetsIsEditingToFalse()
    {
        // Arrange
        TestEditableComponent component = new();
        component.BeginEdit();
        Assert.IsTrue(component.IsEditing);

        // Act
        component.EndEdit();

        // Assert
        Assert.IsFalse(component.IsEditing);
    }

    /// <summary>
    /// Tests that EndEdit can be called multiple times without error when not editing.
    /// </summary>
    [TestMethod]
    public void EndEdit_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        TestEditableComponent component = new();
        component.BeginEdit();
        component.EndEdit();

        // Act & Assert
        component.EndEdit(); // Should not throw
        Assert.IsFalse(component.IsEditing);
    }

    /// <summary>
    /// Tests that BeginEdit sets IsEditing to true when not currently editing.
    /// </summary>
    [TestMethod]
    public void BeginEdit_NotEditing_SetsIsEditingToTrue()
    {
        // Arrange
        var component = new TestEditableComponent();

        // Act
        component.BeginEdit();

        // Assert
        Assert.IsTrue(component.IsEditing);
    }

    /// <summary>
    /// Tests that GetEditableProperties excludes read-only properties.
    /// </summary>
    [TestMethod]
    public void GetEditableProperties_WithReadOnlyProperty_ExcludesReadOnlyProperty()
    {
        // Arrange
        var component = new TestEditableWithReadOnlyProperty();

        // Act
        var properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsFalse(properties.Any(p => p.Name == nameof(TestEditableWithReadOnlyProperty.ReadOnlyProperty)),
            "Read-only properties should be excluded from editable properties.");
    }

    /// <summary>
    /// Tests that GetEditableProperties excludes write-only properties.
    /// </summary>
    [TestMethod]
    public void GetEditableProperties_WithWriteOnlyProperty_ExcludesWriteOnlyProperty()
    {
        // Arrange
        var component = new TestEditableWithWriteOnlyProperty();

        // Act
        var properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsFalse(properties.Any(p => p.Name == nameof(TestEditableWithWriteOnlyProperty.WriteOnlyProperty)),
            "Write-only properties should be excluded from editable properties.");
    }

    /// <summary>
    /// Tests that GetEditableProperties excludes indexer properties.
    /// </summary>
    [TestMethod]
    public void GetEditableProperties_WithIndexerProperty_ExcludesIndexer()
    {
        // Arrange
        var component = new TestEditableWithIndexer();

        // Act
        var properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsFalse(properties.Any(p => p.GetIndexParameters().Length > 0),
            "Indexer properties should be excluded from editable properties.");
    }

    /// <summary>
    /// Tests that GetEditableProperties excludes properties named 'Site'.
    /// </summary>
    [TestMethod]
    public void GetEditableProperties_WithPropertyNamedSite_ExcludesSiteProperty()
    {
        // Arrange
        var component = new TestEditableWithValidProperties();

        // Act
        var properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsFalse(properties.Any(p => p.Name == "Site"),
            "Properties named 'Site' should be excluded from editable properties.");
    }

    /// <summary>
    /// Tests that GetEditableProperties excludes properties named 'IsEditing'.
    /// </summary>
    [TestMethod]
    public void GetEditableProperties_WithPropertyNamedIsEditing_ExcludesIsEditingProperty()
    {
        // Arrange
        var component = new TestEditableWithValidProperties();

        // Act
        var properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsFalse(properties.Any(p => p.Name == "IsEditing"),
            "Properties named 'IsEditing' should be excluded from editable properties.");
    }

    /// <summary>
    /// Tests that GetEditableProperties includes valid readable and writable properties.
    /// </summary>
    [TestMethod]
    public void GetEditableProperties_WithValidProperties_IncludesValidProperties()
    {
        // Arrange
        var component = new TestEditableWithValidProperties();

        // Act
        var properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsTrue(properties.Any(p => p.Name == nameof(TestEditableWithValidProperties.ValidProperty1)),
            "Valid properties should be included in editable properties.");
        Assert.IsTrue(properties.Any(p => p.Name == nameof(TestEditableWithValidProperties.ValidProperty2)),
            "Valid properties should be included in editable properties.");
    }

    /// <summary>
    /// Tests that GetEditableProperties returns empty enumerable when no properties match criteria.
    /// </summary>
    [TestMethod]
    public void GetEditableProperties_WithNoValidProperties_ReturnsEmptyEnumerable()
    {
        // Arrange
        var component = new TestEditableWithNoValidProperties();

        // Act
        var properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.AreEqual(0, properties.Count,
            "Should return empty enumerable when no properties match the editable criteria.");
    }

    /// <summary>
    /// Tests that GetEditableProperties only returns public instance properties.
    /// </summary>
    [TestMethod]
    public void GetEditableProperties_OnlyReturnsPublicInstanceProperties_ExcludesPrivateAndStatic()
    {
        // Arrange
        var component = new TestEditableWithMixedAccessProperties();

        // Act
        var properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsTrue(properties.All(p => p.GetMethod?.IsPublic == true && !p.GetMethod.IsStatic),
            "Only public instance properties should be returned.");
        Assert.IsFalse(properties.Any(p => p.Name == "PrivateProperty"),
            "Private properties should be excluded.");
    }

    /// <summary>
    /// Tests that GetEditableProperties correctly filters mixed valid and invalid properties.
    /// </summary>
    [TestMethod]
    public void GetEditableProperties_WithMixedProperties_FiltersCorrectly()
    {
        // Arrange
        var component = new TestEditableWithMixedProperties();

        // Act
        var properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsTrue(properties.Any(p => p.Name == nameof(TestEditableWithMixedProperties.ValidProperty)),
            "Valid property should be included.");
        Assert.IsFalse(properties.Any(p => p.Name == nameof(TestEditableWithMixedProperties.ReadOnlyProperty)),
            "Read-only property should be excluded.");
        Assert.IsFalse(properties.Any(p => p.Name == "Site"),
            "Site property should be excluded.");
        Assert.IsFalse(properties.Any(p => p.Name == "IsEditing"),
            "IsEditing property should be excluded.");
    }

    /// <summary>
    /// Tests that GetEditableProperties returns properties with both CanRead and CanWrite true.
    /// </summary>
    [TestMethod]
    public void GetEditableProperties_AllReturnedProperties_HaveReadAndWriteCapability()
    {
        // Arrange
        var component = new TestEditableWithValidProperties();

        // Act
        var properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsTrue(properties.All(p => p.CanRead && p.CanWrite),
            "All returned properties should have both CanRead and CanWrite set to true.");
    }

    /// <summary>
    /// Tests that GetEditableProperties returns properties with no index parameters.
    /// </summary>
    [TestMethod]
    public void GetEditableProperties_AllReturnedProperties_HaveNoIndexParameters()
    {
        // Arrange
        var component = new TestEditableWithValidProperties();

        // Act
        var properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsTrue(properties.All(p => p.GetIndexParameters().Length == 0),
            "All returned properties should have no index parameters.");
    }

    #region Test Helper Classes

    /// <summary>
    /// Test helper class with a read-only property.
    /// </summary>
    private sealed class TestEditableWithReadOnlyProperty : EditableComponent
    {
        public string ReadOnlyProperty { get; } = "ReadOnly";
        public string ValidProperty { get; set; } = "Valid";

        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic()
        {
            return GetEditableProperties();
        }
    }

    /// <summary>
    /// Test helper class with a write-only property.
    /// </summary>
    private sealed class TestEditableWithWriteOnlyProperty : EditableComponent
    {
        private string _writeOnly = string.Empty;

        public string WriteOnlyProperty
        {
            set => _writeOnly = value;
        }

        public string ValidProperty { get; set; } = "Valid";

        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic()
        {
            return GetEditableProperties();
        }
    }

    /// <summary>
    /// Test helper class with an indexer property.
    /// </summary>
    private sealed class TestEditableWithIndexer : EditableComponent
    {
        private readonly Dictionary<int, string> _data = new();

        public string this[int index]
        {
            get => _data.TryGetValue(index, out var value) ? value : string.Empty;
            set => _data[index] = value;
        }

        public string ValidProperty { get; set; } = "Valid";

        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic()
        {
            return GetEditableProperties();
        }
    }

    /// <summary>
    /// Test helper class with valid editable properties.
    /// </summary>
    private sealed class TestEditableWithValidProperties : EditableComponent
    {
        public string ValidProperty1 { get; set; } = "Value1";
        public int ValidProperty2 { get; set; } = 42;

        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic()
        {
            return GetEditableProperties();
        }
    }

    /// <summary>
    /// Test helper class with no valid properties (only inherited ones that are filtered).
    /// </summary>
    private sealed class TestEditableWithNoValidProperties : EditableComponent
    {
        public string ReadOnlyProp { get; } = "ReadOnly";

        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic()
        {
            return GetEditableProperties();
        }
    }

    /// <summary>
    /// Test helper class with mixed access level properties.
    /// </summary>
    private sealed class TestEditableWithMixedAccessProperties : EditableComponent
    {
        private string PrivateProperty { get; set; } = "Private";
        public string PublicProperty { get; set; } = "Public";

        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic()
        {
            return GetEditableProperties();
        }
    }

    /// <summary>
    /// Test helper class with a mix of valid and invalid properties.
    /// </summary>
    private sealed class TestEditableWithMixedProperties : EditableComponent
    {
        public string ValidProperty { get; set; } = "Valid";
        public string ReadOnlyProperty { get; } = "ReadOnly";

        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic()
        {
            return GetEditableProperties();
        }
    }

    #endregion
}