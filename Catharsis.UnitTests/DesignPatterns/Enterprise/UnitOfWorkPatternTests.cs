using Catharsis.DesignPatterns.Enterprise;

namespace Catharsis.UnitTests.DesignPatterns.Enterprise;

///<summary>
///Unit tests for the <see cref="UnitOfWorkPattern"/> class.
///</summary>
[TestClass]
public class UnitOfWorkPatternTests
{
    #region UnitOfWork (Action)

    [TestMethod]
    public void UnitOfWork_NullWork_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => UnitOfWorkPattern.UnitOfWork(null!, static () => { }, static () => { })); }

    [TestMethod]
    public void UnitOfWork_NullCommit_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => UnitOfWorkPattern.UnitOfWork(static () => { }, null!, static () => { })); }

    [TestMethod]
    public void UnitOfWork_NullRollback_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => UnitOfWorkPattern.UnitOfWork(static () => { }, static () => { }, null!)); }

    [TestMethod]
    public void UnitOfWork_WorkSucceeds_InvokesCommitNotRollback()
    {
        bool committed = false;
        bool rolledBack = false;

        UnitOfWorkPattern.UnitOfWork(static () => { }, () => committed = true, () => rolledBack = true);

        Assert.IsTrue(committed);
        Assert.IsFalse(rolledBack);
    }

    [TestMethod]
    public void UnitOfWork_WorkThrows_InvokesRollbackAndRethrows()
    {
        bool committed = false;
        bool rolledBack = false;

        Assert.ThrowsExactly<InvalidOperationException>(() => UnitOfWorkPattern.UnitOfWork(static () => throw new InvalidOperationException(), () => committed = true, () => rolledBack = true));

        Assert.IsFalse(committed);
        Assert.IsTrue(rolledBack);
    }

    #endregion

    #region UnitOfWork (Func<TResult>)

    [TestMethod]
    public void UnitOfWorkOfResult_WorkSucceeds_ReturnsResultAndCommits()
    {
        bool committed = false;

        int result = UnitOfWorkPattern.UnitOfWork(static () => 42, () => committed = true, static () => { });

        Assert.AreEqual(42, result);
        Assert.IsTrue(committed);
    }

    [TestMethod]
    public void UnitOfWorkOfResult_WorkThrows_InvokesRollbackAndRethrows()
    {
        bool rolledBack = false;

        Assert.ThrowsExactly<InvalidOperationException>(() => UnitOfWorkPattern.UnitOfWork<int>(static () => throw new InvalidOperationException(), static () => { }, () => rolledBack = true));

        Assert.IsTrue(rolledBack);
    }

    #endregion
}
