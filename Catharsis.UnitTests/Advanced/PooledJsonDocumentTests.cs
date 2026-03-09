using System.Text;
using System.Text.Json;
using Catharsis.Advanced;

namespace Catharsis.UnitTests.Advanced;

///<summary>
///Unit tests for the <see cref="PooledJsonDocument"/> class.
///</summary>
[TestClass]
public class PooledJsonDocumentTests
{
    #region Public methods
    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        PooledJsonDocument doc = PooledJsonDocument.Parse("""{"x":1}""");
        doc.Dispose();
        doc.Dispose();
    }

    [TestMethod]
    public void Parse_Bytes_ReturnsDocument()
    {
        byte[] json = Encoding.UTF8.GetBytes("""{"n":42}""");
        using PooledJsonDocument doc = PooledJsonDocument.Parse(json.AsSpan());
        Assert.AreEqual(42, doc.RootElement.GetProperty("n").GetInt32());
    }

    [TestMethod]
    public void Parse_String_ReturnsDocument()
    {
        using PooledJsonDocument doc = PooledJsonDocument.Parse("""{"key":"value"}""");
        Assert.AreEqual(JsonValueKind.Object, doc.RootElement.ValueKind);
        Assert.AreEqual("value", doc.RootElement.GetProperty("key").GetString());
    }

    [TestMethod]
    public async Task ParseAsync_Stream_ReturnsDocument()
    {
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("""{"a":1}"""));
        using PooledJsonDocument doc = await PooledJsonDocument.ParseAsync(stream);
        Assert.AreEqual(1, doc.RootElement.GetProperty("a").GetInt32());
    }
    #endregion
}
