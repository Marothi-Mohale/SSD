namespace SSD.Application.Contracts.Spotify;

public sealed record SpotifyTrackSummaryResponse(
    string SpotifyId,
    string Title,
    IReadOnlyList<string> Artists,
    string Album,
    string? ArtworkUrl,
    string ExternalUrl,
    string? PreviewUrl);
