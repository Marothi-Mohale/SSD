using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

        factory.SpotifyHandler.EnqueueJson(HttpStatusCode.OK, """
        {
          "access_token": "user-access-token",
          "token_type": "Bearer",
          "scope": "user-read-email user-read-private",
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
    }
}
