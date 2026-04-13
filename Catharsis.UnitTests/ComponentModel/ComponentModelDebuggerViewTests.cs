using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="ComponentModelDebuggerView"/> class.
///</summary>
[TestClass]
public sealed class ComponentModelDebuggerViewTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullComponent_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new ComponentModelDebuggerView(null!)); }
    [TestMethod]
    public void HasErrors_NoErrors_ReturnsFalse()
    {
        ComponentModelDebuggerView view = new(new SimpleDto());

        Assert.IsFalse(view.HasErrors);
    }

    [TestMethod]
    public void HasErrors_WithErrors_ReturnsTrue()
    {
        using TestValidatingComponent component = new();
        component.Name = "Alice";
        component.Name = null;
        ComponentModelDebuggerView view = new(component);

        Assert.IsTrue(view.HasErrors);
    }

    [TestMethod]
    public void Interfaces_ReturnsRelevantInterfaces()
    {
        using TestValidatingComponent component = new();
        ComponentModelDebuggerView view = new(component);

        string[] ifaces = view.Interfaces;

        CollectionAssert.Contains(ifaces, "INotifyPropertyChanged");
        CollectionAssert.Contains(ifaces, "IDisposable");
    }

    [TestMethod]
    public void IsChanged_NonTrackingComponent_ReturnsFalse()
    {
        ComponentModelDebuggerView view = new(new SimpleDto());

        Assert.IsFalse(view.IsChanged);
    }

    [TestMethod]
    public void IsDesignMode_NonSitedComponent_ReturnsFalse()
    {
        ComponentModelDebuggerView view = new(new SimpleDto());

        Assert.IsFalse(view.IsDesignMode);
    }

    [TestMethod]
    public void Properties_ReturnsPropertyEntries()
    {
        SimpleDto dto = new() { Name = "Alice", Age = 30 };
        ComponentModelDebuggerView view = new(dto);

        ComponentModelDebuggerView.PropertyEntry[] props = view.Properties;

        Assert.IsGreaterThanOrEqualTo(2, props.Length);
        Assert.Contains(static p => (p.Name == "Name") && ((string?)p.Value == "Alice"), props);
        Assert.Contains(static p => (p.Name == "Age") && ((int)p.Value! == 30), props);
    }

    [TestMethod]
    public void SiteName_NonSitedComponent_ReturnsNull()
    {
        ComponentModelDebuggerView view = new(new SimpleDto());

        Assert.IsNull(view.SiteName);
    }

    [TestMethod]
    public void TypeName_ReturnsFullTypeName()
    {
        ComponentModelDebuggerView view = new(new SimpleDto());

        Assert.IsNotNull(view.TypeName);
        Assert.Contains("SimpleDto", view.TypeName);
    }

    [TestMethod]
    public void ValidationErrors_NoErrors_ReturnsEmpty()
    {
        using TestValidatingComponent component = new() { Name = "Alice" };
        ComponentModelDebuggerView view = new(component);

        Assert.IsEmpty(view.ValidationErrors);
    }

    [TestMethod]
    public void ValidationErrors_WithErrors_ReturnsEntries()
    {
        using TestValidatingComponent component = new();
        component.Name = "Alice";
        component.Name = null;
        ComponentModelDebuggerView view = new(component);

        ComponentModelDebuggerView.ValidationErrorEntry[] errors = view.ValidationErrors;

        Assert.IsNotEmpty(errors);
        Assert.Contains(static e => e.PropertyName == "Name", errors);
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
