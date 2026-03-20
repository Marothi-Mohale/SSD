using System.Net;

namespace SSD.Infrastructure.Spotify;

internal sealed class SpotifyRetryHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var requestClone = await CloneRequestAsync(request, cancellationToken);

            try
            {
                var response = await base.SendAsync(requestClone, cancellationToken);
                if (attempt == maxAttempts || !ShouldRetry(response.StatusCode))
                {
                    return response;
                }

                var retryDelay = GetRetryDelay(response, attempt);
                response.Dispose();
                await Task.Delay(retryDelay, cancellationToken);
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1)), cancellationToken);
            }
        }

        throw new InvalidOperationException("Spotify retry pipeline exhausted unexpectedly.");
    }

    private static bool ShouldRetry(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;
    }

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        if (response.Headers.RetryAfter?.Delta is { } retryAfter)
        {
            return retryAfter;
        }

        return TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt - 1));
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is not null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            clone.Content = new ByteArrayContent(contentBytes);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        clone.Version = request.Version;
        clone.VersionPolicy = request.VersionPolicy;
        return clone;
    }
}
