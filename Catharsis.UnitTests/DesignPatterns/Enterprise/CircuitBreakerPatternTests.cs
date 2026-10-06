using Catharsis.DesignPatterns.Enterprise;

namespace Catharsis.UnitTests.DesignPatterns.Enterprise;

///<summary>
///Unit tests for the <see cref="CircuitBreakerPattern"/> class.
///</summary>
[TestClass]
public class CircuitBreakerPatternTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NonPositiveThreshold_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new CircuitBreakerPattern(0)); }

    #endregion

    #region Execute

    [TestMethod]
    public void Execute_NullAction_Throws()
    {
        CircuitBreakerPattern breaker = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => breaker.Execute<int>(null!));
    }

    [TestMethod]
    public void Execute_Succeeds_ReturnsResultAndStaysClosed()
    {
        CircuitBreakerPattern breaker = new(failureThreshold: 2);
        int result = breaker.Execute(static () => 42);

        Assert.AreEqual(42, result);
        Assert.IsFalse(breaker.IsOpen);
    }

    [TestMethod]
    public void Execute_FailuresBelowThreshold_StaysClosed()
    {
        CircuitBreakerPattern breaker = new(failureThreshold: 2);

        Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));

        Assert.IsFalse(breaker.IsOpen);
    }

    [TestMethod]
    public void Execute_FailuresReachThreshold_Opens()
    {
        CircuitBreakerPattern breaker = new(failureThreshold: 2);

        for (int i = 0; i < 2; i++)
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));
        }

        Assert.IsTrue(breaker.IsOpen);
    }

    [TestMethod]
    public void Execute_WhenOpen_RejectsWithoutRunningAction()
    {
        CircuitBreakerPattern breaker = new(failureThreshold: 1);
        Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));

        bool ran = false;
        Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute(() => { ran = true; return 1; }));

        Assert.IsFalse(ran);
    }

    [TestMethod]
    public void Execute_SuccessAfterFailure_ResetsFailureCount()
    {
        CircuitBreakerPattern breaker = new(failureThreshold: 2);
        Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));

        breaker.Execute(static () => 1);

        Assert.IsFalse(breaker.IsOpen);
    }

    #endregion

    #region Reset

    [TestMethod]
    public void Reset_WhenOpen_ClosesBreaker()
    {
        CircuitBreakerPattern breaker = new(failureThreshold: 1);
        Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));
        Assert.IsTrue(breaker.IsOpen);

        breaker.Reset();

        Assert.IsFalse(breaker.IsOpen);
    }

    #endregion
}
