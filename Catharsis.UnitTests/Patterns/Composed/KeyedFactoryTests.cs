using Catharsis.Patterns.Composed;

namespace Catharsis.UnitTests.Patterns.Composed;

///<summary>
///Unit tests for the <see cref="KeyedFactory{TKey, T}"/> class.
///</summary>
[TestClass]
public class KeyedFactoryTests
{
    interface IShape { string Name { get; } }

    sealed class Circle : IShape { public string Name => "circle"; }

    sealed class Square : IShape { public string Name => "square"; }

    [TestMethod]
    public void Create_RunsRegisteredCreator()
    {
        KeyedFactory<string, IShape> factory = new KeyedFactory<string, IShape>().Register<Circle>("c").Register("s", static () => new Square());

        Assert.AreEqual("circle", factory.Create("c").Name);
        Assert.AreEqual("square", factory.Create("s").Name);
    }

    [TestMethod]
    public void Create_ReturnsNewInstanceEachCall()
    {
        KeyedFactory<string, IShape> factory = new KeyedFactory<string, IShape>().Register<Circle>("c");

        Assert.AreNotSame(factory.Create("c"), factory.Create("c"));
    }

    [TestMethod]
    public void Create_UnknownKey_Throws() { Assert.ThrowsExactly<KeyNotFoundException>(static () => new KeyedFactory<string, IShape>().Create("x")); }

    [TestMethod]
    public void TryCreate_ReportsPresence()
    {
        KeyedFactory<string, IShape> factory = new KeyedFactory<string, IShape>().Register<Circle>("c");

        Assert.IsTrue(factory.TryCreate("c", out IShape shape));
        Assert.AreEqual("circle", shape.Name);
        Assert.IsFalse(factory.TryCreate("x", out _));
    }

    [TestMethod]
    public void Register_DuplicateKey_Throws()
    {
        KeyedFactory<string, IShape> factory = new KeyedFactory<string, IShape>().Register<Circle>("c");

        Assert.ThrowsExactly<InvalidOperationException>(() => factory.Register<Square>("c"));
    }

    [TestMethod]
    public void Register_NullCreator_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new KeyedFactory<string, IShape>().Register("c", null!)); }

    [TestMethod]
    public void Comparer_IsHonored()
    {
        KeyedFactory<string, IShape> factory = new KeyedFactory<string, IShape>(StringComparer.OrdinalIgnoreCase).Register<Circle>("Circle");

        Assert.AreEqual("circle", factory.Create("CIRCLE").Name);
    }

    [TestMethod]
    public void Works_WithEnumKeys_AndListsKeys()
    {
        KeyedFactory<DayOfWeek, IShape> factory = new KeyedFactory<DayOfWeek, IShape>().Register<Circle>(DayOfWeek.Monday);

        CollectionAssert.AreEqual(new[] { DayOfWeek.Monday }, factory.Keys.ToArray());
    }
}
