using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="ComponentSnapshot{T}"/> class.
///</summary>
[TestClass]
public class ComponentSnapshotTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullSource_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new ComponentSnapshot<Model>(null!)); }

    [TestMethod]
    public void Constructor_CapturesCurrentState()
    {
        Model model = new() { Name = "Alice", Age = 30 };
        ComponentSnapshot<Model> snapshot = new(model);

        model.Name = "Bob";
        snapshot.Restore();

        Assert.AreEqual("Alice", model.Name);
    }

    #endregion

    #region Restore

    [TestMethod]
    public void Restore_RevertsAllTrackedProperties()
    {
        Model model = new() { Name = "Alice", Age = 30 };
        ComponentSnapshot<Model> snapshot = new(model);

        model.Name = "Bob";
        model.Age = 99;
        snapshot.Restore();

        Assert.AreEqual("Alice", model.Name);
        Assert.AreEqual(30, model.Age);
    }

    [TestMethod]
    public void Restore_IgnoresReadOnlyProperty()
    {
        Model model = new() { Name = "Alice" };
        ComponentSnapshot<Model> snapshot = new(model);
        int originalComputed = model.NameLength;

        model.Name = "Bob";
        snapshot.Restore();

        Assert.AreEqual("Alice", model.Name);
        Assert.AreEqual(originalComputed, model.NameLength);
    }

    #endregion

    #region Capture

    [TestMethod]
    public void Capture_UpdatesSnapshotToCurrentState()
    {
        Model model = new() { Name = "Alice" };
        ComponentSnapshot<Model> snapshot = new(model);

        model.Name = "Bob";
        snapshot.Capture();
        model.Name = "Carol";
        snapshot.Restore();

        Assert.AreEqual("Bob", model.Name);
    }

    #endregion

    private sealed class Model
    {
        #region Public properties
        public int Age { get; set; }
        public string? Name { get; set; }
        public int NameLength => Name?.Length ?? 0;
        #endregion
    }
}
