using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SSD.Application.Exceptions;

namespace SSD.Infrastructure.Spotify;

public sealed class SpotifyApiClient(HttpClient httpClient, IOptions<SpotifyOptions> options) : ISpotifyApiClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly SpotifyOptions _options = options.Value;

    public async Task<SpotifyTokenResponse> ExchangeCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _options.RedirectUri,
            ["code_verifier"] = codeVerifier,
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
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildApiUri("me"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<SpotifyCurrentUserResponse>(response, "spotify_profile_unavailable", HttpStatusCode.BadRequest, cancellationToken);
    }

    public async Task<SpotifyTrackApiResponse> GetTrackAsync(string trackId, string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildApiUri($"tracks/{trackId}"));
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

        return await ReadResponseAsync<SpotifyTrackApiResponse>(response, "spotify_track_unavailable", HttpStatusCode.BadRequest, cancellationToken);
    }

    public async Task<SpotifyArtistApiResponse> GetArtistAsync(string artistId, string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildApiUri($"artists/{artistId}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new IntegrationException("spotify_artist_not_found", "The Spotify artist could not be found.", 404);
        }

        return await ReadResponseAsync<SpotifyArtistApiResponse>(response, "spotify_artist_unavailable", HttpStatusCode.BadRequest, cancellationToken);
    }

    public async Task<SpotifyPlaylistApiResponse> GetPlaylistAsync(string playlistId, string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildApiUri($"playlists/{playlistId}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new IntegrationException("spotify_playlist_not_found", "The Spotify playlist could not be found.", 404);
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new IntegrationException("spotify_playlist_unavailable", "The Spotify playlist is private or unavailable for this account.", 403);
        }

        return await ReadResponseAsync<SpotifyPlaylistApiResponse>(response, "spotify_playlist_unavailable", HttpStatusCode.BadRequest, cancellationToken);
    }

    public async Task<SpotifyPagingResponse<SpotifyTrackApiResponse>> GetCurrentUserTopTracksAsync(string accessToken, int limit, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildApiUri($"me/top/tracks?limit={limit}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new IntegrationException("spotify_scope_missing", "The linked Spotify account is missing the required scope for top tracks.", 403);
        }

        return await ReadResponseAsync<SpotifyPagingResponse<SpotifyTrackApiResponse>>(response, "spotify_top_tracks_unavailable", HttpStatusCode.BadGateway, cancellationToken);
    }

    public async Task<SpotifyPagingResponse<SpotifyArtistApiResponse>> GetCurrentUserTopArtistsAsync(string accessToken, int limit, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildApiUri($"me/top/artists?limit={limit}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new IntegrationException("spotify_scope_missing", "The linked Spotify account is missing the required scope for top artists.", 403);
        }

        return await ReadResponseAsync<SpotifyPagingResponse<SpotifyArtistApiResponse>>(response, "spotify_top_artists_unavailable", HttpStatusCode.BadGateway, cancellationToken);
    }

    private Uri BuildApiUri(string relativePath)
    {
        return new Uri(new Uri(_options.ApiBaseUrl), relativePath);
    }

    private static async Task<SpotifyTokenResponse> ReadTokenResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new IntegrationException("spotify_token_error", "Spotify token exchange failed.", 502);
        }

        return await ReadResponseAsync<SpotifyTokenResponse>(response, "spotify_token_error", HttpStatusCode.BadGateway, cancellationToken);
    }

    private static async Task<T> ReadResponseAsync<T>(
        HttpResponseMessage response,
        string code,
        HttpStatusCode failureStatus,
        CancellationToken cancellationToken)
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
