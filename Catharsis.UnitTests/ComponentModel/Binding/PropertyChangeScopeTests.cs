using Catharsis.ComponentModel.Binding;

namespace Catharsis.UnitTests.ComponentModel.Binding;

[TestClass]
public sealed class PropertyChangeScopeTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullCallback_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(() => new PropertyChangeScope(null!)); }
    [TestMethod]
    public void Dispose_CalledTwice_RaisesOnlyOnce()
    {
        int count = 0;
        PropertyChangeScope scope = new PropertyChangeScope(_ => count++);
        scope.RecordChange("Name");

        scope.Dispose();
        scope.Dispose();

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void Dispose_RaisesNotificationsForPendingProperties()
    {
        List<string> raised = new List<string>();
        PropertyChangeScope scope = new PropertyChangeScope(name => raised.Add(name));
        scope.RecordChange("Name");
        scope.RecordChange("Age");

        scope.Dispose();

        Assert.AreEqual(2, raised.Count);
        CollectionAssert.Contains(raised, "Name");
        CollectionAssert.Contains(raised, "Age");
    }

    [TestMethod]
    public void IsDisposed_AfterDispose_ReturnsTrue()
    {
        PropertyChangeScope scope = new PropertyChangeScope(
                                    _ =>
        {
        });
        scope.Dispose();

        Assert.IsTrue(scope.IsDisposed);
    }

    [TestMethod]
    public void IsDisposed_InitiallyFalse()
    {
        PropertyChangeScope scope = new PropertyChangeScope(
                                    _ =>
        {
        });

        Assert.IsFalse(scope.IsDisposed);
    }

    [TestMethod]
    public void PendingCount_AfterDispose_ReturnsZero()
    {
        PropertyChangeScope scope = new PropertyChangeScope(
                                    _ =>
        {
        });
        scope.RecordChange("Name");
        scope.Dispose();

        Assert.AreEqual(0, scope.PendingCount);
    }

    [TestMethod]
    public void RecordChange_AddsPropertyName()
    {
        PropertyChangeScope scope = new PropertyChangeScope(
                                    _ =>
        {
        });

        scope.RecordChange("Name");

        Assert.AreEqual(1, scope.PendingCount);
    }

    [TestMethod]
    public void RecordChange_AfterDispose_ThrowsObjectDisposedException()
    {
        PropertyChangeScope scope = new PropertyChangeScope(
                                    _ =>
        {
        });
        scope.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => scope.RecordChange("Name"));
    }

    [TestMethod]
    public void RecordChange_DuplicateProperty_NotCounted()
    {
        PropertyChangeScope scope = new PropertyChangeScope(
                                    _ =>
        {
        });

        scope.RecordChange("Name");
        scope.RecordChange("Name");

        Assert.AreEqual(1, scope.PendingCount);
    }

    [TestMethod]
    public void RecordChange_NullPropertyName_ThrowsArgumentNullException()
    {
        PropertyChangeScope scope = new PropertyChangeScope(
                                    _ =>
        {
        });

        Assert.ThrowsExactly<ArgumentNullException>(() => scope.RecordChange(null!));
    }
    #endregion
}
