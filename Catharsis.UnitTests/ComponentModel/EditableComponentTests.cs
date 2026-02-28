using System.Reflection;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Tests for the <see cref="EditableComponent"/> class.
///</summary>
[TestClass]
public partial class EditableComponentTests
{
    #region Public methods

    ///<summary>
    ///Tests that BeginEdit sets IsEditing to true when not currently editing.
    ///</summary>
    [TestMethod]
    public void BeginEdit_NotEditing_SetsIsEditingToTrue()
    {
        // Arrange
        TestEditableComponent component = new TestEditableComponent();

        // Act
        component.BeginEdit();

        // Assert
        Assert.IsTrue(component.IsEditing);
    }

    ///<summary>
    ///Tests that CancelEdit can be called multiple times when not editing without error.
    ///</summary>
    [TestMethod]
    public void CancelEdit_CalledMultipleTimesWhenNotEditing_DoesNotThrow()
    {
        // Arrange
        TestEditableComponent component = new TestEditableComponent();

        // Act & Assert
        component.CancelEdit();
        component.CancelEdit();
        component.CancelEdit();

        Assert.IsFalse(component.IsEditing);
    }

    ///<summary>
    ///Tests that CancelEdit sets IsEditing to false when in edit mode with snapshot.
    ///</summary>
    [TestMethod]
    public void CancelEdit_WhenEditingWithSnapshot_SetsIsEditingToFalse()
    {
        // Arrange
        TestEditableComponent component = new TestEditableComponent();
        component.BeginEdit();

        // Act
        component.CancelEdit();

        // Assert
        Assert.IsFalse(component.IsEditing);
    }

    ///<summary>
    ///Tests that Dispose can be called multiple times without error.
    ///</summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        TestEditableComponent component = new TestEditableComponent();
        component.BeginEdit();

        // Act
        component.Dispose();
        component.Dispose();

