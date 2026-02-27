using System.Buffers;
using System.Text.Json;
using Catharsis.Buffers;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Advanced;

/// <summary>
/// Wraps a <see cref="JsonDocument"/> parsed from pooled memory, ensuring both
/// the document and the underlying buffer are properly disposed.
/// </summary>
public sealed class PooledJsonDocument : IDisposable
{
    private readonly byte[]? _rentedBuffer;
    private readonly ArrayPool<byte> _pool;
    private bool _disposed;

    private PooledJsonDocument(JsonDocument document, byte[]? rentedBuffer, ArrayPool<byte> pool)
    {
        Document = document;
        _rentedBuffer = rentedBuffer;
        _pool = pool;
    }

    /// <summary>Gets the parsed <see cref="JsonDocument"/>.</summary>
    public JsonDocument Document { get; }

    /// <summary>Gets the root element of the JSON document.</summary>
    public JsonElement RootElement => Document.RootElement;

    /// <summary>
    /// Parses a <see cref="PooledJsonDocument"/> from the specified UTF-8 JSON bytes using pooled memory.
    /// </summary>
    /// <param name="utf8Json">The UTF-8 JSON data.</param>
    /// <param name="options">Optional JSON document options.</param>
    /// <returns>A <see cref="PooledJsonDocument"/> wrapping the parsed document.</returns>
    public static PooledJsonDocument Parse(ReadOnlySpan<byte> utf8Json, JsonDocumentOptions options = default)
    {
        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        byte[] buffer = pool.Rent(utf8Json.Length);
        utf8Json.CopyTo(buffer);

        JsonDocument doc = JsonDocument.Parse(buffer.AsMemory(0, utf8Json.Length), options);
        return new PooledJsonDocument(doc, buffer, pool);
    }

    /// <summary>
    /// Parses a <see cref="PooledJsonDocument"/> from a string.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="options">Optional JSON document options.</param>
    /// <returns>A <see cref="PooledJsonDocument"/>.</returns>
    public static PooledJsonDocument Parse(string json, JsonDocumentOptions options = default)
    {
        Guard.IsNotNull(json);
        JsonDocument doc = JsonDocument.Parse(json, options);
        return new PooledJsonDocument(doc, null, ArrayPool<byte>.Shared);
    }

    /// <summary>
    /// Asynchronously parses a <see cref="PooledJsonDocument"/> from a stream.
    /// </summary>
    /// <param name="stream">The source stream containing UTF-8 JSON.</param>
    /// <param name="options">Optional JSON document options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="PooledJsonDocument"/>.</returns>
    public static async Task<PooledJsonDocument> ParseAsync(
        Stream stream,
        JsonDocumentOptions options = default,
        CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(stream);
        JsonDocument doc = await JsonDocument.ParseAsync(stream, options, cancellationToken);
        return new PooledJsonDocument(doc, null, ArrayPool<byte>.Shared);
    }

    /// <summary>
    /// Asynchronously parses a <see cref="PooledJsonDocument"/> from a stream.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <param name="options">Optional JSON document options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="PooledJsonDocument"/>.</returns>
    public static async ValueTask<PooledJsonDocument> ParseValueAsync(
        Stream stream,
        JsonDocumentOptions options = default,
        CancellationToken cancellationToken = default)
    {
        return await ParseAsync(stream, options, cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Document.Dispose();
        if (_rentedBuffer is not null)
            _pool.Return(_rentedBuffer);
    }
}
