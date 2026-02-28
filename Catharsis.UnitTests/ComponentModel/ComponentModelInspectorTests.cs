using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public sealed class ComponentModelInspectorTests
{
    #region Public methods
    [TestMethod]
    public void GetComponentModelInterfaces_NullComponent_ThrowsArgumentNullException()
    {
        ComponentModelInspector inspector = new ComponentModelInspector();

        Assert.ThrowsExactly<ArgumentNullException>(() => inspector.GetComponentModelInterfaces(null!));
    }

    [TestMethod]
    public void GetComponentModelInterfaces_ReturnsRelevantInterfaces()
    {
        ComponentModelInspector inspector = new ComponentModelInspector();
        using TestValidatingComponent component = new TestValidatingComponent();

        IReadOnlyList<string> interfaces = inspector.GetComponentModelInterfaces(component);

        CollectionAssert.Contains(interfaces.ToList(), "INotifyPropertyChanged");
        CollectionAssert.Contains(interfaces.ToList(), "INotifyDataErrorInfo");
        CollectionAssert.Contains(interfaces.ToList(), "IDisposable");
    }

    [TestMethod]
    public void GetEventReport_NullComponent_ThrowsArgumentNullException()
    {
        ComponentModelInspector inspector = new ComponentModelInspector();

        Assert.ThrowsExactly<ArgumentNullException>(() => inspector.GetEventReport(null!));
    }

    [TestMethod]
    public void GetEventReport_ReturnsEvents()
    {
        ComponentModelInspector inspector = new ComponentModelInspector();
        using TestValidatingComponent component = new TestValidatingComponent();

        IReadOnlyList<ComponentModelInspector.EventReport> report = inspector.GetEventReport(component);

        Assert.IsNotEmpty(report);
    }

    [TestMethod]
    public void GetPropertyReport_ContainsValidationAttributes()
    {
        ComponentModelInspector inspector = new ComponentModelInspector();
        InspectableDto dto = new InspectableDto();

        IReadOnlyList<ComponentModelInspector.PropertyReport> report = inspector.GetPropertyReport(dto);
        ComponentModelInspector.PropertyReport nameReport = report.First(r => r.Name == "Name");

        Assert.IsGreaterThanOrEqualTo(1, nameReport.ValidationAttributes.Count);
    }

    [TestMethod]
    public void GetPropertyReport_CurrentValue_ReturnsActualValue()
    {
        ComponentModelInspector inspector = new ComponentModelInspector();
        InspectableDto dto = new InspectableDto { Name = "Alice" };

        IReadOnlyList<ComponentModelInspector.PropertyReport> report = inspector.GetPropertyReport(dto);
        ComponentModelInspector.PropertyReport nameReport = report.First(r => r.Name == "Name");

        Assert.AreEqual("Alice", nameReport.CurrentValue);
    }

    [TestMethod]
    public void GetPropertyReport_NullComponent_ThrowsArgumentNullException()
    {
        ComponentModelInspector inspector = new ComponentModelInspector();

        Assert.ThrowsExactly<ArgumentNullException>(() => inspector.GetPropertyReport(null!));
    }

    [TestMethod]
    public void GetPropertyReport_ReturnsAllProperties()
    {
        ComponentModelInspector inspector = new ComponentModelInspector();
        InspectableDto dto = new InspectableDto { Name = "Alice", Age = 30 };

        IReadOnlyList<ComponentModelInspector.PropertyReport> report = inspector.GetPropertyReport(dto);

        Assert.IsGreaterThanOrEqualTo(3, report.Count);
        Assert.IsTrue(report.Any(r => r.Name == "Name"));
        Assert.IsTrue(report.Any(r => r.Name == "Age"));
    }

    [TestMethod]
    public void TryGetValidationErrors_NoErrors_ReturnsFalse()
    {
        ComponentModelInspector inspector = new ComponentModelInspector();
        using TestValidatingComponent component = new TestValidatingComponent { Name = "Alice" };

        bool result = inspector.TryGetValidationErrors(component, out IReadOnlyDictionary<string, IReadOnlyList<string>>? errors);

        Assert.IsFalse(result);
        Assert.IsNull(errors);
    }

    [TestMethod]
    public void TryGetValidationErrors_NonValidatingComponent_ReturnsFalse()
    {
        ComponentModelInspector inspector = new ComponentModelInspector();
        InspectableDto dto = new InspectableDto();

        bool result = inspector.TryGetValidationErrors(dto, out IReadOnlyDictionary<string, IReadOnlyList<string>>? errors);

        Assert.IsFalse(result);
        Assert.IsNull(errors);
    }

    [TestMethod]
    public void TryGetValidationErrors_NullComponent_ThrowsArgumentNullException()
    {
        ComponentModelInspector inspector = new ComponentModelInspector();

        Assert.ThrowsExactly<ArgumentNullException>(() => inspector.TryGetValidationErrors(null!, out _));
    }

    [TestMethod]
    public void TryGetValidationErrors_WithErrors_ReturnsTrueAndErrors()
    {
        ComponentModelInspector inspector = new ComponentModelInspector();
        using TestValidatingComponent component = new TestValidatingComponent();
        component.Name = "Alice";
        component.Name = null;

        bool result = inspector.TryGetValidationErrors(component, out IReadOnlyDictionary<string, IReadOnlyList<string>>? errors);

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
