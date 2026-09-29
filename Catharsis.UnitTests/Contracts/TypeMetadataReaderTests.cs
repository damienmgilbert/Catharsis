using Catharsis.Contracts;
using System.Reflection;

namespace Catharsis.UnitTests.Contracts;

///<summary>
///Unit tests for <see cref="TypeMetadataReader"/>.
///</summary>
[TestClass]
public class TypeMetadataReaderTests
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    sealed class TagAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    [Tag("class")]
    sealed class Sample
    {
        [Tag("one")]
        public int First { get; set; }

        public int Untagged { get; set; }

        [Tag("a")]
        [Tag("b")]
        public int Multi { get; set; }
    }

    [TestMethod]
    public void GetAttribute_Present_ReturnsIt() { Assert.AreEqual("class", new TypeMetadataReader().GetAttribute<TagAttribute>(typeof(Sample))!.Name); }

    [TestMethod]
    public void GetAttribute_Absent_ReturnsNull() { Assert.IsNull(new TypeMetadataReader().GetAttribute<TagAttribute>(typeof(Sample).GetProperty(nameof(Sample.Untagged))!)); }

    [TestMethod]
    public void GetAttribute_Multiple_Throws() { Assert.ThrowsExactly<AmbiguousMatchException>(static () => new TypeMetadataReader().GetAttribute<TagAttribute>(typeof(Sample).GetProperty(nameof(Sample.Multi))!)); }

    [TestMethod]
    public void GetAttributes_Multiple_ReturnsAll()
    {
        IReadOnlyList<TagAttribute> tags = new TypeMetadataReader().GetAttributes<TagAttribute>(typeof(Sample).GetProperty(nameof(Sample.Multi))!);

        CollectionAssert.AreEquivalent(new[] { "a", "b" }, tags.Select(static t => t.Name).ToArray());
    }

    [TestMethod]
    public void GetAttributes_NullMember_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new TypeMetadataReader().GetAttributes<TagAttribute>(null!)); }

    [TestMethod]
    public void GetAnnotatedProperties_ReturnsOnlyTaggedProperties()
    {
        IReadOnlyList<(PropertyInfo Property, TagAttribute Attribute)> found = new TypeMetadataReader().GetAnnotatedProperties<TagAttribute>(typeof(Sample));

        Assert.AreEqual(3, found.Count);
        Assert.IsFalse(found.Any(static f => f.Property.Name == nameof(Sample.Untagged)));
    }

    [TestMethod]
    public void Lookups_AreCached_AndClearResets()
    {
        TypeMetadataReader reader = new();

        reader.GetAttributes<TagAttribute>(typeof(Sample));
        reader.GetAttributes<TagAttribute>(typeof(Sample));
        Assert.AreEqual(1, reader.CachedCount);

        reader.Clear();
        Assert.AreEqual(0, reader.CachedCount);
    }
}
