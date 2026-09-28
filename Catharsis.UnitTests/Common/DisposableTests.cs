using Catharsis.Common;

namespace Catharsis.UnitTests.Common;

///<summary>
///Unit tests for the <see cref="Disposable"/> class.
///</summary>
[TestClass]
public class DisposableTests
{
    #region Create

    [TestMethod]
    public void Create_NullAction_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => Disposable.Create(null!)); }

    [TestMethod]
    public void Create_Dispose_InvokesAction()
    {
        bool invoked = false;
        IDisposable disposable = Disposable.Create(() => invoked = true);

        disposable.Dispose();

        Assert.IsTrue(invoked);
    }

    [TestMethod]
    public void Create_DisposeTwice_InvokesActionOnlyOnce()
    {
        int count = 0;
        IDisposable disposable = Disposable.Create(() => count++);

        disposable.Dispose();
        disposable.Dispose();

        Assert.AreEqual(1, count);
    }

    #endregion

    #region Combine

    [TestMethod]
    public void Combine_NullArray_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => Disposable.Combine(null!)); }

    [TestMethod]
    public void Combine_Dispose_DisposesAllInOrder()
    {
        List<int> order = [];
        IDisposable first = Disposable.Create(() => order.Add(1));
        IDisposable second = Disposable.Create(() => order.Add(2));

        Disposable.Combine(first, second).Dispose();

        CollectionAssert.AreEqual(new[] { 1, 2 }, order);
    }

    #endregion

    #region Empty

    [TestMethod]
    public void Empty_Dispose_DoesNotThrow() { Disposable.Empty.Dispose(); }

    #endregion
}
