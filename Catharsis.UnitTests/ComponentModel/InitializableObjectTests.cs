using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class InitializableObjectTests
{
    private sealed class TestInitializable : InitializableObject
    {
        public bool BeginInitCalled { get; private set; }
        public bool EndInitCalled { get; private set; }

        protected override void OnBeginInit() => BeginInitCalled = true;
        protected override void OnEndInit() => EndInitCalled = true;
    }

    [TestMethod]
    public void BeginInit_SetsIsInitializing()
    {
        var obj = new TestInitializable();

        obj.BeginInit();

        Assert.IsTrue(obj.IsInitializing);
        Assert.IsFalse(obj.IsInitialized);
        Assert.IsTrue(obj.BeginInitCalled);
    }

    [TestMethod]
    public void EndInit_SetsIsInitializedAndRaisesEvent()
    {
        var obj = new TestInitializable();
        var eventRaised = false;
        obj.Initialized += (_, _) => eventRaised = true;

        obj.BeginInit();
        obj.EndInit();

        Assert.IsFalse(obj.IsInitializing);
        Assert.IsTrue(obj.IsInitialized);
        Assert.IsTrue(obj.EndInitCalled);
        Assert.IsTrue(eventRaised);
    }

    [TestMethod]
    public void BeginInit_CalledTwice_Throws()
    {
        var obj = new TestInitializable();
        obj.BeginInit();

        Assert.ThrowsExactly<InvalidOperationException>(() => obj.BeginInit());
    }

    [TestMethod]
    public void EndInit_WithoutBeginInit_Throws()
    {
        var obj = new TestInitializable();

        Assert.ThrowsExactly<InvalidOperationException>(() => obj.EndInit());
    }

    [TestMethod]
    public void MultipleInitCycles_WorkCorrectly()
    {
        var obj = new TestInitializable();
        var initCount = 0;
        obj.Initialized += (_, _) => initCount++;

        obj.BeginInit();
        obj.EndInit();

        obj.BeginInit();
        obj.EndInit();

        Assert.AreEqual(2, initCount);
        Assert.IsTrue(obj.IsInitialized);
    }

    [TestMethod]
    public void InitialState_NotInitializedOrInitializing()
    {
        var obj = new TestInitializable();

        Assert.IsFalse(obj.IsInitializing);
        Assert.IsFalse(obj.IsInitialized);
    }
}
