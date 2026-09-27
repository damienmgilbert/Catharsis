using Catharsis.Xml;
using System.Xml.Linq;

namespace Catharsis.UnitTests.Xml;

///<summary>
///Unit tests for the <see cref="XmlRecordReader{T}"/> class.
///</summary>
[TestClass]
public class XmlRecordReaderTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullRecordElementName_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new XmlRecordReader<Person>(null!)); }

    [TestMethod]
    public void Constructor_WhitespaceRecordElementName_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new XmlRecordReader<Person>("   ")); }

    #endregion

    #region ReadRecords

    [TestMethod]
    public void ReadRecords_NullContainer_Throws()
    {
        XmlRecordReader<Person> reader = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => reader.ReadRecords(null!).ToList());
    }

    [TestMethod]
    public void ReadRecords_ElementChildren_MapsPropertiesByName()
    {
        XDocument document = XDocument.Parse("<Records><Record><Name>Alice</Name><Age>30</Age></Record></Records>");
        XmlRecordReader<Person> reader = new();

        List<Person> records = [.. reader.ReadRecords(document)];

        Assert.HasCount(1, records);
        Assert.AreEqual("Alice", records[0].Name);
        Assert.AreEqual(30, records[0].Age);
    }

    [TestMethod]
    public void ReadRecords_Attributes_MapsPropertiesByName()
    {
        XDocument document = XDocument.Parse("<Records><Record Name=\"Bob\" Age=\"25\" /></Records>");
        XmlRecordReader<Person> reader = new();

        List<Person> records = [.. reader.ReadRecords(document)];

        Assert.AreEqual("Bob", records[0].Name);
        Assert.AreEqual(25, records[0].Age);
    }

    [TestMethod]
    public void ReadRecords_MultipleRecords_ReturnsAllInOrder()
    {
        XDocument document = XDocument.Parse("<Records><Record><Name>Alice</Name></Record><Record><Name>Bob</Name></Record></Records>");
        XmlRecordReader<Person> reader = new();

        List<Person> records = [.. reader.ReadRecords(document)];

        CollectionAssert.AreEqual(new[] { "Alice", "Bob" }, records.Select(static p => p.Name).ToList());
    }

    [TestMethod]
    public void ReadRecords_UnknownElement_IsIgnored()
    {
        XDocument document = XDocument.Parse("<Records><Record><Name>Alice</Name><Unknown>x</Unknown></Record></Records>");
        XmlRecordReader<Person> reader = new();

        List<Person> records = [.. reader.ReadRecords(document)];

        Assert.AreEqual("Alice", records[0].Name);
    }

    [TestMethod]
    public void ReadRecords_CustomRecordElementName_MatchesConfiguredName()
    {
        XDocument document = XDocument.Parse("<Customers><Customer><Name>Alice</Name></Customer></Customers>");
        XmlRecordReader<Person> reader = new("Customer");

        List<Person> records = [.. reader.ReadRecords(document)];

        Assert.AreEqual("Alice", records[0].Name);
    }

    [TestMethod]
    public void ReadRecords_NestedAtAnyDepth_AreFound()
    {
        XDocument document = XDocument.Parse("<Root><Group><Record><Name>Alice</Name></Record></Group></Root>");
        XmlRecordReader<Person> reader = new();

        List<Person> records = [.. reader.ReadRecords(document)];

        Assert.AreEqual("Alice", records[0].Name);
    }

    #endregion

    private sealed class Person
    {
        #region Public properties
        public int Age { get; set; }
        public string? Name { get; set; }
        #endregion
    }
}
