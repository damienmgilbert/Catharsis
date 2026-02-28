using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public sealed class EditableValidatingComponentTests
{
    #region Public methods
    [TestMethod]
    public void BeginEdit_CancelEdit_RevertsValues()
    {
        using TestComponent c = new TestComponent { Name = "Alice", Age = 30 };
        c.BeginEdit();
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
        using TestComponent c = new TestComponent { Name = "Alice" };
        c.BeginEdit();
        c.Name = "Bob";
        c.EndEdit();

        Assert.AreEqual("Bob", c.Name);
        Assert.IsFalse(c.IsEditing);
    }

    [TestMethod]
    public void CurrentErrors_Dictionary()
    {
        using TestComponent c = new TestComponent();
        c.Name = "Alice";
        c.Name = null;

        IReadOnlyDictionary<string, IReadOnlyList<string>> dict = c.CurrentErrors;

        Assert.IsTrue(dict.ContainsKey("Name"));
    }

    [TestMethod]
    public void ErrorsChanged_RaisedOnValidationFailure()
    {
        using TestComponent c = new TestComponent();
        c.Name = "Alice";
        bool raised = false;
        c.ErrorsChanged += (s, e) => raised = true;

        c.Name = null;

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void GetErrors_NullProperty_ReturnsAll()
    {
        using TestComponent c = new TestComponent { Age = 200 };
        c.ValidateAllProperties();

        List<string> all = c.GetErrors(null).Cast<string>().ToList();

        Assert.IsNotEmpty(all);
    }

    [TestMethod]
    public void GetErrors_ReturnsErrorsForProperty()
    {
        using TestComponent c = new TestComponent();
        c.Name = "Alice";
        c.Name = null;

        List<string> errors = c.GetErrors("Name").Cast<string>().ToList();

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void Initial_State_NoErrors()
    {
        using TestComponent c = new TestComponent();

        Assert.IsFalse(c.HasErrors);
        Assert.IsFalse(c.IsEditing);
    }

    [TestMethod]
    public void SetPropertyAndValidate_InvalidValue_HasErrors()
    {
        using TestComponent c = new TestComponent();
        c.Name = "Alice";

        c.Name = null;

        Assert.IsTrue(c.HasErrors);
    }

    [TestMethod]
    public void SetPropertyAndValidate_RaisesPropertyChanged()
    {
        using TestComponent c = new TestComponent();
        string? changedProp = null;
        c.PropertyChanged += (s, e) => changedProp = e.PropertyName;

        c.Name = "Alice";

        Assert.AreEqual("Name", changedProp);
    }

    [TestMethod]
    public void SetPropertyAndValidate_SameValue_ReturnsFalse()
    {
        using TestComponent c = new TestComponent { Name = "Alice" };
        bool raised = false;
        c.PropertyChanged += (s, e) =>
        {
            if(e.PropertyName == "Name")
            {
                raised = true;
            }
        };

        raised = false;
        c.Name = "Alice";

        Assert.IsFalse(raised);
    }

    [TestMethod]
    public void SetPropertyAndValidate_ValidValue_NoErrors()
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
    }

    [TestMethod]
    public void ValidateAllProperties_Valid_ReturnsTrue()
    {
        using TestComponent c = new TestComponent { Name = "Alice", Age = 30 };

        Assert.IsTrue(c.ValidateAllProperties());
    }
    #endregion

    sealed class TestComponent : Catharsis.ComponentModel.EditableValidatingComponent
    {
        #region Fields
        int _age;
        string? _name;
        #endregion

        #region Public properties
        [Range(0, 150, ErrorMessage = "Age must be between 0 and 150.")]
        public int Age { get => _age; set => SetPropertyAndValidate(ref _age, value); }

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get => _name; set => SetPropertyAndValidate(ref _name, value); }
        #endregion
    }
}
