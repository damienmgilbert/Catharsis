using Catharsis.Generics;

namespace Catharsis.UnitTests.Generics;

///<summary>
///Unit tests for the <see cref="ExpressionMapper{TSource, TDest}"/> class.
///</summary>
[TestClass]
public class ExpressionMapperTests
{
    sealed class Person
    {
        public string First { get; set; } = "";
        public string Last { get; set; } = "";
        public int Age { get; set; }
        public string Secret { get; set; } = "";
    }

    sealed class PersonDto
    {
        public string First { get; set; } = "";
        public string Full { get; set; } = "";
        public int Age { get; set; }
        public string Secret { get; set; } = "default";
        public string ReadOnly { get; } = "ro";
        public string Unmatched { get; set; } = "keep";
    }

    static readonly Person Sample = new() { First = "Ada", Last = "Lovelace", Age = 36, Secret = "s3" };

    [TestMethod]
    public void Build_MapsSameNamedCompatibleProperties()
    {
        PersonDto dto = new ExpressionMapper<Person, PersonDto>().Build()(Sample);

        Assert.AreEqual("Ada", dto.First);
        Assert.AreEqual(36, dto.Age);
        Assert.AreEqual("s3", dto.Secret);
    }

    sealed class NarrowSource { public string Age { get; set; } = "36"; }

    sealed class WideTarget { public long Age { get; set; } = -1; }

    [TestMethod]
    public void Build_NonAssignableTypes_AreNotMapped() { Assert.AreEqual(-1L, new ExpressionMapper<NarrowSource, WideTarget>().Build()(new NarrowSource()).Age); }

    [TestMethod]
    public void Build_LeavesUnmatchedAndReadOnlyPropertiesAlone()
    {
        PersonDto dto = new ExpressionMapper<Person, PersonDto>().Build()(Sample);

        Assert.AreEqual("keep", dto.Unmatched);
        Assert.AreEqual("ro", dto.ReadOnly);
        Assert.AreEqual("", dto.Full);
    }

    [TestMethod]
    public void Map_ComputesDestinationFromSourceExpression()
    {
        Func<Person, PersonDto> map = new ExpressionMapper<Person, PersonDto>().Map(static d => d.Full, static s => s.First + " " + s.Last).Build();

        Assert.AreEqual("Ada Lovelace", map(Sample).Full);
    }

    [TestMethod]
    public void Map_OverridesAutomaticMapping()
    {
        Func<Person, PersonDto> map = new ExpressionMapper<Person, PersonDto>().Map(static d => d.First, static s => s.Last).Build();

        Assert.AreEqual("Lovelace", map(Sample).First);
    }

    [TestMethod]
    public void Ignore_KeepsConstructorDefault()
    {
        Func<Person, PersonDto> map = new ExpressionMapper<Person, PersonDto>().Ignore(static d => d.Secret).Build();

        Assert.AreEqual("default", map(Sample).Secret);
    }

    [TestMethod]
    public void Map_NonPropertySelector_Throws()
    {
        ExpressionMapper<Person, PersonDto> mapper = new();
        Assert.ThrowsExactly<ArgumentException>(() => mapper.Map(static d => d.First.Length, static s => s.Age));
    }

    [TestMethod]
    public void Map_ReadOnlyDestination_Throws()
    {
        ExpressionMapper<Person, PersonDto> mapper = new();
        Assert.ThrowsExactly<ArgumentException>(() => mapper.Map(static d => d.ReadOnly, static s => s.First));
    }

    [TestMethod]
    public void Map_NullArguments_Throw()
    {
        ExpressionMapper<Person, PersonDto> mapper = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => mapper.Map(static d => d.First, null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => mapper.Ignore<string>(null!));
    }

    [TestMethod]
    public void Build_CompiledDelegate_CreatesNewInstanceEachCall()
    {
        Func<Person, PersonDto> map = new ExpressionMapper<Person, PersonDto>().Build();

        Assert.AreNotSame(map(Sample), map(Sample));
    }
}
