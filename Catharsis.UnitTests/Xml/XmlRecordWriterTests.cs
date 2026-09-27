using Catharsis.Xml;
using System.Xml.Linq;

namespace Catharsis.UnitTests.Xml;

///<summary>
///Unit tests for the <see cref="XmlRecordWriter{T}"/> class.
///</summary>
[TestClass]
public class XmlRecordWriterTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_WhitespaceRecordElementName_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new XmlRecordWriter<Person>(" ")); }

    #endregion

    #region WriteRecord

    [TestMethod]
    public void WriteRecord_NullRecord_Throws()
    {
        XmlRecordWriter<Person> writer = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => writer.WriteRecord(null!));
    }

    [TestMethod]
    public void WriteRecord_CreatesOneChildElementPerProperty()
    {
        XmlRecordWriter<Person> writer = new();
        XElement element = writer.WriteRecord(new Person("Alice", 30));

        Assert.AreEqual("Record", element.Name.LocalName);
        Assert.AreEqual("Alice", element.Element("Name")!.Value);
        Assert.AreEqual("30", element.Element("Age")!.Value);
    }

    [TestMethod]
    public void WriteRecord_CustomElementName_IsUsed()
    {
        XmlRecordWriter<Person> writer = new("Customer");
        XElement element = writer.WriteRecord(new Person("Alice", 30));

        Assert.AreEqual("Customer", element.Name.LocalName);
    }

    #endregion

    #region WriteRecords

    [TestMethod]
    public void WriteRecords_NullRecords_Throws()
    {
        XmlRecordWriter<Person> writer = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => writer.WriteRecords(null!));
    }

    [TestMethod]
    public void WriteRecords_WhitespaceContainerName_Throws()
    {
        XmlRecordWriter<Person> writer = new();
        Assert.ThrowsExactly<ArgumentException>(() => writer.WriteRecords([], " "));
    }

    [TestMethod]
    public void WriteRecords_WrapsOneChildPerRecord()
    {
        XmlRecordWriter<Person> writer = new();
        XElement container = writer.WriteRecords([new Person("Alice", 30), new Person("Bob", 25)]);

        Assert.AreEqual("Records", container.Name.LocalName);
        Assert.HasCount(2, container.Elements("Record").ToList());
    }

    #endregion

    #region Round-trip with XmlRecordReader

    [TestMethod]
    public void RoundTrip_WriteThenRead_ProducesEquivalentRecord()
    {
        XmlRecordWriter<Person> writer = new();
        XmlRecordReader<Person> reader = new();

        XElement container = writer.WriteRecords([new Person("Alice", 30)]);
        List<Person> records = [.. reader.ReadRecords(container)];

        Assert.AreEqual("Alice", records[0].Name);
        Assert.AreEqual(30, records[0].Age);
    }

    #endregion

    private sealed class Person
    {
        #region Constructors
        public Person() { }

        public Person(string name, int age)
        {
            Name = name;
            Age = age;
        }
        #endregion

        #region Public properties
        public int Age { get; set; }
        public string? Name { get; set; }
        #endregion
    }
}
