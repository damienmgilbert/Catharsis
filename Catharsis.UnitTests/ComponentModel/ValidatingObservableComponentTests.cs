using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public sealed class ValidatingObservableComponentTests
{
    #region Public methods
    [TestMethod]
    public void BeginEdit_CalledTwice_IgnoresSecond()
    {
        using TestComponent c = new TestComponent { Name = "Alice" };
        c.BeginEdit();
        c.Name = "Bob";
        c.BeginEdit(); // should not re-snapshot
        c.CancelEdit();

        Assert.AreEqual("Alice", c.Name);
    }

    [TestMethod]
    public void BeginEdit_CancelEdit_RevertsValues()
    {
        using TestComponent c = new TestComponent { Name = "Alice", Age = 30 };
        c.BeginEdit();
        Assert.IsTrue(c.IsEditing);

        c.Name = "Bob";
        c.Age = 99;
        c.CancelEdit();

        Assert.AreEqual("Alice", c.Name);
        Assert.AreEqual(30, c.Age);
        Assert.IsFalse(c.IsEditing);
    }

    [TestMethod]
    public void BeginEdit_EndEdit_CommitsValues()
    {
        using TestComponent c = new TestComponent { Name = "Alice", Age = 30 };
        c.BeginEdit();
        c.Name = "Bob";
        c.EndEdit();

        Assert.AreEqual("Bob", c.Name);
        Assert.IsFalse(c.IsEditing);
    }

    [TestMethod]
    public void CurrentErrors_ReturnsReadOnlyDictionary()
    {
        using TestComponent c = new TestComponent { Age = 200 };
        c.ValidateAllProperties();

        IReadOnlyDictionary<string, IReadOnlyList<string>> dict = c.CurrentErrors;

        Assert.IsNotEmpty(dict);
    }

    [TestMethod]
    public void GetErrors_NullOrEmpty_ReturnsAll()
    {
        using TestComponent c = new TestComponent { Age = 200 };
        c.ValidateAllProperties();

        List<string> all = c.GetErrors(null).Cast<string>().ToList();

        Assert.IsNotEmpty(all);
    }

    [TestMethod]
    public void GetErrors_UnknownProperty_ReturnsEmpty()
    {
        using TestComponent c = new TestComponent();

        List<string> errors = c.GetErrors("Unknown").Cast<string>().ToList();

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void Initial_State_NoErrors()
    {
        using TestComponent c = new TestComponent();

        Assert.IsFalse(c.HasErrors);
        Assert.IsFalse(c.IsEditing);
    }

    [TestMethod]
    public void SetPropertyValidated_InvalidValue_HasErrors()
    {
        using TestComponent c = new TestComponent();
        c.Name = "Alice";

        c.Name = null;

        Assert.IsTrue(c.HasErrors);
        List<string> errors = c.GetErrors("Name").Cast<string>().ToList();
        Assert.HasCount(1, errors);
        Assert.AreEqual("Name is required.", errors[0]);
    }

    [TestMethod]
    public void SetPropertyValidated_RaisesErrorsChanged()
    {
        using TestComponent c = new TestComponent();
        c.Name = "Alice";
        bool raised = false;
        c.ErrorsChanged += (s, e) => raised = true;

        c.Name = null;

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void SetPropertyValidated_SameValue_ReturnsFalse()
    {
        using TestComponent c = new TestComponent { Name = "Alice" };

        bool changed = false;
        c.PropertyChanged += (s, e) => changed = true;

        // Reset the flag after initial set
        changed = false;
        c.Name = "Alice";

        Assert.IsFalse(changed);
    }

    [TestMethod]
    public void SetPropertyValidated_ValidValue_NoErrors()
    {
        using TestComponent c = new TestComponent();

        c.Name = "Alice";

        Assert.IsFalse(c.HasErrors);
    }

    [TestMethod]
    public void ValidateAllProperties_Invalid_ReturnsFalse()
    {
        using TestComponent c = new TestComponent { Age = 200 };

        Assert.IsFalse(c.ValidateAllProperties());
        Assert.IsTrue(c.HasErrors);
    }

    [TestMethod]
    public void ValidateAllProperties_Valid_ReturnsTrue()
    {
        using TestComponent c = new TestComponent { Name = "Alice", Age = 30 };

        Assert.IsTrue(c.ValidateAllProperties());
        Assert.IsFalse(c.HasErrors);
    }
    #endregion

    sealed class TestComponent : Catharsis.ComponentModel.ValidatingObservableComponent
    {
        #region Fields
        int _age;
        string? _name;
        #endregion

        #region Public properties
        [Range(0, 150, ErrorMessage = "Age must be between 0 and 150.")]
        public int Age { get => _age; set => SetPropertyValidated(ref _age, value); }

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get => _name; set => SetPropertyValidated(ref _name, value); }
        #endregion
    }
}
