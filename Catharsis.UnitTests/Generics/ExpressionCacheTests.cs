using Catharsis.Generics;
using System.Linq.Expressions;

namespace Catharsis.UnitTests.Generics;

///<summary>
///Unit tests for the <see cref="ExpressionCache"/> class.
///</summary>
[TestClass]
public class ExpressionCacheTests
{
    sealed class Sample
    {
        public int Number { get; set; }
        public string Name { get; set; } = "";
        public int Write { private get; set; }
    }

    [TestMethod]
    public void GetOrCompile_SameKey_ReturnsSameDelegate()
    {
        ExpressionCache cache = new();
        Expression<Func<int, int>> first = static x => x + 1;
        Expression<Func<int, int>> second = static x => x + 2;

        Func<int, int> a = cache.GetOrCompile("k", first);
        Func<int, int> b = cache.GetOrCompile("k", second);

        Assert.AreSame(a, b);
        Assert.AreEqual(4, b(3));
        Assert.AreEqual(1, cache.Count);
    }

    [TestMethod]
    public void GetOrCompile_DifferentKeys_CompileSeparately()
    {
        ExpressionCache cache = new();

        Func<int, int> a = cache.GetOrCompile<Func<int, int>>("a", static x => x + 1);
        Func<int, int> b = cache.GetOrCompile<Func<int, int>>("b", static x => x + 2);

        Assert.AreEqual(4, a(3));
        Assert.AreEqual(5, b(3));
    }

    [TestMethod]
    public void GetOrCompile_KeyReusedForDifferentDelegateType_Throws()
    {
        ExpressionCache cache = new();
        cache.GetOrCompile<Func<int, int>>("k", static x => x);

        Assert.ThrowsExactly<InvalidCastException>(() => cache.GetOrCompile<Func<int, string>>("k", static x => x.ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }

    [TestMethod]
    public void GetOrCompile_NullArguments_Throw()
    {
        ExpressionCache cache = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => cache.GetOrCompile<Func<int>>(null!, static () => 1));
        Assert.ThrowsExactly<ArgumentNullException>(() => cache.GetOrCompile<Func<int>>("k", null!));
    }

    [TestMethod]
    public void GetGetter_ReadsProperty_AndIsCached()
    {
        ExpressionCache cache = new();
        Func<Sample, int> getter = cache.GetGetter<Sample, int>(nameof(Sample.Number));

        Assert.AreEqual(9, getter(new Sample { Number = 9 }));
        Assert.AreSame(getter, cache.GetGetter<Sample, int>(nameof(Sample.Number)));
    }

    [TestMethod]
    public void GetGetter_AssignableType_Works()
    {
        Func<Sample, object> getter = new ExpressionCache().GetGetter<Sample, object>(nameof(Sample.Name));

        Assert.AreEqual("n", getter(new Sample { Name = "n" }));
    }

    [TestMethod]
    public void GetGetter_MissingOrWrongTypeOrUnreadable_Throws()
    {
        ExpressionCache cache = new();
        Assert.ThrowsExactly<ArgumentException>(() => cache.GetGetter<Sample, int>("Nope"));
        Assert.ThrowsExactly<ArgumentException>(() => cache.GetGetter<Sample, string>(nameof(Sample.Number)));
        Assert.ThrowsExactly<ArgumentException>(() => cache.GetGetter<Sample, int>(nameof(Sample.Write)));
    }

    [TestMethod]
    public void GetGetter_EmptyName_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new ExpressionCache().GetGetter<Sample, int>("")); }

    [TestMethod]
    public void Clear_DiscardsEntries()
    {
        ExpressionCache cache = new();
        cache.GetGetter<Sample, int>(nameof(Sample.Number));
        cache.Clear();

        Assert.AreEqual(0, cache.Count);
    }
}
