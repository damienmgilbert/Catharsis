using Catharsis.Services;
using System.Text;

namespace Catharsis.UnitTests.Services;

///<summary>
///Unit tests for the <see cref="DefaultPooledObjectPolicy{T}"/> class.
///</summary>
[TestClass]
public class DefaultPooledObjectPolicyTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullFactory_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new DefaultPooledObjectPolicy<object>(null!)); }

    #endregion

    #region Create

    [TestMethod]
    public void Create_InvokesFactory()
    {
        int calls = 0;
        DefaultPooledObjectPolicy<object> policy = new(() => { calls++; return new object(); });

        object created = policy.Create();

        Assert.IsNotNull(created);
        Assert.AreEqual(1, calls);
    }

    #endregion

    #region Return

    [TestMethod]
    public void Return_NoPredicate_AlwaysReturnsTrue()
    {
        DefaultPooledObjectPolicy<object> policy = new(static () => new object());
        Assert.IsTrue(policy.Return(new object()));
    }

    [TestMethod]
    public void Return_WithPredicate_UsesPredicateResult()
    {
        DefaultPooledObjectPolicy<StringBuilder> policy = new(static () => new StringBuilder(), static sb => sb.Length == 0);

        StringBuilder empty = new();
        StringBuilder nonEmpty = new("data");

        Assert.IsTrue(policy.Return(empty));
        Assert.IsFalse(policy.Return(nonEmpty));
    }

    [TestMethod]
    public void Return_PredicateCanResetState_AsSideEffect()
    {
        DefaultPooledObjectPolicy<List<int>> policy = new(static () => [], static list => { list.Clear(); return true; });

        List<int> list = [1, 2, 3];
        bool kept = policy.Return(list);

        Assert.IsTrue(kept);
        Assert.IsEmpty(list);
    }

    #endregion
}
