using Catharsis.Resilience;

namespace Catharsis.Networking;

///<summary>
///An <see cref="HttpMessageHandler"/> that wires a <see cref="RetryPolicy"/> into an <see cref="HttpClient"/> pipeline,
///retrying failed requests according to the configured policy.
///</summary>
///<remarks>
///A <see cref="HttpRequestMessage"/> can only be sent once, so each retry attempt sends a clone of the original request
///(including headers, content, and options) rather than reusing the same instance, which would throw on the second
///attempt.
///</remarks>
///<param name="retryPolicy">The retry policy to apply to every request sent through this handler.</param>
///<exception cref="ArgumentNullException"><paramref name="retryPolicy"/> is <c>null</c>.</exception>
public sealed class RetryableHttpMessageHandler(RetryPolicy retryPolicy) : DelegatingHandler
{
    #region Fields
    private readonly RetryPolicy _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
    #endregion

    #region Private methods
    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpRequestMessage clone = new(request.Method, request.RequestUri) { Version = request.Version };

        if(request.Content is not null)
        {
            MemoryStream contentStream = new();
            await request.Content.CopyToAsync(contentStream, cancellationToken).ConfigureAwait(false);
            contentStream.Position = 0;

            StreamContent content = new(contentStream);

            foreach(KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Content = content;
        }

        foreach(KeyValuePair<string, IEnumerable<string>> header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach(KeyValuePair<string, object?> option in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        return clone;
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _retryPolicy.ExecuteAsync(
               async token =>
               {
                   HttpRequestMessage attemptRequest = await CloneAsync(request, token).ConfigureAwait(false);
                   return await base.SendAsync(attemptRequest, token).ConfigureAwait(false);
               },
               cancellationToken);
    }
    #endregion
}
