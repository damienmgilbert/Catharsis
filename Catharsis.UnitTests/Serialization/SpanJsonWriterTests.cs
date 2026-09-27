using Catharsis.Buffers;
using Catharsis.Serialization;
using System.Text;
using System.Text.Json;

namespace Catharsis.UnitTests.Serialization;

///<summary>
///Unit tests for the <see cref="SpanJsonWriter"/> class.
///</summary>
[TestClass]
public class SpanJsonWriterTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullBufferWriter_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new SpanJsonWriter(null!)); }

    #endregion

    #region Writing

    [TestMethod]
    public void WriteObject_SimpleProperties_ProducesExpectedJson()
    {
        using PooledBuffer<byte> buffer = new();
        using SpanJsonWriter writer = new(buffer);

        writer.WriteStartObject();
        writer.WriteString("name", "Alice");
        writer.WriteNumber("age", 30);
        writer.WriteBoolean("active", true);
        writer.WriteEndObject();
        writer.Flush();

        string json = Encoding.UTF8.GetString(buffer.WrittenSpan);
        using JsonDocument document = JsonDocument.Parse(json);

        Assert.AreEqual("Alice", document.RootElement.GetProperty("name").GetString());
        Assert.AreEqual(30, document.RootElement.GetProperty("age").GetInt32());
        Assert.IsTrue(document.RootElement.GetProperty("active").GetBoolean());
    }

    [TestMethod]
    public void WriteArray_NestedInObject_ProducesExpectedJson()
    {
        using PooledBuffer<byte> buffer = new();
        using SpanJsonWriter writer = new(buffer);

        writer.WriteStartObject();
        writer.WriteStartArray("items");
        writer.WriteStartObject();
        writer.WriteString("id", "1");
        writer.WriteEndObject();
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();

        string json = Encoding.UTF8.GetString(buffer.WrittenSpan);
        using JsonDocument document = JsonDocument.Parse(json);

        Assert.AreEqual("1", document.RootElement.GetProperty("items")[0].GetProperty("id").GetString());
    }

    [TestMethod]
    public void WriteStartArray_TopLevel_ProducesJsonArray()
    {
        using PooledBuffer<byte> buffer = new();
        using SpanJsonWriter writer = new(buffer);

        writer.WriteStartArray();
        writer.WriteStartObject();
        writer.WriteNumber("value", 1);
        writer.WriteEndObject();
        writer.WriteEndArray();
        writer.Flush();

        string json = Encoding.UTF8.GetString(buffer.WrittenSpan);
        using JsonDocument document = JsonDocument.Parse(json);

        Assert.AreEqual(JsonValueKind.Array, document.RootElement.ValueKind);
    }

    #endregion
}
