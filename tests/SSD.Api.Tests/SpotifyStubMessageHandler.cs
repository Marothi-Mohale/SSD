using System.Net;
using System.Text;

namespace SSD.Api.Tests;

internal sealed class SpotifyStubMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    public List<SpotifyStubRequest> Requests { get; } = [];

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
        return SendAsyncCore(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsyncCore(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add(new SpotifyStubRequest(
            request.Method,
            request.RequestUri?.ToString() ?? string.Empty,
            body,
            request.Headers.ToDictionary(header => header.Key, header => string.Join(",", header.Value), StringComparer.OrdinalIgnoreCase)));

        if (_responses.Count == 0)
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        var responseFactory = _responses.Dequeue();
        return responseFactory(request);
    }
}

internal sealed record SpotifyStubRequest(
    HttpMethod Method,
    string Url,
    string? Body,
    IReadOnlyDictionary<string, string> Headers);
