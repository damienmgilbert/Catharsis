using Catharsis.Generics;

namespace Catharsis.UnitTests.Generics;

///<summary>
///Unit tests for the <see cref="Maybe{T}"/> struct.
///</summary>
[TestClass]
public class MaybeTests
{
    [TestMethod]
    public void Some_Null_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => Maybe<string>.Some(null!)); }

    [TestMethod]
    public void From_Null_IsNone() { Assert.IsFalse(Maybe<string>.From(null).HasValue); }

    [TestMethod]
    public void From_Value_IsSome() { Assert.AreEqual("a", Maybe<string>.From("a").Value); }

    [TestMethod]
    public void None_Value_Throws() { Assert.ThrowsExactly<InvalidOperationException>(static () => Maybe<int>.None.Value); }

    [TestMethod]
    public void Default_IsNone() { Assert.IsFalse(default(Maybe<int>).HasValue); }

    [TestMethod]
    public void ImplicitConversion_WrapsValueAndNull()
    {
        Maybe<string> some = "x";
        Maybe<string> none = (string?)null;
        Assert.IsTrue(some.HasValue);
        Assert.IsFalse(none.HasValue);
    }

    [TestMethod]
    public void Map_TransformsValue_NullResultBecomesNone()
    {
        Assert.AreEqual(4, Maybe<int>.Some(2).Map(static x => x * 2).Value);
        Assert.IsFalse(Maybe<int>.Some(2).Map(static _ => (string?)null).HasValue);
        Assert.IsFalse(Maybe<int>.None.Map(static x => x * 2).HasValue);
    }

    [TestMethod]
    public void Bind_ChainsOptions()
    {
        Assert.AreEqual(3, Maybe<int>.Some(2).Bind(static x => Maybe<int>.Some(x + 1)).Value);
        Assert.IsFalse(Maybe<int>.None.Bind(static x => Maybe<int>.Some(x)).HasValue);
    }

    [TestMethod]
    public void Where_FiltersValue()
    {
        Assert.IsTrue(Maybe<int>.Some(4).Where(static x => x > 3).HasValue);
        Assert.IsFalse(Maybe<int>.Some(2).Where(static x => x > 3).HasValue);
    }

    [TestMethod]
    public void Match_PicksBranch()
    {
        Assert.AreEqual("some 1", Maybe<int>.Some(1).Match(static x => $"some {x}", static () => "none"));
        Assert.AreEqual("none", Maybe<int>.None.Match(static x => $"some {x}", static () => "none"));
    }

    [TestMethod]
    public void GetValueOrDefault_And_TryGetValue()
    {
        Assert.AreEqual(9, Maybe<int>.None.GetValueOrDefault(9));
        Assert.IsTrue(Maybe<int>.Some(1).TryGetValue(out int value));
        Assert.AreEqual(1, value);
        Assert.IsFalse(Maybe<int>.None.TryGetValue(out _));
    }

    [TestMethod]
    public void Equality_ComparesPresenceAndValue()
    {
        Assert.IsTrue(Maybe<int>.Some(1) == Maybe<int>.Some(1));
        Assert.IsTrue(Maybe<int>.Some(1) != Maybe<int>.None);
        Assert.IsTrue(Maybe<int>.None == default);
        Assert.AreEqual(Maybe<int>.Some(1).GetHashCode(), Maybe<int>.Some(1).GetHashCode());
    }

    [TestMethod]
    public void ToString_ShowsCase()
    {
        Assert.AreEqual("Some(1)", Maybe<int>.Some(1).ToString());
        Assert.AreEqual("None", Maybe<int>.None.ToString());
    }
}
