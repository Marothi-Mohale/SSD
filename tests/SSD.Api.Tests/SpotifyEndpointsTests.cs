using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using SSD.Application.Contracts.Auth;
using SSD.Application.Contracts.Spotify;

namespace SSD.Api.Tests;

public sealed class SpotifyEndpointsTests(TestAuthWebApplicationFactory factory) : IClassFixture<TestAuthWebApplicationFactory>
{
    [Fact]
    public async Task ResolveTrack_ReturnsMappedMetadata_ForValidSpotifyTrackUrl()
    {
        await factory.ResetDatabaseAsync();
        factory.SpotifyHandler.Reset();
        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "access_token": "catalog-token",
          "token_type": "Bearer",
          "expires_in": 3600
        }
        """);
        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "id": "11dFghVXANMlKmJXsNCbNl",
          "name": "Cut To The Feeling",
          "duration_ms": 207959,
          "preview_url": "https://p.scdn.co/mp3-preview/demo",
          "explicit": false,
          "is_playable": true,
          "external_urls": { "spotify": "https://open.spotify.com/track/11dFghVXANMlKmJXsNCbNl" },
          "artists": [{ "name": "Carly Rae Jepsen" }],
          "album": {
            "name": "Cut To The Feeling",
            "images": [{ "url": "https://i.scdn.co/image/demo", "width": 640, "height": 640 }]
          }
        }
        """);

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/spotify/resolve-track", new SpotifyResolveTrackRequest(
            "https://open.spotify.com/track/11dFghVXANMlKmJXsNCbNl"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<SpotifyTrackResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Cut To The Feeling", payload.Title);
        Assert.Equal("Cut To The Feeling", payload.Album);
        Assert.Equal("Carly Rae Jepsen", Assert.Single(payload.Artists));
        Assert.Equal("https://open.spotify.com/track/11dFghVXANMlKmJXsNCbNl", payload.ExternalUrl);
    }

    [Fact]
    public async Task ResolveTrack_ReturnsBadRequest_ForMalformedLink()
    {
        await factory.ResetDatabaseAsync();
        factory.SpotifyHandler.Reset();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/spotify/resolve-track", new SpotifyResolveTrackRequest("https://example.com/not-spotify"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LinkFlow_And_Me_ReturnLinkedSpotifyProfile()
    {
        await factory.ResetDatabaseAsync();
        factory.SpotifyHandler.Reset();

        using var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "spotifyuser@example.com",
            "StrongPass1234",
            "Spotify User",
            "iPhone"));

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var startResponse = await client.GetAsync("/api/spotify/link/start");
        Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);
        var startPayload = await startResponse.Content.ReadFromJsonAsync<SpotifyAuthorizationStartResponse>();
        Assert.NotNull(startPayload);
        Assert.Contains("accounts.spotify.com/authorize", startPayload.AuthorizationUrl, StringComparison.Ordinal);
        var authorizationQuery = HttpUtility.ParseQueryString(new Uri(startPayload.AuthorizationUrl).Query);
        Assert.Equal("S256", authorizationQuery["code_challenge_method"]);
        Assert.False(string.IsNullOrWhiteSpace(authorizationQuery["code_challenge"]));

        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "access_token": "user-access-token",
          "token_type": "Bearer",
          "scope": "user-read-email user-read-private user-top-read",
          "expires_in": 3600,
          "refresh_token": "user-refresh-token"
        }
        """);
        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "id": "spotify-user-123",
          "display_name": "Spotify Profile",
          "email": "spotify@example.com",
          "country": "US",
          "product": "premium"
        }
        """);

        var callbackResponse = await client.GetAsync($"/api/spotify/link/callback?code=demo-code&state={Uri.EscapeDataString(startPayload.State)}");
        Assert.Equal(HttpStatusCode.OK, callbackResponse.StatusCode);
        var tokenExchangeRequest = Assert.Single(factory.SpotifyHandler.Requests, request => request.Url.Contains("/api/token", StringComparison.Ordinal));
        Assert.Contains("code_verifier=", tokenExchangeRequest.Body, StringComparison.Ordinal);

        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "id": "spotify-user-123",
          "display_name": "Spotify Profile",
          "email": "spotify@example.com",
          "country": "US",
          "product": "premium"
        }
        """);

        var meResponse = await client.GetAsync("/api/spotify/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var mePayload = await meResponse.Content.ReadFromJsonAsync<SpotifyLinkedAccountResponse>();
        Assert.NotNull(mePayload);
        Assert.True(mePayload.IsLinked);
        Assert.Equal("spotify-user-123", mePayload.SpotifyUserId);
        Assert.Equal("spotify@example.com", mePayload.Email);
        Assert.Contains("user-read-email", mePayload.Scopes);
        Assert.Contains("user-top-read", mePayload.Scopes);
    }

    [Fact]
    public async Task ResolveArtist_ReturnsMappedMetadata()
    {
        await factory.ResetDatabaseAsync();
        factory.SpotifyHandler.Reset();
        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "access_token": "catalog-token",
          "token_type": "Bearer",
          "expires_in": 3600
        }
        """);
        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "id": "66CXWjxzNUsdJxJ2JdwvnR",
          "name": "Ariana Grande",
          "genres": ["pop", "dance pop"],
          "popularity": 92,
          "followers": { "total": 123456789 },
          "images": [{ "url": "https://i.scdn.co/image/artist", "width": 640, "height": 640 }],
          "external_urls": { "spotify": "https://open.spotify.com/artist/66CXWjxzNUsdJxJ2JdwvnR" }
        }
        """);

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/spotify/resolve-artist", new SpotifyResolveArtistRequest(
            "https://open.spotify.com/artist/66CXWjxzNUsdJxJ2JdwvnR"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<SpotifyArtistResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Ariana Grande", payload.Name);
        Assert.Contains("pop", payload.Genres);
        Assert.Equal(92, payload.Popularity);
    }

    [Fact]
    public async Task ResolvePlaylist_UsesLinkedToken_AndReturnsPlaylistTracks()
    {
        await factory.ResetDatabaseAsync();
        factory.SpotifyHandler.Reset();

        using var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "playlistuser@example.com",
            "StrongPass1234",
            "Playlist User",
            "Pixel"));

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var startResponse = await client.GetAsync("/api/spotify/link/start");
        var startPayload = await startResponse.Content.ReadFromJsonAsync<SpotifyAuthorizationStartResponse>();
        Assert.NotNull(startPayload);

        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "access_token": "user-access-token",
          "token_type": "Bearer",
          "scope": "user-read-email user-read-private user-top-read",
          "expires_in": 3600,
          "refresh_token": "user-refresh-token"
        }
        """);
        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "id": "spotify-user-123",
          "display_name": "Spotify Profile",
          "email": "spotify@example.com",
          "country": "US",
          "product": "premium"
        }
        """);

        var callbackResponse = await client.GetAsync($"/api/spotify/link/callback?code=demo-code&state={Uri.EscapeDataString(startPayload.State)}");
        Assert.Equal(HttpStatusCode.OK, callbackResponse.StatusCode);

        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "id": "37i9dQZF1DXcBWIGoYBM5M",
          "name": "Today's Top Hits",
          "description": "Mock playlist",
          "collaborative": false,
          "public": true,
          "owner": { "display_name": "Spotify" },
          "images": [{ "url": "https://i.scdn.co/image/playlist", "width": 640, "height": 640 }],
          "external_urls": { "spotify": "https://open.spotify.com/playlist/37i9dQZF1DXcBWIGoYBM5M" },
          "tracks": {
            "total": 1,
            "items": [{
              "track": {
                "id": "11dFghVXANMlKmJXsNCbNl",
                "name": "Cut To The Feeling",
                "duration_ms": 207959,
                "preview_url": "https://p.scdn.co/mp3-preview/demo",
                "explicit": false,
                "is_playable": true,
                "external_urls": { "spotify": "https://open.spotify.com/track/11dFghVXANMlKmJXsNCbNl" },
                "artists": [{ "name": "Carly Rae Jepsen" }],
                "album": {
                  "name": "Cut To The Feeling",
                  "images": [{ "url": "https://i.scdn.co/image/demo", "width": 640, "height": 640 }]
                }
              }
            }]
          }
        }
        """);

        var response = await client.PostAsJsonAsync("/api/spotify/resolve-playlist", new SpotifyResolvePlaylistRequest(
            "https://open.spotify.com/playlist/37i9dQZF1DXcBWIGoYBM5M"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<SpotifyPlaylistResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Today's Top Hits", payload.Name);
        Assert.Equal(1, payload.TrackCount);
        Assert.Equal("Cut To The Feeling", Assert.Single(payload.Tracks).Title);
    }

    [Fact]
    public async Task RecommendationContext_ReturnsMoodAlignedSignals()
    {
        await factory.ResetDatabaseAsync();
        factory.SpotifyHandler.Reset();

        using var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "contextuser@example.com",
            "StrongPass1234",
            "Context User",
            "iPhone"));

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var startResponse = await client.GetAsync("/api/spotify/link/start");
        var startPayload = await startResponse.Content.ReadFromJsonAsync<SpotifyAuthorizationStartResponse>();
        Assert.NotNull(startPayload);

        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "access_token": "user-access-token",
          "token_type": "Bearer",
          "scope": "user-read-email user-read-private user-top-read",
          "expires_in": 3600,
          "refresh_token": "user-refresh-token"
        }
        """);
        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "id": "spotify-user-123",
          "display_name": "Spotify Profile",
          "email": "spotify@example.com",
          "country": "US",
          "product": "premium"
        }
        """);

        await client.GetAsync($"/api/spotify/link/callback?code=demo-code&state={Uri.EscapeDataString(startPayload.State)}");

        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "items": [
            {
              "id": "11dFghVXANMlKmJXsNCbNl",
              "name": "Cut To The Feeling",
              "duration_ms": 207959,
              "preview_url": "https://p.scdn.co/mp3-preview/demo",
              "explicit": false,
              "is_playable": true,
              "external_urls": { "spotify": "https://open.spotify.com/track/11dFghVXANMlKmJXsNCbNl" },
              "artists": [{ "id": "abc", "name": "Carly Rae Jepsen" }],
              "album": {
                "name": "Cut To The Feeling",
                "images": [{ "url": "https://i.scdn.co/image/demo", "width": 640, "height": 640 }]
              }
            }
          ]
        }
        """);
        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "items": [
            {
              "id": "66CXWjxzNUsdJxJ2JdwvnR",
              "name": "Ariana Grande",
              "genres": ["pop", "dance pop"],
              "popularity": 92,
              "followers": { "total": 123456789 },
              "images": [{ "url": "https://i.scdn.co/image/artist", "width": 640, "height": 640 }],
              "external_urls": { "spotify": "https://open.spotify.com/artist/66CXWjxzNUsdJxJ2JdwvnR" }
            }
          ]
        }
        """);

        var response = await client.GetAsync("/api/spotify/recommendation-context?mood=happy");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<SpotifyRecommendationContextResponse>();
        Assert.NotNull(payload);
        Assert.True(payload.IsLinked);
        Assert.Equal("Happy", payload.Mood);
        Assert.Contains("pop", payload.SeedGenres);
        Assert.NotEmpty(payload.Signals);
        Assert.Contains("happy", payload.Explanation, StringComparison.OrdinalIgnoreCase);
    }
}
