using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public sealed class ComponentModelDebuggerViewTests
{
    private sealed class SimpleDto
    {
        public string? Name { get; set; }
        public int Age { get; set; }
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
    public void Constructor_NullComponent_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new ComponentModelDebuggerView(null!));
    }

    [TestMethod]
    public void TypeName_ReturnsFullTypeName()
    {
        var view = new ComponentModelDebuggerView(new SimpleDto());

        Assert.IsNotNull(view.TypeName);
        Assert.IsTrue(view.TypeName.Contains("SimpleDto"));
    }

    [TestMethod]
    public void Properties_ReturnsPropertyEntries()
    {
        var dto = new SimpleDto { Name = "Alice", Age = 30 };
        var view = new ComponentModelDebuggerView(dto);

        var props = view.Properties;

        Assert.IsTrue(props.Length >= 2);
        Assert.IsTrue(props.Any(p => p.Name == "Name" && (string?)p.Value == "Alice"));
        Assert.IsTrue(props.Any(p => p.Name == "Age" && (int)p.Value! == 30));
    }

    [TestMethod]
    public void HasErrors_NoErrors_ReturnsFalse()
    {
        var view = new ComponentModelDebuggerView(new SimpleDto());

        Assert.IsFalse(view.HasErrors);
    }

    [TestMethod]
    public void HasErrors_WithErrors_ReturnsTrue()
    {
        using var component = new TestValidatingComponent();
        component.Name = "Alice";
        component.Name = null;
        var view = new ComponentModelDebuggerView(component);

        Assert.IsTrue(view.HasErrors);
    }

    [TestMethod]
    public void ValidationErrors_WithErrors_ReturnsEntries()
    {
        using var component = new TestValidatingComponent();
        component.Name = "Alice";
        component.Name = null;
        var view = new ComponentModelDebuggerView(component);

        var errors = view.ValidationErrors;

        Assert.IsTrue(errors.Length > 0);
        Assert.IsTrue(errors.Any(e => e.PropertyName == "Name"));
    }

    [TestMethod]
    public void ValidationErrors_NoErrors_ReturnsEmpty()
    {
        using var component = new TestValidatingComponent { Name = "Alice" };
        var view = new ComponentModelDebuggerView(component);

        Assert.AreEqual(0, view.ValidationErrors.Length);
    }

    [TestMethod]
    public void Interfaces_ReturnsRelevantInterfaces()
    {
        using var component = new TestValidatingComponent();
        var view = new ComponentModelDebuggerView(component);

        var ifaces = view.Interfaces;

        CollectionAssert.Contains(ifaces, "INotifyPropertyChanged");
        CollectionAssert.Contains(ifaces, "IDisposable");
    }

    [TestMethod]
    public void IsChanged_NonTrackingComponent_ReturnsFalse()
    {
        var view = new ComponentModelDebuggerView(new SimpleDto());

        Assert.IsFalse(view.IsChanged);
    }

    [TestMethod]
    public void SiteName_NonSitedComponent_ReturnsNull()
    {
        var view = new ComponentModelDebuggerView(new SimpleDto());

        Assert.IsNull(view.SiteName);
    }

    [TestMethod]
    public void IsDesignMode_NonSitedComponent_ReturnsFalse()
    {
        var view = new ComponentModelDebuggerView(new SimpleDto());

        Assert.IsFalse(view.IsDesignMode);
    }
}
