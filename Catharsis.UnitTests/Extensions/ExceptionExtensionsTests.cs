using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

///<summary>
///Unit tests for the <see cref="ExceptionExtensions"/> class.
///</summary>
[TestClass]
public class ExceptionExtensionsTests
{
    #region Flatten

    [TestMethod]
    public void Flatten_NullException_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Exception)null!).Flatten());
    }

    [TestMethod]
    public void Flatten_NoInnerException_ReturnsJustItself()
    {
        InvalidOperationException ex = new("boom");
        List<Exception> result = [.. ex.Flatten()];
        CollectionAssert.AreEqual(new Exception[] { ex }, result);
    }

    [TestMethod]
    public void Flatten_ChainedInnerExceptions_ReturnsOutermostFirst()
    {
        Exception root = new("root");
        Exception middle = new("middle", root);
        Exception outer = new("outer", middle);

        List<Exception> result = [.. outer.Flatten()];

        CollectionAssert.AreEqual(new[] { outer, middle, root }, result);
    }

    [TestMethod]
    public void Flatten_AggregateException_ReturnsSelfAndAllInnerExceptions()
    {
        InvalidOperationException first = new("first");
        ArgumentException second = new("second");
        Exception aggregate = new AggregateException(first, second);

        List<Exception> result = [.. aggregate.Flatten()];

        Assert.HasCount(3, result);
        Assert.AreSame(aggregate, result[0]);
        CollectionAssert.Contains(result, first);
        CollectionAssert.Contains(result, second);
    }

    #endregion

    #region GetAllMessages

    [TestMethod]
    public void GetAllMessages_ChainedExceptions_JoinsMessagesInOrder()
    {
        Exception root = new("root cause");
        Exception outer = new("outer failure", root);

        Assert.AreEqual("outer failure -> root cause", outer.GetAllMessages());
    }

    [TestMethod]
    public void GetAllMessages_CustomSeparator_IsUsed()
    {
        Exception root = new("root");
        Exception outer = new("outer", root);

        Assert.AreEqual("outer | root", outer.GetAllMessages(" | "));
    }

    #endregion

    #region GetRootCause

    [TestMethod]
    public void GetRootCause_NullException_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Exception)null!).GetRootCause());
    }

    [TestMethod]
    public void GetRootCause_NoInnerException_ReturnsItself()
    {
        InvalidOperationException ex = new("boom");
        Assert.AreSame(ex, ex.GetRootCause());
    }

    [TestMethod]
    public void GetRootCause_ChainedExceptions_ReturnsInnermost()
    {
        Exception root = new("root");
        Exception middle = new("middle", root);
        Exception outer = new("outer", middle);

        Assert.AreSame(root, outer.GetRootCause());
    }

    [TestMethod]
    public void GetRootCause_AggregateException_WalksIntoFirstInnerException()
    {
        Exception rootOfFirst = new("root of first");
        Exception first = new("first", rootOfFirst);
        AggregateException aggregate = new(first, new ArgumentException("second"));

        Assert.AreSame(rootOfFirst, aggregate.GetRootCause());
    }

    #endregion
}
