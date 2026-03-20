using System.Net;
using System.Text;

namespace SSD.Api.Tests;

internal sealed class SpotifyStubMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    public List<HttpRequestMessage> Requests { get; } = [];

    public void Reset()
    {
        _responses.Clear();
        Requests.Clear();
    }

    public void EnqueueJson(HttpStatusCode statusCode, string json)
    {
        _responses.Enqueue(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        if (_responses.Count == 0)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        var responseFactory = _responses.Dequeue();
        return Task.FromResult(responseFactory(request));
    }
}
