using Catharsis.Serialization;

namespace Catharsis.UnitTests.Serialization;

///<summary>
///Unit tests for the <see cref="DelimitedRecordWriter{T}"/> class.
///</summary>
[TestClass]
public class DelimitedRecordWriterTests
{
    #region WriteHeader

    [TestMethod]
    public void WriteHeader_NullWriter_Throws()
    {
        DelimitedRecordWriter<Person> writer = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => writer.WriteHeader(null!));
    }

    [TestMethod]
    public void WriteHeader_WritesPropertyNames()
    {
        DelimitedRecordWriter<Person> writer = new();
        using StringWriter output = new();

        writer.WriteHeader(output);

        Assert.AreEqual($"Age,Name{Environment.NewLine}", output.ToString());
    }

    #endregion

    #region WriteRecord

    [TestMethod]
    public void WriteRecord_NullWriter_Throws()
    {
        DelimitedRecordWriter<Person> writer = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => writer.WriteRecord(null!, new Person(30, "Alice")));
    }

    [TestMethod]
    public void WriteRecord_NullRecord_Throws()
    {
        DelimitedRecordWriter<Person> writer = new();
        using StringWriter output = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => writer.WriteRecord(output, null!));
    }

    [TestMethod]
    public void WriteRecord_WritesFieldValues()
    {
        DelimitedRecordWriter<Person> writer = new();
        using StringWriter output = new();

        writer.WriteRecord(output, new Person(30, "Alice"));

        Assert.AreEqual($"30,Alice{Environment.NewLine}", output.ToString());
    }

    [TestMethod]
    public void WriteRecord_ValueContainingDelimiter_IsQuoted()
    {
        DelimitedRecordWriter<Person> writer = new();
        using StringWriter output = new();

        writer.WriteRecord(output, new Person(30, "Smith, Alice"));

        Assert.AreEqual($"30,\"Smith, Alice\"{Environment.NewLine}", output.ToString());
    }

    [TestMethod]
    public void WriteRecord_ValueContainingQuote_IsEscaped()
    {
        DelimitedRecordWriter<Person> writer = new();
        using StringWriter output = new();

        writer.WriteRecord(output, new Person(30, "Ali\"ce"));

        Assert.AreEqual($"30,\"Ali\"\"ce\"{Environment.NewLine}", output.ToString());
    }

    #endregion

    #region Round-trip with DelimitedRecordReader

    [TestMethod]
    public void RoundTrip_WriteThenRead_ProducesEquivalentRecord()
    {
        DelimitedRecordWriter<Person> writer = new();
        DelimitedRecordReader<Person> reader = new();
        using StringWriter output = new();

        writer.WriteHeader(output);
        writer.WriteRecord(output, new Person(30, "Smith, Alice"));

        using StringReader input = new(output.ToString());
        List<Person> records = [.. reader.ReadRecords(input)];

        Assert.AreEqual(30, records[0].Age);
        Assert.AreEqual("Smith, Alice", records[0].Name);
    }

    #endregion

    private sealed class Person
    {
        #region Constructors
        public Person() { }

        public Person(int age, string name)
        {
            Age = age;
            Name = name;
        }
        #endregion

        #region Public properties
        public int Age { get; set; }
        public string? Name { get; set; }
        #endregion
    }
}
