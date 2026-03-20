namespace SSD.Application.Contracts.Spotify;

public sealed record SpotifyPlaylistResponse(
    string SpotifyId,
    string Name,
    string? Description,
    string OwnerDisplayName,
    int TrackCount,
    string? ArtworkUrl,
    string ExternalUrl,
    bool IsCollaborative,
    bool IsPublic,
    IReadOnlyList<SpotifyTrackSummaryResponse> Tracks,
    string Provider = "Spotify");
