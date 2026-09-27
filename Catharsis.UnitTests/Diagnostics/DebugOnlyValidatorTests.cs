using Catharsis.Diagnostics;

namespace Catharsis.UnitTests.Diagnostics;

///<summary>
///Unit tests for the <see cref="DebugOnlyValidator{T}"/> class.
///</summary>
///<remarks>
///These tests only observe validation behavior because the test project is compiled with the <c>DEBUG</c> symbol
///defined, which is what makes the compiler emit the <see cref="DebugOnlyValidator{T}.Validate"/> call sites at all.
///</remarks>
[TestClass]
public class DebugOnlyValidatorTests
{
    #region Public methods

    [TestMethod]
    public void Validate_NullPredicate_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => DebugOnlyValidator<int>.Validate(1, null!));
    }

    [TestMethod]
    public void Validate_PredicateReturnsTrue_DoesNotThrow()
    {
        DebugOnlyValidator<int>.Validate(5, static x => x > 0);
    }

    [TestMethod]
    public void Validate_PredicateReturnsFalse_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => DebugOnlyValidator<int>.Validate(-1, static x => x > 0));
    }

    [TestMethod]
    public void Validate_PredicateReturnsFalse_UsesCustomMessage()
    {
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => DebugOnlyValidator<int>.Validate(-1, static x => x > 0, "must be positive"));
        Assert.AreEqual("must be positive", exception.Message);
    }

    #endregion
}
