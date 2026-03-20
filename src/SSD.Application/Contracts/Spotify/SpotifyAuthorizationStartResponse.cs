namespace SSD.Application.Contracts.Spotify;

public sealed record SpotifyAuthorizationStartResponse(
    string AuthorizationUrl,
    string State);
