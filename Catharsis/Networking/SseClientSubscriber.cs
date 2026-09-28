using Catharsis.Resilience;
using System.Net.Http.Headers;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;

namespace Catharsis.Networking;

///<summary>
///A reconnecting client subscriber for a server-sent-event endpoint: issues the initial and every reconnect
///request, tracks the last received <c>Last-Event-ID</c> and resumes from it, and delegates reconnect/backoff
///timing to a <see cref="Resilience.RetryPolicy"/>.
///</summary>
///<typeparam name="T">The type each event's data is parsed into.</typeparam>
///<param name="httpClient">The client used to issue the (re)connecting HTTP requests.</param>
///<param name="requestUri">The server-sent-event endpoint to subscribe to.</param>
///<param name="itemParser">The parser used to decode each event's payload bytes into a <typeparamref name="T"/>.</param>
///<param name="retryPolicy">The policy governing reconnect backoff. Defaults to a new <see cref="RetryPolicy"/> if not specified.</param>
///<exception cref="ArgumentNullException">
///<paramref name="httpClient"/>, <paramref name="requestUri"/>, or <paramref name="itemParser"/> is <c>null</c>.
///</exception>
public sealed class SseClientSubscriber<T>(HttpClient httpClient, Uri requestUri, SseItemParser<T> itemParser, RetryPolicy? retryPolicy = null)
{
    #region Fields
    readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    readonly Uri _requestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
    readonly SseItemParser<T> _itemParser = itemParser ?? throw new ArgumentNullException(nameof(itemParser));
    readonly RetryPolicy _retryPolicy = retryPolicy ?? new RetryPolicy();
    string? _lastEventId;
    #endregion

    #region Private methods
    async Task<Stream> OpenStreamAsync(CancellationToken cancellationToken)
    {
        HttpRequestMessage request = new(HttpMethod.Get, _requestUri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        if(_lastEventId is not null)
        {
            request.Headers.Add("Last-Event-ID", _lastEventId);
        }

        HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Subscribes to the endpoint, transparently reconnecting (with the configured backoff, resuming from the last
    ///received event ID) whenever the underlying stream fails.
    ///</summary>
    ///<param name="cancellationToken">A token to cancel the subscription.</param>
    ///<returns>An asynchronous sequence of every event received, across any number of reconnects.</returns>
    public async IAsyncEnumerable<SseItem<T>> SubscribeAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            Stream stream = await _retryPolicy.ExecuteAsync(OpenStreamAsync, cancellationToken).ConfigureAwait(false);

            await using(stream.ConfigureAwait(false))
            {
                SseParser<T> parser = SseParser.Create(stream, _itemParser);
                IAsyncEnumerator<SseItem<T>> enumerator = parser.EnumerateAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);

                await using(enumerator.ConfigureAwait(false))
                {
                    while(true)
                    {
                        bool moved;

                        try
                        {
                            moved = await enumerator.MoveNextAsync().ConfigureAwait(false);
                        } catch(IOException)
                        {
                            break;
                        }

                        if(!moved)
                        {
                            break;
                        }

                        SseItem<T> item = enumerator.Current;

                        if(item.EventId is not null)
                        {
                            _lastEventId = item.EventId;
                        }

                        yield return item;
                    }
                }
            }
        }
    }
    #endregion
}
