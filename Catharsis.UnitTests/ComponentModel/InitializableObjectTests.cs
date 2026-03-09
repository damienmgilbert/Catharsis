using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="InitializableObject"/> class.
///</summary>
[TestClass]
public class InitializableObjectTests
{
    #region Public methods
    [TestMethod]
    public void BeginInit_CalledTwice_Throws()
    {
        TestInitializable obj = new TestInitializable();
        obj.BeginInit();

        Assert.ThrowsExactly<InvalidOperationException>(() => obj.BeginInit());
    }

    [TestMethod]
    public void BeginInit_SetsIsInitializing()
    {
        TestInitializable obj = new TestInitializable();

        obj.BeginInit();

        Assert.IsTrue(obj.IsInitializing);
        Assert.IsFalse(obj.IsInitialized);
        Assert.IsTrue(obj.BeginInitCalled);
    }

    [TestMethod]
    public void EndInit_SetsIsInitializedAndRaisesEvent()
    {
        TestInitializable obj = new TestInitializable();
        bool eventRaised = false;
        obj.Initialized += (_, _) => eventRaised = true;

        obj.BeginInit();
        obj.EndInit();

        Assert.IsFalse(obj.IsInitializing);
        Assert.IsTrue(obj.IsInitialized);
        Assert.IsTrue(obj.EndInitCalled);
        Assert.IsTrue(eventRaised);
    }

    [TestMethod]
    public void EndInit_WithoutBeginInit_Throws()
    {
        TestInitializable obj = new TestInitializable();

        Assert.ThrowsExactly<InvalidOperationException>(() => obj.EndInit());
    }

    [TestMethod]
    public void InitialState_NotInitializedOrInitializing()
    {
        TestInitializable obj = new TestInitializable();

        Assert.IsFalse(obj.IsInitializing);
        Assert.IsFalse(obj.IsInitialized);
    }

    [TestMethod]
    public void MultipleInitCycles_WorkCorrectly()
    {
        TestInitializable obj = new TestInitializable();
        int initCount = 0;
        obj.Initialized += (_, _) => initCount++;

        obj.BeginInit();
        obj.EndInit();

        obj.BeginInit();
        obj.EndInit();

        Assert.AreEqual(2, initCount);
        Assert.IsTrue(obj.IsInitialized);
    }
    #endregion

    sealed class TestInitializable : InitializableObject
    {
        #region Protected methods
        protected override void OnBeginInit() { BeginInitCalled = true; }
        protected override void OnEndInit() { EndInitCalled = true; }
        #endregion

        #region Public properties
        public bool BeginInitCalled { get; private set; }

        public bool EndInitCalled { get; private set; }
        #endregion
    }
}
