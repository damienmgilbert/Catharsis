using Catharsis.ComponentModel.Binding;

namespace Catharsis.UnitTests.ComponentModel.Binding;

///<summary>
///Unit tests for the <see cref="PropertyChangeScope"/> class.
///</summary>
[TestClass]
public sealed class PropertyChangeScopeTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullCallback_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new PropertyChangeScope(null!)); }
    [TestMethod]
    public void Dispose_CalledTwice_RaisesOnlyOnce()
    {
        int count = 0;
        PropertyChangeScope scope = new(_ => count++);
        scope.RecordChange("Name");

        scope.Dispose();
        scope.Dispose();

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void Dispose_RaisesNotificationsForPendingProperties()
    {
        List<string> raised = [];
        PropertyChangeScope scope = new(name => raised.Add(name));
        scope.RecordChange("Name");
        scope.RecordChange("Age");

        scope.Dispose();

        Assert.HasCount(2, raised);
        CollectionAssert.Contains(raised, "Name");
        CollectionAssert.Contains(raised, "Age");
    }

    [TestMethod]
    public void IsDisposed_AfterDispose_ReturnsTrue()
    {
        PropertyChangeScope scope = new(
                                    static _ =>
        {
        });
        scope.Dispose();

        Assert.IsTrue(scope.IsDisposed);
    }

    [TestMethod]
    public void IsDisposed_InitiallyFalse()
    {
        PropertyChangeScope scope = new(
                                    static _ =>
        {
        });

        Assert.IsFalse(scope.IsDisposed);
    }

    [TestMethod]
    public void PendingCount_AfterDispose_ReturnsZero()
    {
        PropertyChangeScope scope = new(
                                    static _ =>
        {
        });
        scope.RecordChange("Name");
        scope.Dispose();

        Assert.AreEqual(0, scope.PendingCount);
    }

    [TestMethod]
    public void RecordChange_AddsPropertyName()
    {
        PropertyChangeScope scope = new(
                                    static _ =>
        {
        });

        scope.RecordChange("Name");

        Assert.AreEqual(1, scope.PendingCount);
    }

    [TestMethod]
    public void RecordChange_AfterDispose_ThrowsObjectDisposedException()
    {
        PropertyChangeScope scope = new(
                                    _ =>
        {
        });
        scope.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => scope.RecordChange("Name"));
    }

    [TestMethod]
    public void RecordChange_DuplicateProperty_NotCounted()
    {
        PropertyChangeScope scope = new(
                                    static _ =>
        {
        });

        scope.RecordChange("Name");
        scope.RecordChange("Name");

        Assert.AreEqual(1, scope.PendingCount);
    }

    [TestMethod]
    public void RecordChange_NullPropertyName_ThrowsArgumentNullException()
    {
        PropertyChangeScope scope = new(
                                    _ =>
        {
        });

        Assert.ThrowsExactly<ArgumentNullException>(() => scope.RecordChange(null!));
    }
    #endregion
}
