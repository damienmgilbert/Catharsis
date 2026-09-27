using Catharsis.Serialization;

namespace Catharsis.UnitTests.Serialization;

///<summary>
///Unit tests for the <see cref="DelimitedRecordReader{T}"/> class.
///</summary>
[TestClass]
public class DelimitedRecordReaderTests
{
    #region ReadRecords

    [TestMethod]
    public void ReadRecords_NullReader_Throws()
    {
        DelimitedRecordReader<Person> reader = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => reader.ReadRecords(null!).ToList());
    }

    [TestMethod]
    public void ReadRecords_HeaderOnly_ReturnsNoRecords()
    {
        DelimitedRecordReader<Person> reader = new();
        using StringReader source = new("Name,Age");

        List<Person> records = [.. reader.ReadRecords(source)];

        Assert.IsEmpty(records);
    }

    [TestMethod]
    public void ReadRecords_MapsColumnsByHeaderName()
    {
        DelimitedRecordReader<Person> reader = new();
        using StringReader source = new("Name,Age\nAlice,30\nBob,25");

        List<Person> records = [.. reader.ReadRecords(source)];

        Assert.HasCount(2, records);
        Assert.AreEqual("Alice", records[0].Name);
        Assert.AreEqual(30, records[0].Age);
        Assert.AreEqual("Bob", records[1].Name);
        Assert.AreEqual(25, records[1].Age);
    }

    [TestMethod]
    public void ReadRecords_HeaderOrderReversed_StillMapsCorrectly()
    {
        DelimitedRecordReader<Person> reader = new();
        using StringReader source = new("Age,Name\n30,Alice");

        List<Person> records = [.. reader.ReadRecords(source)];

        Assert.AreEqual("Alice", records[0].Name);
        Assert.AreEqual(30, records[0].Age);
    }

    [TestMethod]
    public void ReadRecords_UnknownColumn_IsIgnored()
    {
        DelimitedRecordReader<Person> reader = new();
        using StringReader source = new("Name,Unknown,Age\nAlice,x,30");

        List<Person> records = [.. reader.ReadRecords(source)];

        Assert.AreEqual("Alice", records[0].Name);
        Assert.AreEqual(30, records[0].Age);
    }

    [TestMethod]
    public void ReadRecords_QuotedFieldWithEmbeddedDelimiter_ParsesCorrectly()
    {
        DelimitedRecordReader<Person> reader = new();
        using StringReader source = new("Name,Age\n\"Smith, Alice\",30");

        List<Person> records = [.. reader.ReadRecords(source)];

        Assert.AreEqual("Smith, Alice", records[0].Name);
    }

    [TestMethod]
    public void ReadRecords_EmptyLines_AreSkipped()
    {
        DelimitedRecordReader<Person> reader = new();
        using StringReader source = new("Name,Age\nAlice,30\n\nBob,25");

        List<Person> records = [.. reader.ReadRecords(source)];

        Assert.HasCount(2, records);
    }

    [TestMethod]
    public void ReadRecords_CustomDelimiter_ParsesCorrectly()
    {
        DelimitedRecordReader<Person> reader = new('\t');
        using StringReader source = new("Name\tAge\nAlice\t30");

        List<Person> records = [.. reader.ReadRecords(source)];

        Assert.AreEqual("Alice", records[0].Name);
        Assert.AreEqual(30, records[0].Age);
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
