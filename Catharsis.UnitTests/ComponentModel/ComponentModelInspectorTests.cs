using Catharsis.ComponentModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public sealed class ComponentModelInspectorTests
{
    private sealed class InspectableDto
    {
        [Required]
        [StringLength(50)]
        public string? Name { get; set; }

        [Range(0, 150)]
        public int Age { get; set; }

        [Browsable(false)]
        public string? Secret { get; set; }
    }

    private sealed class TestValidatingComponent : ValidatingObservableComponent
    {
        private string? _name;

        [Required(ErrorMessage = "Name is required.")]
        public string? Name
        {
            get => _name;
            set => SetPropertyValidated(ref _name, value);
        }
    }

    [TestMethod]
    public void GetPropertyReport_NullComponent_ThrowsArgumentNullException()
    {
        var inspector = new ComponentModelInspector();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => inspector.GetPropertyReport(null!));
    }

    [TestMethod]
    public void GetPropertyReport_ReturnsAllProperties()
    {
        var inspector = new ComponentModelInspector();
        var dto = new InspectableDto { Name = "Alice", Age = 30 };

        var report = inspector.GetPropertyReport(dto);

        Assert.IsTrue(report.Count >= 3);
        Assert.IsTrue(report.Any(r => r.Name == "Name"));
        Assert.IsTrue(report.Any(r => r.Name == "Age"));
    }

    [TestMethod]
    public void GetPropertyReport_ContainsValidationAttributes()
    {
        var inspector = new ComponentModelInspector();
        var dto = new InspectableDto();

        var report = inspector.GetPropertyReport(dto);
        var nameReport = report.First(r => r.Name == "Name");

        Assert.IsTrue(nameReport.ValidationAttributes.Count >= 1);
    }

    [TestMethod]
    public void GetPropertyReport_CurrentValue_ReturnsActualValue()
    {
        var inspector = new ComponentModelInspector();
        var dto = new InspectableDto { Name = "Alice" };

        var report = inspector.GetPropertyReport(dto);
        var nameReport = report.First(r => r.Name == "Name");

        Assert.AreEqual("Alice", nameReport.CurrentValue);
    }

    [TestMethod]
    public void GetEventReport_NullComponent_ThrowsArgumentNullException()
    {
        var inspector = new ComponentModelInspector();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => inspector.GetEventReport(null!));
    }

    [TestMethod]
    public void GetEventReport_ReturnsEvents()
    {
        var inspector = new ComponentModelInspector();
        using var component = new TestValidatingComponent();

        var report = inspector.GetEventReport(component);

        Assert.IsTrue(report.Count > 0);
    }

    [TestMethod]
    public void GetComponentModelInterfaces_NullComponent_ThrowsArgumentNullException()
    {
        var inspector = new ComponentModelInspector();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => inspector.GetComponentModelInterfaces(null!));
    }

    [TestMethod]
    public void GetComponentModelInterfaces_ReturnsRelevantInterfaces()
    {
        var inspector = new ComponentModelInspector();
        using var component = new TestValidatingComponent();

        var interfaces = inspector.GetComponentModelInterfaces(component);

        CollectionAssert.Contains(interfaces.ToList(), "INotifyPropertyChanged");
        CollectionAssert.Contains(interfaces.ToList(), "INotifyDataErrorInfo");
        CollectionAssert.Contains(interfaces.ToList(), "IDisposable");
    }

    [TestMethod]
    public void TryGetValidationErrors_NoErrors_ReturnsFalse()
    {
        var inspector = new ComponentModelInspector();
        using var component = new TestValidatingComponent { Name = "Alice" };

        var result = inspector.TryGetValidationErrors(component, out var errors);

        Assert.IsFalse(result);
        Assert.IsNull(errors);
    }

    [TestMethod]
    public void TryGetValidationErrors_WithErrors_ReturnsTrueAndErrors()
    {
        var inspector = new ComponentModelInspector();
        using var component = new TestValidatingComponent();
        component.Name = "Alice";
        component.Name = null;

        var result = inspector.TryGetValidationErrors(component, out var errors);

        Assert.IsTrue(result);
        Assert.IsNotNull(errors);
        Assert.IsTrue(errors.ContainsKey("Name"));
    }

    [TestMethod]
    public void TryGetValidationErrors_NullComponent_ThrowsArgumentNullException()
    {
        var inspector = new ComponentModelInspector();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => inspector.TryGetValidationErrors(null!, out _));
    }

    [TestMethod]
    public void TryGetValidationErrors_NonValidatingComponent_ReturnsFalse()
    {
        var inspector = new ComponentModelInspector();
        var dto = new InspectableDto();

        var result = inspector.TryGetValidationErrors(dto, out var errors);

        Assert.IsFalse(result);
        Assert.IsNull(errors);
    }
}
