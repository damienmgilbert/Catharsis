using System.Buffers;
using System.Text.Json;

namespace Catharsis.Serialization;

///<summary>
///A minimal facade over <see cref="Utf8JsonWriter"/> for hot-path JSON emission directly into an
///<see cref="IBufferWriter{T}"/> — e.g. <see cref="Catharsis.Buffers.PooledBuffer{T}"/> — instead of building a
///<see cref="string"/> or intermediate <see cref="System.Text.Json.Nodes.JsonNode"/> tree first.
///</summary>
///<remarks>
///This deliberately does not reimplement JSON escaping or encoding: <see cref="Utf8JsonWriter"/> already writes
///directly into the destination buffer's spans with no intermediate allocation, which is the entire point of a
///"span-based" writer. This type exists to pair that writer with this library's own pooled buffers and to expose
///only the handful of members most hot paths need.
///</remarks>
///<param name="bufferWriter">The destination buffer that encoded UTF-8 JSON bytes are written into.</param>
///<param name="options">The writer options to use, or the default options if omitted.</param>
public sealed class SpanJsonWriter(IBufferWriter<byte> bufferWriter, JsonWriterOptions options = default) : IDisposable
{
    #region Fields
    readonly Utf8JsonWriter _writer = new(bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter)), options);
    #endregion

    #region Public methods
    ///<inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose() => _writer.Dispose();

    ///<summary>
    ///Flushes any buffered writer state to the destination buffer.
    ///</summary>
    public void Flush() => _writer.Flush();

    ///<summary>
    ///Writes a <c>true</c> or <c>false</c> property value.
    ///</summary>
    ///<param name="propertyName">The property name.</param>
    ///<param name="value">The value.</param>
    public void WriteBoolean(string propertyName, bool value) => _writer.WriteBoolean(propertyName, value);

    ///<summary>
    ///Ends the current array.
    ///</summary>
    public void WriteEndArray() => _writer.WriteEndArray();

    ///<summary>
    ///Ends the current object.
    ///</summary>
    public void WriteEndObject() => _writer.WriteEndObject();

    ///<summary>
    ///Writes a numeric property value.
    ///</summary>
    ///<param name="propertyName">The property name.</param>
    ///<param name="value">The value.</param>
    public void WriteNumber(string propertyName, double value) => _writer.WriteNumber(propertyName, value);

    ///<summary>
    ///Starts a new array, optionally as the value of a property.
    ///</summary>
    ///<param name="propertyName">The property name, or <c>null</c> to start an unnamed (top-level or array-element) array.</param>
    public void WriteStartArray(string? propertyName = null)
    {
        if (propertyName is null)
        {
            _writer.WriteStartArray();
        }
        else
        {
            _writer.WriteStartArray(propertyName);
        }
    }

    ///<summary>
    ///Starts a new object, optionally as the value of a property.
    ///</summary>
    ///<param name="propertyName">The property name, or <c>null</c> to start an unnamed (top-level or array-element) object.</param>
    public void WriteStartObject(string? propertyName = null)
    {
        if (propertyName is null)
        {
            _writer.WriteStartObject();
        }
        else
        {
            _writer.WriteStartObject(propertyName);
        }
    }

    ///<summary>
    ///Writes a string property value.
    ///</summary>
    ///<param name="propertyName">The property name.</param>
    ///<param name="value">The value.</param>
    public void WriteString(string propertyName, string value) => _writer.WriteString(propertyName, value);
    #endregion
}
