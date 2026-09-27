using Catharsis.Diagnostics;

namespace Catharsis.UnitTests.Diagnostics;

///<summary>
///Unit tests for the <see cref="InvariantGuard{T}"/> class.
///</summary>
[TestClass]
public class InvariantGuardTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullInvariant_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new InvariantGuard<int>(1, null!));
    }

    [TestMethod]
    public void Constructor_InitialValueViolatesInvariant_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new InvariantGuard<int>(-1, static x => x >= 0));
    }

    [TestMethod]
    public void Constructor_InitialValueSatisfiesInvariant_SetsValue()
    {
        InvariantGuard<int> guard = new(5, static x => x >= 0);
        Assert.AreEqual(5, guard.Value);
    }

    #endregion

    #region Value

    [TestMethod]
    public void Value_SetSatisfyingInvariant_Succeeds()
    {
        InvariantGuard<int> guard = new(5, static x => x >= 0);
        guard.Value = 10;
        Assert.AreEqual(10, guard.Value);
    }

    [TestMethod]
    public void Value_SetViolatingInvariant_ThrowsAndKeepsPreviousValue()
    {
        InvariantGuard<int> guard = new(5, static x => x >= 0);

        Assert.ThrowsExactly<InvalidOperationException>(() => guard.Value = -1);
        Assert.AreEqual(5, guard.Value);
    }

    [TestMethod]
    public void Value_SetViolatingInvariant_UsesCustomMessage()
    {
        InvariantGuard<int> guard = new(5, static x => x >= 0, "must be non-negative");

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => guard.Value = -1);
        Assert.AreEqual("must be non-negative", exception.Message);
    }

    #endregion
}
