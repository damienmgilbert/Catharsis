using Catharsis.Networking;
using Catharsis.Resilience;

namespace Catharsis.UnitTests.Networking;

///<summary>
///Unit tests for the <see cref="RetryableHttpMessageHandler"/> class.
///</summary>
[TestClass]
public class RetryableHttpMessageHandlerTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullRetryPolicy_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new RetryableHttpMessageHandler(null!)); }

    #endregion

    #region SendAsync

    [TestMethod]
    public async Task SendAsync_NullRequest_Throws()
    {
        RetryableHttpMessageHandler handler = new(new RetryPolicy());
        handler.InnerHandler = new StubHandler(static _ => new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        using HttpMessageInvoker invoker = new(handler);

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => invoker.SendAsync(null!, CancellationToken.None));
    }

    [TestMethod]
    public async Task SendAsync_FirstAttemptSucceeds_ReturnsResponseWithoutRetrying()
    {
        int attempts = 0;
        RetryableHttpMessageHandler handler = new(new RetryPolicy().MaxAttempts(3))
        {
            InnerHandler = new StubHandler(_ =>
            {
                attempts++;
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            })
        };

        using HttpMessageInvoker invoker = new(handler);
        using HttpRequestMessage request = new(HttpMethod.Get, "https://example.test/resource");

        HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, attempts);
    }

    [TestMethod]
    public async Task SendAsync_TransientFailureThenSuccess_RetriesAndSucceeds()
    {
        int attempts = 0;
        RetryableHttpMessageHandler handler = new(new RetryPolicy().MaxAttempts(3).InitialDelay(TimeSpan.FromMilliseconds(1)).RetryOn<HttpRequestException>())
        {
            InnerHandler = new StubHandler(_ =>
            {
                attempts++;
                return (attempts < 2) ? throw new HttpRequestException("transient") : new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            })
        };

        using HttpMessageInvoker invoker = new(handler);
        using HttpRequestMessage request = new(HttpMethod.Get, "https://example.test/resource");

        HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, attempts);
    }

    [TestMethod]
    public async Task SendAsync_WithContent_ClonesContentForEachAttempt()
    {
        List<string> receivedBodies = [];
        RetryableHttpMessageHandler handler = new(new RetryPolicy().MaxAttempts(3).InitialDelay(TimeSpan.FromMilliseconds(1)).RetryOn<HttpRequestException>())
        {
            InnerHandler = new StubHandler(request =>
            {
                string body = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult() ?? string.Empty;
                receivedBodies.Add(body);
                return (receivedBodies.Count < 2) ? throw new HttpRequestException("transient") : new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            })
        };

        using HttpMessageInvoker invoker = new(handler);
        using HttpRequestMessage request = new(HttpMethod.Post, "https://example.test/resource") { Content = new StringContent("payload") };

        await invoker.SendAsync(request, CancellationToken.None);

        Assert.HasCount(2, receivedBodies);
        Assert.IsTrue(receivedBodies.All(static body => body == "payload"));
    }

    [TestMethod]
    public async Task SendAsync_AllAttemptsFail_ThrowsAggregateException()
    {
        RetryableHttpMessageHandler handler = new(new RetryPolicy().MaxAttempts(2).InitialDelay(TimeSpan.FromMilliseconds(1)).RetryOn<HttpRequestException>())
        {
            InnerHandler = new StubHandler(static _ => throw new HttpRequestException("permanent"))
        };

        using HttpMessageInvoker invoker = new(handler);
        using HttpRequestMessage request = new(HttpMethod.Get, "https://example.test/resource");

        await Assert.ThrowsExactlyAsync<AggregateException>(() => invoker.SendAsync(request, CancellationToken.None));
    }

    #endregion

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        #region Protected methods
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(responder(request));
        #endregion
    }
}
