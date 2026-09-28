using System.Net.ServerSentEvents;
using System.Text.Json;

namespace Catharsis.Networking;

///<summary>
///Writes a sequence of <see cref="SseItem{T}"/> values as server-sent events to a stream, JSON-encoding each
///item's data (via <see cref="System.Text.Json.JsonSerializer"/>) as the event payload.
///</summary>
///<typeparam name="T">The type of each event's data.</typeparam>
///<param name="destination">The stream to write formatted events to.</param>
///<exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
public sealed class SseEventStreamWriter<T>(Stream destination)
{
    #region Fields
    readonly Stream _destination = destination ?? throw new ArgumentNullException(nameof(destination));
    #endregion

    #region Public methods
    ///<summary>
    ///Writes every event in <paramref name="events"/> to the destination stream, flushing after each one.
    ///</summary>
    ///<param name="events">The events to write.</param>
    ///<param name="cancellationToken">A token to cancel the write operation.</param>
    ///<exception cref="ArgumentNullException"><paramref name="events"/> is <c>null</c>.</exception>
    public Task WriteAsync(IAsyncEnumerable<SseItem<T>> events, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);

        return SseFormatter.WriteAsync(events, _destination, static (item, writer) =>
        {
            using Utf8JsonWriter jsonWriter = new(writer);
            JsonSerializer.Serialize(jsonWriter, item.Data);
        }, cancellationToken);
    }
    #endregion
}
