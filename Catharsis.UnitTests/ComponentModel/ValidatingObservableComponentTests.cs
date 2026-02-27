using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public sealed class ValidatingObservableComponentTests
{
    private sealed class TestComponent : Catharsis.ComponentModel.ValidatingObservableComponent
    {
        private string? _name;
        private int _age;

        [Required(ErrorMessage = "Name is required.")]
        public string? Name
        {
            get => _name;
            set => SetPropertyValidated(ref _name, value);
        }

        [Range(0, 150, ErrorMessage = "Age must be between 0 and 150.")]
        public int Age
        {
            get => _age;
            set => SetPropertyValidated(ref _age, value);
        }
    }

    [TestMethod]
    public void Initial_State_NoErrors()
    {
        using var c = new TestComponent();

        Assert.IsFalse(c.HasErrors);
        Assert.IsFalse(c.IsEditing);
    }

    [TestMethod]
    public void SetPropertyValidated_ValidValue_NoErrors()
    {
        using var c = new TestComponent();

        c.Name = "Alice";

        Assert.IsFalse(c.HasErrors);
    }

    [TestMethod]
    public void SetPropertyValidated_InvalidValue_HasErrors()
    {
        using var c = new TestComponent();
        c.Name = "Alice";

        c.Name = null;

        Assert.IsTrue(c.HasErrors);
        var errors = c.GetErrors("Name").Cast<string>().ToList();
        Assert.AreEqual(1, errors.Count);
        Assert.AreEqual("Name is required.", errors[0]);
    }

    [TestMethod]
    public void SetPropertyValidated_SameValue_ReturnsFalse()
    {
        using var c = new TestComponent { Name = "Alice" };

        var changed = false;
        c.PropertyChanged += (s, e) => changed = true;

        // Reset the flag after initial set
        changed = false;
        c.Name = "Alice";

        Assert.IsFalse(changed);
    }

    [TestMethod]
    public void SetPropertyValidated_RaisesErrorsChanged()
    {
        using var c = new TestComponent();
        c.Name = "Alice";
        var raised = false;
        c.ErrorsChanged += (s, e) => raised = true;

        c.Name = null;

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void ValidateAllProperties_Valid_ReturnsTrue()
    {
        using var c = new TestComponent { Name = "Alice", Age = 30 };

        Assert.IsTrue(c.ValidateAllProperties());
        Assert.IsFalse(c.HasErrors);
    }

    [TestMethod]
    public void ValidateAllProperties_Invalid_ReturnsFalse()
    {
        using var c = new TestComponent { Age = 200 };

        Assert.IsFalse(c.ValidateAllProperties());
        Assert.IsTrue(c.HasErrors);
    }

    [TestMethod]
    public void GetErrors_NullOrEmpty_ReturnsAll()
    {
        using var c = new TestComponent { Age = 200 };
        c.ValidateAllProperties();

        var all = c.GetErrors(null).Cast<string>().ToList();

        Assert.IsTrue(all.Count > 0);
    }

    [TestMethod]
    public void GetErrors_UnknownProperty_ReturnsEmpty()
    {
        using var c = new TestComponent();

        var errors = c.GetErrors("Unknown").Cast<string>().ToList();

        Assert.AreEqual(0, errors.Count);
    }

    [TestMethod]
    public void CurrentErrors_ReturnsReadOnlyDictionary()
    {
        using var c = new TestComponent { Age = 200 };
        c.ValidateAllProperties();

        var dict = c.CurrentErrors;

        Assert.IsTrue(dict.Count > 0);
    }

    [TestMethod]
    public void BeginEdit_CancelEdit_RevertsValues()
    {
        using var c = new TestComponent { Name = "Alice", Age = 30 };
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
        using var c = new TestComponent { Name = "Alice", Age = 30 };
        c.BeginEdit();
        c.Name = "Bob";
        c.EndEdit();

        Assert.AreEqual("Bob", c.Name);
        Assert.IsFalse(c.IsEditing);
    }

    [TestMethod]
    public void BeginEdit_CalledTwice_IgnoresSecond()
    {
        using var c = new TestComponent { Name = "Alice" };
        c.BeginEdit();
        c.Name = "Bob";
        c.BeginEdit(); // should not re-snapshot
        c.CancelEdit();

        Assert.AreEqual("Alice", c.Name);
    }
}
