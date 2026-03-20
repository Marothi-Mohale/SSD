namespace SSD.Application.Contracts.Spotify;

public sealed record SpotifyTrackResponse(
    string SpotifyId,
    string Title,
    IReadOnlyList<string> Artists,
    string Album,
    string? ArtworkUrl,
    int DurationMilliseconds,
    string ExternalUrl,
    string? PreviewUrl,
    bool IsPlayable,
    bool IsExplicit,
    string? Market,
    string Provider = "Spotify");
