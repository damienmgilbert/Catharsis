using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class EditableObjectTests
{
    private sealed class Person : EditableObject
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [TestMethod]
    public void BeginEdit_SetsIsEditingToTrue()
    {
        var person = new Person { Name = "Alice", Age = 30 };

        person.BeginEdit();

        Assert.IsTrue(person.IsEditing);
    }

    [TestMethod]
    public void CancelEdit_RestoresOriginalValues()
    {
        var person = new Person { Name = "Alice", Age = 30 };

        person.BeginEdit();
        person.Name = "Bob";
        person.Age = 25;
        person.CancelEdit();

        Assert.AreEqual("Alice", person.Name);
        Assert.AreEqual(30, person.Age);
        Assert.IsFalse(person.IsEditing);
    }

    [TestMethod]
    public void EndEdit_KeepsChangedValues()
    {
        var person = new Person { Name = "Alice", Age = 30 };

        person.BeginEdit();
        person.Name = "Bob";
        person.Age = 25;
        person.EndEdit();

        Assert.AreEqual("Bob", person.Name);
        Assert.AreEqual(25, person.Age);
        Assert.IsFalse(person.IsEditing);
    }

    [TestMethod]
    public void CancelEdit_WithoutBeginEdit_DoesNothing()
    {
        var person = new Person { Name = "Alice", Age = 30 };

        person.CancelEdit();

        Assert.AreEqual("Alice", person.Name);
        Assert.IsFalse(person.IsEditing);
    }

    [TestMethod]
    public void EndEdit_WithoutBeginEdit_DoesNothing()
    {
        var person = new Person { Name = "Alice" };

        person.EndEdit();

        Assert.IsFalse(person.IsEditing);
    }

    [TestMethod]
    public void BeginEdit_CalledTwice_DoesNotResetSnapshot()
    {
        var person = new Person { Name = "Alice" };

        person.BeginEdit();
        person.Name = "Bob";

        // second BeginEdit should be ignored
        person.BeginEdit();
        person.CancelEdit();

        Assert.AreEqual("Alice", person.Name);
    }

    [TestMethod]
    public void MultipleEditCycles_WorkCorrectly()
    {
        var person = new Person { Name = "Alice" };

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
}
