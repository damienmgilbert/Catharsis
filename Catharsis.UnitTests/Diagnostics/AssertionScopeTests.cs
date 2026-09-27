using Catharsis.Diagnostics;

namespace Catharsis.UnitTests.Diagnostics;

///<summary>
///Unit tests for the <see cref="AssertionScope"/> class.
///</summary>
[TestClass]
public class AssertionScopeTests
{
    #region Check

    [TestMethod]
    public void Check_NullMessage_Throws()
    {
        using AssertionScope scope = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => scope.Check(false, null!));
    }

    [TestMethod]
    public void Check_TrueCondition_RecordsNoFailure()
    {
        using AssertionScope scope = new();
        scope.Check(true, "should not appear");
        Assert.IsFalse(scope.HasFailures);
    }

    [TestMethod]
    public void Check_FalseCondition_RecordsFailure()
    {
        AssertionScope scope = new();
        scope.Check(false, "boom");

        Assert.IsTrue(scope.HasFailures);
        CollectionAssert.Contains(scope.Failures.ToList(), "boom");
    }

    [TestMethod]
    public void Check_ReturnsScope_ForChaining()
    {
        using AssertionScope scope = new();
        AssertionScope result = scope.Check(true, "a").Check(true, "b");
        Assert.AreSame(scope, result);
    }

    #endregion

    #region ThrowIfAny / Dispose

    [TestMethod]
    public void ThrowIfAny_NoFailures_DoesNotThrow()
    {
        AssertionScope scope = new();
        scope.Check(true, "fine");
        scope.ThrowIfAny();
    }

    [TestMethod]
    public void ThrowIfAny_WithFailures_ThrowsAggregateExceptionWithAllMessages()
    {
        AssertionScope scope = new();
        scope.Check(false, "first");
        scope.Check(false, "second");

        AggregateException exception = Assert.ThrowsExactly<AggregateException>(scope.ThrowIfAny);

        Assert.HasCount(2, exception.InnerExceptions);
    }

    [TestMethod]
    public void ThrowIfAny_CalledTwice_ThrowsOnlyOnce()
    {
        AssertionScope scope = new();
        scope.Check(false, "boom");

        Assert.ThrowsExactly<AggregateException>(scope.ThrowIfAny);
        scope.ThrowIfAny();
    }

    [TestMethod]
    public void Dispose_WithFailures_Throws()
    {
        AssertionScope scope = new();
        scope.Check(false, "boom");

        Assert.ThrowsExactly<AggregateException>(scope.Dispose);
    }

    [TestMethod]
    public void Dispose_WithoutFailures_DoesNotThrow()
    {
        AssertionScope scope = new();
        scope.Check(true, "fine");
        scope.Dispose();
    }

    #endregion
}
