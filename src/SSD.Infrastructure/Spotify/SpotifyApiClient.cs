using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SSD.Application.Exceptions;

namespace SSD.Infrastructure.Spotify;

public sealed class SpotifyApiClient(HttpClient httpClient, IOptions<SpotifyOptions> options) : ISpotifyApiClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly SpotifyOptions _options = options.Value;

    public async Task<SpotifyTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _options.RedirectUri,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        });

        using var response = await httpClient.PostAsync(_options.TokenBaseUrl, content, cancellationToken);
        return await ReadTokenResponseAsync(response, cancellationToken);
    }

    public async Task<SpotifyTokenResponse> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        });

        using var response = await httpClient.PostAsync(_options.TokenBaseUrl, content, cancellationToken);
        return await ReadTokenResponseAsync(response, cancellationToken);
    }

    public async Task<SpotifyTokenResponse> GetClientCredentialsTokenAsync(CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        });

        using var response = await httpClient.PostAsync(_options.TokenBaseUrl, content, cancellationToken);
        return await ReadTokenResponseAsync(response, cancellationToken);
    }

    public async Task<SpotifyCurrentUserResponse> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(_options.ApiBaseUrl), "me"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<SpotifyCurrentUserResponse>(response, cancellationToken, "spotify_profile_unavailable", HttpStatusCode.BadRequest);
    }

    public async Task<SpotifyTrackApiResponse> GetTrackAsync(string trackId, string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(_options.ApiBaseUrl), $"tracks/{trackId}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new IntegrationException("spotify_track_not_found", "The Spotify track could not be found.", 404);
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new IntegrationException("spotify_track_unavailable", "The Spotify track is not available in the current market.", 403);
        }

        return await ReadResponseAsync<SpotifyTrackApiResponse>(response, cancellationToken, "spotify_track_unavailable", HttpStatusCode.BadRequest);
    }

    private async Task<SpotifyTokenResponse> ReadTokenResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new IntegrationException("spotify_token_error", "Spotify token exchange failed.", 502);
        }

        return await ReadResponseAsync<SpotifyTokenResponse>(response, cancellationToken, "spotify_token_error", HttpStatusCode.BadGateway);
    }

    private static async Task<T> ReadResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken,
        string code,
        HttpStatusCode failureStatus)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new IntegrationException(code, "Spotify returned an unexpected response.", (int)failureStatus);
        }

        var payload = await response.Content.ReadFromJsonAsync<T>(SerializerOptions, cancellationToken);
        if (payload is null)
        {
            throw new IntegrationException(code, "Spotify returned an empty response.", (int)failureStatus);
        }

        return payload;
    }
}
