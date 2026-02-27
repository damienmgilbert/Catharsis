using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public sealed class ComponentModelDebuggerViewTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullComponent_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(() => new ComponentModelDebuggerView(null!)); }
    [TestMethod]
    public void HasErrors_NoErrors_ReturnsFalse()
    {
        ComponentModelDebuggerView view = new ComponentModelDebuggerView(new SimpleDto());

        Assert.IsFalse(view.HasErrors);
    }

    [TestMethod]
    public void HasErrors_WithErrors_ReturnsTrue()
    {
        using TestValidatingComponent component = new TestValidatingComponent();
        component.Name = "Alice";
        component.Name = null;
        ComponentModelDebuggerView view = new ComponentModelDebuggerView(component);

        Assert.IsTrue(view.HasErrors);
    }

    [TestMethod]
    public void Interfaces_ReturnsRelevantInterfaces()
    {
        using TestValidatingComponent component = new TestValidatingComponent();
        ComponentModelDebuggerView view = new ComponentModelDebuggerView(component);

        string[] ifaces = view.Interfaces;

        CollectionAssert.Contains(ifaces, "INotifyPropertyChanged");
        CollectionAssert.Contains(ifaces, "IDisposable");
    }

    [TestMethod]
    public void IsChanged_NonTrackingComponent_ReturnsFalse()
    {
        ComponentModelDebuggerView view = new ComponentModelDebuggerView(new SimpleDto());

        Assert.IsFalse(view.IsChanged);
    }

    [TestMethod]
    public void IsDesignMode_NonSitedComponent_ReturnsFalse()
    {
        ComponentModelDebuggerView view = new ComponentModelDebuggerView(new SimpleDto());

        Assert.IsFalse(view.IsDesignMode);
    }

    [TestMethod]
    public void Properties_ReturnsPropertyEntries()
    {
        SimpleDto dto = new SimpleDto { Name = "Alice", Age = 30 };
        ComponentModelDebuggerView view = new ComponentModelDebuggerView(dto);

        ComponentModelDebuggerView.PropertyEntry[] props = view.Properties;

        Assert.IsTrue(props.Length >= 2);
        Assert.IsTrue(props.Any(p => (p.Name == "Name") && ((string?)p.Value == "Alice")));
        Assert.IsTrue(props.Any(p => (p.Name == "Age") && ((int)p.Value! == 30)));
    }

    [TestMethod]
    public void SiteName_NonSitedComponent_ReturnsNull()
    {
        ComponentModelDebuggerView view = new ComponentModelDebuggerView(new SimpleDto());

        Assert.IsNull(view.SiteName);
    }

    [TestMethod]
    public void TypeName_ReturnsFullTypeName()
    {
        ComponentModelDebuggerView view = new ComponentModelDebuggerView(new SimpleDto());

        Assert.IsNotNull(view.TypeName);
        Assert.IsTrue(view.TypeName.Contains("SimpleDto"));
    }

    [TestMethod]
    public void ValidationErrors_NoErrors_ReturnsEmpty()
    {
        using TestValidatingComponent component = new TestValidatingComponent { Name = "Alice" };
        ComponentModelDebuggerView view = new ComponentModelDebuggerView(component);

        Assert.AreEqual(0, view.ValidationErrors.Length);
    }

    [TestMethod]
    public void ValidationErrors_WithErrors_ReturnsEntries()
    {
        using TestValidatingComponent component = new TestValidatingComponent();
        component.Name = "Alice";
        component.Name = null;
        ComponentModelDebuggerView view = new ComponentModelDebuggerView(component);

        ComponentModelDebuggerView.ValidationErrorEntry[] errors = view.ValidationErrors;

        Assert.IsTrue(errors.Length > 0);
        Assert.IsTrue(errors.Any(e => e.PropertyName == "Name"));
    }
    #endregion

    sealed class SimpleDto
    {
        #region Public properties
        public int Age { get; set; }

        public string? Name { get; set; }
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