        // Assert
        Assert.IsFalse(component.IsEditing);
    }

    ///<summary>
    ///Tests that Dispose with disposing=true sets IsEditing to false.
    ///</summary>
    [TestMethod]
    public void Dispose_DisposingTrue_SetsIsEditingToFalse()
    {
        // Arrange
        TestEditableComponent component = new TestEditableComponent();
        component.BeginEdit();
        Assert.IsTrue(component.IsEditing);

        // Act
        component.Dispose();

        // Assert
        Assert.IsFalse(component.IsEditing);
    }

    ///<summary>
    ///Tests that Dispose with disposing=true when IsEditing is already false keeps it false.
    ///</summary>
    [TestMethod]
    public void Dispose_DisposingTrueWhenNotEditing_IsEditingRemainsFalse()
    {
        // Arrange
        TestEditableComponent component = new TestEditableComponent();
        Assert.IsFalse(component.IsEditing);

        // Act
        component.Dispose();

        // Assert
        Assert.IsFalse(component.IsEditing);
    }

    ///<summary>
    ///Tests that after disposal, IsEditing remains false even after multiple dispose calls.
    ///</summary>
    [TestMethod]
    public void Dispose_MultipleCallsAfterBeginEdit_IsEditingStaysFalse()
    {
        // Arrange
        TestEditableComponent component = new TestEditableComponent();
        component.BeginEdit();
        Assert.IsTrue(component.IsEditing);

        // Act
        component.Dispose();
        Assert.IsFalse(component.IsEditing);
        component.Dispose();

        // Assert
        Assert.IsFalse(component.IsEditing);
    }

    ///<summary>
    ///Tests that Dispose sets IsEditing to false regardless of previous state.
    ///</summary>
    [TestMethod]
    public void Dispose_VariousIsEditingStates_AlwaysSetsToFalse()
    {
        // Arrange - IsEditing = true
        TestEditableComponent component1 = new TestEditableComponent();
        component1.BeginEdit();

        // Arrange - IsEditing = false
        TestEditableComponent component2 = new TestEditableComponent();

        // Act
        component1.Dispose();
        component2.Dispose();

        // Assert
        Assert.IsFalse(component1.IsEditing);
        Assert.IsFalse(component2.IsEditing);
    }

    ///<summary>
    ///Tests that EndEdit can be called multiple times without error when not editing.
    ///</summary>
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

    ///<summary>
    ///Tests that EndEdit successfully ends edit mode and sets IsEditing to false.
    ///</summary>
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

    ///<summary>
    ///Tests that EndEdit does nothing when not currently in edit mode.
    ///</summary>
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

    ///<summary>
    ///Tests that GetEditableProperties returns properties with no index parameters.
    ///</summary>
    [TestMethod]
    public void GetEditableProperties_AllReturnedProperties_HaveNoIndexParameters()
    {
        // Arrange
        TestEditableWithValidProperties component = new TestEditableWithValidProperties();

        // Act
        List<PropertyInfo> properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsTrue(properties.All(p => p.GetIndexParameters().Length == 0), "All returned properties should have no index parameters.");
    }

    ///<summary>
    ///Tests that GetEditableProperties returns properties with both CanRead and CanWrite true.
    ///</summary>
    [TestMethod]
    public void GetEditableProperties_AllReturnedProperties_HaveReadAndWriteCapability()
    {
        // Arrange
        TestEditableWithValidProperties component = new TestEditableWithValidProperties();

        // Act
        List<PropertyInfo> properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsTrue(properties.All(p => p.CanRead && p.CanWrite), "All returned properties should have both CanRead and CanWrite set to true.");
    }

    ///<summary>
    ///Tests that GetEditableProperties only returns public instance properties.
    ///</summary>
    [TestMethod]
    public void GetEditableProperties_OnlyReturnsPublicInstanceProperties_ExcludesPrivateAndStatic()
    {
        // Arrange
        TestEditableWithMixedAccessProperties component = new TestEditableWithMixedAccessProperties();

        // Act
        List<PropertyInfo> properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsTrue(properties.All(p => (p.GetMethod?.IsPublic == true) && !p.GetMethod.IsStatic), "Only public instance properties should be returned.");
        Assert.IsFalse(properties.Any(p => p.Name == "PrivateProperty"), "Private properties should be excluded.");
    }

    ///<summary>
    ///Tests that GetEditableProperties excludes indexer properties.
    ///</summary>
    [TestMethod]
    public void GetEditableProperties_WithIndexerProperty_ExcludesIndexer()
    {
        // Arrange
        TestEditableWithIndexer component = new TestEditableWithIndexer();

        // Act
        List<PropertyInfo> properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsFalse(properties.Any(p => p.GetIndexParameters().Length > 0), "Indexer properties should be excluded from editable properties.");
    }

    ///<summary>
    ///Tests that GetEditableProperties correctly filters mixed valid and invalid properties.
    ///</summary>
    [TestMethod]
    public void GetEditableProperties_WithMixedProperties_FiltersCorrectly()
    {
        // Arrange
        TestEditableWithMixedProperties component = new TestEditableWithMixedProperties();

        // Act
        List<PropertyInfo> properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsTrue(properties.Any(p => p.Name == nameof(EditableComponentTests.TestEditableWithMixedProperties.ValidProperty)), "Valid property should be included.");
        Assert.IsFalse(properties.Any(p => p.Name == nameof(EditableComponentTests.TestEditableWithMixedProperties.ReadOnlyProperty)), "Read-only property should be excluded.");
        Assert.IsFalse(properties.Any(p => p.Name == "Site"), "Site property should be excluded.");
        Assert.IsFalse(properties.Any(p => p.Name == "IsEditing"), "IsEditing property should be excluded.");
    }

    ///<summary>
    ///Tests that GetEditableProperties returns empty enumerable when no properties match criteria.
    ///</summary>
    [TestMethod]
    public void GetEditableProperties_WithNoValidProperties_ReturnsEmptyEnumerable()
    {
        // Arrange
        TestEditableWithNoValidProperties component = new TestEditableWithNoValidProperties();

        // Act
        List<PropertyInfo> properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsEmpty(properties, "Should return empty enumerable when no properties match the editable criteria.");
    }

    ///<summary>
    ///Tests that GetEditableProperties excludes properties named 'IsEditing'.
    ///</summary>
    [TestMethod]
    public void GetEditableProperties_WithPropertyNamedIsEditing_ExcludesIsEditingProperty()
    {
        // Arrange
        TestEditableWithValidProperties component = new TestEditableWithValidProperties();

        // Act
        List<PropertyInfo> properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsFalse(properties.Any(p => p.Name == "IsEditing"), "Properties named 'IsEditing' should be excluded from editable properties.");
    }

    ///<summary>
    ///Tests that GetEditableProperties excludes properties named 'Site'.
    ///</summary>
    [TestMethod]
    public void GetEditableProperties_WithPropertyNamedSite_ExcludesSiteProperty()
    {
        // Arrange
        TestEditableWithValidProperties component = new TestEditableWithValidProperties();

        // Act
        List<PropertyInfo> properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsFalse(properties.Any(p => p.Name == "Site"), "Properties named 'Site' should be excluded from editable properties.");
    }

    ///<summary>
    ///Tests that GetEditableProperties excludes read-only properties.
    ///</summary>
    [TestMethod]
    public void GetEditableProperties_WithReadOnlyProperty_ExcludesReadOnlyProperty()
    {
        // Arrange
        TestEditableWithReadOnlyProperty component = new TestEditableWithReadOnlyProperty();

        // Act
        List<PropertyInfo> properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsFalse(properties.Any(p => p.Name == nameof(EditableComponentTests.TestEditableWithReadOnlyProperty.ReadOnlyProperty)), "Read-only properties should be excluded from editable properties.");
    }

    ///<summary>
    ///Tests that GetEditableProperties includes valid readable and writable properties.
    ///</summary>
    [TestMethod]
    public void GetEditableProperties_WithValidProperties_IncludesValidProperties()
    {
        // Arrange
        TestEditableWithValidProperties component = new TestEditableWithValidProperties();

        // Act
        List<PropertyInfo> properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsTrue(properties.Any(p => p.Name == nameof(EditableComponentTests.TestEditableWithValidProperties.ValidProperty1)), "Valid properties should be included in editable properties.");
        Assert.IsTrue(properties.Any(p => p.Name == nameof(EditableComponentTests.TestEditableWithValidProperties.ValidProperty2)), "Valid properties should be included in editable properties.");
    }

    ///<summary>
    ///Tests that GetEditableProperties excludes write-only properties.
    ///</summary>
    [TestMethod]
    public void GetEditableProperties_WithWriteOnlyProperty_ExcludesWriteOnlyProperty()
    {
        // Arrange
        TestEditableWithWriteOnlyProperty component = new TestEditableWithWriteOnlyProperty();

        // Act
        List<PropertyInfo> properties = component.GetEditablePropertiesPublic().ToList();

        // Assert
        Assert.IsFalse(properties.Any(p => p.Name == nameof(EditableComponentTests.TestEditableWithWriteOnlyProperty.WriteOnlyProperty)), "Write-only properties should be excluded from editable properties.");
    }
    #endregion

    ///<summary>
    ///Helper class for testing EditableComponent.
    ///</summary>
    class TestEditableComponent : EditableComponent
    {
    }

    ///<summary>
    ///Test helper class with a read-only property.
    ///</summary>
    sealed class TestEditableWithReadOnlyProperty : EditableComponent
    {
        #region Public methods
        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic() { return GetEditableProperties(); }
        #endregion

        #region Public properties
        public string ReadOnlyProperty { get; } = "ReadOnly";

        public string ValidProperty { get; set; } = "Valid";
        #endregion
    }

    ///<summary>
    ///Test helper class with a write-only property.
    ///</summary>
    sealed class TestEditableWithWriteOnlyProperty : EditableComponent
    {
        #region Fields
        string _writeOnly = string.Empty;
        #endregion

        #region Public methods
        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic() { return GetEditableProperties(); }
        #endregion

        #region Public properties
        public string ValidProperty { get; set; } = "Valid";

        public string WriteOnlyProperty { set => _writeOnly = value; }
        #endregion
    }

    ///<summary>
    ///Test helper class with an indexer property.
    ///</summary>
    sealed class TestEditableWithIndexer : EditableComponent
    {
        #region Fields
        readonly Dictionary<int, string> _data = new();
        #endregion

        #region Indexers
        public string this[int index] { get => _data.TryGetValue(index, out string? value) ? value : string.Empty; set => _data[index] = value; }
        #endregion

        #region Public methods
        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic() { return GetEditableProperties(); }
        #endregion

        #region Public properties
        public string ValidProperty { get; set; } = "Valid";
        #endregion
    }

    ///<summary>
    ///Test helper class with valid editable properties.
    ///</summary>
    sealed class TestEditableWithValidProperties : EditableComponent
    {
        #region Public methods
        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic() { return GetEditableProperties(); }
        #endregion

        #region Public properties
        public string ValidProperty1 { get; set; } = "Value1";

        public int ValidProperty2 { get; set; } = 42;
        #endregion
    }

    ///<summary>
    ///Test helper class with no valid properties (only inherited ones that are filtered).
    ///</summary>
    sealed class TestEditableWithNoValidProperties : EditableComponent
    {
        #region Public methods
        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic() { return GetEditableProperties(); }
        #endregion

        #region Public properties
        public string ReadOnlyProp { get; } = "ReadOnly";
        #endregion
    }

    ///<summary>
    ///Test helper class with mixed access level properties.
    ///</summary>
    sealed class TestEditableWithMixedAccessProperties : EditableComponent
    {
        #region Private properties
        string PrivateProperty { get; set; } = "Private";
        #endregion

        #region Public methods
        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic() { return GetEditableProperties(); }
        #endregion

        #region Public properties
        public string PublicProperty { get; set; } = "Public";
        #endregion
    }

    ///<summary>
    ///Test helper class with a mix of valid and invalid properties.
    ///</summary>
    sealed class TestEditableWithMixedProperties : EditableComponent
    {
        #region Public methods
        public IEnumerable<PropertyInfo> GetEditablePropertiesPublic() { return GetEditableProperties(); }
        #endregion

        #region Public properties
        public string ReadOnlyProperty { get; } = "ReadOnly";

        public string ValidProperty { get; set; } = "Valid";
        #endregion
    }
}