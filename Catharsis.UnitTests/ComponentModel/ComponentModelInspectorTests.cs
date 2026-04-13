using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="ComponentModelInspector"/> class.
///</summary>
[TestClass]
public sealed class ComponentModelInspectorTests
{
    #region Public methods
    [TestMethod]
    public void GetComponentModelInterfaces_NullComponent_ThrowsArgumentNullException()
    {
        ComponentModelInspector inspector = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => ComponentModelInspector.GetComponentModelInterfaces(null!));
    }

    [TestMethod]
    public void GetComponentModelInterfaces_ReturnsRelevantInterfaces()
    {
        ComponentModelInspector inspector = new();
        using TestValidatingComponent component = new();

        IReadOnlyList<string> interfaces = ComponentModelInspector.GetComponentModelInterfaces(component);

        CollectionAssert.Contains(interfaces.ToList(), "INotifyPropertyChanged");
        CollectionAssert.Contains(interfaces.ToList(), "INotifyDataErrorInfo");
        CollectionAssert.Contains(interfaces.ToList(), "IDisposable");
    }

    [TestMethod]
    public void GetEventReport_NullComponent_ThrowsArgumentNullException()
    {
        ComponentModelInspector inspector = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => ComponentModelInspector.GetEventReport(null!));
    }

    [TestMethod]
    public void GetEventReport_ReturnsEvents()
    {
        ComponentModelInspector inspector = new();
        using TestValidatingComponent component = new();

        IReadOnlyList<ComponentModelInspector.EventReport> report = ComponentModelInspector.GetEventReport(component);

        Assert.IsNotEmpty(report);
    }

    [TestMethod]
    public void GetPropertyReport_ContainsValidationAttributes()
    {
        ComponentModelInspector inspector = new();
        InspectableDto dto = new();

        IReadOnlyList<ComponentModelInspector.PropertyReport> report = ComponentModelInspector.GetPropertyReport(dto);
        ComponentModelInspector.PropertyReport nameReport = report.First(static r => r.Name == "Name");

        Assert.IsGreaterThanOrEqualTo(1, nameReport.ValidationAttributes.Count);
    }

    [TestMethod]
    public void GetPropertyReport_CurrentValue_ReturnsActualValue()
    {
        ComponentModelInspector inspector = new();
        InspectableDto dto = new() { Name = "Alice" };

        IReadOnlyList<ComponentModelInspector.PropertyReport> report = ComponentModelInspector.GetPropertyReport(dto);
        ComponentModelInspector.PropertyReport nameReport = report.First(static r => r.Name == "Name");

        Assert.AreEqual("Alice", nameReport.CurrentValue);
    }

    [TestMethod]
    public void GetPropertyReport_NullComponent_ThrowsArgumentNullException()
    {
        ComponentModelInspector inspector = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => ComponentModelInspector.GetPropertyReport(null!));
    }

    [TestMethod]
    public void GetPropertyReport_ReturnsAllProperties()
    {
        ComponentModelInspector inspector = new();
        InspectableDto dto = new() { Name = "Alice", Age = 30 };

        IReadOnlyList<ComponentModelInspector.PropertyReport> report = ComponentModelInspector.GetPropertyReport(dto);

        Assert.IsGreaterThanOrEqualTo(3, report.Count);
        Assert.Contains(static r => r.Name == "Name", report);
        Assert.Contains(static r => r.Name == "Age", report);
    }

    [TestMethod]
    public void TryGetValidationErrors_NoErrors_ReturnsFalse()
    {
        ComponentModelInspector inspector = new();
        using TestValidatingComponent component = new() { Name = "Alice" };

        bool result = ComponentModelInspector.TryGetValidationErrors(component, out IReadOnlyDictionary<string, IReadOnlyList<string>>? errors);

        Assert.IsFalse(result);
        Assert.IsNull(errors);
    }

    [TestMethod]
    public void TryGetValidationErrors_NonValidatingComponent_ReturnsFalse()
    {
        ComponentModelInspector inspector = new();
        InspectableDto dto = new();

        bool result = ComponentModelInspector.TryGetValidationErrors(dto, out IReadOnlyDictionary<string, IReadOnlyList<string>>? errors);

        Assert.IsFalse(result);
        Assert.IsNull(errors);
    }

    [TestMethod]
    public void TryGetValidationErrors_NullComponent_ThrowsArgumentNullException()
    {
        ComponentModelInspector inspector = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => ComponentModelInspector.TryGetValidationErrors(null!, out _));
    }

    [TestMethod]
    public void TryGetValidationErrors_WithErrors_ReturnsTrueAndErrors()
    {
        ComponentModelInspector inspector = new();
        using TestValidatingComponent component = new();
        component.Name = "Alice";
        component.Name = null;

        bool result = ComponentModelInspector.TryGetValidationErrors(component, out IReadOnlyDictionary<string, IReadOnlyList<string>>? errors);

        Assert.IsTrue(result);
        Assert.IsNotNull(errors);
        Assert.IsTrue(errors.ContainsKey("Name"));
    }
    #endregion

    sealed class InspectableDto
    {
        #region Public properties
        [Range(0, 150)]
        public int Age { get; set; }

        [Required]
        [StringLength(50)]
        public string? Name { get; set; }

        [Browsable(false)]
        public string? Secret { get; set; }
        #endregion
    }

    sealed class TestValidatingComponent : ValidatingObservableComponent
    {
        #region Fields
        string? _name;
        #endregion

        #region Public properties
        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get => _name; set => SetPropertyValidated(ref _name, value); }
        #endregion
    }
}
