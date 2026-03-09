using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="EditableObject"/> class.
///</summary>
[TestClass]
public class EditableObjectTests
{
    #region Public methods
    [TestMethod]
    public void BeginEdit_CalledTwice_DoesNotResetSnapshot()
    {
        Person person = new Person { Name = "Alice" };

        person.BeginEdit();
        person.Name = "Bob";

        // second BeginEdit should be ignored
        person.BeginEdit();
        person.CancelEdit();

        Assert.AreEqual("Alice", person.Name);
    }

    [TestMethod]
    public void BeginEdit_SetsIsEditingToTrue()
    {
        Person person = new Person { Name = "Alice", Age = 30 };

        person.BeginEdit();

        Assert.IsTrue(person.IsEditing);
    }

    [TestMethod]
    public void CancelEdit_RestoresOriginalValues()
    {
        Person person = new Person { Name = "Alice", Age = 30 };

        person.BeginEdit();
        person.Name = "Bob";
        person.Age = 25;
        person.CancelEdit();

        Assert.AreEqual("Alice", person.Name);
        Assert.AreEqual(30, person.Age);
        Assert.IsFalse(person.IsEditing);
    }

    [TestMethod]
    public void CancelEdit_WithoutBeginEdit_DoesNothing()
    {
        Person person = new Person { Name = "Alice", Age = 30 };

        person.CancelEdit();

        Assert.AreEqual("Alice", person.Name);
        Assert.IsFalse(person.IsEditing);
    }

    [TestMethod]
    public void EndEdit_KeepsChangedValues()
    {
        Person person = new Person { Name = "Alice", Age = 30 };

        person.BeginEdit();
        person.Name = "Bob";
        person.Age = 25;
        person.EndEdit();

        Assert.AreEqual("Bob", person.Name);
        Assert.AreEqual(25, person.Age);
        Assert.IsFalse(person.IsEditing);
    }

    [TestMethod]
    public void EndEdit_WithoutBeginEdit_DoesNothing()
    {
        Person person = new Person { Name = "Alice" };

        person.EndEdit();

        Assert.IsFalse(person.IsEditing);
    }

    [TestMethod]
    public void MultipleEditCycles_WorkCorrectly()
    {
        Person person = new Person { Name = "Alice" };

        // First cycle
        person.BeginEdit();
        person.Name = "Bob";
        person.EndEdit();
        Assert.AreEqual("Bob", person.Name);

        // Second cycle
        person.BeginEdit();
        person.Name = "Charlie";
        person.CancelEdit();
        Assert.AreEqual("Bob", person.Name);
    }
    #endregion

    sealed class Person : EditableObject
    {
        #region Public properties
        public int Age { get; set; }

        public string Name { get; set; } = string.Empty;
        #endregion
    }
}
